using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uClient.Comm;

namespace uClient.Broker
{
    // 建议返回 List<KdResult> 或包含 PreviousK/D 的对象
    public class KdResult
    {
        public decimal K { get; set; }
        public decimal D { get; set; }
        public decimal J { get; set; }
        public decimal SlopeK { get; set; }     // K线斜率
        public bool IsGoldenCross { get; set; } // 金叉
        public bool IsDeathCross { get; set; }  // 死叉
        public string Period { get; set; }      // 所属周期
        public string dateTime { get; set; }
    }

    /// <summary>
    /// 策略名称：OPEN_KDJ
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
    public class KDJOpen : StrategyEA
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

            MinCandleCount = IndicatorHelper.GetRequiredCandleCount(period);
            _log.AppendLine($"动态参数[currentOrderType{currentOrderType}][currentDateTime{currentDateTimeStr}][currentPrice{currentPrice}]");
            //currentDateTimeStr = "2025-11-08 00:21:59";
            // --- 1. 获取多周期数据 ---
            // 为计算斜率，我们需要保留最近几个周期的 KD 历史，而不仅仅是最后一根
            if (string.Equals(_IsSimulateTest, "T", StringComparison.OrdinalIgnoreCase))
            {
                var M1M15Datas = DSHelper.GetRecentCandlesOptimizedFast("XAUUSD", "ONE_MIN", (MinCandleCount * 5) + 10, currentDateTimeStr, "M15");
                M1M15Datas = Utils.FillMissingM1Candles(M1M15Datas);
                int filledCount = M1M15Datas.Count(x => x.IsFilled);
                if (filledCount > 0)
                {
                    _log.AppendLine($"自动补齐完成，补齐K线数量: {filledCount}");
                }
                this.M15Candles = Utils.AggregateCandlesStrictLinQ(M1M15Datas, 1, 15);
                klineDataDuration(currentDateTimeStr, M1M15Datas);
            }
            else
            {
                var M1M15Datas = DSHelper.GetRecentCandles("XAUUSD", "ONE_MIN", (MinCandleCount * 5) + 10, currentDateTimeStr);
                M1M15Datas = Utils.FillMissingM1Candles(M1M15Datas);
                int filledCount = M1M15Datas.Count(x => x.IsFilled);
                if (filledCount > 0)
                {
                    _log.AppendLine($"自动补齐完成，补齐K线数量: {filledCount}");
                }
                this.M15Candles = Utils.AggregateCandlesStrictLinQ(M1M15Datas, 1, 15);
                klineDataDuration(currentDateTimeStr, M1M15Datas);
            }

            // 数据完整性检查
            if (M15Candles == null || M15Candles.Count < MinCandleCount)
            {
                _log.AppendLine($"[Wait] M15 K线不足: {M15Candles?.Count},小于{MinCandleCount}");
                return false;
            }

            // --- 1. 调用趋势判断方法 ---
            //TrendState h1Trend = IndicatorHelper.CheckH1Trend(currentPrice,H1Candles,2, Ema_Period, Atr_Period, 1.8m);

            // M5 周期：KDJ 信号 (计算最近几根的 KD 以便进行线性回归)
            List<KdResult> kdHistory = IndicatorHelper.CalculateKdj(M5Candles, period, SlopeWindow);
            var config = IndicatorHelper.GetConfig(period);
            // 调用升级后的斜率判断方法
            // 阈值设为 2.5m (表示 K线穿过D线时，每根棒线拉开 2.5 个刻度的距离)
            //TrendState kdjTrend = GetSharpCrossSignal(kdHistory,1.2m);
            //TrendState kdjTrend = GetSharpCrossSignal2(kdHistory, 1.2m);
            //TrendState kdjTrend = GetSharpCrossSignal3(kdHistory.Last(), kdHistory[kdHistory.Count - 2],3);
            TradeSignal kdjTrend = IndicatorHelper.GetSharpCrossSignal4(kdHistory, config.SlopeThreshold, config.Oversold, config.Overbought);
            // --- 3. 交易决策 ---
            string action = "NONE";

            // 5. 统一的出场与入场逻辑 (M5 交叉即出场)
            if (string.IsNullOrEmpty(currentOrderType) || "CLOSE_BUY".Equals(currentOrderType) || "CLOSE_SELL".Equals(currentOrderType) || "CLOSE_ALL".Equals(currentOrderType))
            {
                if (kdjTrend == TradeSignal.TrendBearish)
                {
                    action = "SELL";
                    _log.AppendLine($"[Exit] 空头趋势,开空单;");
                }
                else if (kdjTrend == TradeSignal.TrendBullish)
                {
                    action = "BUY";
                    _log.AppendLine($"[Exit] 多头趋势,开多单;");
                }
            }

            // 记录详细日志辅助分析
            var allKdStr = string.Join(", ", kdHistory.Select((kd, idx) => $"[{idx}]dateTime={kd.dateTime},K={kd.K:F2},D={kd.D:F2}"));
            //_log.AppendLine($"[Time={timeStr}][KD History: {allKdStr}][H1={h1Trend}][KDJ={kdjTrend}][Action={action}]");
            _log.AppendLine($"[Time={currentDateTimeStr}][KD History: {allKdStr}][KDJ={kdjTrend}][Action={action}]");
            //Console.WriteLine($"[Time={timeStr}][KD History: {allKdStr}][H1={h1Trend}][KDJ_Signal={kdjTrend}][Action={action}]");
            context["StrategyResultOPENKDJ"] = action;
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
