using System;
using System.Collections.Generic;
using System.Linq;
using uClient.Comm;

namespace uClient.Broker
{
    // 假设的 KD 计算结果结构
    public struct StochasticResult
    {
        public decimal K { get; set; }
        public decimal D { get; set; }
    }

    /// <summary>
    /// 指标计算助手类 (已包含完整的 KD 计算逻辑)
    /// </summary>
    public static class IndicatorHelper
    {
        public class PeriodConfig
        {
            // C# 7.3 支持只读自动属性（通过构造函数赋值）
            // 这能保证配置对象在获取后不会被意外修改，安全性更高
            public int KdjN { get; }
            public decimal SlopeThreshold { get; }
            public decimal Overbought { get; }
            public decimal Oversold { get; }

            public PeriodConfig(int kdjN, decimal slopeThreshold, decimal overbought, decimal oversold)
            {
                KdjN = kdjN;
                SlopeThreshold = slopeThreshold;
                Overbought = overbought;
                Oversold = oversold;
            }
        }

        public static PeriodConfig GetConfig(string period)
        {
            // C# 7.3 必须使用传统的 switch 语句
            string p = (period ?? "M5").ToUpper();
            switch (p)
            {
                case "H1":
                    return new PeriodConfig(14, 2.0m, 80m, 20m);
                case "H4":
                    return new PeriodConfig(14, 3.0m, 80m, 20m);
                case "M5":
                    return new PeriodConfig(14, 1.2m, 82m, 18m);
                case "M15":
                case "M30":
                    return new PeriodConfig(14, 1.5m, 75m, 25m);
                default:
                    return new PeriodConfig(14, 1.5m, 75m, 25m);
            }
        }

