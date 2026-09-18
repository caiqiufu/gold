using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uClient.Comm;

namespace uClient.Broker
{
    /// <summary>
    /// 策略名称：H_EA
    ///
    /// 核心交易逻辑 V7：
    ///
    /// 1. H4 趋势过滤：
    ///    - H4 EMA + ATR 趋势结果由外部 StrategyResultTrendEMA 提供。
    ///    - H4 只负责确定交易方向，不负责精确入场。
    ///
    /// 2. M15 KDJ Setup：
    ///    BUY：
    ///      - M15 最近窗口曾进入 J 极端超卖区；
    ///      - M15 K/D 在低于中轴的位置形成金叉；
    ///      - J 开始回升。
    ///
    ///    SELL：
    ///      - M15 最近窗口曾进入 J 极端超买区；
    ///      - M15 K/D 在高于中轴的位置形成死叉；
    ///      - J 开始回落。
    ///
    /// 3. M5 KDJ Trigger：
    ///    - M15 Setup 之后，M5 必须再次出现同方向 K/D 交叉；
    ///    - M5 J 必须与信号方向一致；
    ///    - M5 K/D 必须形成最小有效 Gap；
    ///    - M15 -> M5 必须保持严格时间先后。
    ///
    /// 4. 最终：
    ///    H4 定方向
    ///       ↓
    ///    M15 找回调结束
    ///       ↓
    ///    M5 精确触发
    ///       ↓
    ///    BUY / SELL
    ///
    /// 注意：
    /// - 本策略不再使用 M30 KDJ。
    /// - CalculateKdj() 本身不需要修改，J 已经由 IndicatorHelper 计算。
    /// </summary>
    public class HStrategy : StrategyEA
    {
        // ============================================================
        // K 线列表
        // ============================================================

        public List<Candle> M1Candles { get; set; }
        public List<Candle> M5Candles { get; set; }
        public List<Candle> M15Candles { get; set; }
        public List<Candle> M30Candles { get; set; }
        public List<Candle> H1Candles { get; set; }

        // ============================================================
        // KDJ / 数据参数
        // ============================================================

        private int MinCandleCount = 25;

        private int Kdj_N = 14;

        private const string M5Period = "M5";
        private const string M15Period = "M15";

        // M5 KDJ 输出最近多少根
        private const int M5HistoryWindow = 6;

        // M15 Setup 回看窗口
        private const int M15HistoryWindow = 6;

        // ------------------------------------------------------------
        // KDJ 极值参数
        // ------------------------------------------------------------

        // M15 J <= 20：BUY 极端超卖
        private const decimal M15BuyExtremeJ = 20m;

        // M15 J >= 80：SELL 极端超买
        private const decimal M15SellExtremeJ = 80m;

        // M15 K/D 金叉必须位于 50 以下
        private const decimal M15BuyCrossMaxK = 50m;

        // M15 K/D 死叉必须位于 50 以上
        private const decimal M15SellCrossMinK = 50m;

        // M5 K/D 最小开口，过滤极弱交叉
        private const decimal M5MinGap = 0.50m;

        // M15 Setup 后最多允许等待多少根 M5
        // 3 根 M5 = 15 分钟
        private const int MaxM5BarsAfterM15 = 3;

        private StringBuilder _log = new StringBuilder();

        /// <summary>
        /// 策略核心执行函数
        /// </summary>
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            _log.Clear();

            string _IsSimulateTest =
                context.ContainsKey("IsSimulateTest")
                    ? context["IsSimulateTest"].ToString()
                    : "F";

            decimal Oversold_Level =
                Convert.ToDecimal(this.configParam["Oversold_Level"]);

            decimal Overbought_Level =
                Convert.ToDecimal(this.configParam["Overbought_Level"]);

            string currentOrderType =
                context.ContainsKey("CurrentOrderType")
                    ? context["CurrentOrderType"].ToString()
                    : "NONE";

            string strategyResultTrendEMA =
                context.ContainsKey("StrategyResultTrendEMA")
                    ? context["StrategyResultTrendEMA"].ToString()
                    : "NONE";

            decimal currentPrice =
                decimal.Parse(context["price"].ToString());

            string currentDateTimeStr =
                context.ContainsKey("CurrentDateTime")
                    ? context["CurrentDateTime"].ToString()
                    : DateTime.Now.ToString();

            // ========================================================
            // 1. 基础参数
            // ========================================================

            MinCandleCount =
                Math.Max(
                    IndicatorHelper.GetRequiredCandleCount(M5Period),
                    IndicatorHelper.GetRequiredCandleCount(M15Period));

            _log.AppendLine(
                $"动态参数[currentOrderType={currentOrderType}]" +
                $"[strategyResultTrendEMA={strategyResultTrendEMA}]" +
                $"[currentDateTime={currentDateTimeStr}]");

            // ========================================================
            // 2. H4 趋势必须明确
            // ========================================================

