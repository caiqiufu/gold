using AutoHorseRace.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text.RegularExpressions;
// ============================================================================
// V20260918_CODE_REVIEW_FIXES：本次代码走查后应用的修改，供后续维护对照：
// 1. 三个登陆/扫描动画不再共用同一个 _ProcessingAngle 字段，避免多个定时器同时
//    启用时互相干扰转速（拆分为 _scanProcessingAngle / _loginProcessingAngle /
//    _dsLoginProcessingAngle 三个独立字段）。
// 2. QueryEATBETInfoDataForClosePositionAsync 不再重复实现一遍窗口时间计算，
//    改为直接调用 TryGetTradeTimeContextForClose，避免两处逻辑将来改一处漏一处。
// 3. UpdateAccountBalanceInfo 由 async void 改为 async Task 并在
//    QueryBalanceDataAsync 中 await，同时给余额字段解析加了 try/catch，
//    避免响应结构异常时在 async void 里抛出未处理异常导致进程崩溃。
// 4. ExecuteTrade 的 dynamic 参数改回强类型 EatBetInfo/BetInfo，去掉 DLR 调用开销
//    并恢复编译期类型检查（所有调用方本来传入的就是这两个具体类型）。
// 5. QueryAndApplyMyTradeSnapshotAsync 里的调试日志改为先判断
//    _logger.IsDebugEnabled 再做 JsonConvert.SerializeObject(Formatting.Indented)，
//    避免 Debug 级别关闭时仍白白执行大对象的格式化序列化（每轮轮询都会执行）。
// 6. [EAT-SEND] 常规下单日志由 _logger.Warn 降级为 _logger.Info，避免正常业务
//    事件淹没 Warn/Error 监控信号。
// 7. AutoBetting / ClosePositionByDeadline 里的 Parallel.ForEach 增加
//    MaxDegreeOfParallelism 上限（MaxComboParallelism 常量），避免 combo 数量变多后
//    并发打满线程池、影响其它定时器任务（HTTP 均为阻塞调用）的调度。
// 8. V20260918_STOP_SCAN_ON_DISCONNECT：打水(EA)/读水(DS)账号在余额轮询中被判定为
//    自动断链时（ShowAccountDisconnectedPrompt），如果此时自动扫描仍处于"已启动"状态，
//    会自动调用 UpdateQueryEATBETInfoStatus("扫描已停止") 显式停掉扫描。之前断链只会
//    弹窗提示、把账号 IsLogin 置为 false，但扫描定时器和按钮状态都不会跟着变——扫描
//    按钮表面还显示"已启动"，实际上每一轮都会因为 IsLogin=false 在最前面直接 return，
//    等于空转，容易让人误以为系统仍在正常工作而没有及时去重新登陆。
// 9. V20260918_DS_MULTI_ACCOUNT_DISCONNECT：在第 8 条基础上，把读水侧的自动断链检测
//    从"只查 _DSAccounts 里第一个账户"扩展为"遍历所有已登录的读水账户逐个查询余额"
//    （QueryBalanceDataAsync），任意一个账户查询失败/异常都会独立触发它自己的断链判定
//    与停止扫描，不会被其它账户仍然在线掩盖。相应地：
//      - UpdateAccountBalanceInfo 新增 dsAccount 参数，可对指定的某个读水账户查询；
//      - "已提示过断链"标记由单个 bool（_dsDisconnectNotified）改成按 UserCode 去重的
//        HashSet（_dsDisconnectNotifiedUserCodes），每个账户独立提示、独立恢复；
//      - HandleAccountDisconnected / ShowAccountDisconnectedPrompt / ResetAccountDisconnectedNotifyFlag
//        都新增 account 参数，标明具体是哪个账户；
//      - 界面上仍然只有一组共享的"读水余额"控件，只有断链的正好是当前主账户
//        （_DSAccount，即 _DSAccounts[0]）时才刷新这组控件/把登陆按钮打回"未登陆"，
//        其它账户断链只记录在它自己的 Account 字段上、只触发停止扫描，不影响共享 UI。
// 10. V20260918_SAVECONFIG_DS_MULTI_ACCOUNT_FIX：修复一个真实复现的 BUG——SaveConfig()
//     之前会把 textBoxDSAccountCode/Password/Pin 的原始文本（多账户时是逗号分隔整串，
//     例如 "rh243,mfhk299"）直接写进 _DSAccount.UserCode/Password/Pin；但 _DSAccount 登陆
//     后其实和 _DSAccounts[0] 是同一个对象引用，这一写会把该账户 UserCode 从 "rh243"
//     污染成 "rh243,mfhk299"，导致紧接着 BuildDSAccountsFromInput 按 UserCode 精确匹配
//     复用旧对象时找不到 "rh243"，只能新建一个 IsLogin=false 的全新对象顶替它——账户
//     明明已经登陆（例如 buttonDSLogin_Click 刚登陆成功），却在下一次 SaveConfig（比如
//     切换赛场就会触发）之后被"重置"成未登陆，实测表现为 QueryMarketRace 日志里出现
//     "跳过未登录账户"、且只影响多账户里的第一个（因为只有 _DSAccount 这一个引用被
//     污染，后面的账户对象没被碰到）。修复后，多账户原始文本只写入持久化专用的
//     _MyConfig.DSAccount（不再污染 _DSAccount 本体），并在重新解析 _DSAccounts 后把
//     _DSAccount 显式指向 _DSAccounts[0]，保持二者引用一致。
// 以上均为在不改变原有业务语义前提下的走查修复，其余大量并发/幂等相关注释与逻辑
// （TradeStateStore 原子预占、TradeRecordWriter 串行落库等）均保持不变。
// ============================================================================
namespace AutoHorseRace
{
    public partial class EAForm : Form
    {
        //于后端最短timeout为3秒,前端请求必须大于10秒
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
                baseDelaySeconds: 1,
                randomMinSeconds: 1,
                randomMaxSeconds: 3
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
        // V20260918_CODE_REVIEW_FIXES(1)：原来三个动画共用一个 _ProcessingAngle 字段，
        // 如果扫描动画和登陆动画同时启用，会互相抢同一个角度值，导致转速/相位都不对。
        // 拆分成三个独立字段，每个定时器只驱动自己的角度。
        private float _scanProcessingAngle = 0; // 扫描动画旋转角度
        private float _loginProcessingAngle = 0; // 打水登陆动画旋转角度
        private float _dsLoginProcessingAngle = 0; // 读水登陆动画旋转角度
        private System.Windows.Forms.Timer _scanTimer = new System.Windows.Forms.Timer(); // 负责定时触发重绘
        private System.Windows.Forms.Timer _loginTimer = new System.Windows.Forms.Timer(); // 负责定时触发重绘
        private System.Windows.Forms.Timer _DSLoginTimer = new System.Windows.Forms.Timer(); // 负责定时触发重绘
        private void InitProcessingAnimation()
        {
            _scanTimer.Interval = 30; // 刷新频率，越小越平滑
            _scanTimer.Tick += (s, e) =>
            {
                _scanProcessingAngle = (_scanProcessingAngle + 15) % 360; // 每次旋转15度
                pictureBoxScanProcessing.Invalidate();    // 强制触发 Paint 事件进行重绘
            };
            // 设置 PictureBox 背景为透明，防止盖住界面
            pictureBoxScanProcessing.BackColor = Color.Transparent;
            _loginTimer.Interval = 30; // 刷新频率，越小越平滑
            _loginTimer.Tick += (s, e) =>
            {
                _loginProcessingAngle = (_loginProcessingAngle + 15) % 360; // 每次旋转15度
                pictureBoxLoginProcessing.Invalidate();    // 强制触发 Paint 事件进行重绘
            };
            // 设置 PictureBox 背景为透明，防止盖住界面
            pictureBoxLoginProcessing.BackColor = Color.Transparent;
            _DSLoginTimer.Interval = 30; // 刷新频率，越小越平滑
            _DSLoginTimer.Tick += (s, e) =>
            {
                _dsLoginProcessingAngle = (_dsLoginProcessingAngle + 15) % 360; // 每次旋转15度
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
                // V20260918_CODE_REVIEW_FIXES(3)：UpdateAccountBalanceInfo 改为 async Task 后
                // 这里必须 await，否则内部的异常/耗时就完全脱离了这个方法的生命周期管理。
                await UpdateAccountBalanceInfo("EA");
            }

            // V20260918_DS_MULTI_ACCOUNT_DISCONNECT：之前这里只查第一个读水账户（_DSAccount），
            // 其余读水账户即使真的断链了也检测不到、更不会触发停止扫描。现在改成遍历
            // _DSAccounts 里所有【当前已登录】的账户，逐个查询余额；任何一个账户查询失败/
            // 异常都会独立触发它自己的断链判定（见 HandleAccountDisconnected(accountType, account)），
            // 不会被其它账户的成功状态"掩盖"。
            var dsAccountsToCheck = (_DSAccounts != null && _DSAccounts.Count > 0)
                ? _DSAccounts.Where(a => a != null && a.IsLogin).ToList()
                : (_DSAccount != null && _DSAccount.IsLogin ? new List<Account> { _DSAccount } : new List<Account>());

            bool isFirstDS = true;
            foreach (var dsAccount in dsAccountsToCheck)
            {
                // 扫描一旦被（前面某个账户触发的自动断链）停掉，后面排队的账户就没必要继续查了。
                if (!_EnableBettingInfoRefresh) break;
                // 🔧 按账户自己的服务器地址判断，不再用 _Config.DSServerAddress 整体比较
                // （现在每个 DS 账户可能各自打不同的服务器）。
                if (string.Equals(dsAccount.BrokerServer, _Config.EAServerAddress)) continue;
                // 错开请求，避免多个账号在同一瞬间并发打满网络或服务器：
                // 第一个读水账户沿用原来相对打水账户 5 秒的错峰，之后每个账户之间再错开 2 秒。
                await Task.Delay(isFirstDS ? 5000 : 2000);
                isFirstDS = false;
                await UpdateAccountBalanceInfo("DS", dsAccount);
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
        /// 保护 _tradeRecordWriter 整体替换（Stop+重新创建）与并发 Enqueue 之间的竞态：
        /// AutoToNextRace() 转场时会先 Stop() 排干旧队列、再 new 一个新的 TradeRecordWriter，
        /// 如果这期间恰好有其它线程（ExecuteTrade / RefreshTradeListToDB）正在调用 Enqueue，
        /// 旧队列的 Channel 已经 Complete()，会直接抛 ChannelClosedException，导致这笔落库请求
        /// 静默丢失（不会重试，也不会有死信日志）。所有对 _tradeRecordWriter 的读/写（Enqueue、
        /// Stop+重建）统一经过这把锁，把"排干旧队列+切换到新队列"做成一个不可被 Enqueue 打断的
        /// 原子操作，Enqueue 侧顶多是短暂等锁，不会丢数据。
        /// </summary>
        private readonly object _tradeRecordWriterLock = new object();

        /// <summary>
        /// 统一创建 TradeRecordWriter 的工厂方法，构造参数与原来内联 new 时完全一致，
        /// 提炼出来是为了在 AutoToNextRace() 转场时可以按同样的方式重新创建一个新实例。
        /// </summary>
        private TradeRecordWriter CreateTradeRecordWriter()
        {
            return new TradeRecordWriter(
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
        /// 所有落库 Enqueue 的统一入口：加锁只是为了跟 AutoToNextRace() 里
        /// "Stop 旧队列 + 创建新队列" 那一小段互斥，锁内只做一次 Enqueue（纯内存操作，
        /// 几乎不耗时），不会造成明显阻塞；转场时最多让 Enqueue 侧等待到新队列创建完毕。
        /// </summary>
        private void EnqueueTradeWrite(TradeWriteJob job)
        {
            lock (_tradeRecordWriterLock)
            {
                _tradeRecordWriter?.Enqueue(job);
            }
        }

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
                    // V20260918_CODE_REVIEW_FIXES(5)：这三处调试日志原来无条件对可能较大的对象做
                    // Formatting.Indented 序列化，字符串插值会在到达 _logger.Debug 之前就先把整个
                    // JSON 拼好，即使 Debug 级别被关闭也白白执行了这个开销，而且这个方法每一轮轮询
                    // （最短 1~15 秒一次）都会走到。加上 IsDebugEnabled 判断，Debug 关闭时直接跳过序列化。
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.Debug($"[queryMyTrade Result]:{JsonConvert.SerializeObject(BetBatInfos, Formatting.Indented)}");
                    }
                    var (allMyTrades, eatBetInfosList, eatBetInfoDict) = Utils.Utils.GetBatBetInfo(BetBatInfos);
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.Debug($"[QueryMyTradeInfListDataAsync allMyTrades Result]:{JsonConvert.SerializeObject(allMyTrades, Formatting.Indented)}");
                        _logger.Debug($"[QueryMyTradeInfListDataAsync eatBetInfoDict Result]:{JsonConvert.SerializeObject(eatBetInfoDict, Formatting.Indented)}");
                    }
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

        // V20260918_CODE_REVIEW_FIXES(7)：Parallel.ForEach 处理各 combo 时的最大并发度上限。
        // 原来不限并发，combo 数量一旦变多，会同时开出大量线程池线程去跑阻塞式 HTTP 请求
        // （ExecuteTrade -> HTTPHelper.submitOrder/singleAutoTrade），挤压其它定时器任务的调度。
        // 这里给一个保守的上限；如需调优可以后续挪到 Config 里做成可配置项。
        private const int MaxComboParallelism = 8;

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
            // V20260918_CODE_REVIEW_FIXES(2)：
            // 原来这里独立重写了一遍"解析比赛时间 -> 构造完整 DateTime -> 跨天修正 ->
            // 计算强平窗口"的整套逻辑，跟下面 ClosePositionsByDeadlineAsync 里调用的
            // TryGetTradeTimeContextForClose 是同一件事的两份实现，容易改一处忘了改
            // 另一处导致两条通道判断的窗口不一致。这里直接复用同一个方法作为唯一权威来源。
            // -------------------------------------------------------------------------
            if (!TryGetTradeTimeContextForClose(now, out DateTime raceDateTime, out DateTime normalBetEndTime, out DateTime closeStartTime, out DateTime closeEndTime))
            {
                _logger.Error(
                    $"[强制平仓] 解析/构造比赛时间失败，CurrentRaceTime='{_Config.CurrentRaceTime}'");
                return;
            }

            // -------------------------------------------------------------------------
            // 5. 输出时间诊断日志
            //
            // 这个日志非常重要。
            // 如果以后再次出现"强平没有执行"的问题，可以直接从日志判断
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
            _Log.LogInfo($"[强平]强平前拉取最新交易数据");
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
            // 加锁避免和其它线程正在进行的 Enqueue 竞态（详见 _tradeRecordWriterLock 声明处注释）。
            lock (_tradeRecordWriterLock)
            {
                _tradeRecordWriter?.Stop(TimeSpan.FromSeconds(5));
            }
            if (string.Equals(buttonAccountLogin.Text, "已登陆"))
            {
                HTTPHelper.logout(_Config.EAServerAddress, _EAAccount.UserCode);
            }
            // 🆕 遍历所有 DS 账户逐个登出；每个账户可能配了不同的服务器地址（account.BrokerServer，
            // 来自 Config.json 中逗号分隔的 DSServerAddress 列表），不再统一用 _Config.DSServerAddress 比较/登出。
            if (string.Equals(buttonDSLogin.Text, "已登陆"))
            {
                var accountsToLogout = (_DSAccounts != null && _DSAccounts.Count > 0)
                    ? _DSAccounts
                    : (_DSAccount != null ? new List<Account> { _DSAccount } : new List<Account>());
                foreach (var account in accountsToLogout.Where(a => a != null && a.IsLogin))
                {
                    if (!string.Equals(account.BrokerServer, _Config.EAServerAddress))
                    {
                        HTTPHelper.logout(account.BrokerServer, account.UserCode);
                    }
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
        /// 读水账户（兼容旧代码：始终指向 _DSAccounts 的第一个元素）
        /// </summary>
        public Account _DSAccount;
        /// <summary>
        /// 🆕 多路读水账户列表：由 textBoxDSAccountCode/Password/Pin 三个文本框 与
        /// _Config.DSServerAddress（后台 Config.json 中维护，逗号分隔，不经界面输入）用英文逗号分隔，
        /// 按下标一一对应解析而来（第 i 个账号配第 i 个密码/安码/服务器地址）。
        /// 每个账户可以打不同的服务器，因为服务端不支持同一地址登陆多个账户。
        /// 用于 RefreshBetInfoDataList 的 race 查询（谁先返回非空数据就用谁）。
        /// </summary>
        public List<Account> _DSAccounts = new List<Account>();
        /// <summary>
        /// 🆕 分析串行化专用锁：无论查询侧是几个 DS 账户，AutoBetting（含内部
        /// RefreshMyTradeSnapshot）任意时刻只允许一个实例在跑。
        /// </summary>
        private readonly object _autoBettingLock = new object();
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
            _tradeRecordWriter = CreateTradeRecordWriter();
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
            //读水账号密码（🆕 多账户以英文逗号分隔的原始文本，直接回显到文本框；
            // 服务器地址不经界面输入，只在 Config.json 的 _Config.DSServerAddress 里维护）
            textBoxDSAccountCode.Text = _MyConfig.DSAccount.UserCode;
            textBoxDSAccountPassword.Text = _MyConfig.DSAccount.Password;
            textBoxDSAccountPin.Text = _MyConfig.DSAccount.Pin;
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
        /// 🆕 把逗号分隔的文本拆分并去除首尾空白，空字符串直接返回空列表。
        /// </summary>
        private static List<string> SplitTrim(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new List<string>();
            return text.Split(',').Select(s => s.Trim()).ToList();
        }

        /// <summary>
        /// 🆕 把 textBoxDSAccountCode/Password/Pin 三个文本框 + serverAddressText（即
        /// _Config.DSServerAddress，只在 Config.json 里维护，不经界面输入）的逗号分隔内容，
        /// 按下标一一对应解析成多个 Account，每个账户各自的 BrokerServer 来自
        /// serverAddressText 里对应下标的地址，不再共用同一个服务器地址。
        /// 服务端不支持同一地址登陆多个账户，若解析出重复地址会记录警告（仍会继续解析，
        /// 但这些账户实际登陆时大概率会互相顶掉，由登陆结果的成功/失败明细自然暴露）。
        /// 若能在当前 _DSAccounts 里找到 UserCode 完全相同的既有对象，复用它而不是新建，
        /// 这样能保留该账户已有的 IsLogin 状态，避免每次重新解析都把在线状态清零。
        /// </summary>
        private List<Account> BuildDSAccountsFromInput(string codeText, string passwordText, string pinText, string serverAddressText)
        {
            var codes = SplitTrim(codeText);
            var passwords = SplitTrim(passwordText);
            var pins = SplitTrim(pinText);
            var servers = SplitTrim(serverAddressText);

            if (codes.Count != passwords.Count || codes.Count != pins.Count || codes.Count != servers.Count)
            {
                _Log?.LogInfo($"[读水账户解析] 账号[{codes.Count}个]、密码[{passwords.Count}个]、安码[{pins.Count}个]、" +
                             $"服务器地址[{servers.Count}个]数量不一致，缺失位置将以空字符串填充，" +
                             $"对应账户登陆必然失败，请检查 Config.json 中 DSServerAddress 是否与账号数量一致（逗号分隔）。");
            }

            int n = Math.Max(codes.Count, Math.Max(passwords.Count, Math.Max(pins.Count, servers.Count)));
            var result = new List<Account>();
            for (int i = 0; i < n; i++)
            {
                string code = i < codes.Count ? codes[i] : "";
                if (string.IsNullOrWhiteSpace(code)) continue; // 跳过空账号位（多余逗号等）

                string pwd = i < passwords.Count ? passwords[i] : "";
                string pin = i < pins.Count ? pins[i] : "";
                string server = i < servers.Count ? servers[i] : "";

                var existing = _DSAccounts?.FirstOrDefault(a => a != null && string.Equals(a.UserCode, code, StringComparison.Ordinal));
                var account = existing ?? new Account();
                account.UserCode = code;
                account.Password = pwd;
                account.Pin = pin;
                account.BrokerName = "长城";
                account.BrokerCode = "CC";
                account.BrokerServer = server;
                result.Add(account);
            }

            // 🆕 服务端不支持同一地址登陆多个账户，提前检测重复地址并警告。
            var dupServerGroups = result.Where(a => !string.IsNullOrWhiteSpace(a.BrokerServer))
                .GroupBy(a => a.BrokerServer)
                .Where(g => g.Count() > 1)
                .ToList();
            foreach (var g in dupServerGroups)
            {
                _Log?.LogInfo($"[读水账户解析] ⚠️ 服务器地址[{g.Key}]被多个账户共用: [{string.Join(", ", g.Select(a => a.UserCode))}]，" +
                             $"服务端不支持同一地址多账户同时在线，这些账户登陆时大概率会互相顶掉，请在 Config.json 的 DSServerAddress 中为每个账户配置不同的服务器地址。");
            }

            return result;
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
            // 🆕 从已保存的逗号分隔文本 + _Config.DSServerAddress（Config.json 中维护）
            // 预解析出账户列表（此时全部未登录）。
            if (_DSAccount != null && _Config != null)
            {
                _DSAccounts = BuildDSAccountsFromInput(_MyConfig.DSAccount.UserCode, _MyConfig.DSAccount.Password, _MyConfig.DSAccount.Pin, _Config.DSServerAddress);
                if (_DSAccounts.Count == 0)
                {
                    // 没解析出任何账户时，退化为只含 _DSAccount 自身，避免 _DSAccounts 长期为空列表
                    _DSAccount.BrokerServer = _Config.DSServerAddress;
                    _DSAccounts = new List<Account> { _DSAccount };
                }
                // 保持 _DSAccount 指向解析结果的第一个账户，兼容其它仍引用单个 _DSAccount 的地方
                _DSAccount = _DSAccounts[0];
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
            // 那么它的赛马日期（RaceDate）应该属于"昨天"，而不是今天。
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
                // V20260918_SAVECONFIG_DS_MULTI_ACCOUNT_FIX：这里原来会把 textBoxDSAccountCode/
                // Password/Pin 的原始文本（多账户时是逗号分隔的整串，例如 "rh243,mfhk299"）直接
                // 写进 _DSAccount.UserCode/Password/Pin。但 _DSAccount 登陆后其实和 _DSAccounts[0]
                // 是同一个对象引用（代表一个真实的、单个账户），把整串逗号文本塞进它的 UserCode，
                // 会把这个账户的 UserCode 从 "rh243" 污染成 "rh243,mfhk299"。紧接着下面
                // BuildDSAccountsFromInput 按 UserCode 精确匹配来复用旧账户对象时，"rh243" 就再也
                // 匹配不上被污染成 "rh243,mfhk299" 的旧对象，只能新建一个全新的、IsLogin=false 的
                // Account 顶替它——账户明明已经登陆，却在下一次保存配置（比如切换赛场会触发
                // SaveConfig）之后被"重置"成未登陆，QueryMarketRace 的"跳过未登录账户"、
                // 余额轮询的自动断链判定等所有依赖 IsLogin 的逻辑都会误判它已经掉线。
                // 修复：多账户的原始逗号文本只写进持久化专用的 _MyConfig.DSAccount（和
                // buttonDSLogin_Click 里的做法保持一致），不再写进代表单个账户的 _DSAccount；
                // BuildDSAccountsFromInput 完成后把 _DSAccount 重新指向 _DSAccounts[0]，避免它
                // 变成一个已经不在 _DSAccounts 列表里的"孤儿对象"（否则后续"是否为主账户"之类
                // 的 ReferenceEquals(targetAccount, _DSAccount) 判断也会跟着失真）。
                if (_MyConfig.DSAccount == null)
                {
                    _MyConfig.DSAccount = new Account();
                }
                _MyConfig.DSAccount.UserCode = textBoxDSAccountCode.Text;
                _MyConfig.DSAccount.Password = textBoxDSAccountPassword.Text;
                _MyConfig.DSAccount.Pin = textBoxDSAccountPin.Text;

                if (_DSAccount == null)
                {
                    _DSAccount = new Account();
                }
                // 🆕 保存配置时同步重新解析多账户列表（服务器地址来自 _Config.DSServerAddress，
                // 不经界面输入，需直接编辑 Config.json；按 UserCode 复用旧对象，不影响已登录状态）
                _DSAccounts = BuildDSAccountsFromInput(textBoxDSAccountCode.Text, textBoxDSAccountPassword.Text, textBoxDSAccountPin.Text, _Config?.DSServerAddress);
                if (_DSAccounts.Count == 0)
                {
                    if (_Config != null) _DSAccount.BrokerServer = _Config.DSServerAddress;
                    _DSAccounts = new List<Account> { _DSAccount };
                }
                // 保持 _DSAccount 指向解析结果的第一个账户，兼容其它仍引用单个 _DSAccount 的地方。
                _DSAccount = _DSAccounts[0];
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
                    //注意：_Config.DSServerAddress（逗号分隔的多个服务器地址）不经界面输入，
                    //仍按原有内容原样写回 Config.json，如需修改需直接编辑该配置文件。
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
        // 🔧 EA/DS 各自独立的"已提示过断链"标记，避免账号断线期间每次 _timerRefreshBalance
        // 轮询都弹一次窗，把用户淹没在重复弹窗里。
        // V20260918_DS_MULTI_ACCOUNT_DISCONNECT：读水侧从单个 bool 改成按 UserCode 记录的
        // 集合，因为现在要对 _DSAccounts 里的每个账户分别做"是否已经提示过断链"去重，
        // 不能再用一个全局 bool 笼统代表"读水断链了"。
        private bool _eaDisconnectNotified = false;
        private readonly HashSet<string> _dsDisconnectNotifiedUserCodes = new HashSet<string>(StringComparer.Ordinal);
        /// <summary>
        /// 刷新账户余额信息。
        /// dsAccount：查询哪个读水账户时使用，仅在 accountType=="DS" 时生效；不传则兼容旧调用方式，
        /// 退化为 _DSAccount（多账户列表里的第一个）。
        /// </summary>
        // V20260918_CODE_REVIEW_FIXES(3)：原来是 async void，调用方 QueryBalanceDataAsync 无法
        // await 它、内部若抛出未处理异常也无法被上层 catch，只能靠 SynchronizationContext 兜底
        // （在某些宿主下会直接让进程崩溃）。改为 async Task 并在调用处 await，同时给结果解析
        // 部分加了 try/catch，避免响应结构异常（例如 result["data"] 为 null）时无人兜底。
        // V20260918_DS_MULTI_ACCOUNT_DISCONNECT：新增 dsAccount 参数，支持对指定的某一个读水
        // 账户查询余额/判定断链，而不再永远只查 _DSAccount。
        public async Task UpdateAccountBalanceInfo(string accountType, Account dsAccount = null)
        {
            string serverAddress = "";
            string userCode = "";
            bool isLogin = false;
            Account targetAccount = null;
            // 1. 读取配置与登录状态
            if (string.Equals(accountType, "EA"))
            {
                targetAccount = _EAAccount;
                isLogin = _EAAccount.IsLogin;
                serverAddress = _Config.EAServerAddress;
                userCode = _EAAccount.UserCode;
            }
            else if (string.Equals(accountType, "DS"))
            {
                // 未显式传入具体账户时，兼容旧调用方式，退化为第一个读水账户。
                targetAccount = dsAccount ?? _DSAccount;
                isLogin = targetAccount != null && targetAccount.IsLogin;
                // 🔧 改用账户自己的地址，不再是共用的 _Config.DSServerAddress
                // （现在每个 DS 账户可能各自打不同的服务器）。
                serverAddress = targetAccount?.BrokerServer;
                userCode = targetAccount?.UserCode;
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
                _Log.LogInfo($"[性能监控][UpdateAccountBalanceInfo][queryBalance][{userCode}] 请求异常 耗时: {stopwatch.ElapsedMilliseconds}ms, 错误: {ex.Message}");
                HandleAccountDisconnected(accountType, targetAccount);
                return;
            }
            finally
            {
                stopwatch.Stop();
            }
            long clientElapsedMs = stopwatch.ElapsedMilliseconds;
            _Log.LogInfo($"[性能监控][UpdateAccountBalanceInfo][queryBalance][{userCode}] 客户端总耗时: {clientElapsedMs}ms");
            if (result == null || !(bool)result["success"])
            {
                HandleAccountDisconnected(accountType, targetAccount);
                return;
            }
            // 🔧 本次查询成功，说明账号是通的，清掉"已提示过断链"的标记，
            // 避免下次真的断线时被旧标记永久屏蔽，导致再也不弹窗提醒
            ResetAccountDisconnectedNotifyFlag(accountType, targetAccount);
            // 同时打印后端自身返回的处理耗时（如果有的话）
            _Log.LogInfo($"[性能监控][UpdateAccountBalanceInfo][queryBalance][{userCode}] 后端服务耗时: {result["serverProcessTime"]}");
            try
            {
                string profitAndLoss = result["data"]["pl"].ToString();
                string accountCredit = result["data"]["balance"].ToString();
                string plCleanText = Regex.Replace(profitAndLoss, "<.*?>", string.Empty);
                bool isRed = profitAndLoss.Contains("class=\"RD\"") || profitAndLoss.Contains("class='RD'");

                // 🆕 界面上目前只有一组"读水余额"展示控件，多账户场景下只让当前的
                // 主账户（_DSAccount，即 _DSAccounts[0]）刷新这组共享 UI；其它读水账户
                // 查到的余额只记录在它自己的 Account.ProfitAndLoss/AccountCredit 字段上，
                // 不去抢共享控件，避免多个账户互相覆盖界面显示。
                bool updateSharedUI = string.Equals(accountType, "EA") ||
                    (string.Equals(accountType, "DS") && ReferenceEquals(targetAccount, _DSAccount));

                if (updateSharedUI)
                {
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
                else if (string.Equals(accountType, "DS") && targetAccount != null)
                {
                    targetAccount.ProfitAndLoss = profitAndLoss;
                    targetAccount.AccountCredit = accountCredit;
                }
            }
            catch (Exception ex)
            {
                // 服务器返回 success=true 但 data 结构不符合预期时，不能任由异常从 async 方法里
                // 逃逸（原来 async void 场景下会直接变成未处理异常），记录日志即可，等待下一轮重试。
                _logger.Error(ex, $"[UpdateAccountBalanceInfo][{accountType}][{userCode}] 解析余额响应异常，响应内容: {result?.ToString()}");
            }
        }
        /// <summary>
        /// 余额查询失败（服务器返回失败，或请求本身抛异常）时统一处理：
        /// 提示用户账号已断链需要重新登陆。同一账号在恢复登陆之前只弹一次。
        /// account：具体是哪个账户断了（EA 传 _EAAccount 或省略；DS 必须传具体的那个读水账户，
        /// 省略则退化为 _DSAccount，仅用于兼容旧调用）。
        /// </summary>
        private void HandleAccountDisconnected(string accountType, Account account = null)
        {
            if (string.Equals(accountType, "EA"))
            {
                if (_eaDisconnectNotified) return;
                _eaDisconnectNotified = true;

                string accountName = "打水";
                _Log.LogInfo($"[{accountName}账号]余额查询失败，判定为账号已断链");

                if (this.InvokeRequired)
                    this.Invoke(new Action(() => ShowAccountDisconnectedPrompt(accountType, accountName, _EAAccount)));
                else
                    ShowAccountDisconnectedPrompt(accountType, accountName, _EAAccount);
            }
            else if (string.Equals(accountType, "DS"))
            {
                var dsAccount = account ?? _DSAccount;
                if (dsAccount == null) return;
                string key = dsAccount.UserCode ?? "";

                // V20260918_DS_MULTI_ACCOUNT_DISCONNECT：按 UserCode 去重，_DSAccounts 里
                // 每个读水账户各自独立判定、独立提示、独立触发停止扫描，互不遮盖。
                lock (_dsDisconnectNotifiedUserCodes)
                {
                    if (_dsDisconnectNotifiedUserCodes.Contains(key)) return;
                    _dsDisconnectNotifiedUserCodes.Add(key);
                }

                string accountName = $"读水[{dsAccount.UserCode}]";
                _Log.LogInfo($"[{accountName}账号]余额查询失败，判定为账号已断链");

                if (this.InvokeRequired)
                    this.Invoke(new Action(() => ShowAccountDisconnectedPrompt(accountType, accountName, dsAccount)));
                else
                    ShowAccountDisconnectedPrompt(accountType, accountName, dsAccount);
            }
        }
        private void ShowAccountDisconnectedPrompt(string accountType, string accountName, Account account)
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
                if (account != null) account.IsLogin = false;
                // 🆕 登陆按钮目前只反映"是否有读水账户在线"这一个整体状态（见 anyDSLogin），
                // 只有当断链的正是当前主账户 _DSAccount 时才把按钮打回"未登陆"；其它非主账户
                // 断链不动按钮显示，避免明明还有别的读水账户在线、按钮却被误置为未登陆。
                bool isPrimary = _DSAccount == null || (account != null && string.Equals(account.UserCode, _DSAccount.UserCode, StringComparison.Ordinal));
                if (isPrimary)
                {
                    UpdateDSConnectStatus(buttonDSLogin, 0);
                }
            }

            // V20260918_STOP_SCAN_ON_DISCONNECT：打水(EA)或【任意一个】读水(DS)账号在这里
            // 被判定为自动断链后，如果自动扫描当时还是"已启动"状态，之前不会跟着停——扫描
            // 定时器（_timerRefreshBetInfo）还在跑，只是每一轮 QueryEATBETInfoDataAsync 一进来
            // 就因为 _EAAccount.IsLogin==false 直接 return，相当于空转；扫描按钮却仍显示
            // "扫描已启动"，容易让人误以为系统还在正常工作，没有第一时间去重新登陆。这里
            // 统一在断链后，若扫描仍处于开启状态，就显式调用 UpdateQueryEATBETInfoStatus 停掉
            // 扫描，按钮文字/颜色、_EnableBettingInfoRefresh 标记、定时器都会同步变为"已停止"，
            // 和手动登出时的处理保持一致（buttonAccountLogin_Click / buttonDSLogin_Click 里
            // 登出成功后同样会调用 UpdateQueryEATBETInfoStatus("扫描已停止")）。
            // 🆕 V20260918_DS_MULTI_ACCOUNT_DISCONNECT：现在读水侧的自动断链检测已经覆盖
            // _DSAccounts 里的每一个已登录账户（见 QueryBalanceDataAsync），不再只判第一个；
            // 任意一个打水/读水账户断链，都会走到这里停止扫描。
            if (_EnableBettingInfoRefresh)
            {
                _Log.LogInfo($"[{accountName}账号]检测到自动断链，自动扫描已随之停止，请重新登陆后手动点击「扫描」按钮重新启动。");
                UpdateQueryEATBETInfoStatus("扫描已停止");
            }
        }
        /// <summary>
        /// 清掉"已提示过断链"的标记。account 为 null 时（仅 DS 侧有意义）代表整批清空——
        /// 用于读水账户批量重新登陆后，一次性清掉所有旧账户残留的断链标记；传入具体账户
        /// 则只清掉这一个账户自己的标记。
        /// </summary>
        private void ResetAccountDisconnectedNotifyFlag(string accountType, Account account = null)
        {
            if (string.Equals(accountType, "EA"))
            {
                _eaDisconnectNotified = false;
            }
            else if (string.Equals(accountType, "DS"))
            {
                lock (_dsDisconnectNotifiedUserCodes)
                {
                    if (account != null)
                    {
                        _dsDisconnectNotifiedUserCodes.Remove(account.UserCode ?? "");
                    }
                    else
                    {
                        _dsDisconnectNotifiedUserCodes.Clear();
                    }
                }
            }
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
                if (_DSAccount != null)
                {
                    _DSAccount.ProfitAndLoss = profitAndLoss;
                    _DSAccount.AccountCredit = accountCredit;
                }
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
                    // V20260918_CODE_REVIEW_FIXES(6)：这是每次正常发出吃票请求都会打的日志，是
                    // 预期内的常规业务事件，不是异常，之前用 Warn 级别会跟真正的告警（如
                    // [EAT-BLOCK]）混在一起，降级为 Info。
                    _logger.Info(
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
                // 如果有需要，可以返回一个特定的中间状态码，例如 5 代表"部分挂单中"
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
            // V20260918_CODE_REVIEW_FIXES(7)：加上 MaxDegreeOfParallelism 上限，避免 combo 数量
            // 变多后一次性打满线程池去跑阻塞式 HTTP 请求。
            Parallel.ForEach(bettingInfoDict, new ParallelOptions { MaxDegreeOfParallelism = MaxComboParallelism }, kvp =>
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
        /// <param name="eatBetInfo">用于落库的吃票/下注挂起记录快照，可为 null（手工单不落库时）</param>
        /// <param name="betInfo">本次提交对应的盘口/委托信息</param>
        /// <param name="orderType">M:市价单,P:挂单</param>
        /// <param name="autoFlag"></param>
        /// <returns></returns>
        ///
        // V20260918_CODE_REVIEW_FIXES(4)：原来这里用 dynamic 接收 EatBetInfo/BetInfo：所有调用方
        // 实际上传入的都是具体的 EatBetInfo/BetInfo 类型（AutoBetProcess/AutoEatProcess/
        // ClosePositionByDeadline/buttonTestBet_Click 均是如此），dynamic 除了在每次属性访问时
        // 多一层 DLR 调度开销、丢失编译期类型检查之外没有带来任何好处，这里改回强类型参数。
        public JObject ExecuteTrade(EatBetInfo eatBetInfo, BetInfo betInfo, string orderType, string autoFlag)
        {
            string trade_type = string.Equals(betInfo.action, "EAT") ? "BET" : "EAT";

            // 🔧 关键修复：优先使用调用方已经算好（可能经过"剩余缺口裁剪"）的 betInfo.stakeAmount，
            // 只有它无效（<=0 或转换失败）时才回退到配置里的固定默认值。
            // 之前这里无条件用 _Config.QStakeAmount/QPStakeAmount 覆盖，
            // 会导致 ClosePositionByDeadline 里精心计算的
            // actualStake = Math.Min(remainingEatAmount, configStakeAmount)
            // 完全失效——当前场景因为配置金额恰好等于10、且缺口从未小于10才没有暴露问题，
            // 一旦出现"剩余缺口小于配置金额"（比如部分成交后只差5块）就会按10发送造成超发。
            int betInfoStake = 0;
            try { betInfoStake = (int)betInfo.stakeAmount; } catch { /* 转换失败时忽略，走默认值 */ }
            string stakeAmount = betInfoStake > 0
                ? betInfoStake.ToString()
                : (string.Equals(betInfo.type, "Q") ? _Config.QStakeAmount.ToString() : _Config.QPStakeAmount.ToString());

            string serverProcessTime = "0ms";
            IDictionary<string, string> BettingMarketData = new Dictionary<string, string>
            {
                { "col_name", betInfo.type + "_" + betInfo.action },
                { "combo", betInfo.combo },
                { "q_type", betInfo.type},
                { "type", betInfo.type },
                { "toto", betInfo.toto.ToString() },
                { "limit", betInfo.limit.ToString() },
                { "odds", betInfo.odds.ToString() },
                { "race", betInfo.raceNo },
                { "stake_amount", stakeAmount },
                { "order_type", orderType },
                { "trade_type", trade_type },
                { "action", trade_type },
            };
            _Log.LogInfo($"[{orderType}][{autoFlag}][{betInfo.type}][{betInfo.combo}][{trade_type}][{betInfo.odds}][{betInfo.limit}]下注开始");
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
                    betInfo.action = trade_type;
                    bool accepted = IsOrderAccepted(resultJObject);

                    // 🔧 不再自己开 Task.Run 落库：改为丢进统一的 TradeRecordWriter 队列。
                    // 入队是纯内存操作、几乎不耗时，真正的落库 I/O 由 TradeRecordWriter 的后台
                    // worker 按 dictKey 分片、严格按提交顺序串行处理，既不占用 betLock/eatLock
                    // 的持有时间，又和 RefreshTradeListToDB 的周期性落库共用同一条写入通道，
                    // 彻底避免两条通道互相竞态（详见 TradeRecordWriter.cs 顶部注释）。
                    string dictKey = ComboTradeState.BuildDictKey(betInfo.raceNo, betInfo.type, betInfo.combo);
                    EnqueueTradeWrite(new TradeWriteJob
                    {
                        Kind = TradeWriteJobKind.SubmitResult,
                        DictKey = dictKey,
                        EatBetInfo = eatBetInfo,
                        BetInfo = betInfo,
                        Result = resultJObject,
                        AutoFlag = autoFlag,
                        OrderType = orderType,
                        Accepted = accepted
                    });
                }
                else
                {
                    _Log.LogInfo($"[{betInfo.type}][{betInfo.combo}][{trade_type}][{betInfo.odds}][{betInfo.limit}]手工下注,不更新内存数据");
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
                    // V20260918_CODE_REVIEW_FIXES(7)：加上 MaxDegreeOfParallelism 上限。
                    Parallel.ForEach(filteredEatGroups, new ParallelOptions { MaxDegreeOfParallelism = MaxComboParallelism }, kvp =>
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
                    // V20260918_CODE_REVIEW_FIXES(7)：加上 MaxDegreeOfParallelism 上限。
                    Parallel.ForEach(filteredEatGroups, new ParallelOptions { MaxDegreeOfParallelism = MaxComboParallelism }, kvp =>
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
        /// 刷新盘口数据列表，并且同时处理自动下注。
        /// 🆕 查询侧：对 _DSAccounts 中所有已登录的 DS 账户并发发起 queryMarket，
        /// race 语义——谁先返回"成功且非空"的数据就用谁，其余请求结果忽略，
        /// 只有全部账户都异常或返回空才判定本轮数据为空。
        /// 🆕 分析侧：AutoBetting 通过 _autoBettingLock 任意时刻只允许一个实例串行执行。
        /// 下单侧：AutoBetting → AutoBetProcess/AutoEatProcess → ExecuteTrade 内部
        /// 硬编码使用 _EAAccount，与查询用哪个 DS 账户完全无关。
        /// </summary>
        public void RefreshBetInfoDataList()
        {
            DateTime now = DateTime.Now;
            if (TryGetTradeTimeContext(out DateTime raceDateTime, out DateTime triggerStartTime, out DateTime triggerEndTime))
            {
                var (BetBatInfos, serverProcessTime) = QueryMarketRaceFromAllDSAccounts();

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
                    _Log.LogInfo($"queryMarket 数据为空 [{serverProcessTime}]");
                }
                if (BetBatInfos != null)
                {
                    _Log.LogInfo($"扫描结束,已获取数据 [{serverProcessTime}]");
                    if (_EnableAutoTrade)
                    {
                        _Log.LogInfo("自动下注处理开始");

                        // 🔒 分析串行化：不管本轮数据来自哪个 DS 账户、也不管有多少轮触发在排队，
                        // AutoBetting（含内部 RefreshMyTradeSnapshot）任意时刻只允许一个实例在跑。
                        lock (_autoBettingLock)
                        {
                            RefreshMyTradeSnapshot();
                            if (DateTime.Now >= triggerEndTime)
                            {
                                _Log.LogInfo($"[{DateTime.Now:HH:mm:ss}] queryMyTrade 期间已达到下注截止[{triggerEndTime:HH:mm:ss}]，跳过本轮 AutoBetting，交由强平通道接管");
                                return;
                            }
                            AutoBetting(BetBatInfos);
                        }

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
        /// 🆕 对 _DSAccounts 中所有【已登录】的 DS 账户并发发起 queryMarket 查询（各自打各自的 BrokerServer，
        /// 每个账户的地址来自 Config.json 中 DSServerAddress 逗号分隔列表里对应下标的地址），
        /// race 语义：谁先返回"成功且非空"的数据，就立即用它作为本轮 AutoBetting 的唯一数据源。
        /// 未被选中（含仍在进行中）的请求结果一律忽略，不取消、不合并、不等待。
        /// 只有全部（已登录）账户都异常或返回空，才等它们全部跑完后判定本轮"数据为空"。
        ///
        /// 注意：HTTPHelper.queryMarket 当前签名不支持 CancellationToken，被忽略的慢请求
        /// 无法真正取消，会在后台自然跑完，只是结果不被使用；如果账户数量多、轮询频繁，
        /// 这些"陪跑"请求会持续占用线程池和对端连接数，值得关注。
        /// </summary>
        private (IDictionary<string, List<BetInfo>> data, string serverProcessTime) QueryMarketRaceFromAllDSAccounts()
        {
            if (_DSAccounts == null || _DSAccounts.Count == 0)
            {
                _Log.LogInfo("[QueryMarketRace] _DSAccounts 为空，跳过查询");
                return (null, "0ms");
            }

            var loginAccounts = _DSAccounts.Where(a => a != null && a.IsLogin).ToList();
            if (loginAccounts.Count == 0)
            {
                _Log.LogInfo($"[QueryMarketRace] _DSAccounts[{_DSAccounts.Count}个] 均未登录，跳过查询");
                return (null, "0ms");
            }
            if (loginAccounts.Count < _DSAccounts.Count)
            {
                var offline = _DSAccounts.Where(a => a == null || !a.IsLogin)
                    .Select(a => a == null ? "null" : $"{a.BrokerCode}/{a.UserCode}");
                _Log.LogInfo($"[QueryMarketRace] 跳过未登录账户: [{string.Join(", ", offline)}]");
            }

            var pending = loginAccounts.Select(account => Task.Run(() =>
            {
                try
                {
                    var data = HTTPHelper.queryMarket(
                        account.BrokerServer, account.UserCode,
                        _Config.CurrentRaceDate, _Config.CurrentRaceType, _Config.CurrentRaceNo,
                        out string pt);
                    return (account, data, pt, ex: (Exception)null);
                }
                catch (Exception ex)
                {
                    return (account, data: (IDictionary<string, List<BetInfo>>)null, pt: "异常", ex);
                }
            })).ToList();

            while (pending.Count > 0)
            {
                var finished = (Task<(Account account, IDictionary<string, List<BetInfo>> data, string pt, Exception ex)>)
                    Task.WhenAny(pending).GetAwaiter().GetResult();

                pending.Remove(finished);
                var (account, data, pt, ex) = finished.Result;

                if (ex != null)
                {
                    _Log.LogInfo($"[DS账户][{account.BrokerCode}/{account.BrokerServer}][{account.UserCode}][queryMarket] 请求异常: {ex.Message}，race 中跳过该路，等待其余账户");
                    continue;
                }
                if (data == null || data.Count == 0)
                {
                    _Log.LogInfo($"[DS账户][{account.BrokerCode}/{account.BrokerServer}][{account.UserCode}][queryMarket] 返回数据为空，race 中跳过该路，等待其余账户");
                    continue;
                }

                // ✅ 命中：第一份成功且非空的数据，立即返回，剩余仍在跑的请求结果直接忽略
                _Log.LogInfo($"[DS账户][{account.BrokerCode}/{account.BrokerServer}][{account.UserCode}][queryMarket] race 胜出，用时[{pt}]，采用该路数据进入分析");
                return (data, $"{account.UserCode}:{pt}(race胜出)");
            }

            // 全部（已登录）账户都异常或为空，等它们都跑完才走到这里判定
            return (null, "全部已登录DS账户查询为空或异常");
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
                // 1. 直接在主线程立刻更新 UI（显示"登录中"和动画），绝对不能在这里卡顿！
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
        /// <summary>
        /// 🆕 读水账户登陆：textBoxDSAccountCode/Password/Pin 支持英文逗号分隔多个账户，
        /// 各账户对应的服务器地址来自 _Config.DSServerAddress（同样逗号分隔，只在 Config.json
        /// 中维护，不经界面输入），按下标一一对应。点击登陆后并发对所有解析出的账户逐一尝试登陆
        /// （网络请求本身串行发起、避免瞬间打满连接，但都在后台线程执行、不阻塞 UI），
        /// 只要有一个账户登陆成功就显示"已登陆"；无论成功与否，全部账户的登陆结果都会打印到日志中。
        /// </summary>
        private async void buttonDSLogin_Click(object sender, EventArgs e)
        {
            if (string.Equals(buttonDSLogin.Text, "未登陆"))
            {
                if (string.IsNullOrWhiteSpace(textBoxDSAccountCode.Text) ||
                    string.IsNullOrWhiteSpace(textBoxDSAccountPassword.Text) ||
                    string.IsNullOrWhiteSpace(textBoxDSAccountPin.Text))
                {
                    MessageBox.Show("请输入账号、密码 和 安码（多个账户请用英文逗号分隔）！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var accountsToLogin = BuildDSAccountsFromInput(textBoxDSAccountCode.Text, textBoxDSAccountPassword.Text, textBoxDSAccountPin.Text, _Config.DSServerAddress);
                if (accountsToLogin.Count == 0)
                {
                    MessageBox.Show("未解析到有效的读水账户，请检查输入格式！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (accountsToLogin.Any(a => string.IsNullOrWhiteSpace(a.BrokerServer)))
                {
                    MessageBox.Show("部分账户未配置对应的服务器地址，请检查 Config.json 中 DSServerAddress 是否与账号数量一致（逗号分隔）！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _Log.LogInfo($"开始登陆读水账户[{accountsToLogin.Count}个]: [{string.Join(", ", accountsToLogin.Select(a => $"{a.UserCode}@{a.BrokerServer}"))}]");
                UpdateDSConnectStatus(buttonDSLogin, 2); // 登陆中

                var loginResults = await Task.Run(() =>
                {
                    var results = new List<(Account account, bool success, string message)>();
                    foreach (var account in accountsToLogin)
                    {
                        try
                        {
                            // 🔧 不再跟 _Config.DSServerAddress 整体比较，而是每个账户各自跟
                            // EAServerAddress 比较，因为现在每个 DS 账户可能打不同的服务器地址。
                            if (string.Equals(account.BrokerServer, _Config.EAServerAddress))
                            {
                                results.Add((account, true, "与打水账号同服务器，无需单独登陆"));
                                continue;
                            }
                            JObject loginResult = HTTPHelper.login(account.BrokerServer, account.UserCode, account.Password, account.Pin);
                            if (loginResult != null && (bool)loginResult["success"])
                            {
                                _Log.LogInfo($"[性能监控][buttonDSLogin][login][{account.UserCode}@{account.BrokerServer}] 后端服务耗时: {loginResult["serverProcessTime"]}");
                                results.Add((account, true, "登陆成功"));
                            }
                            else
                            {
                                results.Add((account, false, "登陆失败"));
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.ToString());
                            results.Add((account, false, $"登陆异常: {ex.Message}"));
                        }
                    }
                    return results;
                });

                int successCount = 0;
                foreach (var (account, success, message) in loginResults)
                {
                    account.IsLogin = success;
                    if (success) successCount++;
                }

                _DSAccounts = accountsToLogin;
                // _DSAccount 保留指向第一个账户，兼容其它仍引用单个 _DSAccount 的地方
                // （余额查询 UpdateAccountBalanceInfo("DS")、断线提示 HandleAccountDisconnected("DS") 等）。
                _DSAccount = accountsToLogin[0];
                if (_MyConfig.DSAccount == null) _MyConfig.DSAccount = new Account();
                // 把原始逗号分隔文本整存回配置，下次 LoadConfigFile/IniUI 能原样还原多账户输入框内容。
                // 服务器地址（_Config.DSServerAddress）不经界面输入，此处不改动，仍以 Config.json 为准。
                _MyConfig.DSAccount.UserCode = textBoxDSAccountCode.Text;
                _MyConfig.DSAccount.Password = textBoxDSAccountPassword.Text;
                _MyConfig.DSAccount.Pin = textBoxDSAccountPin.Text;

                // 🆕 无论成功或失败，把全部账户的登陆结果输出到日志。
                if (successCount > 0)
                {
                    _Log.LogInfo($"读水账户登陆完成[{successCount}/{accountsToLogin.Count}个成功]，明细如下：");
                    foreach (var (account, success, message) in loginResults)
                    {
                        _Log.LogInfo($"  [读水账户][{account.UserCode}@{account.BrokerServer}] {(success ? "✅ 成功" : "❌ 失败")} - {message}");
                    }
                    // 🆕 只要有一个账户登陆成功，就显示"已登陆"。
                    ResetAccountDisconnectedNotifyFlag("DS");
                    UpdateDSConnectStatus(buttonDSLogin, 1);
                }
                else
                {
                    _Log.LogInfo($"读水账户全部登陆失败[0/{accountsToLogin.Count}个成功]，明细如下：");
                    foreach (var (account, success, message) in loginResults)
                    {
                        _Log.LogInfo($"  [读水账户][{account.UserCode}@{account.BrokerServer}] ❌ 失败 - {message}");
                    }
                    UpdateDSConnectStatus(buttonDSLogin, 0);
                }
            }
            else if (string.Equals(buttonDSLogin.Text, "已登陆"))
            {
                var accountsToLogout = (_DSAccounts != null && _DSAccounts.Count > 0)
                    ? _DSAccounts
                    : (_DSAccount != null ? new List<Account> { _DSAccount } : new List<Account>());

                foreach (var account in accountsToLogout)
                {
                    if (account == null || !account.IsLogin) continue;
                    // 🔧 按账户自己的服务器地址判断，不再用 _Config.DSServerAddress 整体比较
                    if (string.Equals(account.BrokerServer, _Config.EAServerAddress))
                    {
                        account.IsLogin = false;
                        continue;
                    }
                    JObject logoutResult = HTTPHelper.logout(account.BrokerServer, account.UserCode);
                    if (logoutResult != null && (bool)logoutResult["success"])
                    {
                        _Log.LogInfo($"[性能监控][buttonDSLogin][logout][{account.UserCode}@{account.BrokerServer}] 后端服务耗时: {logoutResult["serverProcessTime"]}");
                    }
                    else
                    {
                        _logger.Error($"[读水账户][{account.UserCode}@{account.BrokerServer}] 登出请求失败或无响应");
                    }
                    account.IsLogin = false;
                }
                _Log.LogInfo("读水账户已全部登出");
                _Log.LogInfo("扫描已停止");
                UpdateQueryEATBETInfoStatus("扫描已停止");
                UpdateDSConnectStatus(buttonDSLogin, 0);
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
            QueryAndApplyRaceInfo();
        }
        /// <summary>
        /// 查询当前赛场（_Config.CurrentRaceType）的场次信息并据此刷新 comboBoxRaceNo/开赛时间：
        /// 优先用后端 HTTPHelper.queryRaceInfo 返回的场次列表；查询失败或返回不成功时，
        /// 回退到数据库配置里的 Horse.{RaceType}.RCsTime 静态时间表。
        /// 从 comboBoxRaceType_SelectedIndexChanged 里抽出来，便于后续在其它地方
        /// （比如切换赛场类型之外的场景）复用同一套刷新逻辑。
        /// </summary>
        private void QueryAndApplyRaceInfo()
        {
            JObject raceInfoResult = HTTPHelper.queryAllRaceInfo(_Config.EAServerAddress, _EAAccount.UserCode, _Config.CurrentRaceDate, _Config.CurrentRaceType, _Config.CurrentRaceNo);
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
                // 🆕 只要 _DSAccounts 中有任意一个账户在线，就允许启动扫描（对应"任一成功即已登陆"）。
                bool anyDSLogin = (_DSAccounts != null && _DSAccounts.Any(a => a != null && a.IsLogin))
                                   || (_DSAccount != null && _DSAccount.IsLogin);
                if (!anyDSLogin)
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
                // 💡 转场前先把服务器端交易数据完整同步到本地数据库，确保服务器端和数据库一致，
                // 避免上一场最后几笔成交/补写还没落库就被下面的清空动作冲掉。
                // ==========================================
                // 1. 先从服务器刷新一次内存快照（_AllMyTradesList 等），保证内存里是最新数据。
                RefreshMyTradeSnapshot();
                // 2. 把最新的内存快照整体入队写库（覆盖 CONFIRMED 状态的所有成交记录）。
                RefreshTradeListToDB();
                // 3. 停止接收新的落库 job，并等待已入队 job 全部写完（最多等 10 秒），
                //    确保上面第 2 步入队的 job 在清空内存字典/状态之前已经落库完成；
                //    随后立即重新创建一个新的 TradeRecordWriter 供下一场继续使用。
                //    整段 Stop+重建 用 _tradeRecordWriterLock 包裹，防止其它线程此时正在
                //    Enqueue（ExecuteTrade / RefreshTradeListToDB 的下一轮定时器）撞上已经
                //    Complete() 的旧 Channel 而抛出 ChannelClosedException 导致数据丢失。
                lock (_tradeRecordWriterLock)
                {
                    _tradeRecordWriter?.Stop(TimeSpan.FromSeconds(10));
                    _tradeRecordWriter = CreateTradeRecordWriter();
                }
                _Log.LogInfo("转场前服务器交易数据已全部同步到本地数据库。");
                // ==========================================
                // 💡 盘口及缓存数据清理（防止上一场数据污染下一场）
                // ==========================================
                // 4. 数据库已经确认写完，现在再清空上一场的全局交易字典与列表。
                _EatBetInfoDict?.Clear();
                _EatBetInfosList?.Clear();
                _AllMyTradesList?.Clear();
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
                        // 在转场前重新查询下一场的开赛时间，避免沿用上一场遗留的 CurrentRaceTime
                        // 导致强平/自动下注时间窗口判断用的是错误的开赛时间。
                        string nextRaceNo = comboBoxRaceNo.Items[nextIndex].ToString();
                        JObject raceInfoResult = HTTPHelper.queryRaceInfo(_Config.EAServerAddress, _EAAccount.UserCode, _Config.CurrentRaceDate, _Config.CurrentRaceType, nextRaceNo);
                        if (raceInfoResult != null && (bool)raceInfoResult["success"])
                        {
                            JObject dataObj = raceInfoResult["data"] as JObject;
                            // 优先在 available_races 里按 race_num 精确匹配下一场；查不到再兜底用 race_time_info
                            // （接口按 race_num 请求时通常只返回这一场，两者内容一致）。
                            string rawTimeStr = null;
                            if (dataObj?["available_races"] is JArray availableRaces)
                            {
                                foreach (var item in availableRaces)
                                {
                                    if (string.Equals(item["race_num"]?.ToString(), nextRaceNo))
                                    {
                                        rawTimeStr = item["time"]?.ToString();
                                        break;
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(rawTimeStr))
                            {
                                rawTimeStr = dataObj?["race_time_info"]?.ToString();
                            }
                            if (!string.IsNullOrEmpty(rawTimeStr) && TryParseRaceTimeTo24Hour(rawTimeStr, out string raceTime24))
                            {
                                _Config.CurrentRaceTime = raceTime24;
                                textBoxCurrentRaceTime.Text = _Config.CurrentRaceTime;
                                _Log.LogInfo($"转场前重新查询到下一场[{nextRaceNo}]开赛时间: '{rawTimeStr}' -> '{raceTime24}'");
                            }
                            else
                            {
                                _logger.Error($"[自动转场] 解析下一场[{nextRaceNo}]开赛时间失败，原始字符串='{rawTimeStr}'，本次转场沿用旧的 CurrentRaceTime='{_Config.CurrentRaceTime}'");
                            }
                        }
                        else
                        {
                            _logger.Error($"[自动转场] queryRaceInfo 查询下一场[{nextRaceNo}]开赛时间失败，本次转场沿用旧的 CurrentRaceTime='{_Config.CurrentRaceTime}'");
                        }
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
        /// 把 queryRaceInfo 等接口返回的、类似 "10:55pm - Race 8" 或单独 "10:55pm"
        /// 这种 12 小时制、带 am/pm 后缀（可能还带 " - Race N" 尾巴）的时间字符串，解析成
        /// _Config.CurrentRaceTime 期望的 "HH:mm" 24 小时制格式——后续 TimeSpan.TryParse 以及
        /// "yyyy-MM-dd HH:mm:ss" 的 DateTime.TryParseExact（强制平仓等时间窗口判断）都是按这个
        /// 格式解析的，格式不对会导致这些判断直接失败退出。
        /// 用手写的正则+数值换算而不是 DateTime.TryParseExact("h:mmtt", ...)，是为了不依赖具体
        /// 运行环境 CultureInfo 对 AM/PM 大小写的处理是否一致，行为更可预测、也方便在失败时
        /// 精确知道是哪一步没匹配上。
        /// </summary>
        private static bool TryParseRaceTimeTo24Hour(string rawTimeStr, out string raceTime24)
        {
            raceTime24 = null;
            if (string.IsNullOrWhiteSpace(rawTimeStr)) return false;
            // 例如 "10:55pm - Race 8" / "10:55pm"，取空格分隔后的第一段 "10:55pm"
            string timePart = rawTimeStr.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (string.IsNullOrEmpty(timePart)) return false;
            var match = System.Text.RegularExpressions.Regex.Match(
                timePart, @"^(?<hour>\d{1,2}):(?<minute>\d{2})\s*(?<ampm>[AaPp][Mm])$");
            if (!match.Success) return false;
            int hour = int.Parse(match.Groups["hour"].Value);
            int minute = int.Parse(match.Groups["minute"].Value);
            if (hour < 1 || hour > 12 || minute < 0 || minute > 59) return false;
            string ampm = match.Groups["ampm"].Value.ToUpperInvariant();
            if (ampm == "PM" && hour != 12) hour += 12;
            else if (ampm == "AM" && hour == 12) hour = 0;
            raceTime24 = $"{hour:D2}:{minute:D2}";
            return true;
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
                EnqueueTradeWrite(new TradeWriteJob
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
                // V20260918_CODE_REVIEW_FIXES(1)：改用独立的 _scanProcessingAngle，不再跟登陆动画共用角度。
                e.Graphics.DrawArc(pen, rect, _scanProcessingAngle, 120);
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
                // V20260918_CODE_REVIEW_FIXES(1)：改用独立的 _loginProcessingAngle。
                e.Graphics.DrawArc(pen, rect, _loginProcessingAngle, 120);
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
                // V20260918_CODE_REVIEW_FIXES(1)：改用独立的 _dsLoginProcessingAngle。
                e.Graphics.DrawArc(pen, rect, _dsLoginProcessingAngle, 120);
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
        /// 刷新下注列表数据，数据来源改为内存中的 _EatBetInfosList（由 RefreshMyTradeSnapshot() /
        /// QueryAndApplyMyTradeSnapshotAsync() 里 Utils.Utils.GetBatBetInfo(...) 构造并整体重新赋值），
        /// 不再每次都查数据库，减少刷新时的 DB 开销。
        ///
        /// 🔧 重新改回读内存：之前（见历史注释）发现的已知缺陷依然存在——如果某个组合还没有
        /// 任何"吃"成交（吃笔数=0，只有 BET），_EatBetInfosList/_EatBetInfoDict 里不会包含
        /// 这些组合，对应行不会出现在表格里（数据库/TradeStateStore 里其实已经正确记录）。
        /// 这是按需求明确接受的已知限制，不在这次改动里处理。后续如需修复，需要
        /// Utils.Utils.GetBatBetInfo（或 EatBetInfo/ComboTradeState 定义）的源码，基于
        /// _Config.TradeStateStore（全量权威的每组合状态）重新构造一份不遗漏"只赌未吃"组合的
        /// 展示用列表，而不是直接依赖 _EatBetInfosList。
        ///
        /// 线程安全说明：_EatBetInfosList 每次都是整体重新赋值为一个新 List（见
        /// RefreshMyTradeSnapshot / QueryAndApplyMyTradeSnapshotAsync 里的 `_EatBetInfosList = eatBetInfosList;`），
        /// 旧的 List 对象发布后不会再被原地修改，所以这里先用局部变量接住当前引用再遍历，
        /// 是一次安全的快照读取，不会跟并发的重新赋值互相踩踏。
        /// </summary>
        private void RefreshBettingInfoDataList()
        {
            if (_EAAccount.IsLogin && _EnableBettingInfoRefresh)
            {
                // 快照当前引用，避免遍历过程中被其它线程重新赋值。
                var bettingInfos = _EatBetInfosList;
                _BettingList.RaiseListChangedEvents = false;
                _BettingList.Clear();
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
