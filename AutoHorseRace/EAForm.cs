using AutoHorseRace.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text.RegularExpressions;
namespace AutoHorseRace
{
    public partial class EAForm : Form
    {
        //后于后端最短timeout为3秒,前端请求必须大于10秒
        // 1. 在窗体顶部直接声明两个独立的定时器实例
        private RandomTaskTimer _timerRefreshMyTrade;
        private RandomTaskTimer _timerRefreshBetInfo;
        private RandomTaskTimer _timerRefreshBalance;
        private RandomTaskTimer _timerRefreshBetInfoForClosePosition;
        /// <summary>
        /// 最新下注列表
        /// </summary>
        private List<EatBetInfo> _EatBetInfosList;
        /// <summary>
        /// 最新下注信息字典表
        /// </summary>
        private Dictionary<string, EatBetInfo> _EatBetInfoDict;
        /// <summary>
        /// 我的交易信息
        /// </summary>
        List<BetInfo> _AllMyTradesList = new List<BetInfo>();
        private void InitializeTimers()
        {
            // 2. 实例化业务 A：基础 10秒 + 随机 1~15秒
            _timerRefreshBalance = new RandomTaskTimer(
                "业务A_余额查询",
                async () => await QueryBalanceDataAsync(),
                baseDelaySeconds: 60,
                randomMinSeconds: 10,
                randomMaxSeconds: 90
            );
            // 2. 实例化业务 A：基础 10秒 + 随机 1~15秒
            _timerRefreshMyTrade = new RandomTaskTimer(
                "业务B_我的交易",
                async () => await QueryMyTradeInfListDataAsync(),
                baseDelaySeconds: 3,
                randomMinSeconds: 1,
                randomMaxSeconds: 15
            );
            // 3. 实例化业务 B：基础 5秒 + 随机 1~5秒
            _timerRefreshBetInfo = new RandomTaskTimer(
                "业务C_盘口信息",
                async () => await QueryEATBETInfoDataAsync(),
                baseDelaySeconds: 3,
                randomMinSeconds: 1,
                randomMaxSeconds: 10
            );
            // 🔧 强平轮询：不在方法内部写循环等待成交，而是靠这个定时器每隔约 5 秒（固定 5 秒 + 0~2 秒随机抖动，
            // 避免多个客户端固定 5 秒整数倍撞车）重新调用一次 QueryEATBETInfoDataForClosePositionAsync，
            // 每次调用都完整执行一轮"删挂单 → 等待服务器数据落地 → 重新同步仓位快照 → 按需重新挂吃注单"，
            // 直到强平窗口结束（开赛时刻）或该组合被 TradeDecisionEngine 判定为不再需要处理为止。
            _timerRefreshBetInfoForClosePosition = new RandomTaskTimer(
                "业务C_强平下注",
                async () => await QueryEATBETInfoDataForClosePositionAsync(),
                baseDelaySeconds: 15,
                randomMinSeconds: 1,
                randomMaxSeconds: 6
            );
        }
        private float _ProcessingAngle = 0; // 旋转角度
        private System.Windows.Forms.Timer _scanTimer = new System.Windows.Forms.Timer(); // 负责定时触发重绘
        private System.Windows.Forms.Timer _loginTimer = new System.Windows.Forms.Timer(); // 负责定时触发重绘
        private System.Windows.Forms.Timer _DSLoginTimer = new System.Windows.Forms.Timer(); // 负责定时触发重绘
        private void InitProcessingAnimation()
        {
            _scanTimer.Interval = 30; // 刷新频率，越小越平滑
            _scanTimer.Tick += (s, e) =>
            {
                _ProcessingAngle = (_ProcessingAngle + 15) % 360; // 每次旋转15度
                pictureBoxScanProcessing.Invalidate();    // 强制触发 Paint 事件进行重绘
            };
            // 设置 PictureBox 背景为透明，防止盖住界面
            pictureBoxScanProcessing.BackColor = Color.Transparent;
            _loginTimer.Interval = 30; // 刷新频率，越小越平滑
            _loginTimer.Tick += (s, e) =>
            {
                _ProcessingAngle = (_ProcessingAngle + 15) % 360; // 每次旋转15度
                pictureBoxLoginProcessing.Invalidate();    // 强制触发 Paint 事件进行重绘
            };
            // 设置 PictureBox 背景为透明，防止盖住界面
            pictureBoxLoginProcessing.BackColor = Color.Transparent;
            _DSLoginTimer.Interval = 30; // 刷新频率，越小越平滑
            _DSLoginTimer.Tick += (s, e) =>
            {
                _ProcessingAngle = (_ProcessingAngle + 15) % 360; // 每次旋转15度
                pictureBoxDSLoginProcessing.Invalidate();    // 强制触发 Paint 事件进行重绘
            };
            // 设置 PictureBox 背景为透明，防止盖住界面
            pictureBoxDSLoginProcessing.BackColor = Color.Transparent;
        }
        // =================================================================
        // ⚡ 具体的业务逻辑处理（包装成 async 任务）
        // =================================================================
        /// <summary>
        /// 查询余额
        /// </summary>
        /// <returns></returns>
        private async Task QueryBalanceDataAsync()
        {
            // 如果没有防重入机制，建议这里也可以考虑加一个类似于前面的 _isRefreshing 锁，防止定时器重叠触发
            if (_EAAccount.IsLogin && _EnableBettingInfoRefresh)
            {
                // 直接异步调用，内部的 HTTP 请求会自动在后台线程执行
                UpdateAccountBalanceInfo("EA");
            }
            if (_DSAccount.IsLogin && _EnableBettingInfoRefresh && !string.Equals(_Config.DSServerAddress, _Config.EAServerAddress))
            {
                // 错开 5 秒请求，避免两个账号在同一瞬间并发打满网络或服务器
                await Task.Delay(5000);
                UpdateAccountBalanceInfo("DS");
            }
        }
        private DateTime _lastRefreshTime = DateTime.MinValue; // 初始化为最小值，确保第一次立即执行
        // 1. 在类中定义并发锁和防重入标志（放在类成员变量区域）
        private readonly object _lockObj = new object();
        private bool _isRefreshing = false;
        private readonly TradeDecisionEngine _decisionEngine = new TradeDecisionEngine();

        /// <summary>
        /// 统一的交易结果/日志落库服务：ExecuteTrade 的实时提交结果、RefreshTradeListToDB 的周期性
        /// 补写，全部经这里 Enqueue 入队，由固定数量的 worker 按 dictKey（raceNo_type_combo）分片、
        /// 严格按提交顺序串行落库，避免多个写入源互相竞态。详见 TradeRecordWriter.cs。
        /// </summary>
        private TradeRecordWriter _tradeRecordWriter;