            if (string.IsNullOrEmpty(strategyResultTrendEMA) ||
                strategyResultTrendEMA == "NONE")
            {
                _log.AppendLine(
                    $"[Wait] 不是明确趋势行情: {strategyResultTrendEMA}");

                return false;
            }

            // ========================================================
            // 3. 获取 M1 数据
            //
            // M15 KDJ 需要明显多于 M5 的历史数据。
            //
            // GetRequiredCandleCount("M15") =
            // KDJ(14) + 40 根预热
            //
            // 因此必须按 M15 * 15 分钟换算 M1 数量。
            // ========================================================

            int requiredM15Candles =
                IndicatorHelper.GetRequiredCandleCount(M15Period);

            int requiredM5Candles =
                IndicatorHelper.GetRequiredCandleCount(M5Period);

            int requiredM1ForM15 =
                requiredM15Candles * 15 + 30;

            int requiredM1ForM5 =
                requiredM5Candles * 5 + 10;

            int requiredM1Count =
                Math.Max(requiredM1ForM15, requiredM1ForM5);

            List<Candle> M1M5Datas;

            if (string.Equals(
                _IsSimulateTest,
                "T",
                StringComparison.OrdinalIgnoreCase))
            {
                M1M5Datas =
                    DSHelper.GetRecentCandlesOptimizedFast(
                        "XAUUSD",
                        "ONE_MIN",
                        requiredM1Count,
                        currentDateTimeStr,
                        "M15");
            }
            else
            {
                M1M5Datas =
                    DSHelper.GetRecentCandles(
                        "XAUUSD",
                        "ONE_MIN",
                        requiredM1Count,
                        currentDateTimeStr);
            }

            if (M1M5Datas == null || M1M5Datas.Count == 0)
            {
                _log.AppendLine(
                    $"[Wait] 未能获取 XAUUSD M1 数据，Required={requiredM1Count}");

                return false;
            }

            // ========================================================
            // 4. 补齐 M1 缺失数据
            // ========================================================

            M1M5Datas =
                Utils.FillMissingM1Candles(M1M5Datas);

            int filledCount =
                M1M5Datas.Count(x => x.IsFilled);

            if (filledCount > 0)
            {
                _log.AppendLine(
                    $"自动补齐完成，补齐K线数量: {filledCount}");
            }

            this.M1Candles = M1M5Datas;

            // ========================================================
            // 5. M1 → M5 / M15
            // ========================================================

            this.M5Candles =
                Utils.AggregateCandlesStrictLinQ(
                    M1M5Datas,
                    1,
                    5);

            this.M15Candles =
                Utils.AggregateCandlesStrictLinQ(
                    M1M5Datas,
                    1,
                    15);

            klineDataDuration(
                currentDateTimeStr,
                M1M5Datas);

            // ========================================================
            // 6. 数据完整性检查
            // ========================================================

            if (M5Candles == null ||
                M5Candles.Count < requiredM5Candles)
            {
                _log.AppendLine(
                    $"[Wait] M5 K线不足: {M5Candles?.Count}, " +
                    $"需要至少 {requiredM5Candles}");

                return false;
            }

            if (M15Candles == null ||
                M15Candles.Count < requiredM15Candles)
            {
                _log.AppendLine(
                    $"[Wait] M15 K线不足: {M15Candles?.Count}, " +
                    $"需要至少 {requiredM15Candles}");

                return false;
            }

            // ========================================================
            // 7. 计算 M15 KDJ
            // ========================================================

            List<KdResult> m15KdHistory =
                IndicatorHelper.CalculateKdj(
                    M15Candles,
                    M15Period,
                    M15HistoryWindow);

            if (m15KdHistory == null ||
                m15KdHistory.Count < 2)
            {
                _log.AppendLine(
                    $"[Wait] M15 KDJ 数据不足: {m15KdHistory?.Count}");

                return false;
            }

            // ========================================================
            // 8. 计算 M5 KDJ
            // ========================================================

            List<KdResult> m5KdHistory =
                IndicatorHelper.CalculateKdj(
                    M5Candles,
                    M5Period,
                    M5HistoryWindow);

            if (m5KdHistory == null ||
                m5KdHistory.Count < 2)
            {
                _log.AppendLine(
                    $"[Wait] M5 KDJ 数据不足: {m5KdHistory?.Count}");

                return false;
            }

            // ========================================================
            // 9. 获取 KDJ 配置
            // ========================================================

            var m5Config =
                IndicatorHelper.GetConfig(M5Period);

            var m15Config =
                IndicatorHelper.GetConfig(M15Period);

            // --------------------------------------------------------
            // 注意：
            // config.Oversold / Overbought 仍然保留，
            // 但新的 Signal6 多周期逻辑主要使用 J=20/80，
            // 防止 M5 的 18/82 配置直接决定 M15 Setup。
            // --------------------------------------------------------

            decimal buyExtremeJ =
                M15BuyExtremeJ;

