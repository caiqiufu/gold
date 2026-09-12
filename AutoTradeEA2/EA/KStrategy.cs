using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uClient.Comm;

namespace uClient.Broker
{

    /// <summary>
    /// K_EA
    /// 意味着市场正在积蓄能量，等待爆发。它常用于 日内交易（Intraday Trading） 
    /// 中作为 突破交易（Breakout Trading） 的前兆
    /// </summary>
    public class KStrategy : StrategyEA
    {
        // ------------------- 策略常量参数 -------------------
        // K 线列表 (需要从外部数据源注入)
        public List<Candle> M5Candles { get; set; } // M5 周期 (入场信号可选)
        public List<Candle> M5KDCandles { get; set; } // M5 周期 (入场信号可选)

        // ------------------- 策略常量参数 -------------------
        // 策略参数
        //最少需要的K线数量
        private int MinCandleCount = 25;
        //KDJ指标的计算回顾周期
        private int Kdj_N = 14;
        //KDJ周期
        private const string period = "M5";
        //回归窗口
        private const int SlopeWindow = 6;


        private StringBuilder _log = new StringBuilder();
        /// <summary>
        /// 返回策略的实时状态描述，供前端或 UI 显示
        /// </summary>
        public override string ToDesc() => _log.ToString();

        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {

            _log.Clear();
            string _IsSimulateTest = context["IsSimulateTest"].ToString();

            decimal Oversold_Level = Convert.ToDecimal(this.configParam["Oversold_Level"]);
            decimal Overbought_Level = Convert.ToDecimal(this.configParam["Overbought_Level"]);
            // 从上下文中提取必要数据
            string currentOrderType = context.ContainsKey("CurrentOrderType") ? context["CurrentOrderType"].ToString() : "NONE";
            decimal currentPrice = decimal.Parse(context["price"].ToString());
            string currentDateTimeStr = context.ContainsKey("CurrentDateTime") ? context["CurrentDateTime"].ToString() : DateTime.Now.ToString();
            int trendCount = Convert.ToInt32(this.configParam["TrendCount"]);


            string EMAATRPeriod = this.configParam["EMAATRPeriod"];
            int EMAperiod = Convert.ToInt32(this.configParam["EMAPeriod"]);
            int ATRperiod = Convert.ToInt32(this.configParam["ATRPeriod"]);
            string analysisDataType = context["analysisDataType"].ToString();

            MinCandleCount = IndicatorHelper.GetRequiredCandleCount(period);

            if (string.Equals(_IsSimulateTest, "T", StringComparison.OrdinalIgnoreCase))
            {
                //var sw = Stopwatch.StartNew(); // 开始计时
                var M1M5Datas = DSHelper.GetRecentCandlesOptimizedFast("XAUUSD", "ONE_MIN", (MinCandleCount * 5) + 10, currentDateTimeStr, "M5");
                M1M5Datas = Utils.FillMissingM1Candles(M1M5Datas);
                int filledCount = M1M5Datas.Count(x => x.IsFilled);
                if (filledCount > 0)
                {
                    _log.AppendLine($"自动补齐完成，补齐K线数量: {filledCount}");
                }
                //sw.Stop(); // 停止计时
                //Console.WriteLine($"GetRecentCandlesOptimized + AggregateCandlesLinQ 消耗时间: {sw.ElapsedMilliseconds} ms");
                this.M5Candles = Utils.AggregateCandlesStrictLinQ(M1M5Datas, 1, 5);
                klineDataDuration(currentDateTimeStr, M1M5Datas);

            }
            else
            {
                var M1M5Datas = DSHelper.GetRecentCandles("XAUUSD", "ONE_MIN", (MinCandleCount * 5) + 10, currentDateTimeStr);
                M1M5Datas = Utils.FillMissingM1Candles(M1M5Datas);
                int filledCount = M1M5Datas.Count(x => x.IsFilled);
                if (filledCount > 0)
                {
                    _log.AppendLine($"自动补齐完成，补齐K线数量: {filledCount}");
                }
                this.M5Candles = Utils.AggregateCandlesStrictLinQ(M1M5Datas, 1, 5);
                klineDataDuration(currentDateTimeStr, M1M5Datas);
            }

            // 数据完整性检查
            if (M5Candles == null || M5Candles.Count < MinCandleCount)
            {
                _log.AppendLine($"[Wait] M5 K线不足: {M5Candles?.Count},小于{MinCandleCount}");
                return false;
            }

            // M5 周期：KDJ 信号 (计算最近几根的 KD 以便进行线性回归)
            List<KdResult> kdHistory = IndicatorHelper.CalculateKdj(M5Candles, period, SlopeWindow);
            var config = IndicatorHelper.GetConfig(period);
            // 调用斜率判断方法判定 H1 级别的信号
            // 阈值设为 1.8m (H1 周期较长，指标变动相对平滑，阈值可比 M5 略低)
            TradeSignal kdjTrend = IndicatorHelper.GetSharpCrossSignal4(kdHistory, config.SlopeThreshold, config.Oversold, config.Overbought);

            // --- 4. 交易决策 ---
            string action = "NONE";

            // 入场逻辑：H1 基础趋势向上 + H1 出现强斜率金叉
            if (kdjTrend == TradeSignal.TrendBullish)
            {
                action = "BUY";
            }
            // 入场逻辑：H1 基础趋势向下 + H1 出现强斜率死叉
            if (kdjTrend == TradeSignal.TrendBearish)
            {
                action = "SELL";
            }

            // --- 5. 记录日志 ---
            var allKdStr = string.Join(", ", kdHistory.Select((kd, idx) => $"[{idx}]dateTime={kd.dateTime},K={kd.K:F2},D={kd.D:F2}"));
            _log.AppendLine($"[Time={currentDateTimeStr}][KD History: {allKdStr}][KDJ_Signal={kdjTrend}][Action={action}]");

            //Utils.LogAllCandles(M5Candles, "M5Candles List");
            //Utils.LogKdjReport(kdHistory, config, action, period, currentDateTimeStr);

            context["StrategyResultK"] = action;
            return action != "NONE";
        }