        /// <summary>
        /// 定时查询并刷新盘口/投注信息的异步方法（已加入防重入与并发安全保护）
        /// </summary>
        private async Task QueryEATBETInfoDataAsync()
        {
            if (!_EAAccount.IsLogin || !_EnableBettingInfoRefresh)
            {
                return;
            }
            // 检查时间间隔
            if ((DateTime.Now - _lastRefreshTime).TotalSeconds < _Config.AutoTradeInterval)
            {
                return;
            }
            // 加锁检查并设置防重入标志，确保上一轮没跑完时绝不开启新一轮
            lock (_lockObj)
            {
                if (_isRefreshing)
                {
                    return;
                }
                _isRefreshing = true;
            }
            _lastRefreshTime = DateTime.Now; // 更新上一次执行时间
            // 开启高精度计时器监控执行耗时
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        RefreshBetInfoDataList();
                    }
                    catch (Exception ex)
                    {
                        // 建议在此处记录错误日志
                        System.Diagnostics.Debug.WriteLine($"[Error] 刷新投注信息异常: {ex.Message}");
                    }
                });
            }
            finally
            {
                stopwatch.Stop();
                _Log.LogInfo($"[性能监控] QueryEATBETInfoDataAsync 执行耗时: {stopwatch.ElapsedMilliseconds} ms");
                // 确保无论成功还是异常，都能正确释放防重入锁
                lock (_lockObj)
                {
                    _isRefreshing = false;
                }
            }
        }
        // 从 QueryMyTradeInfListDataAsync 里抽出来的共享逻辑
        private void RefreshMyTradeSnapshot()
        {
            string serverProcessTime = "0ms";
            IDictionary<string, List<BetInfo>> BetBatInfos = HTTPHelper.queryMyTrade(
                _Config.EAServerAddress, _EAAccount.UserCode,
                _Config.CurrentRaceDate, _Config.CurrentRaceType, _Config.CurrentRaceNo,
                out serverProcessTime);
            _Log.LogInfo($"[性能监控][queryMyTrade] 后端服务耗时: {serverProcessTime}");
            if (BetBatInfos == null) { _Log.LogInfo("扫描结束,无数据"); return; }
            var (allMyTrades, eatBetInfosList, eatBetInfoDict) = Utils.Utils.GetBatBetInfo(BetBatInfos);
            foreach (var kvp in eatBetInfoDict)
            {
                var info = kvp.Value;
                var state = _Config.TradeStateStore.GetOrCreate(info.raceNo, info.type, info.combo);
                state.ApplyServerSnapshot(info);
                foreach (var stale in state.GetStaleIntents())
                {
                    _logger.Error($"[{state.DictKey}] {(stale.IsBet ? "下注" : "吃票")}预占已超过TTL但服务器仍未体现(intentId={stale.Id}, amount={stale.Amount})——" +
                                  $"该组合在服务器确认前会一直被视为'有pending'而拒绝新单，需要人工核实是否GetBatBetInfo/queryMyTrade没能及时反映这笔单子。");
                }
                _Config.TradeStateStore.MarkDirty(state.DictKey);
            }
            _EatBetInfosList = eatBetInfosList;
            _EatBetInfoDict = eatBetInfoDict;
            _AllMyTradesList = allMyTrades;
            RefreshMyTradeList(_AllMyTradesList);
        }
        /// <summary>
        /// 查询我的交易信息列表数据
        /// </summary>
        /// <returns></returns>
        private async Task QueryMyTradeInfListDataAsync()
        {
            // 🔧 自动下注开启时，仓位快照改由 RefreshBetInfoDataList -> RefreshMyTradeSnapshot() 内联刷新，
            // 跟决策循环用同一个节奏；这里只在"未开自动下注、纯扫描监控"时才独立跑，
            // 避免两条通道同时对同一批 ComboTradeState 调用 ApplyServerSnapshot 造成写入乱序
            // （服务器不返回 seq，没法在数据里做防护，只能从根上避免两条通道并存）。
            if (_EAAccount.IsLogin && _EnableBettingInfoRefresh && !_EnableAutoTrade)
            {
                // 采用 TryGetTradeTimeContext 判断当前是否符合配置的交易时间
                if (!TryGetTradeTimeContext(out DateTime raceDateTime, out DateTime triggerStartTime, out DateTime triggerEndTime))
                {
                    // 如果不在交易时间范围内，直接返回，避免不必要的数据库与网络开销
                    return;
                }
                await QueryAndApplyMyTradeSnapshotAsync();
            }
        }

        /// <summary>
        /// 查询我方仓位快照并写入 TradeStateStore（唯一权威写入点：queryMyTrade 是"我方仓位"的唯一真相来源）。
        /// 供 QueryMyTradeInfListDataAsync 及其他需要独立触发一次仓位快照刷新的地方调用。
        /// </summary>
        private async Task QueryAndApplyMyTradeSnapshotAsync()
        {
            // 开启高精度计时器
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            string serverProcessTime = "0ms";
            try
            {
                // 直接在当前上下文中执行
                //Dictionary<string, IDictionary<string, string>> bettingInfoDict = DBHelper.queryAllOpenBettingInfoForRance(_EAAccount.UserCode, _Config.CurrentRaceType, _Config.CurrentRaceDate, _Config.CurrentRaceNo, null, null);
                // 进行网络请求（耗时操作）
                IDictionary<string, List<BetInfo>> BetBatInfos = HTTPHelper.queryMyTrade(_Config.EAServerAddress, _EAAccount.UserCode, _Config.CurrentRaceDate, _Config.CurrentRaceType, _Config.CurrentRaceNo, out serverProcessTime);
                _Log.LogInfo($"[性能监控][QueryMyTradeInfListDataAsync][queryMyTrade] 后端服务耗时: {serverProcessTime}");
                if (BetBatInfos != null)
                {
                    _logger.Debug($"[queryMyTrade Result]:{JsonConvert.SerializeObject(BetBatInfos, Formatting.Indented)}");
                    var (allMyTrades, eatBetInfosList, eatBetInfoDict) = Utils.Utils.GetBatBetInfo(BetBatInfos);
                    _logger.Debug($"[QueryMyTradeInfListDataAsync allMyTrades Result]:{JsonConvert.SerializeObject(allMyTrades, Formatting.Indented)}");
                    _logger.Debug($"[QueryMyTradeInfListDataAsync eatBetInfoDict Result]:{JsonConvert.SerializeObject(eatBetInfoDict, Formatting.Indented)}");
                    // 🔥 唯一权威写入点：queryMyTrade 是"我方仓位"的唯一真相来源
                    foreach (var kvp in eatBetInfoDict)
                    {
                        var info = kvp.Value;
                        var state = _Config.TradeStateStore.GetOrCreate(info.raceNo, info.type, info.combo);
                        state.ApplyServerSnapshot(info);
                        foreach (var stale in state.GetStaleIntents())
                        {
                            _logger.Error($"[{state.DictKey}] {(stale.IsBet ? "下注" : "吃票")}预占已超过TTL但服务器仍未体现(intentId={stale.Id}, amount={stale.Amount})——" +
                                          $"该组合在服务器确认前会一直被视为'有pending'而拒绝新单，需要人工核实是否GetBatBetInfo/queryMyTrade没能及时反映这笔单子。");
                        }
                        _Config.TradeStateStore.MarkDirty(state.DictKey);
                    }
                    _EatBetInfosList = eatBetInfosList;
                    _EatBetInfoDict = eatBetInfoDict;
                    _AllMyTradesList = allMyTrades;
                    RefreshMyTradeList(_AllMyTradesList);
                }
                else
                {
                    _Log.LogInfo($"扫描结束,无数据");
                }
            }
            finally
            {
                // 停止计时并输出耗时
                stopwatch.Stop();
                long elapsedMs = stopwatch.ElapsedMilliseconds;
                _Log.LogInfo($"[性能监控] QueryMyTradeInfListDataAsync 执行耗时: {elapsedMs} ms");
            }
        }

        /// <summary>
        /// 浮点误差容忍度，用于金额/挂单是否为零的判断
        /// </summary>
        // V20260912_CLOSE_WINDOW_AUDIT:
        // 正常下注截止 = Race - _Config.AutoTradeEndTimeDuration
        // 强平启动 = 正常下注截止 + 10 秒
        // 强平窗口 = [强平启动, Race)
        // 正常/强平 EAT 均通过 TryReserveEatIntent + HardRemaining 原子限额。
        private const double AmountEpsilon = 0.001;

        /// <summary>
        /// 强平轮询入口：由 _timerRefreshBetInfoForClosePosition 每隔约 5 秒调用一次，
        /// 不在方法内部循环等待成交，而是依赖定时器的下一次触发形成"轮询"效果。
        /// 每次调用都完整执行一轮：删除现有挂单 → 等待服务器数据落地 → 重新同步仓位快照 →
        /// 按 TradeDecisionEngine 的判断结果重新挂吃注单。
        /// </summary>
        private async Task QueryEATBETInfoDataForClosePositionAsync()
        {
            // =========================================================================
            // 强制平仓时间窗口检查
            //
            // 重要：
            // 这里不能使用 TimeSpan 做比较，因为比赛时间可能是 00:00:00 ~ 05:59:59，
            // 当当前时间处于前一天晚上时，会发生跨天判断错误。
            //
            // 例如：
            // 当前时间       = 2026-09-12 23:58:09
            // 比赛时间       = 00:00:00
            // 实际比赛时间   = 2026-09-13 00:00:00
            //
            // 如果 AutoTradeEndTimeDuration = 120 秒：
            // 强平开始时间   = 2026-09-12 23:58:00
            // 强平结束时间   = 2026-09-13 00:00:00
            //
            // 因此当前时间 23:58:09 应当正确判定为：
            // [强平窗口内]
            // =========================================================================

            DateTime now = DateTime.Now;

            // -------------------------------------------------------------------------
            // 1. 解析当前比赛时间
            // -------------------------------------------------------------------------
            if (!TimeSpan.TryParse(
                    _Config.CurrentRaceTime,
                    out TimeSpan raceTime))
            {
                _logger.Error(
                    $"[强制平仓] 解析当前比赛时间失败，" +
                    $"CurrentRaceTime 格式无效: '{_Config.CurrentRaceTime}'");

                return;
            }

            // -------------------------------------------------------------------------
            // 2. 构造完整的比赛 DateTime
            // -------------------------------------------------------------------------
            string todayStr = now.ToString("yyyy-MM-dd");

            string fullRaceTimeStr =
                $"{todayStr} {_Config.CurrentRaceTime}:00";

            if (!DateTime.TryParseExact(
                    fullRaceTimeStr,
                    "yyyy-MM-dd HH:mm:ss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime raceDateTime))
            {
                _logger.Error(
                    $"[强制平仓] 构造比赛完整时间失败，" +
                    $"FullRaceTime='{fullRaceTimeStr}', " +
                    $"CurrentRaceTime='{_Config.CurrentRaceTime}'");

                return;
            }

            // -------------------------------------------------------------------------
            // 3. 跨天处理
            //
            // 如果当前已经是晚上 20:00 以后，而比赛时间是凌晨
            // 00:00 ~ 05:59，则比赛属于“下一天”。
            //
            // 例如：
            // Now       = 2026-09-12 23:58:09
            // RaceTime  = 00:00:00
            //
            // 初始解析：
            // 2026-09-12 00:00:00
            //
            // 修正后：
            // 2026-09-13 00:00:00
            // -------------------------------------------------------------------------
            if (now.Hour >= 20 && raceDateTime.Hour < 6)
            {
                raceDateTime = raceDateTime.AddDays(1);
            }

            // -------------------------------------------------------------------------
            // 4. 计算正常下注截止时间与强平启动/结束时间
            // 正常下注截止 = 开赛前 AutoTradeEndTimeDuration 秒
            // 强平启动 = 正常下注截止后 10 秒
            // 强平窗口 = [closeStartTime, closeEndTime)，closeEndTime = 开赛后 30 秒
            // V20260913_CLOSE_WINDOW_EXTEND：强平窗口原本在开赛时刻（raceDateTime）就截止，
            // 现在按需求延长到开赛后 30 秒，给强平多留一轮轮询的时间窗口。
            // -------------------------------------------------------------------------
            DateTime normalBetEndTime = raceDateTime.AddSeconds(-_Config.AutoTradeEndTimeDuration);
            DateTime closeStartTime = normalBetEndTime.AddSeconds(10);
            DateTime closeEndTime = raceDateTime.AddSeconds(30);

            // -------------------------------------------------------------------------
            // 5. 输出时间诊断日志
            //
            // 这个日志非常重要。
            // 如果以后再次出现“强平没有执行”的问题，可以直接从日志判断
            // 当前时间、比赛时间以及强平窗口是否正确。
            // -------------------------------------------------------------------------
            _Log.LogInfo(
                $"[强平时间检查] " +
                $"Now=[{now:yyyy-MM-dd HH:mm:ss}] | " +
                $"NormalBetEnd=[{normalBetEndTime:yyyy-MM-dd HH:mm:ss}] | " +
                $"CloseStart=[{closeStartTime:yyyy-MM-dd HH:mm:ss}] | " +
                $"CloseEnd=[{closeEndTime:yyyy-MM-dd HH:mm:ss}] | " +
                $"Race=[{raceDateTime:yyyy-MM-dd HH:mm:ss}] | " +
                $"AutoDeletePendingOrder=[{_Config.AutoDeletePendingOrder}] | " +
                $"AutoClosePosition=[{_Config.AutoClosePosition}]");

            // -------------------------------------------------------------------------
            // 6. 判断是否处于强平窗口
            // 强平窗口严格为 [closeStartTime, closeEndTime)，即正常下注截止后 10 秒启动，开赛后 30 秒结束。
            // -------------------------------------------------------------------------
            if (now < closeStartTime || now >= closeEndTime)
            {
                _Log.LogInfo(
                    $"[强平时间检查] 当前不在强平窗口，跳过本轮。" +
                    $" Now=[{now:yyyy-MM-dd HH:mm:ss}], " +
                    $"Window=[{closeStartTime:yyyy-MM-dd HH:mm:ss} ~ " +
                    $"{closeEndTime:yyyy-MM-dd HH:mm:ss})");

                return;
            }

            // -------------------------------------------------------------------------
            // 7. 当前正式进入强平窗口
            // -------------------------------------------------------------------------
            var stopwatch =
                System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // ---------------------------------------------------------------------
                // 计算距离开赛还有多少秒
                // ---------------------------------------------------------------------
                int remainingSeconds =
                    Math.Max(
                        0,
                        (int)(raceDateTime - now).TotalSeconds);

                _Log.LogInfo(
                    $"距离开赛还有 [{remainingSeconds}] 秒，" +
                    $"开始本轮强平轮询");

                // ---------------------------------------------------------------------
                // 8. 删除挂单
                //
                // AutoDeletePendingOrder 与 AutoClosePosition 是两个独立功能。
                // 删除挂单只由 AutoDeletePendingOrder 控制。
                // ---------------------------------------------------------------------
                if (_Config.AutoDeletePendingOrder)
                {
                    _Log.LogInfo(
                        "[强平] AutoDeletePendingOrder=TRUE，" +
                        "开始执行删除挂单");

                    await DeletePendingOrdersAndCleanLocalStateAsync();
                    if (DateTime.Now >= closeEndTime)
                    {
                        _Log.LogInfo($"[强平] 删除挂单完成后已超过强平窗口结束时间[{closeEndTime:yyyy-MM-dd HH:mm:ss}]，停止本轮强平。");
                        return;
                    }
                }
                else
                {
                    _Log.LogInfo(
                        "[强平] AutoDeletePendingOrder=FALSE，" +
                        "跳过删除挂单");
                }

                // ---------------------------------------------------------------------
                // 9. 强平持仓
                //
                // 注意：
                // 强平持仓不能依赖 AutoDeletePendingOrder。
                // 两个功能完全独立。
                // ---------------------------------------------------------------------
                if (_Config.AutoClosePosition)
                {
                    _Log.LogInfo(
                        "[强平] AutoClosePosition=TRUE，" +
                        "开始执行持仓强平");
                    if (DateTime.Now >= closeEndTime)
                    {
                        _Log.LogInfo($"[强平] 当前已超过强平窗口结束时间[{closeEndTime:yyyy-MM-dd HH:mm:ss}]，不再发送强平订单。");
                        return;
                    }
                    await ClosePositionsByDeadlineAsync();
                }
                else
                {
                    _Log.LogInfo(
                        "[强平] AutoClosePosition=FALSE，" +
                        "跳过持仓强平");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "[强制平仓] 本轮强平执行异常");
            }
            finally
            {
                stopwatch.Stop();

                _Log.LogInfo(
                    $"[性能监控] 本轮强平轮询执行耗时: " +
                    $"{stopwatch.ElapsedMilliseconds} ms" +
                    $"（下一轮由定时器约 5 秒后自动触发）");
            }
        }

        /// <summary>
        /// 强平前置步骤：删除服务器挂单，并在删除成功后同步清理本地数据库开仓记录与内存缓存的 TradeRecordId。
        /// 若服务器删除失败，跳过本地清理，避免与服务器状态不一致。
        /// </summary>
        private async Task DeletePendingOrdersAndCleanLocalStateAsync()
        {
            _Log.LogInfo($"删除挂单");
            // 1. 强平前先删除所有挂单
            JObject deleteAll = HTTPHelper.deleteAll(_Config.EAServerAddress, _EAAccount.UserCode, _Config.CurrentRaceDate, _Config.CurrentRaceType, _Config.CurrentRaceNo, out string serverProcessTime);
            _Log.LogInfo($"[性能监控][QueryEATBETInfoDataForClosePositionAsync][deleteAll] 耗时: {serverProcessTime}");

            if (deleteAll != null && deleteAll.ContainsKey("success") && (bool)deleteAll["success"])
            {
                _Log.LogInfo($"删除挂单:[{deleteAll["message"]}]");

                // 2. 清理本地数据库中的开仓记录标记
                DBHelper.deleteOpenBetRecord(_EAAccount.UserCode, _Config.CurrentRaceDate, _Config.CurrentRaceType, _Config.CurrentRaceNo);

                // 🔧 deleteOpenBetRecord 删的是"bet_executed_amount=0 且 total_bet_amount=bet_pending_amount"这批记录，
                // 跟内存里 ComboTradeState 完全没成交、纯挂单的那批是同一批。数据库行被删掉了，
                // 但 state.TradeRecordId 这个缓存不会自动失效，必须手动同步清零，
                // 否则8秒后 RefreshTradeListToDB 会拿着这个已经不存在的ID去写 hr_trade_detail，
                // 触发外键约束异常（trade_record 这个FK）。
                foreach (var s in _Config.TradeStateStore.GetAll()
                    .Where(s => string.Equals(s.RaceNo, _Config.CurrentRaceNo)
                             && s.BetExecuted <= AmountEpsilon
                             && Math.Abs(s.BetPending - (s.BetExecuted + s.BetPending)) <= AmountEpsilon))
                {
                    s.ResetTradeRecordId();
                }

                // 🔥 BUG 修复（强平"没有相关日志"的根因）：ComboTradeState 内部的
                // _betCommittedHighWatermark / _eatCommittedHighWatermark 是"已执行+挂单中"金额的历史
                // 最大值，只会 Math.Max 往上顶、永远不会自动回落。deleteAll 在这里已经把本场次所有
                // 挂单（包括尚未成交的试探性吃票/下注挂单）真正删除了，这些挂单对应的历史峰值却依然
                // 永久卡在水位线里，导致 EffectiveEatCommitted/EffectiveBetCommitted 被虚高的历史挂单
                // 峰值撑住，TradeDecisionEngine.GetRemainingEatAmount 从此永远算出 0——即使 BetExecuted
                // 和 EatExecuted 之间明明还有真实缺口，该组合也再不会被 ClosePositionsByDeadlineAsync 的
                // 外层 allStates 过滤判定为"需要强平"，且因为在最外层就被滤掉，后面完全不会留下任何日志。
                // deleteAll 成功返回，就是"挂单已被服务器真正删除"这个明确时间点，此时把水位线显式下调回
                // 当前真正已执行（Executed）的金额是绝对安全的——Executed 本身另有单调不减保护，不会被
                // 这个操作误吞任何已成交金额，只会清除掉"已被取消、从未成交"的那部分虚高历史水位。
                foreach (var s in _Config.TradeStateStore.GetAll()
                    .Where(s => string.Equals(s.RaceNo, _Config.CurrentRaceNo)))
                {
                    s.ResetCommittedHighWatermarkAfterCancel();
                }

                // 🔧 删除挂单是一次网络请求，服务器端从"接收删除指令"到"数据真正落库、
                // 后续 queryMyTrade 能查到最新状态"之间存在处理延迟。如果删除后立即查询，
                // 很可能拿到的还是删除前的旧快照（挂单看起来还在），导致误判还有 pending 而跳过强平。
                // 这里随机等待 1~3 秒，给服务器留出数据落地的时间，再进行后续查询。
                int delayMs = new Random().Next(3000, 5001);
                _Log.LogInfo($"删除挂单已提交，等待 {delayMs} ms 让服务器数据落地后再查询最新仓位");
                await Task.Delay(delayMs);
            }
            else
            {
                _logger.Error($"[强制平仓] 删除挂单失败或无响应，跳过本地记录清理，避免与服务器状态不一致。响应: {deleteAll?.ToString() ?? "null"}");
            }
        }

        /// <summary>
        /// 强平核心逻辑（单轮，不循环）：
        /// 1. 重新从服务器同步一次仓位快照（删除挂单 + 等待落地后，旧快照已失效）；
        /// 2. 找出满足强平下单原则的组合——赌注已确认（无赌注挂单）、吃注当前没有挂单，
        ///    且 TradeDecisionEngine.GetRemainingEatAmount 判定确实还有缺口需要补吃；
        /// 3. 按 Q / QP 两种类型分别构建强平请求并提交。
        /// 本轮只挂一次单，若仍未成交，等待定时器下一次触发时会自然再走一遍同样的流程。
        /// </summary>
        private async Task ClosePositionsByDeadlineAsync()
        {
            string serverProcessTime = "0ms";
            if (!TryGetTradeTimeContextForClose(DateTime.Now, out DateTime raceDateTime, out DateTime normalBetEndTime, out DateTime closeStartTime, out DateTime closeEndTime) ||
                DateTime.Now < closeStartTime || DateTime.Now >= closeEndTime)
            {
                _Log.LogInfo($"[强平] 当前不在强平窗口，停止发送强平订单。Window=[{closeStartTime:yyyy-MM-dd HH:mm:ss} ~ {closeEndTime:yyyy-MM-dd HH:mm:ss})");
                return;
            }

            // 重新从服务器同步一次仓位快照：
            // deleteAll/deleteOpenBetRecord 只删除了服务端/本地数据库记录，
            // TradeStateStore 里缓存的 BetExecuted/EffectiveEatCommitted 仍是删除前的旧值，
            // 若不重新拉取，下面基于 allStates 判断"哪些组合需要重新挂吃注单"就会用到过期数据。
            await QueryAndApplyMyTradeSnapshotAsync();
            if (DateTime.Now >= closeEndTime)
            {
                _Log.LogInfo($"[强平] 仓位快照刷新完成后已超过强平窗口结束时间[{closeEndTime:yyyy-MM-dd HH:mm:ss}]，停止本轮强平。");
                return;
            }

            // 🔥 强平下单原则（粗筛）：赌注已确认（BetExecuted > 0 且赌注无挂单）、
            // 吃注当前没有挂单（EffectiveEatPending == 0），
            // 且直接复用 TradeDecisionEngine.GetRemainingEatAmount 判断是否确实还有缺口需要补吃。
            // 🔧 之前的版本自己手写了 "EatExecuted < BetExecuted" 这类判断，没有把"已预占但服务器
            // 尚未确认"的部分算进去，和 AutoEatProcess 用的判断口径不一致，存在并发窗口期超发的风险。
            // 现在统一改成调用同一个引擎方法，确保正常吃注和强平吃注的判断标准完全一致。
            //
            // 🔧 诊断增强：原来这里是一整条 LINQ .Where(...)，任何组合被过滤掉都不会留下任何日志，
            // 出现"某个组合应该强平却完全找不到相关日志"时完全没法定位是卡在哪一个子条件上。
            // 现在展开成显式循环，对当前场次下的每一个组合，把四个子条件的实际数值和判断结果
            // 都打一行 Debug 日志，即使最终被过滤掉也能看到具体原因。
            var candidateStatesForClose = _Config.TradeStateStore.GetAll()
                .Where(s => string.Equals(s.RaceNo, _Config.CurrentRaceNo))
                .ToList();
            _Log.LogInfo($"[强平诊断] 当前场次[{_Config.CurrentRaceNo}]TradeStateStore中共有[{candidateStatesForClose.Count}]个组合，开始逐一评估强平入选条件。");
            var allStates = new List<ComboTradeState>();
            foreach (var s in candidateStatesForClose)
            {
                bool condBetExecuted = s.BetExecuted > AmountEpsilon;
                bool condBetPending = s.EffectiveBetPending <= AmountEpsilon;
                bool condEatPending = s.EffectiveEatPending <= AmountEpsilon;
                double remainingEatAmountForDiag = 0;
                bool condRemaining = false;
                try
                {
                    remainingEatAmountForDiag = _decisionEngine.GetRemainingEatAmount(s);
                    condRemaining = remainingEatAmountForDiag > AmountEpsilon;
                }
                catch (Exception ex)
                {
                    _logger.Error($"[强平诊断][{s.DictKey}] GetRemainingEatAmount 计算异常: {ex.Message}", ex);
                }
                bool passedCloseFilter = condBetExecuted && condBetPending && condEatPending && condRemaining;
                // 🔧 诊断增强：GetRemainingEatAmount 内部用的是 state.EffectiveEatCommitted（可能叠加了
                // 尚未被服务器确认/尚未释放的预占金额），不是这里能直接看到的服务器确认口径 EatExecuted。
                // 把两者都打出来，一旦出现 EffectiveEatCommitted 明显高于 EatExecuted 且长期不回落，
                // 就是"预占卡死导致 remaining 永远算成 0"的直接证据。
                _logger.Debug(
                    $"[强平诊断][{s.DictKey}] " +
                    $"BetExecuted={s.BetExecuted:F3}(>{AmountEpsilon}⇒{condBetExecuted}) | " +
                    $"EffectiveBetPending={s.EffectiveBetPending:F3}(<={AmountEpsilon}⇒{condBetPending}) | " +
                    $"EffectiveEatPending={s.EffectiveEatPending:F3}(<={AmountEpsilon}⇒{condEatPending}) | " +
                    $"EatExecuted={s.EatExecuted:F3}(服务器确认口径) | " +
                    $"EffectiveEatCommitted={s.EffectiveEatCommitted:F3}(引擎实际用于计算缺口的口径) | " +
                    $"RemainingEatAmount={remainingEatAmountForDiag:F3}(>{AmountEpsilon}⇒{condRemaining}) " +
                    $"=> {(passedCloseFilter ? "【入选强平候选】" : "【被过滤，不进入强平】")}");
                if (passedCloseFilter)
                {
                    allStates.Add(s);
                }
            }
            _Log.LogInfo($"[强平诊断] 共[{allStates.Count}]/[{candidateStatesForClose.Count}]个组合通过强平入选条件（Q+QP合计）。");

            var Q_BettingInfoDict = BuildBettingInfoDict(allStates, "Q");
            var QP_BettingInfoDict = BuildBettingInfoDict(allStates, "QP");

            if (Q_BettingInfoDict.Count == 0 && QP_BettingInfoDict.Count == 0)
            {
                _Log.LogInfo("当前无需要重新挂吃注单的组合，本轮强平无需下单");
                return;
            }

            IDictionary<string, List<BetInfo>> BetBatInfos = HTTPHelper.queryMarket(
                _Config.EAServerAddress, _EAAccount.UserCode, _Config.CurrentRaceDate, _Config.CurrentRaceType, _Config.CurrentRaceNo, out serverProcessTime);
            _Log.LogInfo($"[性能监控][QueryEATBETInfoDataForClosePositionAsync][queryMarket] 耗时: {serverProcessTime}");

            if (BetBatInfos == null || BetBatInfos.Count == 0)
            {
                _Log.LogInfo($"无下注数据,不能处理强平赌注");
                return;
            }
            if (DateTime.Now >= closeEndTime)
            {
                _Log.LogInfo($"[强平] queryMarket 完成后已超过强平窗口结束时间[{closeEndTime:yyyy-MM-dd HH:mm:ss}]，停止本轮强平。");
                return;
            }

            // 🔧 V20260913_CLOSE_MATCH_SIDE_FIX：强平要挂的是"吃"单，吃单必须匹配市场上别人挂出来的
            // "赌"(BET)单才能成交——ClosePositionByDeadline 内部会把这里传入列表里、对应 combo 的那笔
            // 记录的 odds/limit 当成"对手盘报价"来定价、下单。之前这里传的是 Q_EATBetInfos/QP_EATBetInfos
            // （market 上别人挂的"吃"单），那是跟我方同边、根本不是可撮合的对手盘，绝大多数组合天然就
            // 匹配不到（诊断日志里反复出现的"在本轮 EATBetInfos[N个不同combo]中找不到精确匹配"根因即在此），
            // 而不是 combo 本身没有报价。现改为传入 Q_BETBetInfos/QP_BETBetInfos（market 上别人挂的"赌"单），
            // 这才是我方"吃"单真正需要撮合的对手盘。
            if (Q_BettingInfoDict.Count > 0)
            {
                _Log.LogInfo($"[Q]强制平赌注");
                List<BetInfo> Q_BETBetInfos = BetBatInfos.ContainsKey("Q_BETBetInfos") ? BetBatInfos["Q_BETBetInfos"] : new List<BetInfo>();
                ClosePositionByDeadline(Q_BettingInfoDict, Q_BETBetInfos, "Q");
            }

            if (QP_BettingInfoDict.Count > 0)
            {
                _Log.LogInfo($"[QP]强制平赌注");
                List<BetInfo> QP_BETBetInfos = BetBatInfos.ContainsKey("QP_BETBetInfos") ? BetBatInfos["QP_BETBetInfos"] : new List<BetInfo>();
                ClosePositionByDeadline(QP_BettingInfoDict, QP_BETBetInfos, "QP");
            }
        }

        /// <summary>
        /// 按指定类型（Q / QP）从仓位状态列表中构建请求所需的字典结构。
        /// </summary>
        private Dictionary<string, IDictionary<string, string>> BuildBettingInfoDict(List<ComboTradeState> allStates, string type)
        {
            var typedStates = allStates.Where(s => string.Equals(s.Type, type)).ToList();

            // 🔧 诊断+防御：原来直接 .ToDictionary(s => s.Combo, ...)，如果同一 type 下出现两条
            // Combo 字符串完全相同的 ComboTradeState（理论上不该发生，但一旦发生 ToDictionary 会
            // 直接抛 ArgumentException），异常发生在 Q_BettingInfoDict/QP_BettingInfoDict 构建阶段，
            // 会导致本轮强平在外层 try/catch 只留下一行"[强制平仓] 本轮强平执行异常"、看不出是哪个
            // combo 重复，且 Q 和 QP 两种类型的强平会被这一个异常一起拖累、全部静默失败。
            // 这里先显式检测重复项并单独报错，再用 GroupBy+First 兜底去重，避免整轮崩溃。
            var dupGroups = typedStates.GroupBy(s => s.Combo).Where(g => g.Count() > 1).ToList();
            foreach (var g in dupGroups)
            {
                _logger.Error(
                    $"[强平诊断][{type}] 发现重复 Combo='{g.Key}'，共[{g.Count()}]条 ComboTradeState " +
                    $"(DictKey: {string.Join(", ", g.Select(s => s.DictKey))})，" +
                    $"原逻辑 ToDictionary 会因此抛异常导致本轮[{type}]强平整体静默失败，已自动去重（保留第一条）。");
            }

            var result = typedStates
                .GroupBy(s => s.Combo)
                .ToDictionary(g => g.Key, g => (IDictionary<string, string>)new Dictionary<string, string>
                {
                    { "combo", g.First().Combo }, { "type", g.First().Type },
                    { "bet_odds", g.First().BetOdds.ToString() },
                    { "bet_pending_amount", g.First().EffectiveBetPending.ToString() },
                    { "eat_pending_amount", g.First().EffectiveEatPending.ToString() }
                });

            // 🔧 诊断：明确打印本轮到底哪些 combo 进了 Q/QP 的强平候选字典——这样即使后面
            // ClosePositionByDeadline 内部因为并发/截断等原因看不全，也能从这一行确认某个
            // 组合（例如 1-7）究竟有没有进入候选集合。
            _Log.LogInfo($"[强平诊断][{type}] 本轮入选强平候选组合[{result.Count}]个: [{string.Join(", ", result.Keys)}]");
            return result;
        }

        /// <summary>
        /// 退出程序时，一键关闭所有后台循环
        /// </summary>
        private void EAForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _timerRefreshBalance.Stop();
            _timerRefreshMyTrade.Stop();
            _timerRefreshBetInfo.Stop();
            _timerRefreshBetInfoForClosePosition.Stop();
            timerRefreshAutoBettingInfo.Stop();
            _Config.TradeStateStore?.Stop();
            // 🔧 停止接收新的落库 job，并给已入队但还没写完的 job 最多 5 秒排干时间，
            // 尽量避免进程退出时丢失最后几笔还没来得及落库的交易结果。
            _tradeRecordWriter?.Stop(TimeSpan.FromSeconds(5));
            if (string.Equals(buttonAccountLogin.Text, "已登陆"))
            {
                HTTPHelper.logout(_Config.EAServerAddress, _EAAccount.UserCode);
            }
            if (string.Equals(buttonDSLogin.Text, "已登陆"))
            {
                if (!string.Equals(_Config.DSServerAddress, _Config.EAServerAddress))
                {
                    HTTPHelper.logout(_Config.DSServerAddress, _DSAccount.UserCode);
                }
            }
        }
        /// <summary>
        /// 配置文件默认目录
        /// </summary>
        public string rootPath = "C:\\HR\\iAutoTrade";
        /// <summary>
        /// 系统配置文件
        /// </summary>
        public string _ConfigFile = "\\Config.json";
        /// <summary>
        /// 操作员配置文件
        /// </summary>
        public string _MyConfigFile = "\\MyConfig.json";
        /// <summary>
        /// 自动交易策略文件
        /// </summary>
        public string _StrategyFile = "\\Strategy.json";
        /// <summary>
        /// 系统日志
        /// </summary>
        public NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 全局配置参数
        /// </summary>
        public Config _Config;
        /// <summary>
        /// 个性化配置参数
        /// </summary>
        public MyConfig _MyConfig;
        /// <summary>
        /// 业务日志
        /// </summary>
        public Log _Log;
        /// <summary>
        /// 当前交易账户信息
        /// </summary>
        public Account _EAAccount;
        /// <summary>
        /// 读水账户
        /// </summary>
        public Account _DSAccount;
        /// <summary>
        /// 自动交易是否启动，自动交易和是否自动选中组合使用
        /// </summary>
        public bool _EnableAutoTrade = false;
        /// <summary>
        /// 自动扫描是否启动
        /// </summary>
        public bool _EnableBettingInfoRefresh = false;
        /// <summary>
        /// 该订单是否为自动交易订单
        /// </summary>
        public bool _IsAutoOperationFlag = false;
        /// <summary>
        /// 账户显示面板
        /// </summary>
        public AccountDisplay _AccountDisplay;
        // 在 Form 级别只初始化一次
        /// <summary>
        /// 自动下注数据列表
        /// </summary>
        private BindingList<EatBetInfo> _BettingList = new BindingList<EatBetInfo>();
        /// <summary>
        /// 我的交易列表
        /// </summary>
        private BindingList<BetInfo> _MyTradeList = new BindingList<BetInfo>();
        /// <summary>
        /// 下注过程中的异常,如果该组合异常已存在，就过滤掉
        /// </summary>
        private IDictionary<string, string> _ErrorList = new Dictionary<string, string>();
        //显示下注明细
        private TradeDetailForm _BettingTradeDetailForm;
        private SettledHistoryForm _SettledHistoryForm;
        private AutoBettingStatusForm _AutoBettingStatusForm;
        /***
         公共方法
         AsynExec 点击按钮后异步响应
         RenderUI 界面渲染
         AsynRenderUI 如果你的后台线程在更新一个UI控件的状态后不需要等待，而是要继续往下处理，那么你就应该使用Invoke来进行异步处理。
         StartDelayTask 开始一个延时任务
         */
        /// <summary>
        /// 点击按钮后异步响应
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected Task AsynExec(Control control, Action action)
        {
            control.Enabled = false;
            return Task.Run(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    control.Invoke(new Action(delegate ()
                    {
                        MessageBox.Show(ex.ToString());
                    }));
                }
                finally
                {
                    control.Invoke(new Action(delegate ()
                    {
                        control.Enabled = true;
                    }));
                }
            });
        }
        /// <summary>
        /// 界面渲染
        /// control.invoke(参数delegate)方法:在拥有此控件的基础窗口句柄的线程上执行指定的委托。
        /// control.begininvoke(参数delegate)方法:在创建控件的基础句柄所在线程上异步执行指定委托。
        /// 如果你的后台线程在更新一个UI控件的状态后不需要等待，而是要继续往下处理，那么你就应该使用Invoke来进行异步处理。
        /// 如果你的后台线程需要操作UI控件，并且需要等到该操作执行完毕才能继续执行，那么你就应该使用Invoke。
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        protected void RenderUI(Control control, Action action)
        {
            if (control.IsHandleCreated)
            {
                control.Invoke(new Action(delegate ()
                {
                    action();
                }));
            }
        }
        /// <summary>
        /// 如果你的后台线程在更新一个UI控件的状态后不需要等待，而是要继续往下处理，那么你就应该使用Invoke来进行异步处理。
        /// </summary>
        /// <param name="control"></param>
        /// <param name="action"></param>
        protected void AsynRenderUI(Control control, Action action)
        {
            if (control.IsHandleCreated)
            {
                control.Invoke(new Action(delegate ()
                {
                    action();
                }));
            }
        }
        /// <summary>
        /// 开始一个延时任务
        /// </summary>
        /// <param name="DelayTime">延时时长（秒）</param>
        /// <param name="taskEndAction">延时时间完毕之后执行的委托（会跳转回UI线程）</param>
        /// <param name="control">UI线程的控件</param>
        public void StartDelayTask(int DelayTime, Action taskEndAction, Control control)
        {
            if (control == null)
            {
                return;
            }
            Task task = new Task(() =>
            {
                try
                {
                    Thread.Sleep(DelayTime * 1000);
                    //返回UI线程
                    control.Invoke(new Action(() =>
                    {
                        taskEndAction();
                    }));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            });
            task.Start();
        }
        public EAForm()
        {
            InitializeComponent();
            InitObject();
        }
        /// <summary>
        /// 实例化对象，该方法只能调用一次
        /// </summary>
        public void InitObject()
        {
            //Log 实例只初始化一次
            _Log = new Log(listBoxOrderLogList, listBoxSystemLogList);
            _Log._TradeWinName = "长城";
            _AccountDisplay = new AccountDisplay
            {
                labelAccountProfitAndLoss = labelAccountProfitAndLoss,
                labelAccountCredit = labelAccountCredit,
                labelDSProfitAndLoss = labelDSProfitAndLoss,
                labelDSCredit = labelDSCredit
            };
            InitializeTimers();

            // 🔧 统一交易落库服务：ExecuteTrade 的实时提交结果、RefreshTradeListToDB 的周期性补写，
            // 全部改为经这里的 Enqueue 入队，由固定数量的 worker 按 dictKey（raceNo_type_combo）
            // 哈希分片、单线程严格按入队顺序（FIFO）串行处理，不同 combo 之间在不同 worker 上并行。
            // 这样彻底合并了原来两条互不知情、可能互相竞态的写入通道（详见 TradeRecordWriter.cs 顶部注释）。
            //
            // 注意：这里用到的 _Config/_EAAccount 此时还没有被 LoadConfigFile()/EAForm_Load 赋值，
            // 但 eaAccountProvider/resolveState 全部是 lambda，只有真正处理 job 时才会读取，
            // 构造阶段（这里）不会触碰这两个字段，所以在 LoadConfigFile 之前构造是安全的。
            _tradeRecordWriter = new TradeRecordWriter(
                workerCount: 4,
                bizLog: _Log,
                eaAccountProvider: () => _EAAccount,
                resolveState: dictKey =>
                {
                    if (_Config?.TradeStateStore == null) return null;
                    var parts = dictKey.Split(new[] { '_' }, 3);
                    return parts.Length == 3 ? _Config.TradeStateStore.GetOrCreate(parts[0], parts[1], parts[2]) : null;
                },
                logInfo: msg => _Log?.LogInfo(msg),
                logError: msg => _logger.Error(msg));
        }
        /// <summary>
        /// 初始化UI参数
        /// </summary>
        public void IniUI()
        {
            _logger.Debug("开始初始化UI");
            //初始平台设置
            //打水账号密码
            textBoxAccountCode.Text = _EAAccount.UserCode;
            textBoxAccountPassword.Text = _EAAccount.Password;
            textBoxAccountPin.Text = _EAAccount.Pin;
            //读水账号密码
            textBoxDSAccountCode.Text = _DSAccount.UserCode;
            textBoxDSAccountPassword.Text = _DSAccount.Password;
            textBoxDSAccountPin.Text = _DSAccount.Pin;
            //自动交易参数
            numericUpDownAutoTradeStartTimeDuration.Value = _Config.AutoTradeStartTimeDuration;
            numericUpDownAutoTradeEndTimeDuration.Value = _Config.AutoTradeEndTimeDuration;
            numericUpDownAutoTradeInterval.Value = _Config.AutoTradeInterval;
            checkBoxAutoToNext.Checked = _Config.AutoToNext;
            checkBoxAutoDeletePendingOrder.Checked = _Config.AutoDeletePendingOrder;
            checkBoxAutoClosePosition.Checked = _Config.AutoClosePosition;
            checkBoxAutoRefreshBetting.Checked = _Config.AutoRefreshBetting;
            checkBoxQAutoBettingFlag.Checked = _Config.QAutoBettingFlag;
            numericUpDownQStakeAmount.Value = _Config.QStakeAmount;
            numericUpDownQStartOdds.Value = _Config.QStartOdds;
            numericUpDownQEndOdds.Value = _Config.QEndOdds;
            numericUpDownQMinLimit.Value = _Config.QMinLimit;
            numericUpDownQEatSpread.Value = _Config.QEatSpread;
            numericUpDownQBetSpread.Value = _Config.QBetSpread;
            numericUpDownQPBetSpread.Value = _Config.QPBetSpread;
            numericUpDownQPEatSpread.Value = _Config.QPEatSpread;
            checkBoxQPAutoBettingFlag.Checked = _Config.QPAutoBettingFlag;
            numericUpDownQPStakeAmount.Value = _Config.QPStakeAmount;
            numericUpDownQPStartOdds.Value = _Config.QPStartOdds;
            numericUpDownQPEndOdds.Value = _Config.QPEndOdds;
            numericUpDownQPMinLimit.Value = _Config.QPMinLimit;
            textBoxQCombos.Text = _Config.QCombos;
            textBoxQPCombos.Text = _Config.QPCombos;
            numericUpDownQLowerThreshold.Value = _Config.QLowerThreshold;
            numericUpDownQPLowerThreshold.Value = _Config.QPLowerThreshold;
            string[] raceTypes = _Config.SysConfig["Horse.RaceType"].Split(",");
            foreach (string raceType in raceTypes)
            {
                comboBoxRaceType.Items.Add(raceType);
            }
            comboBoxRaceType.SelectedItem = _Config.CurrentRaceTypeDesc;
            comboBoxRaceNo.SelectedItem = _Config.CurrentRaceNo;
            textBoxCurrentRaceTime.Text = _Config.CurrentRaceTime;
            // 方式 A：通过索引设置（tabPage3 是第 3 个，所以索引是 2）
            tabControl1.SelectedIndex = 2;
            _Config.TradeStateStore.Start(_EAAccount, _Config,
                logInfo: msg => _Log.LogInfo(msg),
                logError: msg => _logger.Error(msg));
            //Timer 初始化
            _timerRefreshMyTrade.Start();
            timerRefreshAutoBettingInfo.Interval = 3000;
            timerRefreshAutoBettingInfo.Enabled = true;
            timerRefreshAutoBettingInfo.Start();
            //初始化订单列表
            //停止按钮颜色改变事件
        }
        /// <summary>
        /// 加载配置文件
        /// </summary>
        public void LoadConfigFile()
        {
            string newMyConfigFile = rootPath + _MyConfigFile;
            string newConfigFile = rootPath + _ConfigFile;
            _logger.Debug("加载初始化文件开始");
            if (File.Exists(newMyConfigFile))
            {
                _MyConfig = JsonConvert.DeserializeObject<MyConfig>(File.ReadAllText(newMyConfigFile));
                if (_MyConfig != null)
                {
                    if (_MyConfig.TradeAccount == null)
                    {
                        _MyConfig.TradeAccount = new Account();
                    }
                    _EAAccount = _MyConfig.TradeAccount;
                    _EAAccount.IsLogin = false; // 初始化时默认未登录
                    _EAAccount.BrokerName = "长城";
                    _EAAccount.BrokerCode = "CC";
                    if (_MyConfig.DSAccount == null)
                    {
                        _MyConfig.DSAccount = new Account();
                    }
                    _DSAccount = _MyConfig.DSAccount;
                    _DSAccount.IsLogin = false; // 初始化时默认未登录
                    _DSAccount.BrokerName = "长城";
                    _DSAccount.BrokerCode = "CC";
                }
            }
            else
            {
                _logger.Fatal("没有配置文件，系统初始化异常");
            }
            if (File.Exists(newConfigFile))
            {
                _Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(newConfigFile));
            }
            else
            {
                _logger.Fatal("没有初始化配置文件，系统初始化异常");
            }
            _logger.Debug("加载初始化文件完成");
        }
        /// <summary>
        /// 加载配置参数
        /// </summary>
        public virtual void LoadConfig()
        {
            _logger.Debug("开始加载配置参数开始");
            if (_Config != null)
            {
                //获取数据库配置数据
                _Config.SysConfig = DBHelper.getSysConfig();
            }
            else
            {
                _logger.Fatal("没有初始化配置文件，系统初始化异常");
            }
            //加载Account信息后初始化到Log对象
            _Log._Account = _EAAccount;
            _logger.Debug("开始加载配置参数完成");
        }
        /// <summary>
        /// 初始化平台对象
        /// </summary>
        public void GeneratePlatformObj()
        {
            _logger.Debug("TradeForm初始化平台对象开始");
            _logger.Debug("TradeForm初始化平台对象完成");
        }
        public void InitialPara()
        {
            _logger.Debug("初始化参数开始");
            // 获取当前系统时间
            DateTime now = DateTime.Now;
            // 赛马日逻辑处理：
            // 如果当前时间是深夜/凌晨（例如 00:00 到 06:00 之间），
            // 并且系统正在跑的是前一天夜场的最后几场比赛（跨天赛事），
            // 那么它的赛马日期（RaceDate）应该属于“昨天”，而不是今天。
            DateTime targetRaceDate = now;
            if (now.Hour < 6)
            {
                targetRaceDate = now.AddDays(-1);
            }
            // 格式化为你的接口要求的 "dd-MM-yyyy" 格式
            _Config.CurrentRaceDate = targetRaceDate.ToString("dd-MM-yyyy");
            _logger.Debug($"初始化赛马日期完成: _Config.CurrentRaceDate = {_Config.CurrentRaceDate}");
            // 1. 必须在指定 DataSource 之前，先关闭自动生成列
            dataGridViewEATBetInfoList.AutoGenerateColumns = false;
            dataGridViewMyTradeList.AutoGenerateColumns = false;
            // 禁止用户在表格最下方戳出新的空行
            dataGridViewEATBetInfoList.AllowUserToAddRows = false;
            dataGridViewEATBetInfoList.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewMyTradeList.AllowUserToAddRows = false;
            // 在 Form_Load 或初始化方法中添加：
            dataGridViewMyTradeList.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            // 2. 配置连赢表：整行选中、开启整表编辑权限
            dataGridViewEATBetInfoList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewEATBetInfoList.ReadOnly = false;
            // 4. 我的交易表：【修改这里】允许编辑（以便允许勾选 CheckBox），但后面会将文本列锁死
            dataGridViewMyTradeList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewMyTradeList.ReadOnly = false; // 改为 false
            // 5. 进行数据源绑定
            dataGridViewEATBetInfoList.DataSource = _BettingList;
            dataGridViewMyTradeList.DataSource = _MyTradeList;
            // 6. 绑定列的只读状态（分别限制两张表：第一列可动，其余锁死）
            UIUtils.ConfigureColumnsReadOnly(dataGridViewEATBetInfoList);
            UIUtils.ConfigureColumnsReadOnly(dataGridViewMyTradeList); // 【新增】让交易表也应用此风控锁
            // 7. 【修改这里】统一改用 CellClick 事件，实现点击整行任意触碰即勾选
            dataGridViewEATBetInfoList.CellClick += UIUtils.DataGridView_CellClick;
            dataGridViewMyTradeList.CellClick += UIUtils.DataGridView_CellClick; // 【新增】给交易表也绑定该逻辑
            dataGridViewMyTradeList.DataBindingComplete += DataGridViewMyTradeList_DataBindingComplete;
            //初始化扫描进度控件
            InitProcessingAnimation();
            _BettingTradeDetailForm = new TradeDetailForm();
            // 设置父窗口（可选）
            _BettingTradeDetailForm.Owner = this;
            // 先隐藏
            _BettingTradeDetailForm.Hide();
            _SettledHistoryForm = new SettledHistoryForm();
            // 设置父窗口（可选）
            _SettledHistoryForm.Owner = this;
            // 先隐藏
            _SettledHistoryForm.Hide();
            _AutoBettingStatusForm = new AutoBettingStatusForm();
            // 设置父窗口（可选）
            _AutoBettingStatusForm.Owner = this;
            // 先隐藏
            _AutoBettingStatusForm.Hide();
            _logger.Debug("初始化参数完成");
        }
        /// <summary>
        /// 当表格彻底完成数据绑定、生成行、并重绘完毕后，再精准补上 CheckBox 的勾选
        /// </summary>
        private void DataGridViewMyTradeList_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // 如果没有记忆的 Key，直接返回不处理
            if (string.IsNullOrEmpty(_savedCheckedCombinedKey)) return;
            int currentMatchCount = 0;
            foreach (DataGridViewRow row in dataGridViewMyTradeList.Rows)
            {
                var newItem = row.DataBoundItem as BetInfo;
                if (newItem != null)
                {
                    string newCombinedKey = $"{newItem.type}_{newItem.raceNo}_{newItem.combo}_{newItem.action}";
                    if (newCombinedKey == _savedCheckedCombinedKey)
                    {
                        // 只有当重复项的序号也完全对上时，才执行勾选
                        if (currentMatchCount == _savedSameKeyMatchIndex)
                        {
                            // 此时界面已稳定，直接给 Value 赋值即可稳稳勾选成功！
                            row.Cells[0].Value = true;
                            row.Selected = true;       // 恢复高亮
                            // 消费掉这个记忆，防止下次非刷新的绑定时误触发
                            _savedCheckedCombinedKey = string.Empty;
                            break;
                        }
                        currentMatchCount++;
                    }
                }
            }
        }
        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void SaveConfig()
        {
            _logger.Debug("保存配置文件开始");
            string newMyConfigFile = rootPath + _MyConfigFile;
            string newConfigFile = rootPath + _ConfigFile;
            try
            {
                if (_EAAccount == null)
                {
                    _EAAccount = new Account
                    {
                        UserCode = textBoxAccountCode.Text,
                        Password = textBoxAccountPassword.Text,
                        Pin = textBoxAccountPin.Text
                    };
                    _MyConfig.TradeAccount = _EAAccount;
                }
                else
                {
                    _EAAccount.UserCode = textBoxAccountCode.Text;
                    _EAAccount.Password = textBoxAccountPassword.Text;
                    _EAAccount.Pin = textBoxAccountPin.Text;
                }
                if (_DSAccount == null)
                {
                    _DSAccount = new Account
                    {
                        UserCode = textBoxDSAccountCode.Text,
                        Password = textBoxDSAccountPassword.Text,
                        Pin = textBoxDSAccountPin.Text
                    };
                    _MyConfig.DSAccount = _DSAccount;
                }
                else
                {
                    _DSAccount.UserCode = textBoxDSAccountCode.Text;
                    _DSAccount.Password = textBoxDSAccountPassword.Text;
                    _DSAccount.Pin = textBoxDSAccountPin.Text;
                }
                if (_MyConfig != null)
                {
                }
                if (_Config != null)
                {
                    //交易信息设置
                    _Config.AutoTradeStartTimeDuration = (int)numericUpDownAutoTradeStartTimeDuration.Value;
                    _Config.AutoTradeEndTimeDuration = (int)numericUpDownAutoTradeEndTimeDuration.Value;
                    _Config.AutoTradeInterval = (int)numericUpDownAutoTradeInterval.Value;
                    _Config.AutoToNext = checkBoxAutoToNext.Checked;
                    _Config.AutoDeletePendingOrder = checkBoxAutoDeletePendingOrder.Checked;
                    _Config.AutoClosePosition = checkBoxAutoClosePosition.Checked;
                    _Config.AutoRefreshBetting = checkBoxAutoRefreshBetting.Checked;
                    _Config.QAutoBettingFlag = checkBoxQAutoBettingFlag.Checked;
                    _Config.QStakeAmount = (int)numericUpDownQStakeAmount.Value;
                    _Config.QStartOdds = (int)numericUpDownQStartOdds.Value;
                    _Config.QEndOdds = (int)numericUpDownQEndOdds.Value;
                    _Config.QMinLimit = (int)numericUpDownQMinLimit.Value;
                    _Config.QEatSpread = (int)numericUpDownQEatSpread.Value;
                    _Config.QBetSpread = (int)numericUpDownQBetSpread.Value;
                    _Config.QPBetSpread = (int)numericUpDownQPBetSpread.Value;
                    _Config.QPAutoBettingFlag = checkBoxQPAutoBettingFlag.Checked;
                    _Config.QPStakeAmount = (int)numericUpDownQPStakeAmount.Value;
                    _Config.QPStartOdds = (int)numericUpDownQPStartOdds.Value;
                    _Config.QPEndOdds = (int)numericUpDownQPEndOdds.Value;
                    _Config.QPMinLimit = (int)numericUpDownQPMinLimit.Value;
                    _Config.QPEatSpread = (int)numericUpDownQPEatSpread.Value;
                    _Config.QCombos = textBoxQCombos.Text;
                    _Config.QPCombos = textBoxQPCombos.Text;
                    _Config.QLowerThreshold = (int)numericUpDownQLowerThreshold.Value;
                    _Config.QPLowerThreshold = (int)numericUpDownQPLowerThreshold.Value;
                    _Config.CurrentRaceNo = (string)comboBoxRaceNo.SelectedItem;
                    _Config.CurrentRaceTypeDesc = (string)comboBoxRaceType.SelectedItem;
                    _Config.CurrentRaceType = string.IsNullOrEmpty(_Config.CurrentRaceTypeDesc) ? "" : _Config.CurrentRaceTypeDesc.Split("|")[0];
                    _Config.CurrentRaceTime = textBoxCurrentRaceTime.Text;
                    //交易商平台设置
                    //保存该配置信息后需要调用InitEventConfig 把参数解析到变量中
                    File.WriteAllText(newConfigFile, JsonConvert.SerializeObject(_Config));
                    File.WriteAllText(newMyConfigFile, JsonConvert.SerializeObject(_MyConfig));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                _Log.LogInfo(ex.Message + ex.Source);
            }
            _logger.Debug("保存配置文件完成");
        }
        /// <summary>
        /// 连接成功后
        /// </summary>
        public void AfterConnectCompleted()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(AfterConnectCompleted));
                return;
            }
            if (_EAAccount.IsLogin)
            {
                _timerRefreshBalance.Start();
                if (_Config.RaceInfoList != null && _Config.RaceInfoList.Count > 0)
                {
                    comboBoxRaceType.Items.Clear();
                    foreach (RaceInfo raceInfo in _Config.RaceInfoList)
                    {
                        comboBoxRaceType.Items.Add(raceInfo.raceType + "|" + raceInfo.displayName + "|" + raceInfo.category);
                    }
                }
            }
            else
            {
                _timerRefreshBalance.Stop();
            }
            this.Text = $"LZY [{_EAAccount.UserCode}]";
        }
        // 🔧 新增：EA/DS 各自独立的"已提示过断链"标记，避免账号断线期间
        // 每次 _timerRefreshBalance 轮询都弹一次窗，把用户淹没在重复弹窗里。
        private bool _eaDisconnectNotified = false;
        private bool _dsDisconnectNotified = false;
        /// <summary>
        /// 刷新账户余额信息
        /// </summary>
        /// <summary>
        /// 刷新账户余额信息
        /// </summary>
        public async void UpdateAccountBalanceInfo(string accountType)
        {
            string serverAddress = "";
            string userCode = "";
            bool isLogin = false;
            // 1. 读取配置与登录状态
            if (string.Equals(accountType, "EA"))
            {
                isLogin = _EAAccount.IsLogin;
                serverAddress = _Config.EAServerAddress;
                userCode = _EAAccount.UserCode;
            }
            else if (string.Equals(accountType, "DS"))
            {
                isLogin = _DSAccount.IsLogin;
                serverAddress = _Config.DSServerAddress;
                userCode = _DSAccount.UserCode;
            }
            if (!isLogin) return;
            // 2. 🌟 核心：使用 Stopwatch 测量 HTTPHelper.queryBalance 的客户端真实调用耗时
            JObject result = null;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                result = await Task.Run(() => HTTPHelper.queryBalance(serverAddress, userCode));
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _Log.LogInfo($"[性能监控][UpdateAccountBalanceInfo][queryBalance] 请求异常 耗时: {stopwatch.ElapsedMilliseconds}ms, 错误: {ex.Message}");
                HandleAccountDisconnected(accountType);
                return;
            }
            finally
            {
                stopwatch.Stop();
            }
            long clientElapsedMs = stopwatch.ElapsedMilliseconds;
            _Log.LogInfo($"[性能监控][UpdateAccountBalanceInfo][queryBalance] 客户端总耗时: {clientElapsedMs}ms");
            if (result == null || !(bool)result["success"])
            {
                HandleAccountDisconnected(accountType);
                return;
            }
            // 🔧 本次查询成功，说明账号是通的，清掉"已提示过断链"的标记，
            // 避免下次真的断线时被旧标记永久屏蔽，导致再也不弹窗提醒
            ResetAccountDisconnectedNotifyFlag(accountType);
            // 同时打印后端自身返回的处理耗时（如果有的话）
            _Log.LogInfo($"[性能监控][UpdateAccountBalanceInfo][queryBalance] 后端服务耗时: {result["serverProcessTime"]}");
            string profitAndLoss = result["data"]["pl"].ToString();
            string accountCredit = result["data"]["balance"].ToString();
            string plCleanText = Regex.Replace(profitAndLoss, "<.*?>", string.Empty);
            bool isRed = profitAndLoss.Contains("class=\"RD\"") || profitAndLoss.Contains("class='RD'");
            // 3. 🌟 跨线程安全：直接在 UI 控件上使用 BeginInvoke 更新界面
            if (_AccountDisplay.labelAccountCredit.InvokeRequired)
            {
                _AccountDisplay.labelAccountCredit.BeginInvoke(new Action(() => UpdateUIControls(accountType, profitAndLoss, accountCredit, plCleanText, isRed)));
            }
            else
            {
                UpdateUIControls(accountType, profitAndLoss, accountCredit, plCleanText, isRed);
            }
        }
        /// <summary>
        /// 余额查询失败（服务器返回失败，或请求本身抛异常）时统一处理：
        /// 提示用户账号已断链需要重新登陆。同一账号在恢复登陆之前只弹一次。
        /// </summary>
        private void HandleAccountDisconnected(string accountType)
        {
            bool alreadyNotified = string.Equals(accountType, "EA") ? _eaDisconnectNotified : _dsDisconnectNotified;
            if (alreadyNotified) return;

            if (string.Equals(accountType, "EA")) _eaDisconnectNotified = true;
            else if (string.Equals(accountType, "DS")) _dsDisconnectNotified = true;

            string accountName = string.Equals(accountType, "EA") ? "打水" : "读水";
            _Log.LogInfo($"[{accountName}账号]余额查询失败，判定为账号已断链");

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ShowAccountDisconnectedPrompt(accountType, accountName)));
            }
            else
            {
                ShowAccountDisconnectedPrompt(accountType, accountName);
            }
        }
        private void ShowAccountDisconnectedPrompt(string accountType, string accountName)
        {
            // MessageBox.Show 是模态阻塞调用，这一行会一直卡到用户点"确定"才往下走，
            // 所以下面的置位逻辑天然就是"点确定之后才执行"，不需要额外处理。
            MessageBox.Show($"{accountName}账号连接已断开，请重新登陆！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            if (string.Equals(accountType, "EA"))
            {
                _EAAccount.IsLogin = false;
                UpdateLoginConnectStatus(buttonAccountLogin, 0);
            }
            else if (string.Equals(accountType, "DS"))
            {
                _DSAccount.IsLogin = false;
                UpdateDSConnectStatus(buttonDSLogin, 0);
            }
        }
        private void ResetAccountDisconnectedNotifyFlag(string accountType)
        {
            if (string.Equals(accountType, "EA")) _eaDisconnectNotified = false;
            else if (string.Equals(accountType, "DS")) _dsDisconnectNotified = false;
        }
        /// <summary>
        /// 💡 辅助私有方法：专门负责在 UI 线程安全更新控件文本和颜色
        /// </summary>
        private void UpdateUIControls(string accountType, string profitAndLoss, string accountCredit, string plCleanText, bool isRed)
        {
            if (string.Equals(accountType, "EA"))
            {
                _EAAccount.ProfitAndLoss = profitAndLoss;
                _EAAccount.AccountCredit = accountCredit;
                _AccountDisplay.labelAccountCredit.Text = accountCredit;
                _AccountDisplay.labelAccountProfitAndLoss.Text = plCleanText;
                _AccountDisplay.labelAccountProfitAndLoss.ForeColor = isRed ? Color.Red : Color.Black;
            }
            else if (string.Equals(accountType, "DS"))
            {
                _DSAccount.ProfitAndLoss = profitAndLoss;
                _DSAccount.AccountCredit = accountCredit;
                // 注意：核对一下你原代码这里 DS 控件的赋值逻辑是否写反了（原代码 DS 用了 labelAccountCredit/DSCredit 交叉赋值，这里保留你的原意或按需调整）
                _AccountDisplay.labelDSProfitAndLoss.Text = accountCredit;
                _AccountDisplay.labelDSCredit.Text = plCleanText;
                _AccountDisplay.labelDSCredit.ForeColor = isRed ? Color.Red : Color.Black;
            }
        }
        /// <summary>
        /// 更新帐户连接状态
        /// </summary>
        /// <param name="Status">true= 连接，false=断开</param>
        public void UpdateLoginConnectStatus(System.Windows.Forms.Button button, int Status)
        {
            if (button.InvokeRequired)
            {
                button.Invoke(new Action<System.Windows.Forms.Button, int>(UpdateLoginConnectStatus), new object[] { button, Status });
            }
            else
            {
                if (Status == 0)
                {
                    UpdateLoginUI(false, pictureBoxLoginProcessing, _loginTimer);
                    button.Text = "未登陆";
                    button.BackColor = Color.Red;
                    button.Enabled = true;
                }
                if (Status == 1)
                {
                    UpdateLoginUI(false, pictureBoxLoginProcessing, _loginTimer);
                    button.Text = "已登陆";
                    button.BackColor = Color.Green;
                    button.Enabled = true;
                }
                if (Status == 2)
                {
                    UpdateLoginUI(true, pictureBoxLoginProcessing, _loginTimer);
                    button.Text = "登陆中";
                    button.Enabled = false;
                    button.BackColor = Color.Yellow;
                }
            }
        }
        public void UpdateDSConnectStatus(System.Windows.Forms.Button button, int Status)
        {
            if (button.InvokeRequired)
            {
                button.Invoke(new Action<System.Windows.Forms.Button, int>(UpdateDSConnectStatus), new object[] { button, Status });
            }
            else
            {
                if (Status == 0)
                {
                    UpdateLoginUI(false, pictureBoxDSLoginProcessing, _DSLoginTimer);
                    button.Text = "未登陆";
                    button.BackColor = Color.Red;
                    button.Enabled = true;
                }
                if (Status == 1)
                {
                    UpdateLoginUI(false, pictureBoxDSLoginProcessing, _DSLoginTimer);
                    button.Text = "已登陆";
                    button.BackColor = Color.Green;
                    button.Enabled = true;
                }
                if (Status == 2)
                {
                    UpdateLoginUI(true, pictureBoxDSLoginProcessing, _DSLoginTimer);
                    button.Text = "登陆中";
                    button.Enabled = false;
                    button.BackColor = Color.Yellow;
                }
            }
        }
        // =========================================================================
        // 1. 独立锁字典定义（分别管理下注与吃票，互不阻塞）
        // =========================================================================
        private readonly ConcurrentDictionary<string, object> _betLocks = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, object> _eatLocks = new ConcurrentDictionary<string, object>();
        /// <summary>
        /// 自动下注处理
        /// </summary>
        public void AutoBetProcess(List<BetInfo> EATBetInfos, string type, string targetCombo = null)
        {
            if (EATBetInfos == null || EATBetInfos.Count == 0 || string.IsNullOrEmpty(targetCombo)) return;
            if (!TryGetTradeTimeContext(out _, out _, out DateTime triggerEndTime) || DateTime.Now >= triggerEndTime)
            {
                _logger.Debug($"[{type}][{targetCombo}] 已达到正常下注截止时间，跳过 BET。");
                return;
            }
            string dictKey = ComboTradeState.BuildDictKey(_Config.CurrentRaceNo, type, targetCombo);
            object betLock = _betLocks.GetOrAdd(dictKey, _ => new object());
            lock (betLock)
            {
                _Log.LogInfo($"[{type}]开始处理赌票(Combo: {targetCombo}), 候选记录数[{EATBetInfos.Count}]");
                try
                {
                    List<BetInfo> MatchedList = new List<BetInfo>();
                    var bestInCurrentBatch = EATBetInfos.OrderBy(x => x.odds).First();
                    bool isQStake = string.Equals(type, "Q");
                    int configStartOdds = isQStake ? _Config.QStartOdds : _Config.QPStartOdds;
                    int configEndOdds = isQStake ? _Config.QEndOdds : _Config.QPEndOdds;
                    int configBetSpread = isQStake ? _Config.QBetSpread : _Config.QPBetSpread;
                    int configLowerThreshold = isQStake ? _Config.QLowerThreshold : _Config.QPLowerThreshold;
                    int configMinLimit = isQStake ? _Config.QMinLimit : _Config.QPMinLimit;
                    int configStakeAmount = isQStake ? _Config.QStakeAmount : _Config.QPStakeAmount;
                    if (bestInCurrentBatch.odds < configEndOdds || bestInCurrentBatch.odds > configStartOdds)
                    {
                        return;
                    }
                    _Config.TradeStateStore.TryGet(dictKey, out var state); // 全新组合时为 null，属正常情况
                    string blockReason = null;
                    bool isBlocked = state != null && _decisionEngine.ShouldSkipBet(state, out blockReason);
                    if (isBlocked)
                    {
                        _logger.Debug($"组合 [{dictKey}] {blockReason}，下注拦截");
                    }
                    else
                    {
                        bool isOddsValid = bestInCurrentBatch.odds >= configEndOdds && bestInCurrentBatch.odds <= configStartOdds;
                        bool isHeatValid = bestInCurrentBatch.limit >= configMinLimit;
                        if (isOddsValid && isHeatValid)
                        {
                            _logger.Debug($"🔥 触发下注！组合: {bestInCurrentBatch.combo} | 水折: {bestInCurrentBatch.odds}% | 限额: {bestInCurrentBatch.limit}");
                            // 🔧 不再直接修改 bestInCurrentBatch（它是 eatList 里的共享对象，
                            // AutoEatProcess 紧接着会对同一个 eatList 再取一次 bestBet，
                            // 如果这里直接改共享对象的 odds，AutoEatProcess 会看到被改过的值）。
                            int adjustedOdds = bestInCurrentBatch.odds;
                            if (bestInCurrentBatch.odds == configStartOdds && bestInCurrentBatch.amount <= configLowerThreshold)
                            {
                                adjustedOdds = bestInCurrentBatch.odds - 1;
                            }
                            BetInfo newEatInfo = new BetInfo(bestInCurrentBatch)
                            {
                                odds = adjustedOdds - configBetSpread,
                                stakeAmount = configStakeAmount
                            };
                            MatchedList.Add(newEatInfo);
                        }
                    }
                    if (MatchedList.Count > 0)
                    {
                        var comboState = _Config.TradeStateStore.GetOrCreate(_Config.CurrentRaceNo, type, targetCombo);
                        Random random = new Random();
                        int RequestNo = 1;
                        foreach (var betInfoItem in MatchedList)
                        {
                            string intentId = null;
                            try
                            {
                                intentId = comboState.ReserveBetIntent(configStakeAmount, TimeSpan.FromSeconds(60));
                                var pendingInfo = new EatBetInfo
                                {
                                    raceDate = _Config.CurrentRaceDate,
                                    raceType = _Config.CurrentRaceType,
                                    raceNo = _Config.CurrentRaceNo,
                                    type = type,
                                    combo = targetCombo,
                                    betPendingAmount = configStakeAmount,
                                    totalBetAmount = configStakeAmount,
                                    betOdds = betInfoItem.odds
                                };
                                JObject result = ExecuteTrade(pendingInfo, betInfoItem, "P", "Y");

                                // 🔧 关键修复：与强平通道（ClosePositionByDeadline）保持完全一致的口径，
                                // 区分"服务器明确拒绝"（result != null 但 success=false）和
                                // "请求本身异常/网络超时/JSON解析失败"（result == null）两种情况：
                                //   - result == null → 这笔单子最终是否成交是未知的，若立即释放预占，
                                //                       一旦实际上已成交，下一轮扫描会因"没有 pending"
                                //                       而重复提交，造成重复下注/超发。必须保留预占，
                                //                       等待下一轮 RefreshMyTradeSnapshot/ApplyServerSnapshot
                                //                       用服务器权威数据核实。
                                //   - result != null 且未接受 → 服务器已经正常处理完请求并给出明确拒绝，
                                //                       这笔单子确定没有成交，应立即释放预占，
                                //                       允许该 combo 在下一轮被重新判断和提交。
                                if (!IsOrderAccepted(result))
                                {
                                    if (result == null)
                                    {
                                        _logger.Warn($"[{targetCombo}] 下注请求异常（网络超时/连接失败/响应解析失败），" +
                                                     $"保留预占等待服务器快照核实，避免误判后重复下单。");
                                    }
                                    else
                                    {
                                        string rejectMsg = result["message"]?.ToString() ?? "未知拒绝原因";
                                        comboState.ReleaseBetIntent(intentId);
                                        _logger.Warn($"[{targetCombo}] 下注被服务器明确拒绝: {rejectMsg}，释放预占。");
                                    }
                                }
                                _Log.LogInfo($"发送下注第[{RequestNo}]笔...");
                            }
                            catch (Exception ex)
                            {
                                if (intentId != null) comboState.ReleaseBetIntent(intentId);
                                _logger.Error($"[{targetCombo}] 下单异常: {ex.Message}");
                            }
                            if (MatchedList.Count > 1)
                            {
                                RequestNo++;
                                Thread.Sleep(random.Next(100, 500));
                            }
                        }
                    }
                }
                finally
                {
                    _logger.Debug($"[{targetCombo}] 离开下注临界区");
                }
            }
        }
        /// <summary>
        /// 自动吃票处理
        /// </summary>
        /// <summary>
        /// 自动吃票处理
        /// </summary>
        public void AutoEatProcess(List<BetInfo> BetBetInfos, string type, string targetCombo = null)
        {
            if (BetBetInfos == null || BetBetInfos.Count == 0 || string.IsNullOrEmpty(targetCombo)) return;
            if (!TryGetTradeTimeContext(out _, out _, out DateTime triggerEndTime) || DateTime.Now >= triggerEndTime)
            {
                _logger.Debug($"[{type}][{targetCombo}] 已达到正常下注截止时间，跳过 EAT。");
                return;
            }
            string dictKey = ComboTradeState.BuildDictKey(_Config.CurrentRaceNo, type, targetCombo);
            object eatLock = _eatLocks.GetOrAdd(dictKey, _ => new object());
            _logger.Debug($"[诊断] AutoEatProcess targetCombo原始值='{targetCombo}' 长度={targetCombo.Length} 拼出dictKey='{dictKey}'");
            _logger.Debug($"[诊断] TradeStateStore现有近似条目: " +
                string.Join(", ", _Config.TradeStateStore.GetAll()
                    .Where(s => s.RaceNo == _Config.CurrentRaceNo && s.Type == type)
                    .Select(s => $"'{s.DictKey}'(combo原始='{s.Combo}')")));
            lock (eatLock)
            {
                _Log.LogInfo($"[{type}]开始处理吃票(Combo: {targetCombo}), 候选记录数[{BetBetInfos.Count}]");
                try
                {
                    if (!_Config.TradeStateStore.TryGet(dictKey, out var state))
                    {
                        _logger.Debug($"无交易记录或组合 [{dictKey}] 不存在，跳过吃票");
                        return;
                    }
                    _logger.Debug(state.DescribeForLog());
                    if (_decisionEngine.ShouldSkip(state, out string reason))
                    {
                        _logger.Debug($"[{dictKey}] {reason}，吃票跳过");
                        return;
                    }
                    bool isQStake = string.Equals(type, "Q");
                    int configSpread = isQStake ? _Config.QEatSpread : _Config.QPEatSpread;
                    int configStakeAmount = isQStake ? _Config.QStakeAmount : _Config.QPStakeAmount;
                    var bestBet = BetBetInfos
                        .Where(b => string.Equals(b.combo, targetCombo))
                        .OrderBy(b => b.odds)
                        .FirstOrDefault();
                    if (bestBet == null)
                    {
                        _logger.Debug($"[{targetCombo}] 当前候选记录未满足吃票条件");
                        return;
                    }
                    bestBet.action = "BET";
                    if (bestBet.odds < configSpread + state.BetOdds)
                    {
                        bestBet.odds = configSpread + (int)state.BetOdds;
                    }
                    bestBet.stakeAmount = configStakeAmount;
                    double engineRemaining = _decisionEngine.GetRemainingEatAmount(state);
                    double hardRemaining = state.GetHardRemainingEatAmount();
                    double requestedStake = Math.Min(
                        Math.Min(engineRemaining, hardRemaining),
                        configStakeAmount);
                    _logger.Debug(
                        $"[EAT-AUDIT] [{dictKey}] " +
                        $"BET_EXEC={state.BetExecuted:F3} | " +
                        $"EAT_EXEC={state.EatExecuted:F3} | " +
                        $"EAT_PENDING={state.EatPending:F3} | " +
                        $"EFFECTIVE_EAT={state.EffectiveEatCommitted:F3} | " +
                        $"ENGINE_REMAIN={engineRemaining:F3} | " +
                        $"HARD_REMAIN={hardRemaining:F3} | " +
                        $"REQUEST={requestedStake:F3}");
                    if (requestedStake <= 0)
                    {
                        _logger.Debug($"[{dictKey}] 剩余可吃金额为0，跳过本次吃票");
                        return;
                    }
                    if (!state.TryReserveEatIntent(
                            requestedStake,
                            TimeSpan.FromSeconds(60),
                            out string intentId,
                            out double reservedStake))
                    {
                        _logger.Warn(
                            $"[EAT-BLOCK] [{dictKey}] 原子预占失败，禁止吃票。" +
                            $"BET_EXEC={state.BetExecuted:F3}, " +
                            $"EAT_EXEC={state.EatExecuted:F3}, " +
                            $"EAT_PENDING={state.EatPending:F3}, " +
                            $"EFFECTIVE_EAT={state.EffectiveEatCommitted:F3}, " +
                            $"HARD_REMAIN={state.GetHardRemainingEatAmount():F3}, " +
                            $"REQUEST={requestedStake:F3}");
                        return;
                    }
                    if (reservedStake + 0.001 < requestedStake)
                    {
                        state.ReleaseEatIntent(intentId);
                        _logger.Warn(
                            $"[EAT-BLOCK] [{dictKey}] 原子预占金额被裁剪。" +
                            $"REQUEST={requestedStake:F3}, RESERVED={reservedStake:F3}，" +
                            $"为避免订单金额与预占金额不一致，本次取消下单。");
                        return;
                    }
                    double finalStake = reservedStake;
                    _logger.Warn(
                        $"[EAT-SEND] [{dictKey}] " +
                        $"BET_EXEC={state.BetExecuted:F3} | " +
                        $"EFFECTIVE_EAT_BEFORE={state.EffectiveEatCommitted - finalStake:F3} | " +
                        $"HARD_REMAIN_BEFORE={state.GetHardRemainingEatAmount() + finalStake:F3} | " +
                        $"STAKE={finalStake:F3}");
                    try
                    {
                        var pendingInfo = new EatBetInfo
                        {
                            raceDate = _Config.CurrentRaceDate,
                            raceType = _Config.CurrentRaceType,
                            raceNo = _Config.CurrentRaceNo,
                            type = type,
                            combo = targetCombo,
                            eatPendingAmount = finalStake,
                            totalEatAmount = finalStake,
                            eatOdds = bestBet.odds
                        };
                        bestBet.stakeAmount = (int)finalStake;
                        if (DateTime.Now >= triggerEndTime)
                        {
                            state.ReleaseEatIntent(intentId);
                            _logger.Debug($"[{targetCombo}] EAT 发送前已达到正常下注截止[{triggerEndTime:HH:mm:ss}]，取消本次 EAT。");
                            return;
                        }
                        JObject result = ExecuteTrade(pendingInfo, bestBet, "P", "Y");
                        if (IsOrderAccepted(result))
                        {
                            _Log.LogInfo(
                                $"[{targetCombo}] 吃票指令已发送 | amount={finalStake:F3} | " +
                                $"BET_EXEC={state.BetExecuted:F3} | " +
                                $"EFFECTIVE_EAT={state.EffectiveEatCommitted:F3}");
                        }
                        else
                        {
                            if (result == null)
                            {
                                _logger.Warn(
                                    $"[{targetCombo}] 吃票请求异常（网络超时/连接失败/响应解析失败），" +
                                    $"保留预占等待服务器快照核实，避免误判后重复下单。");
                            }
                            else
                            {
                                string rejectMsg = result["message"]?.ToString() ?? "未知拒绝原因";
                                state.ReleaseEatIntent(intentId);
                                _logger.Warn(
                                    $"[{targetCombo}] 吃票被服务器明确拒绝: {rejectMsg}，释放预占。");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        state.ReleaseEatIntent(intentId);
                        _logger.Error($"[{targetCombo}] 吃票下单异常: {ex.Message}");
                    }
                }
                finally
                {
                    _logger.Debug($"[{targetCombo}] 离开吃票临界区");
                }
            }
        }
        /// <summary>
        /// 判断当前下注情况
        /// 1:全新没有下注
        /// 2:已下注并完全平仓
        /// 3:已下过吃注,待下赌注
        /// 4:已下过赌注,待下吃注
        /// 0:异常状态
        /// </summary>
        /// <param name="bettingInfoDict"></param>
        /// <returns></returns>
        public int CurrentBettingPendingStatus(IDictionary<string, string> bettingInfoDict, double stakeAmount)
        {
            int currentBettingPendingStatus = 0;
            if (bettingInfoDict == null || bettingInfoDict.Count == 0)
            {
                return 1; // 1: 全新没有下注
            }
            // 安全解析各项金额
            double.TryParse(bettingInfoDict.ContainsKey("total_eat_amount") ? bettingInfoDict["total_eat_amount"] : "0", out double currentTotalEatAmount);
            double.TryParse(bettingInfoDict.ContainsKey("eat_executed_amount") ? bettingInfoDict["eat_executed_amount"] : "0", out double currentEatExecutedAmount);
            double.TryParse(bettingInfoDict.ContainsKey("eat_pending_amount") ? bettingInfoDict["eat_pending_amount"] : "0", out double currentEatPending);
            double.TryParse(bettingInfoDict.ContainsKey("total_bet_amount") ? bettingInfoDict["total_bet_amount"] : "0", out double currentTotalBetAmount);
            double.TryParse(bettingInfoDict.ContainsKey("bet_executed_amount") ? bettingInfoDict["bet_executed_amount"] : "0", out double currentBetExecutedAmount);
            double.TryParse(bettingInfoDict.ContainsKey("bet_pending_amount") ? bettingInfoDict["bet_pending_amount"] : "0", out double currentBetPending);
            // 状态 1: 全新没有下注（所有金额均为 0）
            if (currentEatExecutedAmount == 0 && currentBetExecutedAmount == 0
                && currentTotalEatAmount == 0 && currentTotalBetAmount == 0
                && currentEatPending == 0 && currentBetPending == 0)
            {
                return 1;
            }
            // 状态 2: 已下注并完全平仓（吃和赌的总额相等，且全部执行完毕，没有挂单）
            if (currentTotalEatAmount > 0 && currentTotalBetAmount > 0
                && currentTotalEatAmount == currentTotalBetAmount
                && currentTotalEatAmount == currentEatExecutedAmount
                && currentTotalBetAmount == currentBetExecutedAmount
                && currentEatPending == 0 && currentBetPending == 0)
            {
                return 2;
            }
            // 状态 3: 已下过吃注（且吃已完全执行完毕），当前有赌注正在等待/排队 (bet_pending > 0)
            if (currentTotalEatAmount > 0
                && currentTotalEatAmount == currentEatExecutedAmount
                && currentBetPending > 0)
            {
                return 3;
            }
            // 状态 4: 已下过赌注（且赌已完全执行完毕），当前有吃注正在等待/排队 (eat_pending > 0)
            if (currentTotalBetAmount > 0
                && currentTotalBetAmount == currentBetExecutedAmount
                && currentEatPending > 0)
            {
                return 4;
            }
            // 💡 建议补充：如果以上都不满足，但存在任意 Pending，可以归为通用排队/处理中状态（或保持返回 0 视你业务而定）
            if (currentEatPending > 0 || currentBetPending > 0)
            {
                // 如果有需要，可以返回一个特定的中间状态码，例如 5 代表“部分挂单中”
            }
            return currentBettingPendingStatus; // 默认返回 0（代表其他或未知中间状态）
        }
        /// <summary>
        /// 强迫策略，逐步增加水折平仓
        /// </summary>
        /// <param name="bettingInfoDict"></param>
        /// <param name="BETBetInfos">
        /// market 上别人挂出来的"赌"(BET)单——即我方即将提交的"吃"(EAT)单真正要撮合的对手盘。
        /// 调用方必须传 queryMarket 返回的 Q_BETBetInfos/QP_BETBetInfos，不能传 EAT 侧数据
        /// （EAT 侧是别人也在等吃、跟我方同边，不是可撮合的对手盘，会导致几乎所有 combo 都
        /// 精确匹配不到，参见下面 [强平诊断] 日志）。
        /// </param>
        /// <param name="type"></param>
        public void ClosePositionByDeadline(Dictionary<string, IDictionary<string, string>> bettingInfoDict, List<BetInfo> BETBetInfos, string type)
        {
            if (bettingInfoDict == null || bettingInfoDict.Count == 0)
            {
                return;
            }
            _Log.LogInfo($"BETBetInfos[{BETBetInfos?.Count ?? 0}],开始处理强平...");

            // 🔧 诊断：真正决定"某个组合是否会实际下强平单"的关键一步，是后面 Parallel.ForEach
            // 里 `BETBetInfos.Where(b => string.Equals(comboKey, b.combo))` 这行精确字符串匹配——
            // 如果 queryMarket 返回的盘口数据里，这个 combo 的命名格式（马号顺序、是否带括号、
            // 有无多余空格等）跟 TradeStateStore 内部的 Combo 格式对不上，就会匹配不到，
            // 只留下一行不痛不痒的"分析数据[无]"，看不出到底是"没报价"还是"格式不一致"。
            // 这里在批处理开始前，对 bettingInfoDict 里的每个 combo 统一做一次全量核对并提前列出来，
            // 避免等到 Parallel.ForEach 并发写日志、可能被截断/看不全。
            var eatComboSet = new HashSet<string>((BETBetInfos ?? new List<BetInfo>()).Select(b => b.combo));
            foreach (var comboKeyToCheck in bettingInfoDict.Keys)
            {
                if (eatComboSet.Contains(comboKeyToCheck)) continue;
                string normalized = comboKeyToCheck.Trim('(', ')', ' ');
                string reversed = normalized.Contains('-')
                    ? string.Join("-", normalized.Split('-').Reverse())
                    : normalized;
                var nearMatches = eatComboSet
                    .Where(c => string.Equals((c ?? "").Trim('(', ')', ' '), normalized, StringComparison.Ordinal)
                             || string.Equals(c, reversed, StringComparison.Ordinal)
                             || string.Equals((c ?? "").Trim('(', ')', ' '), reversed, StringComparison.Ordinal))
                    .ToList();
                _logger.Warn(
                    $"[强平诊断][{type}][{comboKeyToCheck}] 在本轮市场 BET 侧数据[{eatComboSet.Count}个不同combo]中找不到精确匹配 b.combo=='{comboKeyToCheck}'，" +
                    $"稍后会走入'分析数据[无]'分支、不会下单。" +
                    (nearMatches.Count > 0
                        ? $" 发现疑似格式不一致的近似匹配: [{string.Join(", ", nearMatches)}]，请核对 combo 命名格式（顺序/括号/空格）是否一致。"
                        : " 且未发现任何近似格式的candidate，该组合本轮 queryMarket 返回的 BET 侧盘口数据里可能完全没有报价（即当前没有任何人挂'赌'单可供我方'吃'）。"));
            }

            bool isQStake = string.Equals(type, "Q");
            double configSpread = isQStake ? _Config.QEatSpread : _Config.QPEatSpread;
            double configStartOdds = isQStake ? _Config.QStartOdds : _Config.QPStartOdds;
            double configEndOdds = isQStake ? _Config.QEndOdds : _Config.QPEndOdds;
            double configMinLimit = isQStake ? _Config.QMinLimit : _Config.QPMinLimit;
            double configStakeAmount = isQStake ? _Config.QStakeAmount : _Config.QPStakeAmount;
            // 使用 Parallel.ForEach 按照 combo 分组进行多线程并发处理
            Parallel.ForEach(bettingInfoDict, kvp =>
            {
                var itemDict = kvp.Value;
                string comboKey = kvp.Key;
                string dictKey = ComboTradeState.BuildDictKey(_Config.CurrentRaceNo, type, comboKey);
                // 🔧 复用 AutoEatProcess 同一把按 dictKey 分片的锁：强平通道和正常吃票通道
                // 必须互斥，否则两边会各自通过"没有 pending 就可以提交"的检查，导致同一个
                // combo 被吃两次。
                object eatLock = _eatLocks.GetOrAdd(dictKey, _ => new object());
                lock (eatLock)
                {
                    try
                    {
                        if (!TryGetTradeTimeContextForClose(DateTime.Now, out _, out _, out DateTime closeStartTime, out DateTime closeEndTime) ||
                            DateTime.Now < closeStartTime || DateTime.Now >= closeEndTime)
                        {
                            _logger.Debug($"[{dictKey}] 已离开强平窗口，跳过本组合。");
                            return;
                        }
                        if (!_Config.TradeStateStore.TryGet(dictKey, out var state))
                        {
                            _logger.Debug($"[{dictKey}] 无交易记录，强平跳过");
                            return;
                        }

                        // 🔧 直接复用 TradeDecisionEngine.ShouldSkip，和 AutoEatProcess 用同一套判断口径。
                        // 🔧 诊断增强：这里的 state 是在 Parallel.ForEach 内部重新 TryGet 出来的最新快照，
                        // 跟外层 allStates 过滤时用的快照之间存在时间差（TOCTOU），有可能外层判定"入选"，
                        // 到这里 ShouldSkip 又因为状态已变化而判定跳过。把当时的关键数值一并打出来，
                        // 方便和外层"[强平诊断]"那条日志的数值做对比，确认是否真的发生了状态漂移。
                        if (_decisionEngine.ShouldSkip(state, out string skipReason))
                        {
                            _logger.Debug($"[{dictKey}] {skipReason}，强平跳过 | " +
                                          $"BetExecuted={state.BetExecuted:F3}, EatExecuted={state.EatExecuted:F3}, " +
                                          $"EffectiveBetPending={state.EffectiveBetPending:F3}, EffectiveEatPending={state.EffectiveEatPending:F3}");
                            return;
                        }

                        // 🔥 剩余需要补吃的精确金额，用于下单量裁剪，避免超过实际缺口
                        double remainingEatAmount = _decisionEngine.GetRemainingEatAmount(state);

                        double.TryParse(itemDict.ContainsKey("bet_odds") ? itemDict["bet_odds"] : "0", out double betOdds);
                        if (BETBetInfos != null && BETBetInfos.Count > 0)
                        {
                            var targetEatBetInfo = BETBetInfos.Where(b => string.Equals(comboKey, b.combo)).MinBy(b => b.odds);
                            if (targetEatBetInfo != null)
                            {
                                if (targetEatBetInfo.odds > 81)
                                {
                                    targetEatBetInfo.odds = targetEatBetInfo.odds - 1;
                                }
                                targetEatBetInfo.action = "BET";
                                // 🔒 强平也必须经过与 AutoEatProcess 相同的原子 EAT 上限检查，防止正常吃票与强平并发超发。
                                var comboState = _Config.TradeStateStore.GetOrCreate(_Config.CurrentRaceNo, type, comboKey);
                                double requestedStake = Math.Min(remainingEatAmount, configStakeAmount);
                                if (requestedStake <= AmountEpsilon)
                                {
                                    // 🔧 诊断：原来这里是纯静默 return，如果 remainingEatAmount 在这一刻
                                    // 被算成 <=0（哪怕外层 allStates 判断时是 >0，同样可能是 TOCTOU 状态漂移
                                    // 导致），之前完全没有任何日志能解释这个组合为什么没有下强平单。
                                    _logger.Debug($"[{dictKey}] 强平跳过：remainingEatAmount={remainingEatAmount:F3}, " +
                                                  $"configStakeAmount={configStakeAmount:F3} ⇒ requestedStake={requestedStake:F3} <= {AmountEpsilon}，无需下单。");
                                    return;
                                }
                                if (!comboState.TryReserveEatIntent(requestedStake, TimeSpan.FromSeconds(60), out string intentId, out double reservedStake))
                                {
                                    _logger.Warn($"[EAT-BLOCK][强平][{dictKey}] 原子预占失败，禁止强平吃票。REQUEST={requestedStake:F3}, HARD_REMAIN={comboState.GetHardRemainingEatAmount():F3}");
                                    return;
                                }
                                if (reservedStake + AmountEpsilon < requestedStake)
                                {
                                    comboState.ReleaseEatIntent(intentId);
                                    _logger.Warn($"[EAT-BLOCK][强平][{dictKey}] 原子预占金额被裁剪。REQUEST={requestedStake:F3}, RESERVED={reservedStake:F3}，取消本次强平下单。");
                                    return;
                                }
                                double actualStake = reservedStake;
                                targetEatBetInfo.stakeAmount = (int)actualStake;
                                if (DateTime.Now >= closeEndTime)
                                {
                                    comboState.ReleaseEatIntent(intentId);
                                    _logger.Debug($"[{dictKey}] 强平订单发送前已超过强平窗口结束时间[{closeEndTime:HH:mm:ss}]，取消本次强平 EAT。");
                                    return;
                                }
                                var pendingInfo = new EatBetInfo
                                {
                                    raceDate = _Config.CurrentRaceDate,
                                    raceType = _Config.CurrentRaceType,
                                    raceNo = _Config.CurrentRaceNo,
                                    type = type,
                                    combo = comboKey,
                                    eatPendingAmount = actualStake,
                                    totalEatAmount = actualStake,
                                    eatOdds = targetEatBetInfo.odds
                                };
                                JObject result = ExecuteTrade(pendingInfo, targetEatBetInfo, "M", "SO");

                                // 🔧 关键修复：区分"真正的网络异常/超时"和"服务器明确拒绝"两种情况，
                                // 不再无差别地对所有失败都保留预占。
                                //
                                // 依据 HTTPHelper.singleAutoTrade 的修复：
                                //   - result == null        → HTTP 请求本身异常、或响应体无法解析为合法 JSON，
                                //                              这笔单子最终是否成交是"未知"的，必须保留预占，
                                //                              等下一轮 ApplyServerSnapshot（服务器权威快照）核实，
                                //                              避免"以为没成交、实际已成交"导致的重复提交和超发。
                                //   - result != null 且未接受 → 服务器已经正常处理完请求，并给出了明确的业务拒绝
                                //                              （例如 {"success":false,"message":"...你的交易不成功。"}），
                                //                              这笔单子确定没有成交。继续保留预占没有意义，只会白白
                                //                              占用 60 秒 TTL，阻止这个 combo 在下一轮被重新判断和
                                //                              重试，必须立即释放。
                                if (!IsOrderAccepted(result))
                                {
                                    if (result == null)
                                    {
                                        _logger.Warn($"[{dictKey}] 强平市价单请求异常（网络超时/连接失败/响应解析失败），" +
                                                     $"保留预占等待服务器快照核实，避免误判后重复下单。");
                                    }
                                    else
                                    {
                                        string rejectMsg = result["message"]?.ToString() ?? "未知拒绝原因";
                                        comboState.ReleaseEatIntent(intentId);
                                        _logger.Warn($"[{dictKey}] 强平市价单被服务器明确拒绝: {rejectMsg}，" +
                                                     $"释放预占，允许下一轮强平重新判断该组合。");
                                    }
                                }

                                _Log.LogInfo($"[强制下注][{type}][{comboKey}][赌注水折{betOdds}]赌作实,吃等待,分析数据[{BETBetInfos.Count}]下吃票[水折{targetEatBetInfo.odds}][本次吃{actualStake}]");
                            }
                            else
                            {
                                _Log.LogInfo($"[强制下注][{type}][{comboKey}][赌注水折{betOdds}]赌作实,吃等待,分析数据[无]");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"[ClosePositionByDeadline] 处理 combo [{comboKey}] 异常: {ex.Message}", ex);
                    }
                }
            });
        }
        // 辅助方法：把重复的下注和日志逻辑抽离出来
        /// <summary>
        /// 执行下注
        /// </summary>
        /// <param name="EatBetInfo"></param>
        /// <param name="BetInfo"></param>
        /// <param name="orderType">M:市价单,P:挂单</param>
        /// <param name="autoFlag"></param>
        /// <returns></returns>
        ///
        public JObject ExecuteTrade(dynamic EatBetInfo, dynamic BetInfo, string orderType, string autoFlag)
        {
            string trade_type = string.Equals(BetInfo.action, "EAT") ? "BET" : "EAT";

            // 🔧 关键修复：优先使用调用方已经算好（可能经过"剩余缺口裁剪"）的 BetInfo.stakeAmount，
            // 只有它无效（<=0 或转换失败）时才回退到配置里的固定默认值。
            // 之前这里无条件用 _Config.QStakeAmount/QPStakeAmount 覆盖，
            // 会导致 ClosePositionByDeadline 里精心计算的
            // actualStake = Math.Min(remainingEatAmount, configStakeAmount)
            // 完全失效——当前场景因为配置金额恰好等于10、且缺口从未小于10才没有暴露问题，
            // 一旦出现"剩余缺口小于配置金额"（比如部分成交后只差5块）就会按10发送造成超发。
            int betInfoStake = 0;
            try { betInfoStake = (int)BetInfo.stakeAmount; } catch { /* dynamic 转换失败时忽略，走默认值 */ }
            string stakeAmount = betInfoStake > 0
                ? betInfoStake.ToString()
                : (string.Equals(BetInfo.type, "Q") ? _Config.QStakeAmount.ToString() : _Config.QPStakeAmount.ToString());

            string serverProcessTime = "0ms";
            IDictionary<string, string> BettingMarketData = new Dictionary<string, string>
            {
                { "col_name", BetInfo.type + "_" + BetInfo.action },
                { "combo", BetInfo.combo },
                { "q_type", BetInfo.type},
                { "type", BetInfo.type },
                { "toto", BetInfo.toto.ToString() },
                { "limit", BetInfo.limit.ToString() },
                { "odds", BetInfo.odds.ToString() },
                { "race", BetInfo.raceNo },
                { "stake_amount", stakeAmount },
                { "order_type", orderType },
                { "trade_type", trade_type },
                { "action", trade_type },
            };
            _Log.LogInfo($"[{orderType}][{autoFlag}][{BetInfo.type}][{BetInfo.combo}][{trade_type}][{BetInfo.odds}][{BetInfo.limit}]下注开始");
            JObject resultJObject = null;
            if (string.Equals(orderType, "P"))
            {
                resultJObject = HTTPHelper.submitOrder(_Config.EAServerAddress, _EAAccount, _Config.CurrentRaceType, _Config.CurrentRaceDate, Convert.ToInt32(stakeAmount), BettingMarketData, out serverProcessTime);
                _Log.LogInfo($"[性能监控][ExecuteTrade][submitOrder] 后端服务耗时: {serverProcessTime}");
            }
            if (string.Equals(orderType, "M"))
            {
                resultJObject = HTTPHelper.singleAutoTrade(_Config.EAServerAddress, _EAAccount, _Config.CurrentRaceType, _Config.CurrentRaceDate, Convert.ToInt32(stakeAmount), BettingMarketData, out serverProcessTime);
                _Log.LogInfo($"[性能监控][ExecuteTrade][singleAutoTrade] 后端服务耗时: {serverProcessTime}");
            }
            if (resultJObject != null)
            {
                if (!string.Equals(autoFlag, "N"))
                {
                    BetInfo.action = trade_type;
                    bool accepted = IsOrderAccepted(resultJObject);

                    // 🔧 不再自己开 Task.Run 落库：改为丢进统一的 TradeRecordWriter 队列。
                    // 入队是纯内存操作、几乎不耗时，真正的落库 I/O 由 TradeRecordWriter 的后台
                    // worker 按 dictKey 分片、严格按提交顺序串行处理，既不占用 betLock/eatLock
                    // 的持有时间，又和 RefreshTradeListToDB 的周期性落库共用同一条写入通道，
                    // 彻底避免两条通道互相竞态（详见 TradeRecordWriter.cs 顶部注释）。
                    string dictKey = ComboTradeState.BuildDictKey((string)BetInfo.raceNo, (string)BetInfo.type, (string)BetInfo.combo);
                    _tradeRecordWriter.Enqueue(new TradeWriteJob
                    {
                        Kind = TradeWriteJobKind.SubmitResult,
                        DictKey = dictKey,
                        EatBetInfo = EatBetInfo,
                        BetInfo = BetInfo,
                        Result = resultJObject,
                        AutoFlag = autoFlag,
                        OrderType = orderType,
                        Accepted = accepted
                    });
                }
                else
                {
                    _Log.LogInfo($"[{BetInfo.type}][{BetInfo.combo}][{trade_type}][{BetInfo.odds}][{BetInfo.limit}]手工下注,不更新内存数据");
                }
            }
            return resultJObject;
        }
        private bool IsOrderAccepted(JObject resultJObject)
        {
            if (resultJObject == null) return false;

            // 优先看顶层 success（singleAutoTrade / M 单返回的是扁平结构，没有 data 包裹）
            bool? topLevelSuccess = resultJObject["success"]?.Value<bool>();

            // 如果存在 data.confirmed，按原逻辑（兼容 P 单/submitOrder 可能的嵌套结构）
            var dataConfirmed = resultJObject["data"]?["confirmed"]?.Value<bool>();
            if (dataConfirmed.HasValue)
            {
                return topLevelSuccess == true && dataConfirmed == true;
            }

            // 没有 data.confirmed 字段时，直接信任顶层 success
            return topLevelSuccess == true;
        }
        /// <summary>
        /// 自动下注处理
        /// </summary>
        /// <param name="BetBatInfos"></param>
        public void AutoBetting(IDictionary<string, List<BetInfo>> BetBatInfos)
        {
            // ==========================================
            // 1. 连赢自动下注处理 (Q)
            // ==========================================
            if (BetBatInfos.ContainsKey("Q_BETBetInfos") || BetBatInfos.ContainsKey("Q_EATBetInfos"))
            {
                List<BetInfo> Q_BETBetInfos = BetBatInfos.ContainsKey("Q_BETBetInfos") ? BetBatInfos["Q_BETBetInfos"] : new List<BetInfo>();
                List<BetInfo> Q_EATBetInfos = BetBatInfos.ContainsKey("Q_EATBetInfos") ? BetBatInfos["Q_EATBetInfos"] : new List<BetInfo>();
                _Log.LogInfo($"[Q]下注等待[{Q_BETBetInfos.Count}],吃票等待[{Q_EATBetInfos.Count}],处理开始...");
                if (_Config.QAutoBettingFlag)
                {
                    System.Diagnostics.Stopwatch totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    _Log.LogInfo("[Q]自动下注处理开始（多线程并行）");
                    // 1. 获取允许的集合
                    var allowedQCombos = Utils.Utils.GetAllowedSet(_Config.QCombos);
                    // 2. 分组转字典
                    var eatGroups = Q_EATBetInfos.GroupBy(x => x.combo).ToDictionary(g => g.Key, g => g.ToList());
                    // 3. 动态过滤（若为 null 则直接保留全部）
                    var filteredEatGroups = allowedQCombos == null
                        ? eatGroups
                        : eatGroups.Where(g => allowedQCombos.Contains(g.Key)).ToDictionary(g => g.Key, g => g.Value);
                    // 4. 并行遍历过滤后的字典
                    Parallel.ForEach(filteredEatGroups, kvp =>
                    {
                        string comboKey = kvp.Key;        // 当前的 combo 字符串 (如 "1-2")
                        var eatList = kvp.Value;          // 对应的 List<BetInfo>
                        // 直接判断列表不为空即可执行
                        if (eatList != null && eatList.Count > 0)
                        {
                            AutoBetProcess(eatList, "Q", comboKey);
                            AutoEatProcess(eatList, "Q", comboKey);
                        }
                    });
                    totalStopwatch.Stop();
                    _Log.LogInfo($"[Q]并行处理完毕，总耗时: {totalStopwatch.ElapsedMilliseconds}ms");
                }
            }
            // ==========================================
            // 2. 位置Q自动下注处理 (QP)
            // ==========================================
            if (BetBatInfos.ContainsKey("QP_BETBetInfos") || BetBatInfos.ContainsKey("QP_EATBetInfos"))
            {
                List<BetInfo> QP_BETBetInfos = BetBatInfos.ContainsKey("QP_BETBetInfos") ? BetBatInfos["QP_BETBetInfos"] : new List<BetInfo>();
                List<BetInfo> QP_EATBetInfos = BetBatInfos.ContainsKey("QP_EATBetInfos") ? BetBatInfos["QP_EATBetInfos"] : new List<BetInfo>();
                _Log.LogInfo($"[QP]下注等待[{QP_BETBetInfos.Count}],吃票等待[{QP_EATBetInfos.Count}],处理开始...");
                if (_Config.QPAutoBettingFlag)
                {
                    System.Diagnostics.Stopwatch totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    _Log.LogInfo("[QP]自动下注处理开始（多线程并行）");
                    // 1. 获取允许的集合
                    // 🔧 QPCombos 配置框里填的是不带括号的 "4-8,4-9,..."（跟 Q 的填写格式保持一致，
                    // 方便用户输入），GetAllowedSet 解析出来的也是不带括号的纯文本；但 QP（位置Q）
                    // 这个盘口实际下发的 combo 字段是带括号的，例如 "(4-8)"。如果不在这里统一加上
                    // 括号，下面 filteredEatGroups 用 allowedQPCombos.Contains(g.Key) 比较时，
                    // "4-8" 永远匹配不上实际的 "(4-8)"，会把所有 QP 组合都过滤成空，导致 QP 的
                    // 自动下注/吃票完全不会触发。
                    var rawAllowedQPCombos = Utils.Utils.GetAllowedSet(_Config.QPCombos);
                    HashSet<string> allowedQPCombos = rawAllowedQPCombos == null
                        ? null
                        : new HashSet<string>(rawAllowedQPCombos.Select(c => $"({c})"));
                    // 2. 分组转字典
                    var eatGroups = QP_EATBetInfos.GroupBy(x => x.combo).ToDictionary(g => g.Key, g => g.ToList());
                    // 3. 动态过滤（若为 null 则直接保留全部）
                    var filteredEatGroups = allowedQPCombos == null
                        ? eatGroups
                        : eatGroups.Where(g => allowedQPCombos.Contains(g.Key)).ToDictionary(g => g.Key, g => g.Value);
                    // 4. 并行遍历过滤后的字典
                    Parallel.ForEach(filteredEatGroups, kvp =>
                    {
                        string comboKey = kvp.Key;        // 当前的 combo 字符串 (如 "1-2")
                        var eatList = kvp.Value;          // 对应的 List<BetInfo>
                        // 直接判断列表不为空即可执行
                        if (eatList != null && eatList.Count > 0)
                        {
                            AutoBetProcess(eatList, "QP", comboKey);
                            AutoEatProcess(eatList, "QP", comboKey);
                        }
                    });
                }
            }
        }
        /// <summary>
        /// 刷新盘口数据列表，并且同时处理自动下注
        /// </summary>
        public void RefreshBetInfoDataList()
        {
            string serverProcessTime = "0ms";
            DateTime now = DateTime.Now;
            if (TryGetTradeTimeContext(out DateTime raceDateTime, out DateTime triggerStartTime, out DateTime triggerEndTime))
            {
                IDictionary<string, List<BetInfo>> BetBatInfos = null;
                if (string.Equals(_Config.DSServerAddress, _Config.EAServerAddress))
                {
                    BetBatInfos = HTTPHelper.queryMarket(_Config.EAServerAddress, _EAAccount.UserCode, _Config.CurrentRaceDate, _Config.CurrentRaceType, _Config.CurrentRaceNo, out serverProcessTime);
                }
                else
                {
                    BetBatInfos = HTTPHelper.queryMarket(_Config.DSServerAddress, _DSAccount.UserCode, _Config.CurrentRaceDate, _Config.CurrentRaceType, _Config.CurrentRaceNo, out serverProcessTime);
                }

                // 🔧 网络请求存在耗时，执行到这里时墙钟时间可能已经越过 triggerEndTime，
                // 此时强平通道很可能已经并行启动。为避免两条通道同时对同一批 combo 各判各的、
                // 各提交各的，这里用最新时间再校验一次，一旦越界就直接放弃本轮 AutoBetting，
                // 把这批 combo 完全交给强平通道处理。
                if (DateTime.Now >= triggerEndTime)
                {
                    _Log.LogInfo($"[{DateTime.Now:HH:mm:ss}] queryMarket 期间已达到下注截止[{triggerEndTime:HH:mm:ss}]，本轮跳过正常 AutoBetting，交由强平通道接管");
                    return;
                }

                if (BetBatInfos == null || BetBatInfos.Count == 0)
                {
                    _Log.LogInfo("queryMarket 数据为空");
                }
                if (BetBatInfos != null)
                {
                    _Log.LogInfo($"扫描结束,已获取数据");
                    if (_EnableAutoTrade)
                    {
                        _Log.LogInfo("自动下注处理开始");
                        RefreshMyTradeSnapshot();
                        if (DateTime.Now >= triggerEndTime)
                        {
                            _Log.LogInfo($"[{DateTime.Now:HH:mm:ss}] queryMyTrade 期间已达到下注截止[{triggerEndTime:HH:mm:ss}]，跳过本轮 AutoBetting，交由强平通道接管");
                            return;
                        }
                        AutoBetting(BetBatInfos);
                        _Log.LogInfo("自动下注处理结束");
                    }
                    else
                    {
                        _Log.LogInfo("自动下注未启动");
                    }
                }
            }
            else
            {
                if (now < triggerStartTime)
                {
                    HandleBeforeTradeTime(now, triggerStartTime);
                }
                // V20260913_CLOSE_WINDOW_EXTEND：强平窗口已延长到开赛后 30 秒（见 TryGetTradeTimeContextForClose
                // 的 closeEndTime），这里判断"是否需要检查/启动强平定时器"的右边界也要跟着放宽到
                // raceDateTime.AddSeconds(30)，否则开赛后到强平窗口真正结束这段时间会漏检。
                else if (now >= triggerEndTime && now < raceDateTime.AddSeconds(30))
                {
                    CheckAndStartClosePosition(now);
                }
                // V20260913_AUTO_NEXT_RACE_DELAY：自动转场从"开赛后 30 秒"延后到"开赛后 60 秒"，
                // 留出更充分的时间让强平窗口（开赛后 30 秒结束）先跑完最后一轮，避免转场把
                // 还没来得及处理完的强平定时器/状态提前清掉。
                else if (now > raceDateTime.AddSeconds(60))
                {
                    _Log.LogInfo($"超过开赛时间60秒,启动自动转场");
                    _Log.LogInfo($"[{now.ToString("yyyy-MM-dd HH:mm:ss")}]超过开赛时间[{raceDateTime.AddSeconds(60).ToString("yyyy-MM-dd HH:mm:ss")}]");
                    AutoToNextRace();
                }
            }
        }
        /// <summary>
        /// 计算时间并直接在内部判断当前是否符合下注配置时间区间
        /// </summary>
        private bool TryGetTradeTimeContext(out DateTime raceDateTime, out DateTime triggerStartTime, out DateTime triggerEndTime)
        {
            raceDateTime = default;
            triggerStartTime = default;
            triggerEndTime = default;
            string todayStr = DateTime.Now.ToString("yyyy-MM-dd");
            string fullRaceTimeStr = $"{todayStr} {_Config.CurrentRaceTime}:00";
            if (!DateTime.TryParseExact(fullRaceTimeStr, "yyyy-MM-dd HH:mm:ss",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out raceDateTime))
            {
                System.Diagnostics.Debug.WriteLine($"[Error] 开赛时间格式解析失败: {fullRaceTimeStr}");
                return false;
            }
            // 跨天智能修正逻辑
            DateTime now = DateTime.Now;
            if (now.Hour >= 20 && raceDateTime.Hour < 6)
            {
                raceDateTime = raceDateTime.AddDays(1);
            }
            // 计算触发下注的临界时间点
            double startDuration = _Config.AutoTradeStartTimeDuration;
            double endDuration = _Config.AutoTradeEndTimeDuration;
            triggerStartTime = raceDateTime.AddMinutes(-startDuration);
            triggerEndTime = raceDateTime.AddSeconds(-endDuration);
            // 直接在方法内判断并返回结果
            return now >= triggerStartTime && now < triggerEndTime;
        }
        /// <summary>
        /// 处理未到下注时间的逻辑
        /// </summary>
        private void HandleBeforeTradeTime(DateTime now, DateTime triggerStartTime)
        {
            _Log.LogInfo($"[{now.ToString("yyyy-MM-dd HH:mm:ss")}]未到自动下注时间[{triggerStartTime.ToString("yyyy-MM-dd HH:mm:ss")}]");
        }
        /// <summary>
        /// 检查并启动自动强平逻辑
        /// </summary>
        private void CheckAndStartClosePosition(DateTime now)
        {
            if (!TryGetTradeTimeContextForClose(now, out DateTime raceDateTime, out DateTime normalBetEndTime, out DateTime closeStartTime, out DateTime closeEndTime)) return;
            if (now < closeStartTime || now >= closeEndTime) return;
            if (!_timerRefreshBetInfoForClosePosition.IsRunning)
            {
                _Log.LogInfo($"[{now:yyyy-MM-dd HH:mm:ss}] 正常下注截止[{normalBetEndTime:yyyy-MM-dd HH:mm:ss}]，10秒缓冲后进入强平[{closeStartTime:yyyy-MM-dd HH:mm:ss}]，强平窗口至[{closeEndTime:yyyy-MM-dd HH:mm:ss}]结束，启动自动强平/平仓定时器");
                _timerRefreshBetInfoForClosePosition.Start();
            }
        }
        // V20260913_CLOSE_WINDOW_EXTEND：新增 closeEndTime 出参 = 开赛时间（raceDateTime）+30 秒，
        // 强平窗口由原来的 [closeStartTime, raceDateTime) 延长为 [closeStartTime, closeEndTime)，
        // 所有引用本方法判断"是否还在强平窗口内"的调用点都必须改用 closeEndTime 作为窗口右边界，
        // 而不是继续用 raceDateTime（那样窗口就还是没延长，等于没改）。
        private bool TryGetTradeTimeContextForClose(DateTime now, out DateTime raceDateTime, out DateTime normalBetEndTime, out DateTime closeStartTime, out DateTime closeEndTime)
        {
            raceDateTime = default; normalBetEndTime = default; closeStartTime = default; closeEndTime = default;
            string fullRaceTimeStr = $"{now:yyyy-MM-dd} {_Config.CurrentRaceTime}:00";
            if (!DateTime.TryParseExact(fullRaceTimeStr, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out raceDateTime)) return false;
            if (now.Hour >= 20 && raceDateTime.Hour < 6) raceDateTime = raceDateTime.AddDays(1);
            normalBetEndTime = raceDateTime.AddSeconds(-_Config.AutoTradeEndTimeDuration);
            closeStartTime = normalBetEndTime.AddSeconds(10);
            closeEndTime = raceDateTime.AddSeconds(30);
            return true;
        }
        private async void buttonAccountLogin_Click(object sender, EventArgs e)
        {
            _Log.LogInfo($"开始登陆[{_EAAccount.UserCode}]");
            if (string.Equals(buttonAccountLogin.Text, "未登陆"))
            {
                // ⚡ 检查 UserCode、Password、Pin 是否为空或未填写
                if (string.IsNullOrWhiteSpace(_EAAccount.UserCode) ||
                    string.IsNullOrWhiteSpace(_EAAccount.Password) ||
                    string.IsNullOrWhiteSpace(_EAAccount.Pin))
                {
                    System.Windows.Forms.MessageBox.Show("请输入账号、密码 和 安码！", "提示", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return; // 终止后续登录逻辑
                }
                // 1. 直接在主线程立刻更新 UI（显示“登录中”和动画），绝对不能在这里卡顿！
                UpdateLoginConnectStatus(buttonAccountLogin, 2);
                // 2. 将耗时的网络登录请求放到后台线程异步执行，避免卡死界面
                bool isSuccess = await Task.Run(() =>
                {
                    try
                    {
                        // 如果这里有真实的 HTTP 请求，放这里执行
                        JObject loginResult = HTTPHelper.login(_Config.EAServerAddress, _EAAccount.UserCode, _EAAccount.Password, _EAAccount.Pin);
                        if (loginResult != null && (bool)loginResult["success"])
                        {
                            _Log.LogInfo($"[性能监控][buttonAccountLogin][login] 后端服务耗时: {loginResult["serverProcessTime"]}");
                            if (loginResult["data"] != null)
                            {
                                var dataObj = loginResult["data"];
                                // 1. 获取原始 balance 字符串
                                string balanceStr = dataObj["balance"]?.ToString() ?? "0.00";
                                // 2. 解析 pl 字符串及其样式
                                string plRaw = dataObj["pl"]?.ToString() ?? "0";
                                string plCleanText = Regex.Replace(plRaw, "<.*?>", string.Empty);
                                bool isRed = plRaw.Contains("class=\"RD\"") || plRaw.Contains("class='RD'");
                                // 使用 Invoke 跨线程更新 UI 控件
                                _AccountDisplay.labelAccountCredit.Invoke((MethodInvoker)delegate
                                {
                                    _AccountDisplay.labelAccountCredit.Text = balanceStr;
                                    _AccountDisplay.labelAccountProfitAndLoss.Text = plCleanText;
                                    _AccountDisplay.labelAccountProfitAndLoss.ForeColor = isRed ? Color.Red : Color.Black;
                                });
                                JObject queryTodayRacesResult = HTTPHelper.queryTodayRacesInfo(_Config.EAServerAddress, _EAAccount.UserCode);
                                if (queryTodayRacesResult != null && (bool)queryTodayRacesResult["success"])
                                {
                                    _Log.LogInfo($"[性能监控][buttonAccountLogin][queryTodayRacesInfo] 后端服务耗时: {queryTodayRacesResult["serverProcessTime"]}");
                                    // 4. 解析 races 列表
                                    var racesInfo = queryTodayRacesResult["venues"] as JArray;
                                    if (racesInfo != null)
                                    {
                                        List<RaceInfo> raceInfoList = new List<RaceInfo>();
                                        foreach (var item in racesInfo)
                                        {
                                            RaceInfo raceInfo = new RaceInfo
                                            {
                                                country = item["country"]?.ToString() ?? string.Empty,
                                                name = item["name"]?.ToString() ?? string.Empty,
                                                displayName = item["display_name"]?.ToString() ?? string.Empty,
                                                category = item["category"]?.ToString() ?? string.Empty,
                                                raceType = item["race_type"]?.ToString() ?? string.Empty,
                                                raceDate = item["race_date"]?.ToString() ?? string.Empty,
                                            };
                                            raceInfoList.Add(raceInfo);
                                        }
                                        _Config.RaceInfoList = raceInfoList; // 将解析后的赛程信息列表存储到配置对象中
                                    }
                                }
                            }
                            return true;
                        }
                        else
                        {
                            _Log.LogInfo($"登陆异常");
                            return false;
                        }
                        // 模拟耗时网络请求（测试用）
                        //System.Threading.Thread.Sleep(2000);
                        //return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.ToString());
                        return false;
                    }
                });
                // 3. 网络请求结束后，根据结果切回 UI 线程更新后续状态
                if (isSuccess)
                {
                    _Log.LogInfo("登陆成功");
                    // 启动后台初始化任务
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            _Log.LogInfo("系统初始化进行中...");
                            await Task.Delay(5000);
                            _EAAccount.IsLogin = true;
                            ResetAccountDisconnectedNotifyFlag("EA"); // 🔧 重新登录成功，清掉上次断链的提示标记
                            // 跨线程安全更新登录成功 UI
                            this.Invoke(new Action(() =>
                            {
                                UpdateLoginConnectStatus(buttonAccountLogin, 1);
                                AfterConnectCompleted();
                            }));
                            _Log.LogInfo("系统初始化完成");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.ToString());
                        }
                    });
                }
                else
                {
                    _Log.LogInfo("登陆失败");
                    // 登录失败时恢复按钮状态
                    UpdateLoginConnectStatus(buttonAccountLogin, 0);
                }
            }
            if (string.Equals(buttonAccountLogin.Text, "已登陆"))
            {
                // 如果这里有真实的 HTTP 请求，放这里执行
                JObject logoutResult = HTTPHelper.logout(_Config.EAServerAddress, _EAAccount.UserCode);
                if (logoutResult == null)
                {
                    _Log.LogInfo("登出失败");
                    _EAAccount.IsLogin = false;
                    UpdateLoginConnectStatus(buttonAccountLogin, 0);
                }
                else
                {
                    if ((bool)logoutResult["success"])
                    {
                        _Log.LogInfo($"[性能监控][buttonAccountLogin][logout] 后端服务耗时: {logoutResult["serverProcessTime"]}");
                        _Log.LogInfo("已登出");
                        _EAAccount.IsLogin = false;
                        _Log.LogInfo("扫描已停止");
                        UpdateQueryEATBETInfoStatus("扫描已停止");
                        UpdateLoginConnectStatus(buttonAccountLogin, 0);
                    }
                }
            }
        }
        private async void buttonDSLogin_Click(object sender, EventArgs e)
        {
            _Log.LogInfo($"开始登陆[{_DSAccount.UserCode}]");
            if (string.Equals(buttonDSLogin.Text, "未登陆"))
            {
                // ⚡ 检查 UserCode、Password、Pin 是否为空或未填写
                if (string.IsNullOrWhiteSpace(_DSAccount.UserCode) ||
                    string.IsNullOrWhiteSpace(_DSAccount.Password) ||
                    string.IsNullOrWhiteSpace(_DSAccount.Pin))
                {
                    System.Windows.Forms.MessageBox.Show("请输入账号、密码 和 安码！", "提示", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return; // 终止后续登录逻辑
                }
                // 1. 直接在主线程立刻更新 UI（显示“登录中”和动画）
                UpdateDSConnectStatus(buttonDSLogin, 2);
                // 2. 将耗时的网络登录请求放到后台线程异步执行，避免卡死界面
                bool isSuccess = await Task.Run(() =>
                {
                    try
                    {
                        if (string.Equals(_Config.DSServerAddress, _Config.EAServerAddress))
                        {
                            return true;
                        }
                        else
                        {
                            // 如果这里有真实的 HTTP 请求，放这里执行
                            JObject loginResult = HTTPHelper.login(_Config.DSServerAddress, _DSAccount.UserCode, _DSAccount.Password, _DSAccount.Pin);
                            if (loginResult == null)
                            {
                                _Log.LogInfo("登陆异常");
                                return false;
                            }
                            else
                            {
                                _Log.LogInfo($"[性能监控][buttonDSLogin][login] 后端服务耗时: {loginResult["serverProcessTime"]}");
                                return (bool)loginResult["success"];
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.ToString());
                        return false;
                    }
                });
                if (isSuccess)
                {
                    _Log.LogInfo("登陆成功");
                    // 3. 启动后台初始化任务（避免阻塞 UI）
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            _Log.LogInfo("系统初始化进行中...");
                            await Task.Delay(5000);
                            _DSAccount.IsLogin = true;
                            ResetAccountDisconnectedNotifyFlag("DS"); // 🔧 重新登录成功，清掉上次断链的提示标记
                            // 跨线程安全更新登录成功 UI
                            this.Invoke(new Action(() =>
                            {
                                UpdateDSConnectStatus(buttonDSLogin, 1);
                            }));
                            _Log.LogInfo("系统初始化完成");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.ToString());
                        }
                    });
                }
                else
                {
                    _Log.LogInfo("登陆失败");
                    // 登录失败时恢复按钮状态
                    UpdateDSConnectStatus(buttonDSLogin, 0);
                }
            }
            if (string.Equals(buttonDSLogin.Text, "已登陆"))
            {
                if (string.Equals(_Config.DSServerAddress, _Config.EAServerAddress))
                {
                    _Log.LogInfo("已登出");
                    _DSAccount.IsLogin = false;
                    UpdateDSConnectStatus(buttonDSLogin, 0);
                }
                else
                {
                    // 如果这里有真实的 HTTP 请求，放这里执行
                    JObject logoutResult = HTTPHelper.logout(_Config.DSServerAddress, _DSAccount.UserCode);
                    if (logoutResult == null)
                    {
                        _Log.LogInfo("登出失败");
                        _DSAccount.IsLogin = false;
                        UpdateDSConnectStatus(buttonDSLogin, 0);
                    }
                    else
                    {
                        if ((bool)logoutResult["success"])
                        {
                            _Log.LogInfo($"[性能监控][buttonDSLogin][logout] 后端服务耗时: {logoutResult["serverProcessTime"]}");
                            _Log.LogInfo("已登出");
                            _DSAccount.IsLogin = false;
                            _Log.LogInfo("扫描已停止");
                            UpdateQueryEATBETInfoStatus("扫描已停止");
                            UpdateDSConnectStatus(buttonDSLogin, 0);
                        }
                    }
                }
            }
        }
        private void EAForm_Load(object sender, EventArgs e)
        {
            _Log.LogInfo("******************************************************");
            _Log.LogInfo("*** 大道至简，知易行难，知行合一，得到成功 ***");
            _Log.LogInfo("******************************************************");
            _Log.LogInfo("加载Form开始");
            this.Text = "LZY";
            LoadConfigFile();
            DBUtils.connAddress = _Config.DefaultDBAddress;
            if (!string.IsNullOrEmpty(_Config.EAInfoAddress))
            {
                DBUtils.connEAInfoAddress = _Config.EAInfoAddress;
            }
            DBUtils.createConn("");
            DBUtils.createEaConn("");
            HTTPHelper.AutoHorseRaceFastAPI = _Config.EAServerAddress;
            LoadConfig();
            if (_EAAccount != null)
            {
                //先初始化平台对象,UI加载后再修改平台应用对象
                GeneratePlatformObj();
                IniUI();
            }
            InitialPara();
            _Log.LogInfo("加载Form完成");
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxAccountCode.Text))
            {
                MessageBox.Show("账号不能为空");
                return;
            }
            if (string.IsNullOrEmpty(textBoxAccountPassword.Text))
            {
                MessageBox.Show("密码不能为空");
                return;
            }
            if (string.IsNullOrEmpty(textBoxAccountPin.Text))
            {
                MessageBox.Show("安码不能为空");
                return;
            }
            SaveConfig();
            _Log.LogInfo("信息保存成功");
        }
        private void comboBoxRaceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Config.CurrentRaceTypeDesc = comboBoxRaceType.SelectedItem.ToString();
            _Config.CurrentRaceType = _Config.CurrentRaceTypeDesc.Split("|")[0];
            if (_Config.RaceInfoList != null && _Config.RaceInfoList.Count > 0)
            {
                foreach (var raceInfo in _Config.RaceInfoList)
                {
                    if (string.Equals(raceInfo.raceType, _Config.CurrentRaceType))
                    {
                        _Config.CurrentRaceInfo = raceInfo;
                    }
                }
            }
            JObject raceInfoResult = HTTPHelper.queryRaceInfo(_Config.EAServerAddress, _EAAccount.UserCode, _Config.CurrentRaceDate, _Config.CurrentRaceType, _Config.CurrentRaceNo);
            if (raceInfoResult != null && (bool)raceInfoResult["success"])
            {
                _Log.LogInfo($"[性能监控][comboBoxRaceType][queryRaceInfo] 后端服务耗时: {raceInfoResult["serverProcessTime"]}");
                if (raceInfoResult.ContainsKey("races") && raceInfoResult["races"] != null)
                {
                    JArray availableRacesJArray = (JArray)raceInfoResult["races"];
                    List<IDictionary<string, object>> raceAvailableRaces = new List<IDictionary<string, object>>();
                    foreach (var item in availableRacesJArray)
                    {
                        IDictionary<string, object> raceInfoDict = new Dictionary<string, object>
                            {
                                { "race_num", item["race_num"]?.ToString() ?? string.Empty },
                                { "time", item["time"]?.ToString() ?? string.Empty }
                            };
                        raceAvailableRaces.Add(raceInfoDict);
                    }
                    if (_Config.CurrentRaceInfo != null)
                    {
                        _Config.CurrentRaceInfo.raceAvailableRaces = raceAvailableRaces;
                    }
                    if (_Config.CurrentRaceInfo?.raceAvailableRaces != null && _Config.CurrentRaceInfo.raceAvailableRaces.Count > 0)
                    {
                        comboBoxRaceNo.Items.Clear();
                        textBoxCurrentRaceTime.Text = "";
                        foreach (var raceInfo in _Config.CurrentRaceInfo.raceAvailableRaces)
                        {
                            comboBoxRaceNo.Items.Add(raceInfo["race_num"]);
                        }
                        comboBoxRaceNo.SelectedIndex = 0;
                    }
                }
            }
            else
            {
                if (_Config != null)
                {
                    //获取数据库配置数据
                    _Config.SysConfig = DBHelper.getSysConfig();
                }
                string racesTimeStr = _Config.SysConfig.ContainsKey($"Horse.{_Config.CurrentRaceType}.RCsTime") ? _Config.SysConfig[$"Horse.{_Config.CurrentRaceType}.RCsTime"] : "";
                if (!string.IsNullOrEmpty(racesTimeStr))
                {
                    string[] racesTimes = racesTimeStr.Split(',');
                    comboBoxRaceNo.Items.Clear();
                    for (int i = 1; i <= racesTimes.Length; i++)
                    {
                        comboBoxRaceNo.Items.Add(i.ToString());
                    }
                    // 默认选中第一项
                    if (comboBoxRaceNo.Items.Count > 0)
                    {
                        if (comboBoxRaceNo.SelectedItem != null && _Config.CurrentRaceNo != null && string.Equals(comboBoxRaceNo.SelectedItem, _Config.CurrentRaceNo))
                        {
                            _Config.CurrentRaceTime = racesTimes[Convert.ToInt32(_Config.CurrentRaceNo) - 1];
                            textBoxCurrentRaceTime.Text = _Config.CurrentRaceTime;
                            _Log.LogInfo($"切换赛场为[{_Config.CurrentRaceTypeDesc}][{_Config.CurrentRaceNo}]");
                        }
                        else
                        {
                            if (_Config.CurrentRaceNo != null)
                            {
                                comboBoxRaceNo.SelectedItem = _Config.CurrentRaceNo;
                            }
                            else
                            {
                                comboBoxRaceNo.SelectedItem = 1;
                            }
                            textBoxCurrentRaceTime.Text = _Config.CurrentRaceTime;
                        }
                    }
                }
            }
        }
        private void comboBoxRaceRound_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Config.CurrentRaceNo = comboBoxRaceNo.SelectedItem.ToString();
            if (_Config.CurrentRaceInfo != null)
            {
                if (_Config.CurrentRaceInfo.raceAvailableRaces != null && _Config.CurrentRaceInfo.raceAvailableRaces.Count > 0)
                {
                    foreach (var raceInfo in _Config.CurrentRaceInfo.raceAvailableRaces)
                    {
                        if (string.Equals(raceInfo["race_num"], _Config.CurrentRaceNo))
                        {
                            //"time": "12:14pm - Race 7",
                            string timeStr = raceInfo["time"]?.ToString() ?? string.Empty; // 例如 "12:14pm - Race 7"
                            string[] parts = timeStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 0)
                            {
                                // parts[0] 的值通常是 "12:14pm" 或 "12:14"
                                _Config.CurrentRaceTimeDesc = parts[0];
                            }
                            textBoxCurrentRaceTime.Text = _Config.CurrentRaceTime;
                            RefreshAutoBettingInfo();
                            _Log.LogInfo($"切换赛场为[{_Config.CurrentRaceTypeDesc}][{_Config.CurrentRaceNo}]");
                        }
                    }
                }
            }
            else
            {
                string[] racesTimes = _Config.SysConfig[$"Horse.{_Config.CurrentRaceType}.RCsTime"].Split(',');
                _Config.CurrentRaceTime = racesTimes[Convert.ToInt32(_Config.CurrentRaceNo) - 1];
                textBoxCurrentRaceTime.Text = _Config.CurrentRaceTime;
                RefreshAutoBettingInfo();
                _Log.LogInfo($"切换赛场为[{_Config.CurrentRaceTypeDesc}][{_Config.CurrentRaceNo}]");
            }
        }
        public void UpdateQueryEATBETInfoStatus(string status)
        {
            if (buttonQueryEATBETInfo.InvokeRequired)
            {
                buttonQueryEATBETInfo.Invoke(new Action<string>(UpdateQueryEATBETInfoStatus), new object[] { status });
            }
            else
            {
                if (string.Equals(status, "扫描已启动"))
                {
                    _EnableBettingInfoRefresh = true;
                    buttonQueryEATBETInfo.Text = "扫描已启动";
                    buttonQueryEATBETInfo.BackColor = Color.Green;
                    // 场景 A：只想单独修改基础等待时间
                    _timerRefreshBetInfo.SetBaseDelay(_Config.AutoTradeInterval);
                    _timerRefreshBetInfo.Start();
                    UpdateScanUI(true);
                }
                else
                {
                    _EnableBettingInfoRefresh = false;
                    buttonQueryEATBETInfo.Text = "扫描已停止";
                    buttonQueryEATBETInfo.BackColor = Color.Red;
                    _timerRefreshBetInfo.Stop();
                    UpdateScanUI(false);
                }
            }
        }
        private async void buttonQueryEATBETInfo_Click(object sender, EventArgs e)
        {
            if (string.Equals(buttonQueryEATBETInfo.Text, "扫描已停止"))
            {
                if (!_EAAccount.IsLogin)
                {
                    MessageBox.Show("请先登陆打水账号,再启动自动扫描！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!_DSAccount.IsLogin)
                {
                    MessageBox.Show("请先登陆读水账号,再启动自动扫描！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(_Config.CurrentRaceType))
                {
                    MessageBox.Show("请先选择赛事！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(_Config.CurrentRaceNo))
                {
                    MessageBox.Show("请先选择场次！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // 🔥 冷启动/断线重连保护：在打开自动扫描开关之前，先用数据库里已有的记录把 TradeStateStore 预热一遍，
                // 避免"进程刚起来、TradeStateStore 还是空的"这段窗口期里，AutoBetProcess 把已经有仓位的组合
                // 误判成"全新组合"而放行重复下单——这正是当初那个根因 bug 的同类风险，必须堵上。
                List<EatBetInfo> historyRows = DBHelper.queryBettingInfoList(_EAAccount.UserCode, _Config.CurrentRaceType, _Config.CurrentRaceDate, _Config.CurrentRaceNo);
                _Config.TradeStateStore.WarmUpFromDatabase(historyRows ?? new List<EatBetInfo>());
                _Log.LogInfo("扫描已启动");
                UpdateQueryEATBETInfoStatus("扫描已启动");
            }
            else
            {
                _Log.LogInfo("扫描已停止");
                UpdateQueryEATBETInfoStatus("扫描已停止");
            }
        }
        private void button2_Click_1(object sender, EventArgs e)
        {
            SaveConfig();
            _Log.LogInfo("信息保存成功");
        }
        /// <summary>
        /// 更新自动下注状态
        /// </summary>
        /// <param name="status"></param>
        public void UpdateAutoBettingStatus(string status)
        {
            // 如果窗体已经被销毁或正在关闭，直接拦截，防止后台线程强行 Invoke 引发崩溃
            if (this.IsDisposed || !this.IsHandleCreated) return;
            // 封装一个纯粹的执行主体
            Action updateAction = () =>
            {
                if (!string.Equals(status, labelAutoBettingStatus.Text))
                {
                    if (string.Equals(status, "已启动"))
                    {
                        labelAutoBettingStatus.Text = "已启动";
                        labelAutoBettingStatus.BackColor = Color.Green;
                        _EnableAutoTrade = true;
                        _Log.LogInfo("已启动");
                    }
                    else
                    {
                        labelAutoBettingStatus.Text = "已停止";
                        labelAutoBettingStatus.BackColor = Color.Red;
                        _EnableAutoTrade = false;
                        _Log.LogInfo("已停止");
                    }
                }
            };
            // 智能判断：如果当前已经在主线程了，直接运行；如果不在主线程，才切回主线程
            if (this.InvokeRequired)
            {
                this.Invoke(updateAction);
            }
            else
            {
                updateAction();
            }
        }
        // 在类级别定义两个变量，用来在刷新和绑定完成之间传递记忆状态
        private string _savedCheckedCombinedKey = string.Empty;
        private int _savedSameKeyMatchIndex = 0;
        /// <summary>
        /// 我的交易数据
        /// </summary>
        public void RefreshMyTradeList(List<BetInfo> BetInfos)
        {
            // 🚀 核心优化：确保后台线程执行（如果有实际数据库操作可写在这里面）
            Task.Run(() =>
            {
                // 假如有耗时的数据库操作，可以在这里处理...
                // 比如：_repository.SaveOrUpdate(BetInfos);
                // 2. 切回主线程刷新 UI 控件
                this.Invoke(new Action(() =>
                {
                    // ==================== 1. 记忆阶段（支持重复行精准定位） ====================
                    _savedCheckedCombinedKey = string.Empty;
                    _savedSameKeyMatchIndex = 0;
                    foreach (DataGridViewRow row in dataGridViewMyTradeList.Rows)
                    {
                        if (Convert.ToBoolean(row.Cells[0].Value) == true)
                        {
                            var originalItem = row.DataBoundItem as BetInfo;
                            if (originalItem != null)
                            {
                                _savedCheckedCombinedKey = originalItem.seq;
                                for (int i = 0; i < row.Index; i++)
                                {
                                    var prevItem = dataGridViewMyTradeList.Rows[i].DataBoundItem as BetInfo;
                                    if (prevItem != null && prevItem.seq == _savedCheckedCombinedKey)
                                    {
                                        _savedSameKeyMatchIndex++;
                                    }
                                }
                            }
                            break;
                        }
                    }
                    // ==================== 2. 我的交易 列表数据刷新 ====================
                    _MyTradeList.RaiseListChangedEvents = false;
                    // 直接清空数据源（注意：Clear 会自动移除所有行，无需在前面单独去设 row.Cells[0].Value = false）
                    _MyTradeList.Clear();
                    // 如果 BetInfos 不为空或 null，则批量加载新数据
                    if (BetInfos != null && BetInfos.Count > 0)
                    {
                        foreach (var item in BetInfos)
                        {
                            if (item != null) //防止集合中夹杂 null 元素
                            {
                                _MyTradeList.Add(item);
                            }
                        }
                    }
                    _MyTradeList.RaiseListChangedEvents = true;
                    // 触发数据绑定更新，此时如果 BetInfos 为 null 或空，表格会自动变为空白
                    _MyTradeList.ResetBindings();
                }));
            });
        }
        /// <summary>/// 自动转场更新/// </summary>
        public void AutoToNextRace()
        {
            if (_Config.AutoToNext)
            {
                _Log.LogInfo("自动转场处理");
                _timerRefreshBetInfoForClosePosition.Stop();
                // ==========================================
                // 💡 盘口及缓存数据清理（防止上一场数据污染下一场）
                // ==========================================
                // 1. 清空上一场的全局交易字典与列表
                _EatBetInfoDict?.Clear();
                _EatBetInfosList?.Clear();
                _AllMyTradesList?.Clear();
                RefreshMyTradeList(null);
                _Config.TradeStateStore.ClearRace(_Config.CurrentRaceNo); // 只清上一场
                _betLocks?.Clear();
                _eatLocks?.Clear();
                _Log.LogInfo("上一场的 LastOdds、comboLocks、本地防重缓存、挂单全集及并发锁已全部清理完成。");
                this.Invoke(new Action(() =>
                {
                    if (comboBoxRaceNo.SelectedIndex < comboBoxRaceNo.Items.Count - 1)
                    {
                        int currentIndex = comboBoxRaceNo.SelectedIndex;
                        int nextIndex = currentIndex + 1;
                        _Log.LogInfo($"自动切换到下一场:[{comboBoxRaceNo.Text}] -> [{comboBoxRaceNo.Items[nextIndex]}]");
                        comboBoxRaceNo.SelectedIndex = nextIndex;
                    }
                    else
                    {
                        _Log.LogInfo("最后一场已结束,停止自动扫描");
                        UpdateQueryEATBETInfoStatus("扫描已停止");
                    }
                    SaveConfig();
                }));
            }
            else
            {
                _Log.LogInfo("自动转场未启动");
            }
        }
        /// <summary>
        /// 定时刷新下注列表，数据来源位本地数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerRefreshAutoBettingInfo_Tick(object sender, EventArgs e)
        {
            RefreshAutoBettingInfo();
        }
        /// <summary>
        /// 刷新下注列表
        /// </summary>
        public void RefreshAutoBettingInfo()
        {
            if (_EAAccount.IsLogin && _EnableBettingInfoRefresh)
            {
                RefreshTradeListToDB();
                if (_Config.AutoRefreshBetting)
                {
                    RefreshBettingInfoDataList();
                }
            }
        }
        /// <summary>
        /// 刷新下注列表到数据库
        /// </summary>
        public void RefreshTradeListToDB()
        {
            var allTrades = _AllMyTradesList;
            if (allTrades == null) return;
            foreach (var trade in allTrades.Where(t => string.Equals(t.status, "CONFIRMED", StringComparison.OrdinalIgnoreCase)))
            {
                string dictKey = ComboTradeState.BuildDictKey(trade.raceNo, trade.type, trade.combo);
                if (!_Config.TradeStateStore.TryGet(dictKey, out var state)) continue;
                if (state.TradeRecordId <= 0)
                {
                    continue;
                }
                // 🔧 不再自己直接写库：统一交给 TradeRecordWriter，跟 ExecuteTrade 的实时落库共用
                // 同一条按 dictKey 分片、严格串行的写入通道，彻底消除"两条通道同时写同一个
                // TradeRecordId"导致的外键竞态（原来靠 ResetTradeRecordId() 硬扛的那个问题）。
                // 失败重试 / 死信日志 / TradeRecordId 失效后的重置，全部下沉到 TradeRecordWriter 里统一处理。
                _tradeRecordWriter.Enqueue(new TradeWriteJob
                {
                    Kind = TradeWriteJobKind.PeriodicRefresh,
                    DictKey = dictKey,
                    BetInfo = trade
                });
            }
        }
        private void checkBoxAutoBetting_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAutoBetting.Checked)
            {
                UpdateAutoBettingStatus("已启动");
            }
            else
            {
                UpdateAutoBettingStatus("已停止");
            }
        }
        public void UpdateScanUI(bool isScanning)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateScanUI(isScanning)));
                return;
            }
            if (isScanning)
            {
                // 开始扫描
                pictureBoxScanProcessing.Visible = true;
                _scanTimer.Start();
            }
            else
            {
                // 停止扫描
                _scanTimer.Stop();
                pictureBoxScanProcessing.Visible = false;
            }
        }
        public void UpdateLoginUI(bool isLogining, PictureBox pictureBoxProcessing, System.Windows.Forms.Timer _timer)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateLoginUI(isLogining, pictureBoxProcessing, _timer)));
                return;
            }
            if (isLogining)
            {
                // 开始扫描
                pictureBoxProcessing.Visible = true;
                _timer.Start();
            }
            else
            {
                // 停止扫描
                _timer.Stop();
                pictureBoxProcessing.Visible = false;
            }
        }
        private void pictureBoxScanProcessing_Paint(object sender, PaintEventArgs e)
        {
            // 如果没有在扫描中，就不画任何东西
            if (!_scanTimer.Enabled) return;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            // 定义圆环的区域
            Rectangle rect = new Rectangle(2, 2, pictureBoxScanProcessing.Width - 6, pictureBoxScanProcessing.Height - 6);
            // 使用深蓝色或你喜欢的颜色画圆弧
            using (Pen pen = new Pen(Color.FromArgb(0, 120, 215), 3))
            {
                // 绘制一段120度的弧线
                e.Graphics.DrawArc(pen, rect, _ProcessingAngle, 120);
            }
        }
        private void pictureBoxLoginProcessing_Paint(object sender, PaintEventArgs e)
        {
            // 如果没有在扫描中，就不画任何东西
            if (!_loginTimer.Enabled) return;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            // 定义圆环的区域
            Rectangle rect = new Rectangle(2, 2, pictureBoxLoginProcessing.Width - 6, pictureBoxLoginProcessing.Height - 6);
            // 使用深蓝色或你喜欢的颜色画圆弧
            using (Pen pen = new Pen(Color.FromArgb(0, 120, 215), 3))
            {
                // 绘制一段120度的弧线
                e.Graphics.DrawArc(pen, rect, _ProcessingAngle, 120);
            }
        }
        private void dataGridViewEATBetInfoList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if (dataGridViewEATBetInfoList.Columns[e.ColumnIndex].Name == "TradeDetail")
            {
                DataGridViewRow row = dataGridViewEATBetInfoList.Rows[e.RowIndex];
                Dictionary<string, string> rowData = new Dictionary<string, string>();
                foreach (DataGridViewCell cell in row.Cells)
                {
                    string colName = dataGridViewEATBetInfoList.Columns[cell.ColumnIndex].Name;
                    rowData[colName] = cell.Value?.ToString() ?? "";
                }
                _BettingTradeDetailForm.LoadData(rowData);
                _BettingTradeDetailForm.Show();
                _BettingTradeDetailForm.BringToFront();
                _BettingTradeDetailForm.Activate();
            }
        }
        private void buttonTradeRecords_Click(object sender, EventArgs e)
        {
            if (!_EAAccount.IsLogin)
            {
                MessageBox.Show("请先登陆打水账号！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Dictionary<string, string> rowData = new Dictionary<string, string>()
            {
                { "serverAddress", _Config.EAServerAddress },
                { "username", _EAAccount.UserCode }
            };
            _SettledHistoryForm.Show();
            _SettledHistoryForm.BringToFront();
            _SettledHistoryForm.Activate();
            _SettledHistoryForm.LoadDataSettledHistoryInfo(_Log, rowData);
        }
        private void dataGridViewEATBetInfoList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // 确保行有效且不是新行
            if (e.RowIndex < 0 || e.RowIndex >= dataGridViewEATBetInfoList.Rows.Count) return;
            DataGridViewRow row = dataGridViewEATBetInfoList.Rows[e.RowIndex];
            string columnName = dataGridViewEATBetInfoList.Columns[e.ColumnIndex].Name;
            // 获取两个作实（Executed）字段的值
            decimal betExecVal = 0;
            decimal eatExecVal = 0;
            bool hasBetExec = row.Cells["betExecutedAmount"].Value != null &&
                              decimal.TryParse(row.Cells["betExecutedAmount"].Value.ToString(), out betExecVal);
            bool hasEatExec = row.Cells["eatExecutedAmount"].Value != null &&
                              decimal.TryParse(row.Cells["eatExecutedAmount"].Value.ToString(), out eatExecVal);
            // 检查是否满足：两者相等且都大于 0
            bool isMatchedAndValid = hasBetExec && hasEatExec && betExecVal == eatExecVal && betExecVal > 0;
            // 1. 处理吃作实列 (eatExecutedAmount)
            if (columnName == "eatExecutedAmount")
            {
                if (isMatchedAndValid)
                {
                    e.CellStyle.BackColor = System.Drawing.Color.LightGreen; // 浅绿色
                }
                else if (eatExecVal > 0)
                {
                    e.CellStyle.BackColor = System.Drawing.Color.Yellow; // 不满足条件但大于0时保持原有的黄色
                }
            }
            // 2. 处理赌作实列 (betExecutedAmount)
            else if (columnName == "betExecutedAmount")
            {
                if (isMatchedAndValid)
                {
                    e.CellStyle.BackColor = System.Drawing.Color.LightGreen; // 浅绿色
                }
                else if (betExecVal > 0)
                {
                    e.CellStyle.BackColor = System.Drawing.Color.LightBlue; // 不满足条件但大于0时保持原有的蓝色
                }
            }
            // 3. 处理等待列 (Pending)，维持你原本的逻辑
            else if (columnName == "eatPendingAmount")
            {
                if (decimal.TryParse(e.Value?.ToString(), out decimal val) && val > 0)
                {
                    e.CellStyle.BackColor = System.Drawing.Color.Yellow;
                }
            }
            else if (columnName == "betPendingAmount")
            {
                if (decimal.TryParse(e.Value?.ToString(), out decimal val) && val > 0)
                {
                    e.CellStyle.BackColor = System.Drawing.Color.LightBlue;
                }
            }
        }
        private void pictureBoxDSLoginProcessing_Paint(object sender, PaintEventArgs e)
        {
            // 如果没有在扫描中，就不画任何东西
            if (!_DSLoginTimer.Enabled) return;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            // 定义圆环的区域
            Rectangle rect = new Rectangle(2, 2, pictureBoxDSLoginProcessing.Width - 6, pictureBoxDSLoginProcessing.Height - 6);
            // 使用深蓝色或你喜欢的颜色画圆弧
            using (Pen pen = new Pen(Color.FromArgb(0, 120, 215), 3))
            {
                // 绘制一段120度的弧线
                e.Graphics.DrawArc(pen, rect, _ProcessingAngle, 120);
            }
        }
        private void buttonAutoBettingStatus_Click(object sender, EventArgs e)
        {
            if (!_EAAccount.IsLogin)
            {
                MessageBox.Show("请先登陆打水账号！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(_Config.CurrentRaceDate) || comboBoxRaceType.SelectedIndex == -1)
            {
                MessageBox.Show("请先选择赛事！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Dictionary<string, string> rowData = new Dictionary<string, string>()
            {
                { "serverAddress", _Config.EAServerAddress },
                { "username", _EAAccount.UserCode },
                { "raceType", _Config.CurrentRaceType },
                { "raceDate", _Config.CurrentRaceDate },
                { "raceNo", _Config.CurrentRaceNo }
            };
            _AutoBettingStatusForm.LoadData(rowData);
            _AutoBettingStatusForm.Show();
            _AutoBettingStatusForm.BringToFront();
            _AutoBettingStatusForm.Activate();
        }
        private void buttonTestBet_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "是否手动下注",
                "提示",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning
            );
            if (result == DialogResult.OK)
            {
                string serverProcessTime = "0ms";
                IDictionary<string, List<BetInfo>> BetBatInfos = HTTPHelper.queryMarket(_Config.EAServerAddress, _EAAccount.UserCode, _Config.CurrentRaceDate, _Config.CurrentRaceType, _Config.CurrentRaceNo, out serverProcessTime);
                _Log.LogInfo($"[性能监控][buttonTestBet][queryMarket] 后端服务耗时: {serverProcessTime}");
                List<BetInfo> BeInfos = new List<BetInfo>();
                if (BetBatInfos != null && BetBatInfos.Count > 0)
                {
                    if (BetBatInfos.ContainsKey("Q_BETBetInfos"))
                    {
                        List<BetInfo> Q_BETBetInfos = BetBatInfos["Q_BETBetInfos"];
                        if (Q_BETBetInfos != null && Q_BETBetInfos.Count > 0)
                        {
                            BeInfos.Add(Q_BETBetInfos[0]);
                        }
                    }
                    if (BetBatInfos.ContainsKey("Q_EATBetInfos"))
                    {
                        List<BetInfo> Q_EATBetInfos = BetBatInfos["Q_EATBetInfos"];
                        if (Q_EATBetInfos != null && Q_EATBetInfos.Count > 0)
                        {
                            BeInfos.Add(Q_EATBetInfos[0]);
                        }
                    }
                    if (BetBatInfos.ContainsKey("QP_BETBetInfos"))
                    {
                        List<BetInfo> QP_BETBetInfos = BetBatInfos["QP_BETBetInfos"];
                        if (QP_BETBetInfos != null && QP_BETBetInfos.Count > 0)
                        {
                            BeInfos.Add(QP_BETBetInfos[0]);
                        }
                    }
                    if (BetBatInfos.ContainsKey("QP_EATBetInfos"))
                    {
                        List<BetInfo> QP_EATBetInfos = BetBatInfos["QP_EATBetInfos"];
                        if (QP_EATBetInfos != null && QP_EATBetInfos.Count > 0)
                        {
                            BeInfos.Add(QP_EATBetInfos[0]);
                        }
                    }
                    if (BeInfos.Count > 0)
                    {
                        Random random = new Random(); // 在循环外部初始化随机数生成器
                        foreach (var BetInfo in BeInfos)
                        {
                            ExecuteTrade(null, BeInfos[0], "M", "N");
                            // 每次循环结束时，在 100 毫秒到 1000 毫秒之间随机暂停
                            int delayMs = random.Next(500, 1001); // 100 到 1000（包含100，不包含1001）
                            Thread.Sleep(delayMs);
                        }
                    }
                }
            }
            else if (result == DialogResult.Cancel)
            {
                // 用户点击【取消】
                // 停止当前操作
                return;
            }
        }
        private void buttonTestRefresh_Click(object sender, EventArgs e)
        {
            // 1. 在后台线程进行网络请求（耗时操作）
            //List<BetInfo> BetInfos = HTTPHelper.queryMyTrade(_Config.EAServerAddress, _EAAccount.UserCode, _Config.CurrentRaceDate, _Config.CurrentRaceType, _Config.CurrentRaceNo);
            //RefreshMyTradeList(BetInfos);
            // 1. 在后台线程进行网络请求（耗时操作）
            //List<BetInfo> BetInfos = HTTPHelper.queryMyTrade(_Config.EAServerAddress, _EAAccount.UserCode, _Config.CurrentRaceDate, _Config.CurrentRaceType, _Config.CurrentRaceNo);
            //RefreshMyTradeList(BetInfos);
        }
        private void buttonManualAutoBetting_Click(object sender, EventArgs e)
        {
            RefreshBetInfoDataList();
        }
        private void checkBoxAutoDeletePendingOrder_CheckedChanged(object sender, EventArgs e)
        {
            _Config.AutoDeletePendingOrder = checkBoxAutoDeletePendingOrder.Checked;
        }
        private void checkBoxAutoClosePosition_CheckedChanged(object sender, EventArgs e)
        {
            _Config.AutoClosePosition = checkBoxAutoClosePosition.Checked;
        }
        private void buttonOpenQSelector_Click(object sender, EventArgs e)
        {
            // 传入当前文本框里已有的内容，以及当前赛事的最大马匹数（比如 14）
            using (var selectorForm = new ComboSelectorForm(textBoxQCombos.Text, "Q", maxHorseNo: 14))
            {
                if (selectorForm.ShowDialog(this) == DialogResult.OK)
                {
                    // 用户点击了确定，将子窗体选择好的规范字符串赋回给主界面的输入框
                    textBoxQCombos.Text = selectorForm.SelectedCombosResult;
                }
            }
        }
        private void buttonOpenQPSelector_Click(object sender, EventArgs e)
        {
            // 传入当前文本框里已有的内容，以及当前赛事的最大马匹数（比如 14）
            using (var selectorForm = new ComboSelectorForm(textBoxQPCombos.Text, "QP", maxHorseNo: 14))
            {
                if (selectorForm.ShowDialog(this) == DialogResult.OK)
                {
                    // 用户点击了确定，将子窗体选择好的规范字符串赋回给主界面的输入框
                    textBoxQPCombos.Text = selectorForm.SelectedCombosResult;
                }
            }
        }
        /// <summary>
        /// 刷新下注列表数据，数据来源为本地数据库。
        ///
        /// 🔧 已回退"改读内存 _EatBetInfosList"的调整：实测发现只要一个组合还没有产生任何
        /// "吃"的成交（比如截图里的场景——3 个组合都只提交了 BET、吃笔数=0），
        /// Utils.Utils.GetBatBetInfo(...) 生成的 _EatBetInfosList/_EatBetInfoDict 里就不会包含
        /// 这些组合（或者字段填充不满足表格列绑定的预期），导致表格整体空白——但数据库里
        /// （及 TradeStateStore 内存状态里）其实已经正确记录了这些下注。
        /// 由于目前没有 Utils.Utils.GetBatBetInfo 的源码，无法确认它具体按什么口径过滤/构造
        /// _EatBetInfosList，为避免继续猜测导致界面再次出问题，这里先改回从数据库查询展示，
        /// 恢复到已验证可用的状态。如果之后要重新尝试"读内存以避免落库延迟"，需要先拿到
        /// Utils.Utils.GetBatBetInfo（或 EatBetInfo 类定义）的源码，确认它是否遗漏了"只有赌、
        /// 还没吃"的组合，再按 ComboTradeState（这个是全量、权威的每组合状态）重新构造展示用的
        /// EatBetInfo 列表，而不是直接依赖 _EatBetInfosList。
        /// </summary>
        private void RefreshBettingInfoDataList()
        {
            if (_EAAccount.IsLogin && _EnableBettingInfoRefresh)
            {
                _BettingList.RaiseListChangedEvents = false;
                _BettingList.Clear();
                List<EatBetInfo> bettingInfos = DBHelper.queryBettingInfoList(_EAAccount.UserCode, _Config.CurrentRaceType, _Config.CurrentRaceDate, _Config.CurrentRaceNo);
                if (bettingInfos != null && bettingInfos.Count > 0)
                {
                    foreach (var item in bettingInfos)
                    {
                        _BettingList.Add(item);
                    }
                }
                _BettingList.RaiseListChangedEvents = true;
                _BettingList.ResetBindings();
            }
        }
        private void buttonRefreshBettingStatus_Click(object sender, EventArgs e)
        {
            RefreshBettingInfoDataList();
        }
        private void checkBoxAutoRefreshBetting_CheckedChanged(object sender, EventArgs e)
        {
            _Config.AutoRefreshBetting = checkBoxAutoRefreshBetting.Checked;
        }
    }
}
