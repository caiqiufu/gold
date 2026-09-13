using AutoHorseRace.Utils;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace AutoHorseRace
{
    /// <summary>
    /// 落库 job 的类型：
    /// SubmitResult    —— ExecuteTrade 下单/吃票/强平提交后，服务器返回结果需要落库（原 ProcessSubmitOrderResult 的逻辑）。
    /// PeriodicRefresh —— RefreshTradeListToDB 对已 CONFIRMED 的交易做的周期性补写（"Auto Refresh"）。
    /// 两种 job 统一从同一套队列、按 dictKey 分片串行处理，避免两条通道同时写同一笔 TradeRecordId 产生竞态。
    /// </summary>
    public enum TradeWriteJobKind
    {
        SubmitResult,
        PeriodicRefresh
    }

    public class TradeWriteJob
    {
        public TradeWriteJobKind Kind;

        /// <summary>raceNo_type_combo，用于路由到固定 worker，保证同一 combo 的所有写入严格按提交顺序串行处理</summary>
        public string DictKey;

        /// <summary>仅 SubmitResult 需要；PeriodicRefresh 不用</summary>
        public EatBetInfo EatBetInfo;

        /// <summary>SubmitResult 时是本次下单的 BetInfo；PeriodicRefresh 时是待补写的那笔 trade</summary>
        public BetInfo BetInfo;

        /// <summary>仅 SubmitResult 需要：服务器对本次提交的原始返回</summary>
        public JObject Result;

        public string AutoFlag;
        public string OrderType;
        public bool Accepted;

        public DateTime EnqueuedAt = DateTime.Now;
    }

    /// <summary>
    /// 统一的交易结果/日志落库服务。
    ///
    /// 背景：原来 hr_trade_record / hr_trade_detail 有两个互不知道彼此存在的写入源——
    /// 1) ExecuteTrade -> ProcessSubmitOrderResult：下单结果一回来就落库；
    /// 2) timerRefreshAutoBettingInfo_Tick -> RefreshTradeListToDB：每 3 秒一次对已 CONFIRMED
    ///    的交易做"Auto Refresh"补写。
    /// 两者并发写同一个 combo 的记录时会出现竞态——比如 EAT 落库时 BET 刚生成的 TradeRecordId
    /// 还没来得及写回，或者 TradeRecordId 已经因为"删除挂单"被清理但另一条通道还拿着旧值去写，
    /// 触发外键约束异常（历史上靠手动 ResetTradeRecordId() 硬顶的那个问题正是这个原因）。
    ///
    /// 现在改成：所有写入都必须经过这里的 Enqueue，内部按 dictKey（raceNo_type_combo）哈希
    /// 路由到固定数量的 worker，每个 worker 单线程、严格按入队顺序（FIFO）处理自己那一片
    /// combo 的 job；不同 combo 分布在不同 worker 上并行处理，互不阻塞。这样：
    ///   - Enqueue 本身是 O(1) 无锁操作，ExecuteTrade 里调用它几乎不占用 betLock/eatLock 的持有时间；
    ///   - 同一个 combo 的 BET / EAT / PeriodicRefresh 永远排队串行执行，SaveTradeRecord 生成的
    ///     TradeRecordId 写回一定发生在下一个依赖它的 job 被处理之前；
    ///   - 落库失败会重试，重试耗尽后落"死信日志"而不是静默丢失。
    /// </summary>
    public class TradeRecordWriter
    {
        private static readonly Dictionary<string, string> AutoFlagDescriptions = new Dictionary<string, string>
        {
            { "Y", "自动" },
            { "SO", "强制" },
            { "N", "手动" }
        };

        private readonly Channel<TradeWriteJob>[] _channels;
        private readonly Task[] _workers;
        private readonly int _workerCount;
        private readonly Log _bizLog;
        private readonly Func<Account> _eaAccountProvider;
        private readonly Func<string, ComboTradeState> _resolveState;
        private readonly Action<string> _logInfo;
        private readonly Action<string> _logError;

        /// <summary>
        /// 🔧 按"组合+方向(BET/EAT)"分别记录各自的 TradeRecordId。
        ///
        /// 根因：同一个组合(combo)的赌腿(BET)和吃腿(EAT)是服务器两笔独立确认的订单，各自在
        /// hr_trade_record 里对应独立的一行、独立的 ID。但 ComboTradeState.TradeRecordId 是
        /// 按 dictKey（raceNo_type_combo，不区分方向）存的单一字段——BET 提交成功后被设成 BET
        /// 自己的 ID，紧接着 EAT 提交成功又把它覆盖成 EAT 自己的 ID。此后 RefreshTradeListToDB
        /// 对这个 combo 的 BET 和 EAT 两笔 CONFIRMED 记录分别入队做"Auto Refresh"补写时，
        /// 两个 job 的 DictKey 相同，都会从 state.TradeRecordId 读到同一个（很可能是错的那一个）
        /// ID，导致其中一腿的周期性补写被错误地写到了另一腿的 trade_record_id 下——这正是数据库里
        /// 缺一条日志（或日志被记错方向）的根因。
        ///
        /// 这里在 TradeRecordWriter 内部按 "dictKey_方向" 维护一份独立缓存，SubmitResult 落库时
        /// 各自写各自的，PeriodicRefresh 时也各自读各自的，彻底避免两腿互相覆盖。
        /// 不需要改动 ComboTradeState：state.TradeRecordId 仍然保留，作为"该组合是否已产生过
        /// 任意一条 trade_record"的粗粒度信号，供 RefreshTradeListToDB 的前置判断和 FK 失效重置使用。
        /// </summary>
        private readonly ConcurrentDictionary<string, int> _tradeRecordIdByLeg = new ConcurrentDictionary<string, int>();

        private static string LegKey(string dictKey, string action)
        {
            return $"{dictKey}_{(string.IsNullOrEmpty(action) ? "UNKNOWN" : action.ToUpperInvariant())}";
        }

        /// <param name="workerCount">并发 worker 数量，同一个 combo 永远落在同一个 worker 上串行处理</param>
        /// <param name="bizLog">业务日志（下注流水），传入时即可用，不需要延迟</param>
        /// <param name="eaAccountProvider">
        /// 延迟获取 _EAAccount 的回调：TradeRecordWriter 通常在 _EAAccount 被赋值之前就已经构造
        /// （EAForm.InitObject 早于 LoadConfigFile 执行），所以必须用回调而不是直接传值，
        /// 只有真正处理 job 时才会读取，构造阶段不会触碰。
        /// </param>
        /// <param name="resolveState">
        /// 按 dictKey 取（或补建）对应的 ComboTradeState，用于回写 TradeRecordId / 重置失效缓存。
        /// 同样延迟到处理 job 时才调用。
        /// </param>
        public TradeRecordWriter(
            int workerCount,
            Log bizLog,
            Func<Account> eaAccountProvider,
            Func<string, ComboTradeState> resolveState,
            Action<string> logInfo,
            Action<string> logError)
        {
            _workerCount = Math.Max(1, workerCount);
            _bizLog = bizLog;
            _eaAccountProvider = eaAccountProvider;
            _resolveState = resolveState;
            _logInfo = logInfo;
            _logError = logError;

            _channels = new Channel<TradeWriteJob>[_workerCount];
            for (int i = 0; i < _workerCount; i++)
            {
                _channels[i] = Channel.CreateBounded<TradeWriteJob>(new BoundedChannelOptions(1000)
                {
                    SingleReader = true,
                    SingleWriter = false,
                    FullMode = BoundedChannelFullMode.Wait // 宁可让入队方短暂阻塞等待，也不丢单
                });
            }

            _workers = new Task[_workerCount];
            for (int i = 0; i < _workerCount; i++)
            {
                int idx = i;
                _workers[i] = Task.Run(() => WorkerLoopAsync(idx));
            }
        }

        /// <summary>
        /// 入队：O(1) 无锁操作。按 dictKey 哈希固定路由到同一个 worker，
        /// 保证同一个 combo 的所有 job 严格按调用顺序串行处理。
        /// </summary>
        public void Enqueue(TradeWriteJob job)
        {
            if (job == null || string.IsNullOrEmpty(job.DictKey))
            {
                _logError?.Invoke("[TradeRecordWriter] job 或 DictKey 为空，已丢弃本次落库请求");
                return;
            }
            int idx = (int)((uint)job.DictKey.GetHashCode() % _workerCount);
            if (!_channels[idx].Writer.TryWrite(job))
            {
                _logError?.Invoke($"[TradeRecordWriter] worker[{idx}] 队列已满，job 入队阻塞等待: {job.DictKey}");
                _channels[idx].Writer.WriteAsync(job).AsTask().GetAwaiter().GetResult();
            }
        }

        private async Task WorkerLoopAsync(int idx)
        {
            var reader = _channels[idx].Reader;
            await foreach (var job in reader.ReadAllAsync())
            {
                await ProcessWithRetryAsync(job);
            }
        }

        private async Task ProcessWithRetryAsync(TradeWriteJob job)
        {
            const int maxAttempts = 3;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    ProcessJob(job);
                    return;
                }
                catch (MySql.Data.MySqlClient.MySqlException ex) when (ex.Message.Contains("foreign key constraint"))
                {
                    // 永久性错误：TradeRecordId 已失效（比如被"删除挂单"清理掉了），重试没有意义，
                    // 重置缓存后直接跳过，避免同一个坏 ID 反复报同一个错。
                    var state = _resolveState(job.DictKey);
                    state?.ResetTradeRecordId();
                    // 同时清掉这一腿(BET/EAT)自己缓存的 TradeRecordId，避免下一轮 Auto Refresh
                    // 继续拿着同一个已失效的 ID 重试、反复触发同一个外键错误。
                    _tradeRecordIdByLeg.TryRemove(LegKey(job.DictKey, job.BetInfo?.action), out _);
                    _logError?.Invoke($"[TradeRecordWriter][{job.DictKey}] TradeRecordId 已失效，重置缓存并跳过本次写入");
                    return;
                }
                catch (Exception ex)
                {
                    if (attempt == maxAttempts)
                    {
                        // 落"死信日志"，不静默丢失，方便人工核对 / 补录
                        _logError?.Invoke(
                            $"[TradeRecordWriter][{job.DictKey}] 写入失败(已重试{maxAttempts}次): {ex.Message}\n" +
                            $"job.Kind={job.Kind}, job.AutoFlag={job.AutoFlag}, job.OrderType={job.OrderType}, " +
                            $"job.EnqueuedAt={job.EnqueuedAt:yyyy-MM-dd HH:mm:ss.fff}");
                        return;
                    }
                    await Task.Delay(attempt * 300);
                }
            }
        }

        private void ProcessJob(TradeWriteJob job)
        {
            var eaAccount = _eaAccountProvider?.Invoke();
            if (eaAccount == null)
            {
                _logError?.Invoke($"[TradeRecordWriter][{job.DictKey}] 账户信息尚未就绪，跳过本次写入");
                return;
            }

            if (job.Kind == TradeWriteJobKind.SubmitResult)
            {
                var betInfo = job.BetInfo;
                if (betInfo == null) return;

                string tradeAction = betInfo.action == "BET" ? "赌" : "吃";
                betInfo.status = job.Accepted ? "SUCCESS" : "REJECTED";
                betInfo.remark = job.Result?["message"]?.ToString() ?? string.Empty;
                if (string.Equals(job.AutoFlag, "SO"))
                {
                    betInfo.remark = "[强平]" + betInfo.remark;
                }

                string flagDesc = AutoFlagDescriptions.TryGetValue(job.AutoFlag ?? "", out var desc) ? desc : "手动";
                _bizLog?.LogTradeRecord(
                    eaAccount.UserCode,
                    $"[{flagDesc}]  场次:{betInfo.raceNo}  马号:{betInfo.combo}  类型:{betInfo.type}{tradeAction}  " +
                    $"金额:{betInfo.stakeAmount}  折头:{betInfo.odds}  状态:{betInfo.status}  返回:{betInfo.remark}");

                if (job.Accepted)
                {
                    // betInfo 自己的 raceType/raceDate/raceNo，而不是 _Config.CurrentRaceType/CurrentRaceDate：
                    // 如果提交和落库之间恰好跨了场次切换，_Config.Current* 可能已经变成下一场的值了。
                    int generatedTradeRecordId = DBHelper.SaveTradeRecord(
                        eaAccount, betInfo.raceType, betInfo.raceDate, betInfo.raceNo,
                        job.EatBetInfo, job.AutoFlag, betInfo.remark);

                    if (generatedTradeRecordId > 0)
                    {
                        DBHelper.saveTradeDetailLog(
                            eaAccount, betInfo.raceType, betInfo.raceDate, betInfo.raceNo,
                            generatedTradeRecordId, job.OrderType, job.AutoFlag, betInfo, betInfo.remark);

                        // 🔑 按"这一腿"(BET/EAT)独立记住自己的 TradeRecordId，
                        // 供后续这一腿自己的 PeriodicRefresh 使用，不会被另一腿覆盖。
                        _tradeRecordIdByLeg[LegKey(job.DictKey, betInfo.action)] = generatedTradeRecordId;

                        // state.TradeRecordId 仍然写回，继续作为"该组合是否已产生过任意一条
                        // trade_record"的粗粒度信号（RefreshTradeListToDB 的前置判断要用），
                        // 但不再是 PeriodicRefresh 真正落库时使用的 ID。
                        var state = _resolveState(job.DictKey);
                        state?.SetTradeRecordId(generatedTradeRecordId);
                    }
                }
            }
            else // PeriodicRefresh
            {
                var trade = job.BetInfo;
                if (trade == null) return;

                var state = _resolveState(job.DictKey);
                if (state == null || state.TradeRecordId <= 0) return;

                // 🔑 优先使用"这一腿"(trade.action = BET/EAT)自己的 TradeRecordId；
                // 只有在异常情况下（比如这一腿理论上不可能出现、但缓存确实还没命中）才退化
                // 使用 state.TradeRecordId 兜底。正常路径下，能走到 PeriodicRefresh 说明这一腿
                // 之前必然经过 SubmitResult 成功落库，_tradeRecordIdByLeg 里一定已经有它自己的 ID。
                string legKey = LegKey(job.DictKey, trade.action);
                int tradeRecordId = _tradeRecordIdByLeg.TryGetValue(legKey, out var legTradeRecordId) && legTradeRecordId > 0
                    ? legTradeRecordId
                    : state.TradeRecordId;
                if (tradeRecordId <= 0) return;

                DBHelper.saveTradeDetailLog(
                    eaAccount, trade.raceType, trade.raceDate, trade.raceNo,
                    tradeRecordId, "P", "Y", trade, "Auto Refresh");
            }
        }

        /// <summary>
        /// 关闭写入服务：停止接收新 job，并给已入队但还没处理完的 job 最多 drainTimeout 的排干时间。
        /// </summary>
        public void Stop(TimeSpan drainTimeout)
        {
            foreach (var ch in _channels)
            {
                ch.Writer.Complete();
            }
            try
            {
                Task.WaitAll(_workers, drainTimeout);
            }
            catch (AggregateException)
            {
                // 忽略排干超时/取消产生的聚合异常，不影响程序正常退出
            }
        }
    }
}