        public void klineDataDuration(string timeStr, List<Candle> candles)
        {
            if (candles != null && candles.Any())
            {
                var startTime = candles.First().Time.ToString("yyyy-MM-dd HH:mm:ss");
                var endTime = candles.Last().Time.ToString("yyyy-MM-dd HH:mm:ss");
                // 完善日志字符串
                _log.AppendLine($"M1 K线获取成功 - 数量: {candles.Count}, 起始时间: {startTime}, 结束时间: {endTime}");

                // ✅ 输出所有 M1 K线详情
                /*foreach (var c in candles)
                {
                    _log.AppendLine(
                        $"Time: {c.Time:yyyy-MM-dd HH:mm:ss} | " +
                        $"O: {c.Open:F2} | " +
                        $"H: {c.High:F2} | " +
                        $"L: {c.Low:F2} | " +
                        $"C: {c.Close:F2} | " +
                        $"V: {c.Volume}"
                    );
                }

                _log.AppendLine("================================================");
                _log.AppendLine("");*/

                // ✅ 输出 M5 聚合数据
                if (this.M5Candles != null && this.M5Candles.Any())
                {
                    var m5Start = this.M5Candles.First().Time.ToString("yyyy-MM-dd HH:mm:ss");
                    var m5End = this.M5Candles.Last().Time.ToString("yyyy-MM-dd HH:mm:ss");
                    _log.AppendLine($"M5 聚合完成 - 数量: {this.M5Candles.Count}, 起始时间: {m5Start}, 结束时间: {m5End}");

                    /*foreach (var c in this.M5Candles)
                    {
                        _log.AppendLine(
                            $"Time: {c.Time:yyyy-MM-dd HH:mm:ss} | " +
                            $"O: {c.Open:F2} | " +
                            $"H: {c.High:F2} | " +
                            $"L: {c.Low:F2} | " +
                            $"C: {c.Close:F2} | " +
                            $"V: {c.Volume}"
                        );
                    }

                    _log.AppendLine("================================================");
                    _log.AppendLine("");*/
                }
            }
            else
            {
                _log.AppendLine($"警告: 未能获取到 XAUUSD 的 M1 原始数据。请求参数: timeStr={timeStr}, count={MinCandleCount * 5}");
            }
        }
    }
}