            decimal sellExtremeJ =
                M15SellExtremeJ;

            // 如果外部配置有明显不同，可记录下来辅助审计，
            // 但不直接改变新的多周期逻辑。
            _log.AppendLine(
                $"KDJ Config[M5 Oversold={m5Config.Oversold}, " +
                $"M5 Overbought={m5Config.Overbought}, " +
                $"M15 Oversold={m15Config.Oversold}, " +
                $"M15 Overbought={m15Config.Overbought}]");

            // ========================================================
            // 10. H4 + M15 Setup + M5 Trigger
            // ========================================================

            TradeSignal kdjTrend =
                IndicatorHelper.GetSharpCrossSignal6(
                    m15KdHistory,
                    m5KdHistory,
                    strategyResultTrendEMA,
                    buyExtremeJ,
                    sellExtremeJ,
                    50m,
                    M5MinGap,
                    M15HistoryWindow,
                    MaxM5BarsAfterM15);

            // ========================================================
            // 11. 交易决策
            // ========================================================

            string action = "NONE";

            if (action == "NONE")
            {
                if (kdjTrend == TradeSignal.TrendBullish)
                    action = "BUY";
                else if (kdjTrend == TradeSignal.TrendBearish)
                    action = "SELL";
            }

            // ========================================================
            // 12. 详细日志
            // ========================================================

            string m15KdStr =
                string.Join(
                    ", ",
                    m15KdHistory.Select(
                        (kd, idx) =>
                            $"[{idx}]dateTime={kd.dateTime}," +
                            $"K={kd.K:F2}," +
                            $"D={kd.D:F2}," +
                            $"J={kd.J:F2}"));

            string m5KdStr =
                string.Join(
                    ", ",
                    m5KdHistory.Select(
                        (kd, idx) =>
                            $"[{idx}]dateTime={kd.dateTime}," +
                            $"K={kd.K:F2}," +
                            $"D={kd.D:F2}," +
                            $"J={kd.J:F2}"));

            _log.AppendLine(
                $"[Time={currentDateTimeStr}]" +
                $"[H4={strategyResultTrendEMA}]" +
                $"[M15 KD History: {m15KdStr}]" +
                $"[M5 KD History: {m5KdStr}]" +
                $"[KDJ={kdjTrend}]" +
                $"[Action={action}]");

            _log.AppendLine(
                $"[KDJ Rules]" +
                $" M15BuyExtremeJ<={buyExtremeJ}" +
                $" M15SellExtremeJ>={sellExtremeJ}" +
                $" M15BuyCrossK<={M15BuyCrossMaxK}" +
                $" M15SellCrossK>={M15SellCrossMinK}" +
                $" M5MinGap={M5MinGap}" +
                $" MaxM5BarsAfterM15={MaxM5BarsAfterM15}");

            context["StrategyResultH"] = action;

            return action != "NONE";
        }

        /// <summary>
        /// 输出 M1/M5/M15 数据范围。
        /// </summary>
        public void klineDataDuration(
            string timeStr,
            List<Candle> candles)
        {
            if (candles != null && candles.Any())
            {
                var startTime =
                    candles.First().Time
                        .ToString("yyyy-MM-dd HH:mm:ss");

                var endTime =
                    candles.Last().Time
                        .ToString("yyyy-MM-dd HH:mm:ss");

                _log.AppendLine(
                    $"M1 K线获取成功 - 数量: {candles.Count}, " +
                    $"起始时间: {startTime}, " +
                    $"结束时间: {endTime}");

                if (this.M5Candles != null &&
                    this.M5Candles.Any())
                {
                    var m5Start =
                        this.M5Candles.First().Time
                            .ToString("yyyy-MM-dd HH:mm:ss");

                    var m5End =
                        this.M5Candles.Last().Time
                            .ToString("yyyy-MM-dd HH:mm:ss");

                    _log.AppendLine(
                        $"M5 聚合完成 - 数量: {this.M5Candles.Count}, " +
                        $"起始时间: {m5Start}, " +
                        $"结束时间: {m5End}");
                }

                if (this.M15Candles != null &&
                    this.M15Candles.Any())
                {
                    var m15Start =
                        this.M15Candles.First().Time
                            .ToString("yyyy-MM-dd HH:mm:ss");

                    var m15End =
                        this.M15Candles.Last().Time
                            .ToString("yyyy-MM-dd HH:mm:ss");

                    _log.AppendLine(
                        $"M15 聚合完成 - 数量: {this.M15Candles.Count}, " +
                        $"起始时间: {m15Start}, " +
                        $"结束时间: {m15End}");
                }
            }
            else
            {
                _log.AppendLine(
                    $"警告: 未能获取到 XAUUSD 的 M1 原始数据。 " +
                    $"请求参数: timeStr={timeStr}");
            }
        }

        /// <summary>
        /// 返回策略实时状态描述。
        /// </summary>
        public override string ToDesc()
            => _log.ToString();
    }
}
