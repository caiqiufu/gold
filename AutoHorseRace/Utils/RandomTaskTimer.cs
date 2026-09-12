namespace AutoHorseRace.Utils
{
    public class RandomTaskTimer
    {
        private CancellationTokenSource _cts;
        private readonly Func<Task> _action;      // 异步业务方法

        // 移除 readonly，允许动态修改
        private int _baseDelaySec;                 // 基础等待时间（秒）
        private int _randomMinSec;                 // 随机秒数最小值
        private int _randomMaxSec;                 // 随机秒数最大值（包含）

        private readonly Random _random = new Random();
        private readonly string _taskName;         // 任务名称（用于日志区分）

        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 初始化动态随机定时器
        /// </summary>
        public RandomTaskTimer(string taskName, Func<Task> action, int baseDelaySeconds, int randomMinSeconds, int randomMaxSeconds)
        {
            _taskName = taskName;
            _action = action;
            _baseDelaySec = baseDelaySeconds;
            _randomMinSec = randomMinSeconds;
            _randomMaxSec = randomMaxSeconds;
        }

        #region 💡 独立修改任意单个参数的方法

        /// <summary>
        /// 仅修改基础等待时间（秒）
        /// </summary>
        public void SetBaseDelay(int baseDelaySeconds)
        {
            _baseDelaySec = baseDelaySeconds;
            _logger.Info($"[{_taskName}] 基础等待时间已更新为: {baseDelaySeconds}s");
        }

        /// <summary>
        /// 仅修改随机秒数最小值
        /// </summary>
        public void SetRandomMin(int randomMinSeconds)
        {
            _randomMinSec = randomMinSeconds;
            _logger.Info($"[{_taskName}] 随机最小值已更新为: {randomMinSeconds}s");
        }

        /// <summary>
        /// 仅修改随机秒数最大值
        /// </summary>
        public void SetRandomMax(int randomMaxSeconds)
        {
            _randomMaxSec = randomMaxSeconds;
            _logger.Info($"[{_taskName}] 随机最大值已更新为: {randomMaxSeconds}s");
        }

        /// <summary>
        /// 同时修改所有 3 个参数
        /// </summary>
        public void Configure(int baseDelaySeconds, int randomMinSeconds, int randomMaxSeconds)
        {
            _baseDelaySec = baseDelaySeconds;
            _randomMinSec = randomMinSeconds;
            _randomMaxSec = randomMaxSeconds;
            _logger.Info($"[{_taskName}] 参数全部更新 -> 基础延迟: {baseDelaySeconds}s, 随机范围: [{randomMinSeconds}s ~ {randomMaxSeconds}s]");
        }

        #endregion

        /// <summary>
        /// 检查当前任务是否正在运行
        /// </summary>
        public bool IsRunning => _cts != null;

        /// <summary>
        /// 独立启动任务
        /// </summary>
        public void Start()
        {
            if (_cts != null)
            {
                _logger.Warn($"[{_taskName}] 已经在运行中，请勿重复启动。");
                return;
            }

            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            Task.Run(async () =>
            {
                _logger.Info($"[{_taskName}] 后台随机定时任务已成功启动。(基础延迟: {_baseDelaySec}s, 随机范围: [{_randomMinSec}s~{_randomMaxSec}s])");

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // 每次循环时实时读取当前最新的参数计算
                        int randomDelaySec = _random.Next(_randomMinSec, _randomMaxSec + 1);
                        int totalDelayMs = (_baseDelaySec + randomDelaySec) * 1000;

                        _logger.Info($"[{_taskName}] 预计在 {totalDelayMs / 1000.0} 秒后执行下一次业务查询...");

                        await Task.Delay(totalDelayMs, token);
                        await _action();
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"[{_taskName}] 内部业务执行发生异常: {ex.Message}");
                        await Task.Delay(3000, token);
                    }
                }

                _logger.Info($"[{_taskName}] 后台随机定时任务已彻底安全停止。");
            }, token);
        }

        /// <summary>
        /// 独立停止任务
        /// </summary>
        public void Stop()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
                _logger.Info($"[{_taskName}] 已发出停止信号。");
            }
        }
    }
}