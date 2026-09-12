using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uClient.Comm;

namespace uClient.Broker
{
    /// <summary>
    /// TREND_KDJ
    /// KDJ 判断趋势
    /// 参数:BUY TREND:买入趋势,SELL TREND卖出趋势,NO TREND:无交易趋势
    /// 返回true,未超过开仓数量,可以开仓,否则不能开仓
    /// </summary>
    public class TrendKDJ : StrategyEA
    {
        private StringBuilder _log = new StringBuilder();
        /// <summary>
        /// 返回策略的实时状态描述，供前端或 UI 显示
        /// </summary>
        public override string ToDesc() => _log.ToString();

        // K 线列表 (需要从外部数据源注入)
        public List<Candle> TargetCandles { get; set; }
        //最少需要的K线数量
        private int MinCandleCount = 25;
        //KDJ周期
        private const string period = "H1";


        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            _log.Clear();

            string _IsSimulateTest = context["IsSimulateTest"].ToString();
            // 从上下文中提取必要数据
            string currentOrderType = context.ContainsKey("CurrentOrderType") ? context["CurrentOrderType"].ToString() : "NONE";
            decimal currentPrice = decimal.Parse(context["price"].ToString());
            string currentDateTimeStr = context.ContainsKey("CurrentDateTime") ? context["CurrentDateTime"].ToString() : DateTime.Now.ToString();

            MinCandleCount = IndicatorHelper.GetRequiredCandleCount(period);

            _log.AppendFormat($"动态参数[currentOrderType{currentOrderType}][currentDateTime{currentDateTimeStr}][MinCandleCount{MinCandleCount}]");

            //currentDateTimeStr = "2026-02-11 16:00:42";
            // --- 1. 获取多周期数据 ---
            // 为计算斜率，我们需要保留最近几个周期的 KD 历史，而不仅仅是最后一根
            if (string.Equals(_IsSimulateTest, "T", StringComparison.OrdinalIgnoreCase))
            {
                var M1M5Datas = DSHelper.GetRecentCandlesOptimizedFast("XAUUSD", "ONE_MIN", (MinCandleCount * 60) + 10, currentDateTimeStr, "H1");
                M1M5Datas = Utils.FillMissingM1Candles(M1M5Datas);
                int filledCount = M1M5Datas.Count(x => x.IsFilled);
                if (filledCount > 0)
                {
                    _log.AppendLine($"自动补齐完成，补齐K线数量: {filledCount}");
                }
                TargetCandles = Utils.AggregateCandlesStrictLinQ(M1M5Datas, 1, 60);
            }
            else
            {
                var M1M5Datas = DSHelper.GetRecentCandles("XAUUSD", "ONE_MIN", (MinCandleCount * 60) + 10, currentDateTimeStr);
                M1M5Datas = Utils.FillMissingM1Candles(M1M5Datas);
                int filledCount = M1M5Datas.Count(x => x.IsFilled);
                if (filledCount > 0)
                {
                    _log.AppendLine($"自动补齐完成，补齐K线数量: {filledCount}");
                }
                TargetCandles = Utils.AggregateCandlesStrictLinQ(M1M5Datas, 1, 60);
            }

            // 数据完整性检查
            if (TargetCandles == null || TargetCandles.Count < MinCandleCount)
            {
                _log.AppendLine($"[Wait] K线不足: {TargetCandles?.Count},小于{MinCandleCount}");
                return false;
            }

            string strategyResult = "NONE";
            // --- 1. 获取配置与数据 ---
            // 统一从配置类获取阈值，方便后续回测微调
            var config = IndicatorHelper.GetConfig("H1");
            var kdHistory = IndicatorHelper.CalculateKdj(TargetCandles, "H1", 3);

            if (kdHistory == null || kdHistory.Count < 2)
            {
                _log.AppendLine("KDJ数据不足，不做决策");
                return false;
            }

            // C# 7.3 建议使用 Count - N 访问，更加明确
            var current = kdHistory[kdHistory.Count - 1];
            var previous = kdHistory[kdHistory.Count - 2];

