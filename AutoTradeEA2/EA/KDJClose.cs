using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uClient.Comm;

namespace uClient.Broker
{

    /// <summary>
    /// 策略名称：CLOSE_KDJ
    /// 核心交易逻辑：
    /// 1. 趋势过滤 (H1周期)：
    ///    - 使用 EMA(20) 作为基准趋势线。
    ///    - 使用 ATR(14) 衡量波动，并限制价格与均线的偏离度，防止极端行情下盲目追涨杀跌。
    /// 2. 动能触发 (M5周期)：
    ///    - 计算 KDJ(14, 3, 3) 指标。
    ///    - 寻找超买/超卖区域内的金叉或死叉作为入场点。
    /// 3. 退出机制：
    ///    - 当 M5 动能发生反转交叉时，平掉现有仓位以锁定利润或控制亏损。
    /// </summary>
    public class KDJClose : StrategyEA
    {


        // K 线列表 (需要从外部数据源注入)
        public List<Candle> M1Candles { get; set; }
        public List<Candle> M5Candles { get; set; }

        public List<Candle> M15Candles { get; set; }
        public List<Candle> M30Candles { get; set; } // M5 周期 (入场信号可选)
        public List<Candle> H1Candles { get; set; } // H1 周期 (反转形态、共振、平仓)
        // ------------------- 策略常量参数 -------------------
        // 策略参数
        //最少需要的K线数量
        private int MinCandleCount = 25;
        //KDJ指标的计算回顾周期
        private int Kdj_N = 14;
        //KDJ周期
        private const string period = "M15";
        //回归窗口
        private const int SlopeWindow = 6;

        private StringBuilder _log = new StringBuilder();

        /// <summary>
        /// 策略核心执行函数，由系统定时或按Tick触发
        /// </summary>
        /// <param name="eaChart">图表对象</param>
        /// <param name="context">上下文信息，包含当前价格、持仓状态等</param>
        /// <returns>是否有操作执行</returns>
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

            string action = "NONE";

            // 5. 统一的出场与入场逻辑 (M5 交叉即出场)
            if (string.Equals(currentOrderType, "BUY") || string.Equals(currentOrderType, "SELL"))
            {
                if (currentOrderType == "BUY" && kdjTrend == TradeSignal.TrendBearish)
                {
                    action = "CLOSE_BUY";
                    _log.AppendLine($"[Exit] M5死叉出现，多单平仓;");
                }
                else if (currentOrderType == "SELL" && kdjTrend == TradeSignal.TrendBullish)
                {
                    action = "CLOSE_SELL";
                    _log.AppendLine($"[Exit] M5金叉出现，空单平仓;");
                }
            }

            // --- 5. 记录日志 ---
            var allKdStr = string.Join(", ", kdHistory.Select((kd, idx) => $"[{idx}]dateTime={kd.dateTime},K={kd.K:F2},D={kd.D:F2}"));
            _log.AppendLine($"[Time={currentDateTimeStr}][KD History: {allKdStr}][KDJ_Signal={kdjTrend}][Action={action}]");
            context["StrategyResultCLOSEKDJ"] = action;
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

                // 如果需要记录 M5 聚合后的状态
                if (this.M15Candles != null && this.M15Candles.Any())
                {
                    var m5Start = this.M15Candles.First().Time.ToString("yyyy-MM-dd HH:mm:ss");
                    var m5End = this.M15Candles.Last().Time.ToString("yyyy-MM-dd HH:mm:ss");
                    _log.AppendLine($"M15 聚合完成 - 数量: {this.M15Candles.Count}, 起始时间: {m5Start}, 结束时间: {m5End}");
                }
            }
            else
            {
                _log.AppendLine($"警告: 未能获取到 XAUUSD 的 M1 原始数据。请求参数: timeStr={currentDateTimeStr}, count={MinCandleCount * 5}");
            }
        }

        /// <summary>
        /// 返回策略的实时状态描述，供前端或 UI 显示
        /// </summary>
        public override string ToDesc() => _log.ToString();
    }
}
