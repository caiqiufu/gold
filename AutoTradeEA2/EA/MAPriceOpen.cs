using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uClient.Comm;

namespace uClient.Broker
{
    /// <summary>
    /// OPEN_MA_PRICE
    /// EMA 判断趋势,大于EMA价格买入,小于EMA价格卖出
    /// 参数:BUY TREND:买入趋势,SELL TREND卖出趋势,NO TREND:无交易趋势
    /// 返回true,未超过开仓数量,可以开仓,否则不能开仓
    /// </summary>
    public class MAPriceOpen : StrategyEA
    {
        // K 线列表 (需要从外部数据源注入)
        public List<Candle> M1Candles { get; set; }
        public List<Candle> M5Candles { get; set; }
        public List<Candle> M30Candles { get; set; } // M5 周期 (入场信号可选)
        public List<Candle> H1Candles { get; set; } // H1 周期 (反转形态、共振、平仓)
        // ------------------- 策略常量参数 -------------------
        // 策略参数

        private StringBuilder _log = new StringBuilder();
        /// <summary>
        /// 返回策略的实时状态描述，供前端或 UI 显示
        /// </summary>
        public override string ToDesc() => _log.ToString();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            _log.Clear();

            string currentOrderType = context["CurrentOrderType"].ToString();
            string _IsSimulateTest = context["IsSimulateTest"].ToString();
            string EMAATRPeriod = this.configParam["EMAATRPeriod"];
            int EMAperiod = Convert.ToInt32(this.configParam["EMAPeriod"]);
            int ATRperiod = Convert.ToInt32(this.configParam["ATRPeriod"]);
            string analysisDataType = context["analysisDataType"].ToString();

            decimal currentPrice = decimal.Parse(context["price"].ToString());
            int EMAPriceDiff = Convert.ToInt32(this.configParam["EMAPriceDiff"]);

            string currentDateTimeStr = context.ContainsKey("CurrentDateTime")
                ? context["CurrentDateTime"].ToString()
                : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            int M1KCount = IndicatorHelper.CalculateRequiredCount("M30", "M1", 1, EMAperiod, ATRperiod);

            if (string.Equals(_IsSimulateTest, "T", StringComparison.OrdinalIgnoreCase))
            {
                var M1Datas = DSHelper.GetRecentCandlesOptimizedFast("XAUUSD", "ONE_MIN", M1KCount + 150, currentDateTimeStr, "M30");
                this.M30Candles = Utils.AggregateCandlesStrictLinQ(M1Datas, 1, 30);
                klineDataDuration(currentDateTimeStr, M30Candles);
            }
            else
            {
                var M1Datas = DSHelper.GetRecentCandles("XAUUSD", "ONE_MIN", M1KCount + 150, currentDateTimeStr);
                this.M30Candles = Utils.AggregateCandlesStrictLinQ(M1Datas, 1, 30);
                klineDataDuration(currentDateTimeStr, M30Candles);
            }

            if (M30Candles == null || M30Candles.Count < Math.Max(EMAperiod, ATRperiod))
            {
                _log.AppendLine($"M30 K线数据不足 EMA:{EMAperiod} ATR:{ATRperiod}");
                return false;
            }
            _log.AppendLine($"动态参数[currentOrderType{currentOrderType}][currentDateTime{currentDateTimeStr}][currentPrice{currentPrice}]");
            // ===============================
            // 1️⃣ 计算 EMA
            // ===============================
            decimal emaPrice = IndicatorHelper.CalculateStandardEma(
                M30Candles.Select(c => c.Close).ToList(),
                EMAperiod);

            // ===============================
            // 2️⃣ 计算 ATR（用于动态波动判断）
            // ===============================
            decimal atrValue = IndicatorHelper.CalculateATR(M30Candles, ATRperiod);

            // 动态缓冲区（可调系数）
            decimal dynamicBuffer = atrValue * 1.2m;

            _log.AppendLine(
                $"动态参数[EMAperiod:{EMAperiod}]" +
                $"[ATRperiod:{ATRperiod}]" +
                $"[ATR:{atrValue:F2}]" +
                $"[Buffer:{dynamicBuffer:F2}]" +
                $"[EMAPriceDiff:{EMAPriceDiff}]" +
                $"[Time:{currentDateTimeStr}]" +
                $"[Price:{currentPrice}]" +
                $"[EMA:{emaPrice}]");

            // ===============================
            // 3️⃣ 交易决策（波动自适应）
            // ===============================
            string action = "NONE";

            if (string.IsNullOrEmpty(currentOrderType) ||
                "CLOSE_BUY".Equals(currentOrderType) ||
                "CLOSE_SELL".Equals(currentOrderType) ||
                "CLOSE_ALL".Equals(currentOrderType))
            {
                decimal upperTrigger = emaPrice + EMAPriceDiff;
                decimal lowerTrigger = emaPrice - EMAPriceDiff;

                // 上破 EMA
                if (currentPrice >= upperTrigger &&
                    currentPrice <= upperTrigger + dynamicBuffer)
                {
                    action = "EMA_PRICE_ABOVE";
                    _log.AppendLine(
                        $"价格突破EMA上方 " +
                        $"[Price:{currentPrice}] " +
                        $"[Trigger:{upperTrigger}] " +
                        $"[Buffer:{dynamicBuffer:F2}]");
                }
                // 下破 EMA
                else if (currentPrice <= lowerTrigger &&
                         currentPrice >= lowerTrigger - dynamicBuffer)
                {
                    action = "EMA_PRICE_BELOW";
                    _log.AppendLine(
                        $"价格跌破EMA下方 " +
                        $"[Price:{currentPrice}] " +
                        $"[Trigger:{lowerTrigger}] " +
                        $"[Buffer:{dynamicBuffer:F2}]");
                }
                else
                {
                    _log.AppendLine(
                        $"价格[currentPrice{currentPrice}]" +
                        $"不在EMA突破范围[{upperTrigger}-{upperTrigger + dynamicBuffer}]" +
                        $"不在EMA跌破范围[{lowerTrigger - dynamicBuffer}-{lowerTrigger}]" +
                        $"不符合EMA突破");
                }
            }

            context["StrategyResultEMAPrice"] = action;
            return action != "NONE";
        }

        public void klineDataDuration(string currentDateTimeStr, List<Candle> candles)
        {
            // 检查数据是否存在以避免 InvalidOperationException
            if (candles != null && candles.Any())
            {
                // 获取起始和结束 K 线的时间戳
                // 假设 Candle 对象具有 Time 或 OpenTime 属性
                var startTime = candles.First().Time.ToString("yyyy-MM-dd HH:mm:ss");
                var endTime = candles.Last().Time.ToString("yyyy-MM-dd HH:mm:ss");

                // 完善日志字符串
                _log.AppendLine($"M1 K线获取成功 - 数量: {candles.Count}, 起始时间: {startTime}, 结束时间: {endTime}");

                // 如果需要记录 M30 聚合后的状态
                if (this.M30Candles != null && this.M30Candles.Any())
                {
                    var m30Start = this.M30Candles.First().Time.ToString("yyyy-MM-dd HH:mm:ss");
                    var m30End = this.M30Candles.Last().Time.ToString("yyyy-MM-dd HH:mm:ss");
                    _log.AppendLine($"M30 聚合完成 - 数量: {this.M30Candles.Count}, 起始时间: {m30Start}, 结束时间: {m30End}");

                    /*foreach (var c in this.M30Candles)
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
                _log.AppendLine($"警告: 未能获取到 XAUUSD 的 M1 原始数据。请求参数: timeStr={currentDateTimeStr}");
            }
        }
    }
}