            // --- 2. 核心决策逻辑 ---

            // 【多头决策 BUY】
            // 条件拆解：
            // 1. 状态：底部金叉 (Current K < Oversold)
            // 2. 动能：斜率大于阈值 (SlopeK > minSlope)
            // 3. 加速：当前动能强于上一小时 (SlopeK > Prev.SlopeK)
            // 4. 防纠缠：K/D 已经拉开距离 (K - D > Gap)
            if (current.IsGoldenCross && current.K < config.Oversold)
            {
                if (current.SlopeK > config.SlopeThreshold && current.SlopeK > previous.SlopeK)
                {
                    // 计算开口宽度
                    if ((current.K - current.D) > 1.5m)
                    {
                        strategyResult = "BUY TREND";
                    }
                }
            }
            // 【空头决策 SELL】
            // 条件拆解：
            // 1. 状态：顶部死叉 (Current K > Overbought)
            // 2. 动能：向下斜率绝对值够大 (SlopeK < -minSlope)
            // 3. 加速：下坠动能正在加强 (SlopeK < Prev.SlopeK)
            // 4. 防纠缠：D/K 已经拉开距离 (D - K > Gap)
            else if (current.IsDeathCross && current.K > config.Overbought)
            {
                if (current.SlopeK < -config.SlopeThreshold && current.SlopeK < previous.SlopeK)
                {
                    if ((current.D - current.K) > 1.5m)
                    {
                        strategyResult = "SELL TREND";
                    }
                }
            }

            // --- 5. 记录日志 (全量信息记录) ---

            // 1. 格式化 KDJ 历史序列：包含时间、K/D/J、斜率、开口间距以及交叉状态
            var allKdStr = string.Join("\n    ", kdHistory.Select((kd, idx) =>
                $"[{idx}] {kd.dateTime} | K:{kd.K:F2} D:{kd.D:F2} J:{kd.J:F2} | " +
                $"SlopeK:{kd.SlopeK:F2} | Gap:{Math.Abs(kd.K - kd.D):F2} | " +
                $"Cross:{(kd.IsGoldenCross ? "GOLDEN" : kd.IsDeathCross ? "DEATH" : "NONE")}"
            ));

            // 2. 组装最终日志内容
            StringBuilder fullLog = new StringBuilder();
            fullLog.AppendLine("==================== KDJ Strategy Execution Report ====================");
            fullLog.AppendLine($"[Execution Time]   : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            fullLog.AppendLine($"[Current Bar Time] : {currentDateTimeStr}");
            fullLog.AppendLine($"[Target Period]    : {period}");
            fullLog.AppendLine("-------------------- Market Data History --------------------");
            fullLog.AppendLine($"    {allKdStr}");
            fullLog.AppendLine("-------------------- Decision Metadata ----------------------");
            fullLog.AppendLine($"[Config Threshold] : Slope > {config.SlopeThreshold}, Overbought:{config.Overbought}, Oversold:{config.Oversold}");
            fullLog.AppendLine($"[Current Metrics]  : K={current.K:F2}, Slope={current.SlopeK:F2}, PrevSlope={previous.SlopeK:F2}, Gap={Math.Abs(current.K - current.D):F2}");
            fullLog.AppendLine($"[Strategy Result]  : {strategyResult}");
            fullLog.AppendLine("=======================================================================");

            //_log.AppendLine(fullLog.ToString());

            Console.WriteLine(fullLog.ToString());

            // --- 3. 结果处理 ---
            // 使用 C# 模式匹配 (7.0+) 简化字符串判断
            if (strategyResult is "BUY TREND" || strategyResult is "SELL TREND")
            {
                context["StrategyResultTrendKDJ"] = strategyResult;
                _log.AppendLine($"策略[{this.name}]触发信号: {strategyResult}, K:{current.K}, Slope:{current.SlopeK}");
                return true;
            }
            else
            {
                _log.AppendLine($"策略[{this.name}]未触发信号: K={current.K}, Slope={current.SlopeK}, 无有效趋势");
                return false;
            }
        }
    }
}
