
using System;
using System.Collections.Generic;
using System.Diagnostics;
using uClient.Comm;

namespace uClient.Broker
{
    /// <summary>
    /// 策略
    /// </summary>
    public abstract class StrategyEA
    {
        public Log _Log;
        /// <summary>
        /// EA code
        /// </summary>

        public string code { set; get; }
        /// <summary>
        /// EA Name
        /// </summary>
        public string name { set; get; }
        /// <summary>
        /// EA 值
        /// </summary>
        public string value { set; get; }
        /// <summary>
        /// EA 类型,BUY,SELL,CLOSE_BUY,CLOSE_SELL
        /// </summary>
        public string type { set; get; }
        /// <summary>
        /// Symbol
        /// </summary>
        public string symbol { set; get; }

        /// <summary>
        /// 总览数据配置
        /// </summary>
        public double sumConfigValue { set; get; }
        public double sumConfigValue1 { set; get; }
        /// <summary>
        /// 近1天数据配置
        /// </summary>
        public double dailyConfigValue { set; get; }
        public double dailyConfigValue1 { set; get; }
        /// <summary>
        /// 分时数据配置
        /// </summary>
        public double hourlyConfigValue { set; get; }
        public double hourlyConfigValue1 { set; get; }

        /// <summary>
        /// 黄金白银变化率数据配置
        /// </summary>
        public double gsConfigValue { set; get; }
        public double gsConfigValue1 { set; get; }

        /// <summary>
        /// 不执行策略值
        /// </summary>
        public double noTradeConfigValue { set; get; }

        /// <summary>
        /// 持仓比例，百分比
        /// </summary>
        public double lotProportion { set; get; }

        /// <summary>
        /// 生效时间
        /// </summary>
        public DateTime effectiveTime { set; get; }
        /// <summary>
        /// 失效时间
        /// </summary>
        public DateTime expireTime { set; get; }

        /// <summary>
        /// 配置参数
        /// </summary>
        public Dictionary<string, string> configParam = new Dictionary<string, string>();

        /// <summary>
        /// 启用状态
        /// </summary>
        public bool active { set; get; }
        /// <summary>
        /// 该EA是否需要执行
        /// </summary>
        /// <returns></returns>
        public bool IsAvailable()
        {
            if (IsEffectived() && !IsExpired() && active)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 是否已生效
        /// </summary>
        /// <returns></returns>
        public bool IsEffectived()
        {
            if (DateTime.Now.CompareTo(effectiveTime) > 0)
            {
                //已生效
                return true;
            }
            else
            {
                //未生效
                return false;
            }
        }
        /// <summary>
        /// 是否已过期
        /// </summary>
        /// <returns></returns>
        public bool IsExpired()
        {
            if (DateTime.Now.CompareTo(expireTime) > 0)
            {
                //未过期
                return true;
            }
            else
            {
                //已过期
                return false;
            }
        }
        /// <summary>
        /// 其他参数
        /// </summary>
        public TradeParametre TradePara { set; get; }

        /// <summary>
        /// EA 执行，该返回被EA实例重载
        /// sum 总览
        /// daily 近1天
        /// hourly 分时
        /// price 分价
        /// context  上下文数据
        /// </summary>
        /// <returns></returns>
        public abstract bool Execute(EAChart eaChart, IDictionary<string, Object> context);

        /// <summary>
        /// 策略描述
        /// </summary>
        public abstract string ToDesc();

        // Stopwatch 是 .NET 提供的专门用于高精度测量的计时器。
        // 它通过访问底层硬件计数器来工作，不会受系统时间同步（NTP）的影响。
        private Stopwatch _profiler = new Stopwatch();

        // 用于存储人类可读的开始时间点。
        // DateTime 适合记录具体的“年月日时分秒”，方便在日志中对齐查看。
        private DateTime _logStartTime;

        /// <summary>
        /// 【开启计时器】
        /// 建议在策略逻辑或数据库查询的最开始调用。
        /// </summary>
        public void StartTimer()
        {
            // 记录当前的挂钟时间（系统本地时间）
            _logStartTime = DateTime.Now;

            // Restart() 的作用是将计时器清零并立即重新开始计时。
            // 这比先 Reset() 再 Start() 更高效且连续。
            _profiler.Restart();

            // 打印开始日志。HH:mm:ss.fff 表示 24 小时制且精确到毫秒。
            Console.WriteLine($"[{name}][开始计时] 时点: {_logStartTime:HH:mm:ss.fff}");
        }

        /// <summary>
        /// 【结束计时并输出结果】
        /// </summary>
        /// <param name="taskName">当前执行的任务描述，如 "数据获取" 或 "策略计算"</param>
        public void EndTimer(string taskName = "Task")
        {
            // 停止计时，冻结经过的时间
            _profiler.Stop();

            // 记录结束时的挂钟时间
            DateTime endTime = DateTime.Now;

            // 计算从 StartTimer 到现在经过的总毫秒数。
            // _profiler.ElapsedMilliseconds 返回的是 long 整数（毫秒）。
            // 如果需要更高精度，可以使用 _profiler.Elapsed.TotalMilliseconds (返回 double)。
            long elapsedMs = _profiler.ElapsedMilliseconds;

            // 输出格式化日志
            // {endTime:HH:mm:ss.fff}: 任务结束的具体时间
            // {taskName}: 传入的任务标签
            // {elapsedMs}: 实际消耗的时间（毫秒）
            Console.WriteLine($"[{name}][结束计时] 时点: {endTime:HH:mm:ss.fff} | 任务: {taskName} | 耗时: {elapsedMs}ms");

            // 性能分析提示：
            // 在量化交易中，如果单次循环耗时超过 500ms，通常需要检查网络延迟或数据库索引。
        }
    }
}