        /// <summary>
        /// 确保与主流软件一致的 EMA 计算方法
        /// </summary>
        /// <param name="prices">价格序列</param>
        /// <param name="period">周期 (N)</param>
        /// <returns>最后一根的 EMA 值</returns>
        public static decimal CalculateStandardEma(List<decimal> prices, int period)
        {
            if (prices == null || prices.Count < period) return 0;

            // 1. 计算平滑因子 k（类型转换很重要，确保高精度计算）
            decimal k = 2.0m / (period + 1);

            // 2. 初始化种子值：大多数标准算法使用前 N 个周期的 SMA 作为第一个 EMA
            decimal ema = prices.Take(period).Average();

            // 3. 从第 N 个元素开始迭代（标准公式：EMA = (Close - EMA_prev) * k + EMA_prev）
            for (int i = period; i < prices.Count; i++)
            {
                ema = (prices[i] - ema) * k + ema;
            }

            // 4. 舍入处理：交易软件内部通常保留很多位，仅在显示时舍入
            return Math.Round(ema, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 计算 ATR (平均真实波幅)
        /// </summary>
        public static decimal CalculateATR(List<Candle> candles, int period)
        {
            if (candles.Count <= 1) return 0;

            List<decimal> trList = new List<decimal>();
            // 计算真实波幅 TR
            for (int i = 1; i < candles.Count; i++)
            {
                decimal high = candles[i].High;
                decimal low = candles[i].Low;
                decimal prevClose = candles[i - 1].Close;
                decimal tr = Math.Max(high - low, Math.Max(Math.Abs(high - prevClose), Math.Abs(low - prevClose)));
                trList.Add(tr);
            }

            // 对 TR 进行简单平均（SMA 方式）
            if (trList.Count < period)
            {
                return Math.Round(trList.Average(), 2, MidpointRounding.AwayFromZero);
            }
            return Math.Round(trList.Skip(trList.Count - period).Average(), 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 计算获取指定高周期趋势所需的最小低周期 K 线数量
        /// </summary>
        /// <param name="targetTimeframe">高周期，如 "H4"</param>
        /// <param name="sourceTimeframe">低周期，如 "M5"</param>
        /// <param name="confirmationCount">连续确认次数 (1,2,n)</param>
        /// <param name="emaPeriod">EMA 周期</param>
        /// <param name="atrPeriod">ATR 周期</param>
        /// <param name="bufferCount">额外缓冲 K 线数量，建议至少 10 根</param>
        /// <returns>总共需要请求的低周期 K 线数量</returns>
        public static int CalculateRequiredCount(string targetTimeframe, string sourceTimeframe,
                                                 int confirmationCount, int emaPeriod, int atrPeriod, int bufferCount = 10)
        {
            // 1. 将周期转换为分钟数
            int GetMinutes(string tf)
            {
                switch (tf.ToUpper())
                {
                    case "H4": return 240;
                    case "H1": return 60;
                    case "M30": return 30;
                    case "M15": return 15;
                    case "M5": return 5;
                    case "M1": return 1;
                    default: throw new ArgumentException("未知周期: " + tf);
                }
            }

            int targetMinutes = GetMinutes(targetTimeframe); // 高周期每根分钟数
            int sourceMinutes = GetMinutes(sourceTimeframe); // 低周期每根分钟数

            // 2. 高周期所需 K 线数量
            int requiredTargetCandles = Math.Max(emaPeriod, atrPeriod) + (confirmationCount - 1) + bufferCount;

            // 3. 换算成低周期 K 线数量
            int requiredSourceCandles = requiredTargetCandles * (targetMinutes / sourceMinutes);
            return requiredSourceCandles;
        }

        public static TradeSignal GetSharpCrossSignal(List<KdResult> kdHistory, decimal minSlopeThreshold = 2.5m)
        {
            if (kdHistory.Count < 2) return TradeSignal.TrendNeutral;

            var current = kdHistory.Last();
            var prev = kdHistory[kdHistory.Count - 2];

            // 1. 基础金叉/死叉判定
            bool isGolden = prev.K < prev.D && current.K > current.D;
            bool isDeath = prev.K > prev.D && current.K < current.D;
            if (!isGolden && !isDeath) return TradeSignal.TrendNeutral;

            // 2. 计算 K线 和 D线 各自的回归斜率 (Slope)，window 取 kdHistory.Count
            decimal slopeK = CalculateLinearSlope(kdHistory.Select(x => x.K).ToList());
            decimal slopeD = CalculateLinearSlope(kdHistory.Select(x => x.D).ToList());

            // 3. 计算"弹出速率"：K线穿过D线后的发散强度
            decimal spreadSlope = slopeK - slopeD;

            // 4. 阈值过滤
            if (isGolden && spreadSlope > minSlopeThreshold) return TradeSignal.TrendBullish;
            if (isDeath && spreadSlope < -minSlopeThreshold) return TradeSignal.TrendBearish;
            return TradeSignal.TrendNeutral;
        }

        public static TradeSignal GetSharpCrossSignal2(List<KdResult> kdHistory, decimal Oversold_Level, decimal Overbought_Level, decimal minSlopeThreshold = 2.5m)
        {
            if (kdHistory.Count < 2) return TradeSignal.TrendNeutral;

            var current = kdHistory.Last();
            var prev = kdHistory[kdHistory.Count - 2];

            // 1. 基础金叉/死叉判定
            bool isGolden = prev.K < prev.D && current.K > current.D && current.K < Oversold_Level;
            bool isDeath = prev.K > prev.D && current.K < current.D && current.K > Overbought_Level;
            if (!isGolden && !isDeath) return TradeSignal.TrendNeutral;

            // 2. 计算 K线 和 D线 各自的回归斜率 (Slope)，window 取 kdHistory.Count
            decimal slopeK = CalculateLinearSlope(kdHistory.Select(x => x.K).ToList());
            decimal slopeD = CalculateLinearSlope(kdHistory.Select(x => x.D).ToList());

            // 3. 计算"弹出速率"：K线穿过D线后的发散强度
            decimal spreadSlope = slopeK - slopeD;

            // 4. 阈值过滤
            if (isGolden && spreadSlope > minSlopeThreshold) return TradeSignal.TrendBullish;
            if (isDeath && spreadSlope < -minSlopeThreshold) return TradeSignal.TrendBearish;
            return TradeSignal.TrendNeutral;
        }

        /// <summary>
        /// 判断 KDJ 交叉信号及角度强度
        /// </summary>
        /// <returns>1: 强力金叉, -1: 强力死叉, 0: 无效信号</returns>
        public static TradeSignal GetSharpCrossSignal3(KdResult currentKd, KdResult prevKd, decimal Oversold_Level, decimal Overbought_Level, decimal minAngle = 5m)
        {
            // 1. 基本交叉条件判断
            bool isGoldenCross = prevKd.K < prevKd.D && currentKd.K > currentKd.D && currentKd.K < Oversold_Level;
            bool isDeathCross = prevKd.K > prevKd.D && currentKd.K < currentKd.D && currentKd.K > Overbought_Level;
            if (!isGoldenCross && !isDeathCross) return TradeSignal.TrendNeutral;

            // 2. 计算速度差（角度量化）：金叉时 slopeDiff 为正，死叉时为负
            decimal slopeK = currentKd.K - prevKd.K;
            decimal slopeD = currentKd.D - prevKd.D;
            decimal slopeDiff = slopeK - slopeD;

            // 3. 强度过滤：取绝对值判断角度是否超过门槛
            if (Math.Abs(slopeDiff) > minAngle)
            {
                if (isGoldenCross) return TradeSignal.TrendBullish;  // 大角度金叉
                if (isDeathCross) return TradeSignal.TrendBearish;   // 大角度死叉
            }
            return TradeSignal.TrendNeutral; // 虽有交叉但角度太小，视为无效震荡
        }

        /// <summary>
        /// 增强版尖锐交叉信号判定 (V4.1 - 深度优化版)
        /// 优化点：
        /// 1. 增加 [50中轴死区] 过滤，防止在中位震荡区频繁开仓。
        /// 2. 引入 [加速发散] 判定：不仅要求斜率差，还要求 K 线自身的加速度方向正确。
        /// 3. 动态 Gap 逻辑：在超卖/超买程度更深时，允许稍微缩窄 Gap 要求。
        /// </summary>
        public static TradeSignal GetSharpCrossSignal4(List<KdResult> kdHistory, decimal minSlopeThreshold, decimal Oversold_Level, decimal Overbought_Level)
        {
            // --- 0. 基础数据安全校验（需校验远端索引 0,1,2 以及末尾 3 根线，建议序列长度至少为 5） ---
            if (kdHistory == null || kdHistory.Count < 5) return TradeSignal.TrendNeutral;

            // 定位关键时间节点
            var current = kdHistory.Last();                 // T 时刻：当前正在形成的 K 线（用于 T+1 确认）
            var prev = kdHistory[kdHistory.Count - 2];       // T-1 时刻：预设的交叉发生点
            var prevPrev = kdHistory[kdHistory.Count - 3];   // T-2 时刻：交叉前的状态参考点

            // --- 1. 基础交叉判定（交叉必须发生在 prev 根） ---
            // 金叉条件：T-2 时 K<D，且 T-1 时 K 上穿 D，且 T-1 处于超卖区（寻找低位起爆点）
            bool isGoldenCrossOccurred = prevPrev.K < prevPrev.D && prev.K > prev.D && prev.K < Oversold_Level;
            // 死叉条件：T-2 时 K>D，且 T-1 时 K 下穿 D，且 T-1 处于超买区（寻找高位回落点）
            bool isDeathCrossOccurred = prevPrev.K > prevPrev.D && prev.K < prev.D && prev.K > Overbought_Level;

            // --- 2. T+1 延续性确认（防止瞬时穿刺后立即回弹） ---
            bool isGoldenConfirmed = isGoldenCrossOccurred && current.K > current.D;
            bool isDeathConfirmed = isDeathCrossOccurred && current.K < current.D;
            if (!isGoldenConfirmed && !isDeathConfirmed) return TradeSignal.TrendNeutral;

            // --- 3. 远端历史排列判定（过滤震荡行情） ---
            // 校验 kdHistory 列表最开头三根线是否保持稳定的排列方向，确保信号发生前经历了明显的压制或拉升。
            if (isGoldenConfirmed)
            {
                // 金叉要求远端三根线必须是稳定的空头排列 (K < D)
                bool isHistoryStableBearish = (kdHistory[0].K < kdHistory[0].D) &&
                                              (kdHistory[1].K < kdHistory[1].D) &&
                                              (kdHistory[2].K < kdHistory[2].D);
                if (!isHistoryStableBearish) return TradeSignal.TrendNeutral;
            }

            if (isDeathConfirmed)
            {
                // 死叉要求远端三根线必须是稳定的多头排列 (K > D)
                bool isHistoryStableBullish = (kdHistory[0].K > kdHistory[0].D) &&
                                              (kdHistory[1].K > kdHistory[1].D) &&
                                              (kdHistory[2].K > kdHistory[2].D);
                if (!isHistoryStableBullish) return TradeSignal.TrendNeutral;
            }

            // --- 4. 斜率发散判定（量化交叉的"锐利"程度） ---
            // 截取最后 3 根线（交叉前、交叉中、确认中）计算 K 线和 D 线的运行轨迹
            var lastThree = kdHistory.Skip(kdHistory.Count - 3).ToList();
            decimal slopeK = CalculateLinearSlope(lastThree.Select(x => x.K).ToList()); // K 线斜率：短期爆发力
            decimal slopeD = CalculateLinearSlope(lastThree.Select(x => x.D).ToList()); // D 线斜率：中期趋势线
            decimal spreadSlope = slopeK - slopeD; // 差值越大，K 线相对 D 线发散得越快，交叉越"锐利"

            // --- 5. 最终信号输出 ---
            if (isGoldenConfirmed && spreadSlope > minSlopeThreshold) return TradeSignal.TrendBullish; // 金叉确认且发散动能超过阈值
            if (isDeathConfirmed && spreadSlope < -minSlopeThreshold) return TradeSignal.TrendBearish; // 死叉确认且发散动能（负值）低于阈值
            return TradeSignal.TrendNeutral;
        }

        /// <summary>
        /// 判定 KDJ 锐利交叉信号 (Sharp Cross Signal)
        /// 优化点：动态近端历史校验、高性能斜率计算、开口宽度过滤
        /// </summary>
        /// <param name="kdHistory">KD 历史序列</param>
        /// <param name="minSlopeThreshold">最小斜率发散阈值</param>
        /// <param name="enforceZones">是否强制执行高位死叉/低位金叉过滤</param>
        public static TradeSignal GetSharpCrossSignal5(List<KdResult> kdHistory, decimal minSlopeThreshold, decimal Oversold_Level, decimal Overbought_Level, bool enforceZones = true)
        {
            // --- 0. 基础数据安全校验（为确保"滑动历史校验"有效，建议序列长度至少为 6） ---
            int count = kdHistory?.Count ?? 0;
            if (count < 6) return TradeSignal.TrendNeutral;

            int lastIdx = count - 1;
            var current = kdHistory[lastIdx];       // T 时刻 (确认位)
            var prev = kdHistory[lastIdx - 1];      // T-1 时刻 (交叉位)
            var prevPrev = kdHistory[lastIdx - 2];  // T-2 时刻 (交叉前)

            // --- 1. 基础交叉判定 ---
            bool isGoldenCrossOccurred = prevPrev.K < prevPrev.D && prev.K > prev.D; // 金叉：K 线上穿 D 线
            bool isDeathCrossOccurred = prevPrev.K > prevPrev.D && prev.K < prev.D;  // 死叉：K 下穿 D 线

            // 区域强制过滤 (Zone Filter)：金叉需在超卖区附近，死叉需在超买区附近
            if (enforceZones)
            {
                if (isGoldenCrossOccurred && prev.K > Oversold_Level) isGoldenCrossOccurred = false;
                if (isDeathCrossOccurred && prev.K < Overbought_Level) isDeathCrossOccurred = false;
            }

            // --- 2. T+1 延续性确认（增加微小开口校验 0.3m，防止 K/D 极其贴合时的虚假信号） ---
            bool isGoldenConfirmed = isGoldenCrossOccurred && current.K > current.D && (current.K - current.D) > 0.3m;
            bool isDeathConfirmed = isDeathCrossOccurred && current.K < current.D && (current.D - current.K) > 0.3m;
            if (!isGoldenConfirmed && !isDeathConfirmed) return TradeSignal.TrendNeutral;

            // --- 3. 历史排列判定（核心优化：由远端改为近端）---
            // 逻辑：确保在交叉点(T-1)之前的 3 根 K 线处于稳定的对立排列，过滤震荡。检查 T-3, T-4, T-5 是否排列整齐。
            for (int i = 3; i <= 5; i++)
            {
                var hist = kdHistory[lastIdx - i];
                if (isGoldenConfirmed)
                {
                    if (hist.K >= hist.D) return TradeSignal.TrendNeutral; // 金叉前必须是稳定的空头排列
                }
                else if (isDeathConfirmed)
                {
                    if (hist.K <= hist.D) return TradeSignal.TrendNeutral; // 死叉前必须是稳定的多头排列
                }
            }

            // --- 4. 斜率发散判定 (Momentum Filter)：高性能算法，直接计算 T 到 T-2 的平均变化率 ---
            decimal slopeK = (current.K - prevPrev.K) / 2m;
            decimal slopeD = (current.D - prevPrev.D) / 2m;
            decimal spreadSlope = slopeK - slopeD; // 发散速度：K 线摆动速度减去 D 线平滑速度

            // --- 5. 最终信号输出 ---
            if (isGoldenConfirmed && spreadSlope > minSlopeThreshold)
                return TradeSignal.TrendBullish;
            if (isDeathConfirmed && spreadSlope < -minSlopeThreshold)
                return TradeSignal.TrendBearish;
            return TradeSignal.TrendNeutral;
        }

        /// <summary>
        /// H4 趋势 + M5 KDJ 极值反转入场信号
        ///
        /// V6 核心入场逻辑：
        ///                    H4 Trend
        ///                       │
        ///                ┌──────┴──────┐
        ///                ↓             ↓
        ///               UP            DOWN
        ///                │             │
        ///                ↓             ↓
        ///           M5 KDJ < 18    M5 KDJ > 82
        ///                │             │
        ///                ↓             ↓
        ///             金叉确认        死叉确认
        ///                │             │
        ///                ↓             ↓
        ///               BUY           SELL
        ///
        /// BUY:  H4 = BUY TREND，M5 K < Oversold，T-1 K <= D，T K > D
        /// SELL: H4 = SELL TREND，M5 K > Overbought，T-1 K >= D，T K < D
        ///
        /// 注意：
        /// 1. H4 只负责方向过滤
        /// 2. M5 KDJ 负责实际入场
        /// 3. 不使用斜率、不使用额外历史排列
        /// 4. 这是一个纯净的趋势回调入场模型
        /// </summary>
        public static TradeSignal GetSharpCrossSignal6(List<KdResult> kdHistory, string h4Trend, decimal oversoldLevel = 18m, decimal overboughtLevel = 82m)
        {
            // 0. 基础数据校验
            if (kdHistory == null || kdHistory.Count < 2)
                return TradeSignal.TrendNeutral;
            if (string.IsNullOrWhiteSpace(h4Trend))
                return TradeSignal.TrendNeutral;

            // 1. 获取当前和上一根 M5 KDJ
            var current = kdHistory[kdHistory.Count - 1];
            var prev = kdHistory[kdHistory.Count - 2];

            // 2. 标准化 H4 趋势字符串
            string trend = h4Trend.Trim().ToUpper();
            bool h4Bullish = trend == "BUY TREND" || trend == "BUY" || trend == "UP" || trend == "BULLISH";
            bool h4Bearish = trend == "SELL TREND" || trend == "SELL" || trend == "DOWN" || trend == "BEARISH";

            // 3. 如果 H4 没有明确趋势，禁止入场
            if (!h4Bullish && !h4Bearish)
                return TradeSignal.TrendNeutral;

            // 4. H4 UP → 只寻找 M5 超卖区金叉
            if (h4Bullish)
            {
                bool oversold = current.K < oversoldLevel; // M5 当前 K 必须处于超卖区域
                bool goldenCross = prev.K <= prev.D && current.K > current.D; // 上一根 K <= D，当前 K > D
                if (oversold && goldenCross)
                    return TradeSignal.TrendBullish;
                return TradeSignal.TrendNeutral;
            }

            // 5. H4 DOWN → 只寻找 M5 超买区死叉
            if (h4Bearish)
            {
                bool overbought = current.K > overboughtLevel; // M5 当前 K 必须处于超买区域
                bool deathCross = prev.K >= prev.D && current.K < current.D; // 上一根 K >= D，当前 K < D
                if (overbought && deathCross)
                    return TradeSignal.TrendBearish;
                return TradeSignal.TrendNeutral;
            }

            return TradeSignal.TrendNeutral;
        }


        /// <summary>
        /// H4 Trend + M15 KDJ Setup + M5 KDJ Trigger (V7.1 Closed-Candle)
        ///
        /// 多周期 KDJ 入场逻辑：
        ///
        /// H4
        ///   ↓
        /// 确定交易方向
        ///   ↓
        /// M15 Setup
        ///   - BUY：最近窗口曾进入 J <= 20
        ///   - M15 K/D 金叉位置 K <= 50
        ///   - J 开始回升
        ///   ↓
        /// M5 Trigger
        ///   - M15 Setup 后发生 M5 金叉
        ///   - M5 J 继续上升
        ///   - M5 K/D Gap >= 最小值
        ///
        /// SELL 对称处理。
        ///
        /// 注意：
        /// 1. H4 只负责方向。
        /// 2. M15 负责 Setup，不负责最终执行。
        /// 3. M5 负责最终 Trigger。
        /// 4. 不修改 CalculateKdj()，J 已经由 3K-2D 计算。
        /// 5. 默认只使用已收盘 M15/M5 K线，避免实时未收盘K线重绘。
        /// </summary>
        public static TradeSignal GetSharpCrossSignal6(
            List<KdResult> m15KdHistory,
            List<KdResult> m5KdHistory,
            string h4Trend,
            decimal buyExtremeJ = 20m,
            decimal sellExtremeJ = 80m,
            decimal midLevel = 50m,
            decimal minM5Gap = 0.50m,
            int m15Lookback = 6,
            int maxM5BarsAfterM15 = 3,
            bool requireClosedCandles = true)
        {
            // ========================================================
            // V7.1
            // H4 Trend → M15 Setup → M5 Trigger
            //
            // 重要：
            // 1. IndicatorHelper 只负责“入场信号”，不负责 TP/SL 执行。
            // 2. TP/SL 金额由 ManagePosition / strategy config 管理。
            // 3. requireClosedCandles=true 时：
            //      M15/M5 当前正在形成的K线不能参与入场。
            // 4. M5 Trigger 必须严格落在：
            //      M15SetupTime < M5Time <= M15SetupTime + N×5min
            // ========================================================

            if (m15KdHistory == null || m5KdHistory == null)
                return TradeSignal.TrendNeutral;

            if (m15KdHistory.Count < 3 || m5KdHistory.Count < 3)
                return TradeSignal.TrendNeutral;

            if (string.IsNullOrWhiteSpace(h4Trend))
                return TradeSignal.TrendNeutral;

            string trend = h4Trend.Trim().ToUpper();

            bool h4Bullish =
                trend == "BUY TREND" ||
                trend == "BUY" ||
                trend == "UP" ||
                trend == "BULLISH";

            bool h4Bearish =
                trend == "SELL TREND" ||
                trend == "SELL" ||
                trend == "DOWN" ||
                trend == "BEARISH";

            if (!h4Bullish && !h4Bearish)
                return TradeSignal.TrendNeutral;

            m15Lookback = Math.Max(2, m15Lookback);
            maxM5BarsAfterM15 = Math.Max(1, maxM5BarsAfterM15);

            // ========================================================
            // 1. 先确定“可用的最后一根K线”
            // ========================================================
            // KDJ history 通常可能包含正在形成的当前K线。
            // Closed candle mode 下，最后一根直接排除。
            int m15Last =
                requireClosedCandles
                    ? m15KdHistory.Count - 2
                    : m15KdHistory.Count - 1;

            int m5Last =
                requireClosedCandles
                    ? m5KdHistory.Count - 2
                    : m5KdHistory.Count - 1;

            if (m15Last < 2 || m5Last < 2)
                return TradeSignal.TrendNeutral;

            int m15First =
                Math.Max(1, m15Last - m15Lookback + 1);

            // ========================================================
            // 2. M15 Setup
            // ========================================================
            int setupIndex = -1;

            if (h4Bullish)
            {
                for (int i = m15First; i <= m15Last; i++)
                {
                    var prev = m15KdHistory[i - 1];
                    var current = m15KdHistory[i];

                    bool goldenCross =
                        prev.K <= prev.D &&
                        current.K > current.D;

                    if (!goldenCross)
                        continue;

                    // 金叉必须发生在中轴下方
                    if (current.K > midLevel)
                        continue;

                    // J 必须开始回升
                    if (current.J <= prev.J)
                        continue;

                    // 在 Setup 发生之前/当根，必须曾进入极端超卖。
                    // 不允许把未来K线拿来证明过去的Setup。
                    bool hadExtremeOversold = false;

                    int extremeStart =
                        Math.Max(0, i - m15Lookback + 1);

                    for (int j = extremeStart; j <= i; j++)
                    {
                        if (m15KdHistory[j].J <= buyExtremeJ)
                        {
                            hadExtremeOversold = true;
                            break;
                        }
                    }

                    if (!hadExtremeOversold)
                        continue;

                    // 找到最近的有效Setup。
                    setupIndex = i;
                }
            }
            else
            {
                for (int i = m15First; i <= m15Last; i++)
                {
                    var prev = m15KdHistory[i - 1];
                    var current = m15KdHistory[i];

                    bool deathCross =
                        prev.K >= prev.D &&
                        current.K < current.D;

                    if (!deathCross)
                        continue;

                    // 死叉必须发生在中轴上方
                    if (current.K < midLevel)
                        continue;

                    // J 必须开始回落
                    if (current.J >= prev.J)
                        continue;

                    // 在Setup发生之前/当根，必须曾进入极端超买。
                    bool hadExtremeOverbought = false;

                    int extremeStart =
                        Math.Max(0, i - m15Lookback + 1);

                    for (int j = extremeStart; j <= i; j++)
                    {
                        if (m15KdHistory[j].J >= sellExtremeJ)
                        {
                            hadExtremeOverbought = true;
                            break;
                        }
                    }

                    if (!hadExtremeOverbought)
                        continue;

                    setupIndex = i;
                }
            }

            if (setupIndex < 0)
                return TradeSignal.TrendNeutral;

            // ========================================================
            // 3. M15 Setup 时间
            // ========================================================
            DateTime m15SetupTime;

            if (!DateTime.TryParse(
                m15KdHistory[setupIndex].dateTime,
                out m15SetupTime))
            {
                return TradeSignal.TrendNeutral;
            }

            // ========================================================
            // 4. M5 精确时间窗口
            // ========================================================
            // 原 V7：
            //     直接取最后3根M5
            //
            // V7.1：
            //     必须严格满足
            //
            //     SetupTime < M5Time
            //     M5Time <= SetupTime + 15min
            //
            // 这样不会因为“最后3根M5”恰好落在别的时间区间而误触发。
            DateTime m5WindowEnd =
                m15SetupTime.AddMinutes(maxM5BarsAfterM15 * 5);

            if (h4Bullish)
            {
                for (int i = 1; i <= m5Last; i++)
                {
                    var prev = m5KdHistory[i - 1];
                    var current = m5KdHistory[i];

                    DateTime m5Time;

                    if (!DateTime.TryParse(
                        current.dateTime,
                        out m5Time))
                    {
                        continue;
                    }

                    if (m5Time <= m15SetupTime)
                        continue;

                    if (m5Time > m5WindowEnd)
                        continue;

                    bool goldenCross =
                        prev.K <= prev.D &&
                        current.K > current.D;

                    if (!goldenCross)
                        continue;

                    decimal gap = current.K - current.D;

                    if (gap < minM5Gap)
                        continue;

                    // M5 J必须继续向上
                    if (current.J <= prev.J)
                        continue;

                    return TradeSignal.TrendBullish;
                }
            }
            else
            {
                for (int i = 1; i <= m5Last; i++)
                {
                    var prev = m5KdHistory[i - 1];
                    var current = m5KdHistory[i];

                    DateTime m5Time;

                    if (!DateTime.TryParse(
                        current.dateTime,
                        out m5Time))
                    {
                        continue;
                    }

                    if (m5Time <= m15SetupTime)
                        continue;

                    if (m5Time > m5WindowEnd)
                        continue;

                    bool deathCross =
                        prev.K >= prev.D &&
                        current.K < current.D;

                    if (!deathCross)
                        continue;

                    decimal gap = current.D - current.K;

                    if (gap < minM5Gap)
                        continue;

                    // M5 J必须继续向下
                    if (current.J >= prev.J)
                        continue;

                    return TradeSignal.TrendBearish;
                }
            }

            return TradeSignal.TrendNeutral;
        }

        /// <summary>
        /// V7.1 风险参数转换辅助。
        /// IndicatorHelper 不执行平仓，只提供价格距离计算，
        /// 供策略层/ManagePosition 使用。
        /// </summary>
        /// <param name="moneyPerLot">USD / 1 lot</param>
        /// <param name="usdPerLotPerPrice">XAUUSD每1 lot、价格移动1.00对应USD</param>
        /// <returns>对应的价格距离</returns>
        public static decimal MoneyPerLotToPriceDistance(
            decimal moneyPerLot,
            decimal usdPerLotPerPrice = 100m)
        {
            if (moneyPerLot <= 0 || usdPerLotPerPrice <= 0)
                return 0m;

            return moneyPerLot / usdPerLotPerPrice;
        }

        /// <summary>
        /// V7.1 根据策略金额参数计算TP1/SL价格。
        /// 这里只计算价格，不修改Broker止损。
        /// </summary>
        public static decimal CalculateStrategyTargetPrice(
            bool isBuy,
            decimal entryPrice,
            decimal moneyPerLot,
            decimal usdPerLotPerPrice = 100m)
        {
            decimal distance =
                MoneyPerLotToPriceDistance(
                    moneyPerLot,
                    usdPerLotPerPrice);

            if (distance <= 0)
                return entryPrice;

            return isBuy
                ? entryPrice + distance
                : entryPrice - distance;
        }

        /// <summary>
        /// V7.1 根据策略stopLoss金额计算风险止损参考价格。
        /// 仅返回价格参考，不发送任何Broker SL修改请求。
        /// </summary>
        public static decimal CalculateStrategyStopPrice(
            bool isBuy,
            decimal entryPrice,
            decimal stopLossMoneyPerLot,
            decimal usdPerLotPerPrice = 100m)
        {
            decimal distance =
                MoneyPerLotToPriceDistance(
                    stopLossMoneyPerLot,
                    usdPerLotPerPrice);

            if (distance <= 0)
                return entryPrice;

            return isBuy
                ? entryPrice - distance
                : entryPrice + distance;
        }

        // 线性回归斜率计算辅助函数
        private static decimal CalculateLinearSlope(List<decimal> data)
        {
            int n = data.Count;
            decimal sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            for (int i = 0; i < n; i++)
            {
                decimal x = i + 1;
                decimal y = data[i];
                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }
            decimal divisor = (n * sumX2 - sumX * sumX);
            return divisor == 0 ? 0 : (n * sumXY - sumX * sumY) / divisor;
        }

        /// <summary>
        /// 根据周期策略获取计算所需的最小数据长度。
        /// 核心逻辑：获取周期配置中的 KdjN 并叠加 40 根 K 线作为算法平滑预热期。
        /// </summary>
        /// <param name="period">周期字符串 (如: M5, H1 等)</param>
        /// <returns>建议加载的最小 K 线序列长度 (包含平滑算法所需的预热期)</returns>
        public static int GetRequiredCandleCount(string period)
        {
            // 直接复用 GetConfig 逻辑获取 KdjN 参数，确保参数维护在 GetConfig 一个地方，避免逻辑同步出错
            var config = GetConfig(period);
            // 核心计算：回溯窗口(KdjN) + 预热长度(40)，40 根是为了让递归的平滑算法（SMA/EMA）达到数值稳定状态
            return config.KdjN + 40;
        }

        /// <summary>
        /// 计算对齐 MT5 逻辑的 KDJ 序列 (双重 SMA 平滑)
        /// </summary>
        public static List<KdResult> CalculateKdj(List<Candle> candles, string period, int historyWindow = 1)
        {
            if (candles == null || candles.Count == 0) return new List<KdResult>();

            // 1. 获取配置
            var config = GetConfig(period);
            int kdjN = config.KdjN;      // 对应 MT5 %K周期 (截图为 14)
            int slowing = 3;             // 对应 MT5 减缓 (截图为 3)
            int mD = 3;                  // 对应 MT5 %D周期 (截图为 3)

            // 2. 基础数据校验：MT5 计算 D 线至少需要 N + slowing + mD 根 K 线
            if (candles.Count < kdjN + slowing + mD) return new List<KdResult>();

            // 临时存储中间计算结果
            List<decimal> rsvList = new List<decimal>();
            List<KdResult> allResults = new List<KdResult>();

            // 3. 第一步：计算所有 K 线的 RSV
            for (int i = 0; i < candles.Count; i++)
            {
                if (i < kdjN - 1)
                {
                    rsvList.Add(50m); // 初始填充
                    continue;
                }

                decimal highN = decimal.MinValue;
                decimal lowN = decimal.MaxValue;
                for (int j = i - kdjN + 1; j <= i; j++)
                {
                    if (candles[j].High > highN) highN = candles[j].High;
                    if (candles[j].Low < lowN) lowN = candles[j].Low;
                }

                decimal close = candles[i].Close;
                decimal rsv = (highN == lowN) ? 50m : (close - lowN) / (highN - lowN) * 100m;
                rsvList.Add(rsv);
            }

            // 4. 第二步：通过 RSV 的 SMA 计算 K 线（对齐 MT5 的"减缓"，K = SMA(RSV, slowing)）
            List<decimal> kList = new List<decimal>(new decimal[candles.Count]);
            for (int i = 0; i < rsvList.Count; i++)
            {
                if (i < kdjN + slowing - 2) continue;

                decimal sumRsv = 0;
                for (int j = i - slowing + 1; j <= i; j++)
                {
                    sumRsv += rsvList[j];
                }
                kList[i] = sumRsv / slowing;
            }

            // 5. 第三步：通过 K 线的 SMA 计算 D 线（MT5 的 D = SMA(K, mD)）
            for (int i = 0; i < kList.Count; i++)
            {
                if (i < kdjN + slowing + mD - 3) continue;

                decimal sumK = 0;
                for (int j = i - mD + 1; j <= i; j++)
                {
                    sumK += kList[j];
                }
                decimal currK = kList[i];
                decimal currD = sumK / mD;
                decimal currJ = 3 * currK - 2 * currD;

                allResults.Add(new KdResult
                {
                    K = currK,
                    D = currD,
                    J = currJ,
                    dateTime = candles[i].Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    Period = period
                });
            }

            // 6. 封装返回指定的 historyWindow 数量并计算斜率/金叉
            List<KdResult> finalResults = new List<KdResult>();
            int count = allResults.Count;
            if (count == 0) return finalResults;

            int startIndex = Math.Max(1, count - historyWindow);
            for (int i = startIndex; i < count; i++)
            {
                var current = allResults[i];
                var previous = allResults[i - 1];

                current.K = Math.Round(current.K, 2);
                current.D = Math.Round(current.D, 2);
                current.J = Math.Round(current.J, 2);
                current.SlopeK = Math.Round(current.K - previous.K, 2);
                current.IsGoldenCross = (previous.K <= previous.D) && (current.K > current.D);
                current.IsDeathCross = (previous.K >= previous.D) && (current.K < current.D);

                finalResults.Add(current);
            }

            return finalResults;
        }

        /// <summary>
        /// 开仓时创建仓位对象并计算初始风险 R
        /// </summary>
        public static TradePosition OpenPosition(bool isBuy, decimal entry, decimal stopLoss)
        {
            // 1. 基础校验
            if (entry <= 0)
                throw new ArgumentException("Entry price must be > 0");
            if (stopLoss <= 0)
                throw new ArgumentException("StopLoss must be > 0");

            // 2. 止损方向校验（关键）
            if (isBuy && stopLoss >= entry)
                throw new ArgumentException("Buy单止损必须小于入场价");
            if (!isBuy && stopLoss <= entry)
                throw new ArgumentException("Sell单止损必须大于入场价");

            // 3. 计算风险 R
            decimal risk = Math.Abs(entry - stopLoss);
            if (risk == 0)
                throw new ArgumentException("RiskR cannot be 0");

            // 4. 创建仓位对象
            return new TradePosition
            {
                eaTradeId = DBUtils.GetUniqueSerialNumber(),
                IsBuy = isBuy,
                EntryPrice = entry,
                StopLoss = stopLoss,            // 当前止损（会动态变化）
                InitialStopLoss = stopLoss,     // 建议保留初始止损（用于回测/分析）
                RiskR = risk,
                HighestPrice = entry,           // 初始化价格轨迹
                LowestPrice = entry,
                BreakEvenActivated = false,     // 状态初始化
                TP1Hit = false,
                TP2Hit = false,
                OppositeSignalCount = 0,        // EMA确认用
                OpenTime = DateTime.Now         // 可选（如果加了这些字段）
            };
        }

        /// <summary>
        /// 持仓管理核心逻辑（Money-Risk V7.1）。
        ///
        /// keepProfitMoneyPerLot = TP1金额（USD / lot）
        /// stopLossMoneyPerLot = 1R金额（USD / lot）
        /// goldUsdPerLotPerPrice = XAUUSD每1 lot、价格移动1.00对应USD
        ///
        /// 本方法不修改Broker SL，只产生策略层平仓信号。
        /// </summary>
        public static List<TradeSignal> ManagePositionSignal(
            TradePosition pos,
            decimal price,
            decimal atr,
            decimal ema,
            string strategyResult,
            decimal keepProfitMoneyPerLot,
            decimal stopLossMoneyPerLot,
            decimal goldUsdPerLotPerPrice = 100m)
        {
            var signals = new List<TradeSignal>();

            if (pos == null || price <= 0)
                return signals;

            if (goldUsdPerLotPerPrice <= 0)
                goldUsdPerLotPerPrice = 100m;
            if (keepProfitMoneyPerLot <= 0)
                keepProfitMoneyPerLot = 20m;
            if (stopLossMoneyPerLot <= 0)
                stopLossMoneyPerLot = 30m;

            bool atrReady = atr > 0;
            bool emaReady = ema > 0;

            // 1. 更新MFE轨迹；只记录价格，不移动Broker SL。
            if (pos.IsBuy)
                pos.HighestPrice = Math.Max(pos.HighestPrice, price);
            else
                pos.LowestPrice = Math.Min(pos.LowestPrice, price);

            // 2. 当前价格盈利 → USD/lot
            decimal profitPrice = pos.IsBuy
                ? price - pos.EntryPrice
                : pos.EntryPrice - price;

            decimal profitMoneyPerLot =
                profitPrice * goldUsdPerLotPerPrice;

            decimal currentR = stopLossMoneyPerLot > 0
                ? profitMoneyPerLot / stopLossMoneyPerLot
                : 0m;

            // 3. 策略止损：以stopLoss金额为1R。
            decimal stopDistance =
                stopLossMoneyPerLot / goldUsdPerLotPerPrice;

            bool stopLossHit = pos.IsBuy
                ? price <= pos.EntryPrice - stopDistance
                : price >= pos.EntryPrice + stopDistance;

            if (stopLossHit)
            {
                signals.Add(TradeSignal.StopLossHit);
                return signals;
            }

            // 4. TP1：keepProfit直接定义为USD/lot目标。
            if (!pos.TP1Hit && profitMoneyPerLot >= keepProfitMoneyPerLot)
            {
                pos.TP1Hit = true;
                signals.Add(TradeSignal.TakeProfit1);
                return signals;
            }

            // 5. TP2：1.80R；同时允许TP1完成后的明显回撤提前第二批平仓。
            if (pos.TP1Hit && !pos.TP2Hit)
            {
                decimal pullback = pos.IsBuy
                    ? Math.Max(0m, pos.HighestPrice - price)
                    : Math.Max(0m, price - pos.LowestPrice);

                decimal pullbackAtr = atrReady ? pullback / atr : 0m;

                decimal tp2MoneyPerLot = stopLossMoneyPerLot * 1.80m;
                decimal pullbackMinMoneyPerLot = stopLossMoneyPerLot * 1.20m;

                bool normalTP2 = profitMoneyPerLot >= tp2MoneyPerLot;
                bool pullbackTP2 =
                    atrReady &&
                    profitMoneyPerLot >= pullbackMinMoneyPerLot &&
                    pullbackAtr >= 0.80m;

                if (normalTP2 || pullbackTP2)
                {
                    pos.TP2Hit = true;
                    signals.Add(TradeSignal.TakeProfit2);
                    return signals;
                }
            }

            // 6. EMA趋势反转退出。
            if (emaReady && atrReady)
            {
                decimal emaBuffer = atr * 0.20m;
                bool oppositeTrend = pos.IsBuy
                    ? string.Equals(strategyResult, "SELL TREND", StringComparison.OrdinalIgnoreCase) && price < ema - emaBuffer
                    : string.Equals(strategyResult, "BUY TREND", StringComparison.OrdinalIgnoreCase) && price > ema + emaBuffer;

                if (oppositeTrend)
                    pos.OppositeSignalCount++;
                else
                    pos.OppositeSignalCount = 0;

                decimal pullback = pos.IsBuy
                    ? Math.Max(0m, pos.HighestPrice - price)
                    : Math.Max(0m, price - pos.LowestPrice);

                if (pos.OppositeSignalCount >= 2 && pullback > atr)
                    signals.Add(TradeSignal.ExitByEMA);
            }

            return signals;
        }

        /// <summary>
        /// 兼容旧调用方。默认使用20 USD/lot TP1、30 USD/lot 1R、100 USD/lot/1.00价格。
        /// </summary>
        public static List<TradeSignal> ManagePositionSignal(
            TradePosition pos,
            decimal price,
            decimal atr,
            decimal ema,
            string strategyResult)
        {
            return ManagePositionSignal(
                pos,
                price,
                atr,
                ema,
                strategyResult,
                20m,
                30m,
                100m);
        }

        /// <summary>
        /// 计算初始止损价格（结合 ATR 波动止损 + 结构止损）
        ///
        /// 该方法用于开仓阶段生成"初始风险边界"，属于交易系统风控核心组件。
        ///
        /// 止损由两部分构成：
        /// 1. ATR止损：基于市场波动性（动态风险）
        /// 2. Swing止损：基于结构高低点（市场结构风险）
        ///
        /// 最终止损取更远的一侧，以确保：
        /// - 多单：止损在结构低点或ATR止损中更低的位置
        /// - 空单：止损在结构高点或ATR止损中更高的位置
        ///
        /// 设计目标：
        /// - 避免止损过近被噪音扫掉
        /// - 保证趋势行情有足够空间运行
        /// - 同时兼顾结构破位风险
        ///
        /// 风控原则：
        /// - ATR控制"波动风险"
        /// - Swing控制"结构风险"
        /// - 取更远 = 优先保护仓位存活率
        /// </summary>
        /// <param name="isBuy">是否为多单（true=Buy, false=Sell）</param>
        /// <param name="entryPrice">开仓价格</param>
        /// <param name="atr">平均真实波幅（ATR）</param>
        /// <param name="swingHigh">最近结构高点（用于空单止损参考）</param>
        /// <param name="swingLow">最近结构低点（用于多单止损参考）</param>
        /// <param name="atrMultiplier">ATR倍数（默认2倍，用于控制止损宽度）</param>
        /// <returns>最终初始止损价格（decimal）</returns>
        public static decimal CalculateInitialStopLoss(bool isBuy, decimal entryPrice, decimal atr, decimal swingHigh, decimal swingLow, decimal atrMultiplier = 2m)
        {
            // 1. ATR保护（防止为0或异常值）
            if (atr <= 0)
                atr = entryPrice * 0.005m; // 默认0.5%波动替代值

            decimal atrSL;
            decimal swingSL;

            if (isBuy)
            {
                atrSL = entryPrice - atr * atrMultiplier; // 多单 ATR止损（价格下方）
                swingSL = (swingLow > 0 && swingLow < entryPrice) ? swingLow : atrSL; // 多单结构止损（必须低于入场价才有效）

                decimal finalSL = Math.Min(atrSL, swingSL); // 取更保守（更低）的止损
                if (finalSL >= entryPrice) // 安全保护：确保止损一定在入场价下方
                    finalSL = atrSL;
                return finalSL;
            }
            else
            {
                atrSL = entryPrice + atr * atrMultiplier; // 空单 ATR止损（价格上方）
                swingSL = (swingHigh > 0 && swingHigh > entryPrice) ? swingHigh : atrSL; // 空单结构止损（必须高于入场价才有效）

                decimal finalSL = Math.Max(atrSL, swingSL); // 取更保守（更高）的止损
                if (finalSL <= entryPrice) // 安全保护：确保止损一定在入场价上方
                    finalSL = atrSL;
                return finalSL;
            }
        }

        private static DateTime lastH4Time = DateTime.MinValue;

        /// <summary>
        /// 判断是否出现新的 H4 K线，只在新H4开始时返回 true
        /// </summary>
        public static bool IsNewH4Candle()
        {
            DateTime now = DateTime.UtcNow; // 建议使用UTC时间（外汇黄金服务器时间通常接近UTC）
            int h4Block = now.Hour / 4; // 当前属于哪个 H4 周期
            DateTime currentH4OpenTime = new DateTime(now.Year, now.Month, now.Day, h4Block * 4, 0, 0); // 当前 H4 K线开始时间

            // 如果和上一次记录的H4时间不同，说明新K线开始
            if (currentH4OpenTime > lastH4Time)
            {
                lastH4Time = currentH4OpenTime;
                return true;
            }
            return false;
        }
    }
}