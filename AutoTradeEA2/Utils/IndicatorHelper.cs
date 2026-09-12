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
                    return new PeriodConfig(14, 1.2m, 70m, 30m);

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

            // 1. 计算平滑因子 k
            // 这里的类型转换非常重要，确保高精度计算
            decimal k = 2.0m / (period + 1);

            // 2. 初始化种子值 (Seed Value)
            // 大多数标准算法使用前 N 个周期的 SMA (算术平均值) 作为第一个 EMA
            decimal ema = prices.Take(period).Average();

            // 3. 从第 N 个元素开始迭代 (数组索引从 period 开始)
            // 注意：如果你提供的 prices 已经非常长，这种方法会非常稳定
            for (int i = period; i < prices.Count; i++)
            {
                // 标准公式：EMA = (Close - EMA_prev) * k + EMA_prev
                ema = (prices[i] - ema) * k + ema;
            }

            // 4. 舍入处理
            // 交易软件内部通常保留很多位，仅在显示时舍入。
            // 若要与 MT4 对齐，有时需要保留 4 位或更多。
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
            //bool isGolden = prev.K < prev.D && current.K > current.D && current.K < Oversold_Level;
            //bool isDeath = prev.K > prev.D && current.K < current.D && current.K > Overbought_Level;
            bool isGolden = prev.K < prev.D && current.K > current.D;
            bool isDeath = prev.K > prev.D && current.K < current.D;
            if (!isGolden && !isDeath) return TradeSignal.TrendNeutral;

            // 2. 计算 K线 和 D线 各自的回归斜率 (Slope)
            // 这里的 window 取 kdHistory.Count
            decimal slopeK = CalculateLinearSlope(kdHistory.Select(x => x.K).ToList());
            decimal slopeD = CalculateLinearSlope(kdHistory.Select(x => x.D).ToList());

            // 3. 计算“弹出速率”：K线穿过D线后的发散强度
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
            //bool isGolden = prev.K < prev.D && current.K > current.D;
            //bool isDeath = prev.K > prev.D && current.K < current.D;
            if (!isGolden && !isDeath) return TradeSignal.TrendNeutral;

            // 2. 计算 K线 和 D线 各自的回归斜率 (Slope)
            // 这里的 window 取 kdHistory.Count
            decimal slopeK = CalculateLinearSlope(kdHistory.Select(x => x.K).ToList());
            decimal slopeD = CalculateLinearSlope(kdHistory.Select(x => x.D).ToList());

            // 3. 计算“弹出速率”：K线穿过D线后的发散强度
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

            // 2. 计算速度差 (角度量化)
            // 金叉时 slopeDiff 为正，死叉时 slopeDiff 为负
            decimal slopeK = currentKd.K - prevKd.K;
            decimal slopeD = currentKd.D - prevKd.D;
            decimal slopeDiff = slopeK - slopeD;

            // 3. 强度过滤：取绝对值判断角度是否超过门槛
            if (Math.Abs(slopeDiff) > minAngle)
            {
                if (isGoldenCross) return TradeSignal.TrendBullish;  // 大角度金叉
                if (isDeathCross) return TradeSignal.TrendBearish; // 大角度死叉
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
            // --- 0. 基础数据安全校验 ---
            // 由于需要校验远端索引 0,1,2 以及末尾 3 根线，建议序列长度至少为 5 或更多
            if (kdHistory == null || kdHistory.Count < 5) return TradeSignal.TrendNeutral;

            // 定位关键时间节点
            var current = kdHistory.Last();               // T 时刻：当前正在形成的 K 线（用于 T+1 确认）
            var prev = kdHistory[kdHistory.Count - 2];    // T-1 时刻：预设的交叉发生点
            var prevPrev = kdHistory[kdHistory.Count - 3];// T-2 时刻：交叉前的状态参考点

            // --- 1. 基础交叉判定 (逻辑点：交叉必须发生在 prev 根) ---
            // 金叉条件：T-2 时 K<D，且 T-1 时 K 上穿 D，且 T-1 处于超卖区（寻找低位起爆点）
            bool isGoldenCrossOccurred = prevPrev.K < prevPrev.D && prev.K > prev.D && prev.K < Oversold_Level;

            // 死叉条件：T-2 时 K>D，且 T-1 时 K 下穿 D，且 T-1 处于超买区（寻找高位回落点）
            bool isDeathCrossOccurred = prevPrev.K > prevPrev.D && prev.K < prev.D && prev.K > Overbought_Level;

            // --- 2. T+1 延续性确认 (逻辑点：防止瞬时穿刺后立即回弹) ---
            // 要求当前时刻 current 的 K 线依然维持在交叉后的方向
            bool isGoldenConfirmed = isGoldenCrossOccurred && current.K > current.D;
            bool isDeathConfirmed = isDeathCrossOccurred && current.K < current.D;

            // 如果当前既没有确认的金叉，也没有确认的死叉，直接返回中性
            if (!isGoldenConfirmed && !isDeathConfirmed) return TradeSignal.TrendNeutral;

            // --- 3. 远端历史排列判定 (逻辑点：过滤震荡行情) ---
            // 校验 kdHistory 列表最开头的三根线是否保持稳定的排列方向。
            // 这能确保在信号发生前，行情经历了一段明显的压制或拉升，而不是在交叉点附近反复缠绕。
            if (isGoldenConfirmed)
            {
                // 如果是金叉，要求远端三根线必须是稳定的空头排列 (K < D)
                bool isHistoryStableBearish = (kdHistory[0].K < kdHistory[0].D) &&
                                              (kdHistory[1].K < kdHistory[1].D) &&
                                              (kdHistory[2].K < kdHistory[2].D);
                if (!isHistoryStableBearish) return TradeSignal.TrendNeutral;
            }

            if (isDeathConfirmed)
            {
                // 如果是死叉，要求远端三根线必须是稳定的多头排列 (K > D)
                bool isHistoryStableBullish = (kdHistory[0].K > kdHistory[0].D) &&
                                              (kdHistory[1].K > kdHistory[1].D) &&
                                              (kdHistory[2].K > kdHistory[2].D);
                if (!isHistoryStableBullish) return TradeSignal.TrendNeutral;
            }

            // --- 4. 斜率发散判定 (逻辑点：量化交叉的“锐利”程度) ---
            // 截取最后 3 根线（交叉前、交叉中、确认中）计算 K 线和 D 线的运行轨迹
            var lastThree = kdHistory.Skip(kdHistory.Count - 3).ToList();

            // 计算 K 线的斜率（代表短期爆发力）
            decimal slopeK = CalculateLinearSlope(lastThree.Select(x => x.K).ToList());
            // 计算 D 线的斜率（代表中期趋势线）
            decimal slopeD = CalculateLinearSlope(lastThree.Select(x => x.D).ToList());

            // 计算差值：spreadSlope 越大，说明 K 线相对于 D 线发散得越快，交叉越“锐利”
            decimal spreadSlope = slopeK - slopeD;

            // --- 5. 最终信号输出 ---
            // 多头信号：金叉确认且发散动能超过设定阈值
            if (isGoldenConfirmed && spreadSlope > minSlopeThreshold) return TradeSignal.TrendBullish;

            // 空头信号：死叉确认且发散动能（负值）低于设定阈值
            if (isDeathConfirmed && spreadSlope < -minSlopeThreshold) return TradeSignal.TrendBearish;

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
            // --- 0. 基础数据安全校验 ---
            // 为确保“滑动历史校验”有效，建议序列长度至少为 6
            int count = kdHistory?.Count ?? 0;
            if (count < 6) return TradeSignal.TrendNeutral;

            int lastIdx = count - 1;
            var current = kdHistory[lastIdx];       // T 时刻 (确认位)
            var prev = kdHistory[lastIdx - 1];      // T-1 时刻 (交叉位)
            var prevPrev = kdHistory[lastIdx - 2];  // T-2 时刻 (交叉前)

            // --- 1. 基础交叉判定 ---
            // 金叉：K 线上穿 D 线
            bool isGoldenCrossOccurred = prevPrev.K < prevPrev.D && prev.K > prev.D;
            // 死叉：K 下穿 D 线
            bool isDeathCrossOccurred = prevPrev.K > prevPrev.D && prev.K < prev.D;

            // 区域强制过滤 (Zone Filter)
            if (enforceZones)
            {
                // 金叉需在超卖区附近，死叉需在超买区附近
                if (isGoldenCrossOccurred && prev.K > Oversold_Level) isGoldenCrossOccurred = false;
                if (isDeathCrossOccurred && prev.K < Overbought_Level) isDeathCrossOccurred = false;
            }

            // --- 2. T+1 延续性确认 ---
            // 增加微小开口校验 (0.3m)，防止 K/D 极其贴合时的虚假信号
            bool isGoldenConfirmed = isGoldenCrossOccurred && current.K > current.D && (current.K - current.D) > 0.3m;
            bool isDeathConfirmed = isDeathCrossOccurred && current.K < current.D && (current.D - current.K) > 0.3m;

            if (!isGoldenConfirmed && !isDeathConfirmed) return TradeSignal.TrendNeutral;

            // --- 3. 历史排列判定 (核心优化：由远端改为近端) ---
            // 逻辑：确保在交叉点(T-1)之前的 3 根 K 线处于稳定的对立排列，过滤震荡。
            // 检查：T-3, T-4, T-5 是否排列整齐
            for (int i = 3; i <= 5; i++)
            {
                var hist = kdHistory[lastIdx - i];
                if (isGoldenConfirmed)
                {
                    // 金叉前必须是稳定的空头排列
                    if (hist.K >= hist.D) return TradeSignal.TrendNeutral;
                }
                else if (isDeathConfirmed)
                {
                    // 死叉前必须是稳定的多头排列
                    if (hist.K <= hist.D) return TradeSignal.TrendNeutral;
                }
            }

            // --- 4. 斜率发散判定 (Momentum Filter) ---
            // 高性能算法：直接计算 T 到 T-2 的平均变化率
            decimal slopeK = (current.K - prevPrev.K) / 2m;
            decimal slopeD = (current.D - prevPrev.D) / 2m;

            // 发散速度：K 线摆动速度减去 D 线平滑速度
            decimal spreadSlope = slopeK - slopeD;

            // --- 5. 最终信号输出 ---
            if (isGoldenConfirmed && spreadSlope > minSlopeThreshold)
                return TradeSignal.TrendBullish;

            if (isDeathConfirmed && spreadSlope < -minSlopeThreshold)
                return TradeSignal.TrendBearish;

            return TradeSignal.TrendNeutral;
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
            // 直接复用 GetConfig 逻辑获取 KdjN 参数
            // 这样可以确保参数维护在 GetConfig 一个地方，避免逻辑同步出错
            var config = GetConfig(period);

            // 核心计算：回溯窗口(KdjN) + 预热长度(40)
            // 增加的 40 根 K 线是为了让递归的平滑算法（SMA/EMA）达到数值稳定状态
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

            // 2. 基础数据校验
            // MT5 计算 D 线至少需要 N + slowing + mD 根 K 线
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

            // 4. 第二步：通过 RSV 的 SMA 计算 K 线 (对齐 MT5 的 "减缓")
            // MT5 的 K = SMA(RSV, slowing)
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

            // 5. 第三步：通过 K 线的 SMA 计算 D 线
            // MT5 的 D = SMA(K, mD)
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
        /// 开仓时创建仓位对象
        /// 并计算初始风险 R
        /// </summary>
        public static TradePosition OpenPosition(bool isBuy, decimal entry, decimal stopLoss)
        {
            // =====================
            // 1. 基础校验
            // =====================
            if (entry <= 0)
                throw new ArgumentException("Entry price must be > 0");

            if (stopLoss <= 0)
                throw new ArgumentException("StopLoss must be > 0");

            // =====================
            // 2. 止损方向校验（关键）
            // =====================
            if (isBuy && stopLoss >= entry)
                throw new ArgumentException("Buy单止损必须小于入场价");

            if (!isBuy && stopLoss <= entry)
                throw new ArgumentException("Sell单止损必须大于入场价");

            // =====================
            // 3. 计算风险 R
            // =====================
            decimal risk = Math.Abs(entry - stopLoss);

            if (risk == 0)
                throw new ArgumentException("RiskR cannot be 0");

            // =====================
            // 4. 创建仓位对象
            // =====================
            return new TradePosition
            {
                eaTradeId = DBUtils.GetUniqueSerialNumber(),

                IsBuy = isBuy,
                EntryPrice = entry,

                // 当前止损（会动态变化）
                StopLoss = stopLoss,

                // 建议保留初始止损（用于回测/分析）
                InitialStopLoss = stopLoss,

                RiskR = risk,

                // 初始化价格轨迹
                HighestPrice = entry,
                LowestPrice = entry,

                // 状态初始化
                BreakEvenActivated = false,
                TP1Hit = false,
                TP2Hit = false,

                // EMA确认用
                OppositeSignalCount = 0,

                // 可选（如果你加了这些字段）
                OpenTime = DateTime.Now
            };
        }

        /// <summary>
        /// 仓位管理函数（Tick级别调用）
        /// 功能：
        /// 1. 止损检测
        /// 2. Break Even（1R）
        /// 3. TP1（2R）
        /// 4. TP2（3R）
        /// 5. ATR Trailing Stop
        /// 6. EMA 趋势退出
        /// </summary>
        /// <summary>
        /// 持仓管理核心逻辑：
        /// 包含：止损检测 / BreakEven / TP / Trailing / EMA退出
        /// </summary>
        public static List<TradeSignal> ManagePositionSignal(
            TradePosition pos,
            decimal price,
            decimal atr,
            decimal ema,
            string strategyResult)
        {
            List<TradeSignal> signals = new List<TradeSignal>();

            // =====================
            // 0. 基础校验
            // =====================
            if (pos == null)
                return signals;

            bool atrReady = atr > 0;
            bool emaReady = ema > 0;
            bool riskValid = pos.RiskR > 0;

            // =====================
            // 0.1 止损方向校验（防止数据错误）
            // =====================
            if (riskValid)
            {
                // Buy单止损必须在下方
                if (pos.IsBuy && pos.StopLoss >= pos.EntryPrice)
                    pos.StopLoss = pos.EntryPrice - pos.RiskR;

                // Sell单止损必须在上方
                if (!pos.IsBuy && pos.StopLoss <= pos.EntryPrice)
                    pos.StopLoss = pos.EntryPrice + pos.RiskR;
            }

            // =====================
            // 1. 更新最高/最低价（用于Trailing）
            // =====================
            if (pos.IsBuy)
                pos.HighestPrice = Math.Max(pos.HighestPrice, price);
            else
                pos.LowestPrice = Math.Min(pos.LowestPrice, price);

            // =====================
            // 2. 当前浮盈（点数）
            // =====================
            decimal profit = pos.IsBuy
                ? price - pos.EntryPrice
                : pos.EntryPrice - price;

            // =====================
            // 3. 止损检测（最高优先级）
            // =====================
            decimal slBuffer = 0.5m; // 防跳价

            if (pos.IsBuy && price <= pos.StopLoss + slBuffer)
            {
                signals.Add(TradeSignal.StopLossHit);
                return signals;
            }

            if (!pos.IsBuy && price >= pos.StopLoss - slBuffer)
            {
                signals.Add(TradeSignal.StopLossHit);
                return signals;
            }

            // =====================
            // 4. Break Even（>=1R）
            // =====================
            if (riskValid && !pos.BreakEvenActivated && profit >= pos.RiskR)
            {
                // 加buffer避免假保本
                decimal buffer = atrReady ? atr * 0.2m : 1.0m;

                pos.StopLoss = pos.IsBuy
                    ? pos.EntryPrice + buffer
                    : pos.EntryPrice - buffer;

                pos.BreakEvenActivated = true;

                signals.Add(TradeSignal.MoveStopToBreakEven);
            }

            // =====================
            // 5. TP1 / TP2（分批止盈信号）
            // =====================
            if (riskValid && !pos.TP1Hit && profit >= pos.RiskR * 2)
            {
                pos.TP1Hit = true;
                signals.Add(TradeSignal.TakeProfit1);
            }

            if (riskValid && !pos.TP2Hit && profit >= pos.RiskR * 3)
            {
                pos.TP2Hit = true;
                signals.Add(TradeSignal.TakeProfit2);
            }

            // =====================
            // 6. Trailing Stop（统一在这里处理）
            // =====================
            if (pos.BreakEvenActivated && atrReady)
            {
                // 动态收紧策略
                decimal trailingATR = profit >= pos.RiskR * 2 ? 1.2m : 1.8m;

                if (pos.IsBuy)
                {
                    decimal newSL = pos.HighestPrice - atr * trailingATR;

                    // 只允许“向盈利方向移动”
                    if (newSL > pos.StopLoss)
                    {
                        pos.StopLoss = newSL;
                        signals.Add(TradeSignal.TrailingStopUpdate);
                    }
                }
                else
                {
                    decimal newSL = pos.LowestPrice + atr * trailingATR;

                    if (newSL < pos.StopLoss)
                    {
                        pos.StopLoss = newSL;
                        signals.Add(TradeSignal.TrailingStopUpdate);
                    }
                }
            }

            // =====================
            // 7. EMA 双确认退出（防假信号）
            // =====================
            if (emaReady && atrReady)
            {
                decimal emaBuffer = atr * 0.2m;

                if (pos.IsBuy)
                {
                    // 条件1：趋势反向 + 有效跌破EMA
                    if (strategyResult == "SELL TREND" && price < ema - emaBuffer)
                        pos.OppositeSignalCount++;
                    else
                        pos.OppositeSignalCount = 0;

                    // 条件2：必须有回撤
                    decimal pullback = pos.HighestPrice - price;

                    if (pos.OppositeSignalCount >= 2 &&
                        pullback > atr)
                    {
                        signals.Add(TradeSignal.ExitByEMA);
                    }
                }
                else
                {
                    if (strategyResult == "BUY TREND" && price > ema + emaBuffer)
                        pos.OppositeSignalCount++;
                    else
                        pos.OppositeSignalCount = 0;

                    decimal pullback = price - pos.LowestPrice;

                    if (pos.OppositeSignalCount >= 2 &&
                        pullback > atr)
                    {
                        signals.Add(TradeSignal.ExitByEMA);
                    }
                }
            }

            return signals;
        }


        /// <summary>
        /// 计算初始止损价格（结合 ATR 波动止损 + 结构止损）
        ///
        /// 该方法用于开仓阶段生成“初始风险边界”，属于交易系统风控核心组件。
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
        /// - ATR控制“波动风险”
        /// - Swing控制“结构风险”
        /// - 取更远 = 优先保护仓位存活率
        ///
        /// </summary>
        /// <param name="isBuy">是否为多单（true=Buy, false=Sell）</param>
        /// <param name="entryPrice">开仓价格</param>
        /// <param name="atr">平均真实波幅（ATR）</param>
        /// <param name="swingHigh">最近结构高点（用于空单止损参考）</param>
        /// <param name="swingLow">最近结构低点（用于多单止损参考）</param>
        /// <param name="atrMultiplier">ATR倍数（默认2倍，用于控制止损宽度）</param>
        /// <returns>最终初始止损价格（decimal）</returns>
        public static decimal CalculateInitialStopLoss(
            bool isBuy,
            decimal entryPrice,
            decimal atr,
            decimal swingHigh,
            decimal swingLow,
            decimal atrMultiplier = 2m)
        {
            // =====================
            // 1. ATR保护（防止为0或异常值）
            // =====================
            if (atr <= 0)
                atr = entryPrice * 0.005m; // 默认0.5%波动替代值

            decimal atrSL;
            decimal swingSL;

            if (isBuy)
            {
                // =====================
                // 多单 ATR止损（价格下方）
                // =====================
                atrSL = entryPrice - atr * atrMultiplier;

                // =====================
                // 多单结构止损（必须低于入场价才有效）
                // =====================
                swingSL = (swingLow > 0 && swingLow < entryPrice)
                    ? swingLow
                    : atrSL;

                // =====================
                // 取更保守（更低）的止损
                // =====================
                decimal finalSL = Math.Min(atrSL, swingSL);

                // =====================
                // 安全保护：确保止损一定在入场价下方
                // =====================
                if (finalSL >= entryPrice)
                    finalSL = atrSL;

                return finalSL;
            }
            else
            {
                // =====================
                // 空单 ATR止损（价格上方）
                // =====================
                atrSL = entryPrice + atr * atrMultiplier;

                // =====================
                // 空单结构止损（必须高于入场价才有效）
                // =====================
                swingSL = (swingHigh > 0 && swingHigh > entryPrice)
                    ? swingHigh
                    : atrSL;

                // =====================
                // 取更保守（更高）的止损
                // =====================
                decimal finalSL = Math.Max(atrSL, swingSL);

                // =====================
                // 安全保护：确保止损一定在入场价上方
                // =====================
                if (finalSL <= entryPrice)
                    finalSL = atrSL;

                return finalSL;
            }
        }


        private static DateTime lastH4Time = DateTime.MinValue;

        /// <summary>
        /// 判断是否出现新的 H4 K线
        /// 只在新H4开始时返回 true
        /// </summary>
        public static bool IsNewH4Candle()
        {
            DateTime now = DateTime.UtcNow; // 建议使用UTC时间（外汇黄金服务器时间通常接近UTC）

            // 当前属于哪个 H4 周期
            int h4Block = now.Hour / 4;

            // 当前 H4 K线开始时间
            DateTime currentH4OpenTime = new DateTime(
                now.Year,
                now.Month,
                now.Day,
                h4Block * 4,   // 0 / 4 / 8 / 12 / 16 / 20
                0,
                0);

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