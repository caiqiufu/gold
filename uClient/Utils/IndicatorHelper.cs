using DocumentFormat.OpenXml.Office.CoverPageProps;
using M4.UserCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uClient.Comm
{
    /// <summary>
    /// 存储 KD 随机指标计算结果的结构。
    /// </summary>
    /// <summary>
    /// 存储 KD 随机指标 (Stochastic Oscillator) 的计算结果。
    /// KD 指标是动量震荡指标，用于衡量收盘价相对于给定周期内价格范围的位置，
    /// 主要用于识别超买/超卖区域和趋势反转信号。
    /// </summary>
    public struct Stochastic
    {
        /// <summary>
        /// %K 线（快线或慢速 K 线，取决于计算方法）。
        /// 通常是经过平滑处理的原始随机值（RSV）。
        /// 它对市场价格变化更为敏感，用于捕捉短期动量。
        /// </summary>
        public decimal K { get; set; }

        /// <summary>
        /// %D 线（慢线）。
        /// 是 %K 线的移动平均值（通常是 3 周期 SMA）。
        /// 它对价格变化更平滑，反映了市场的中期趋势方向。
        /// %K 线与 %D 线的交叉是重要的交易信号（金叉/死叉）。
        /// </summary>
        public decimal D { get; set; }

        /// <summary>
        /// 返回 KD 值的格式化字符串表示，便于日志输出和调试。
        /// 格式为 "K=xx.xx, D=yy.yy"。
        /// </summary>
        /// <returns>包含 K 和 D 值的字符串。</returns>
        public override string ToString()
        {
            return $"K={K:F2}, D={D:F2}";
        }
    }

    /// <summary>
    /// 包含所有指标计算和交易信号检测的辅助类。
    /// </summary>
    public class IndicatorHelper
    {
        // --- KD 默认参数 ---
        private const int K_PERIOD = 14;
        private const int K_SMOOTH_PERIOD = 3;
        private const int D_SMOOTH_PERIOD = 3;

        // --- H1/M5 共用的超买超卖线 ---
        private const decimal OVERBOUGHT_LEVEL = 80m; // 原超买线
        private const decimal OVERSOLD_LEVEL = 20m;  // 原超卖线

        // --- 新的测试/弱化过滤级别 ---
        private const decimal WEAK_OVERBOUGHT_LEVEL = 70m; // 弱化超买线
        private const decimal WEAK_OVERSOLD_LEVEL = 30m;  // 弱化超卖线

        // --- KD 核心计算方法 ---

        /// <summary>
        /// 📊 计算慢速 KD 随机指标 (Slow Stochastic Oscillator) 的 %K 和 %D 值。
        /// 该方法通过迭代计算历史 Raw %K 值，并进行两次简单移动平均 (SMA) 来实现平滑。
        /// </summary>
        /// <param name="candles">K线数据列表。</param>
        /// <param name="kPeriod">计算原始随机值 (RSV) 的观察周期 N。</param>
        /// <param name="kSmoothPeriod">计算平滑 %K 线的周期 S。</param>
        /// <param name="dSmoothPeriod">计算 %D 线的周期 M。</param>
        /// <returns>包含最新计算周期 %K 和 %D 值的 Stochastic 结构。</returns>
        public static Stochastic CalculateStochastic(
            List<Candle> candles,
            int kPeriod = K_PERIOD,
            int kSmoothPeriod = K_SMOOTH_PERIOD,
            int dSmoothPeriod = D_SMOOTH_PERIOD)
        {
            // 最小 K 线需求：N + S + M - 2 (确保能完成所有平滑计算)
            int minRequiredCandles = kPeriod + kSmoothPeriod + dSmoothPeriod - 2;
            if (candles == null || candles.Count < minRequiredCandles)
            {
                Console.WriteLine($"[KD Calc Error] 数据不足，需要至少 {minRequiredCandles} 根K线。当前只有 {candles?.Count ?? 0} 根。");
                return new Stochastic { K = 50m, D = 50m };
            }

            // --- 步骤 1: 计算所有历史 K 线的原始随机值 (Raw %K 或 RSV) ---
            var rawKList = new List<decimal>();

            for (int i = kPeriod - 1; i < candles.Count; i++)
            {
                var subset = candles.Skip(i - kPeriod + 1).Take(kPeriod).ToList();

                decimal lowestLow = subset.Min(c => c.Low);
                decimal highestHigh = subset.Max(c => c.High);
                decimal currentClose = candles[i].Close;

                decimal rawK;
                if (highestHigh == lowestLow) rawK = 50m; // 避免除以零
                else rawK = 100m * (currentClose - lowestLow) / (highestHigh - lowestLow);

                rawKList.Add(rawK);
            }

            // --- 步骤 2: 计算平滑 %K (Slow %K = SMA(Raw %K, S)) ---
            var finalKList = new List<decimal>();
            // 检查 rawKList 长度是否足够平滑
            if (rawKList.Count < kSmoothPeriod)
            {
                return new Stochastic { K = 50m, D = 50m };
            }

            for (int j = kSmoothPeriod - 1; j < rawKList.Count; j++)
            {
                decimal sum = rawKList.Skip(j - kSmoothPeriod + 1).Take(kSmoothPeriod).Sum();
                decimal finalK = sum / kSmoothPeriod;
                finalKList.Add(finalK);
            }

            // --- 步骤 3: 计算 %D (%D = SMA(Final %K, M)) ---
            var finalDList = new List<decimal>();
            // 检查 finalKList 长度是否足够平滑
            if (finalKList.Count < dSmoothPeriod)
            {
                return new Stochastic { K = 50m, D = 50m };
            }

            for (int l = dSmoothPeriod - 1; l < finalKList.Count; l++)
            {
                decimal sum = finalKList.Skip(l - dSmoothPeriod + 1).Take(dSmoothPeriod).Sum();
                decimal finalD = sum / dSmoothPeriod;
                finalDList.Add(finalD);
            }

            // --- 步骤 4: 返回最新的 K 和 D 值 ---
            if (finalDList.Count == 0) return new Stochastic { K = 50m, D = 50m };

            decimal latestK = finalKList.Last();
            decimal latestD = finalDList.Last();

            return new Stochastic
            {
                K = latestK,
                D = latestD
            };
        }

        // --- 周期间接调用方法 ---

        /// <summary>
        /// 计算 M5 周期 KD 值。
        /// </summary>
        public static Stochastic CalculateM5Stochastic(List<Candle> m5Candles)
        {
            return CalculateStochastic(m5Candles, K_PERIOD, K_SMOOTH_PERIOD, D_SMOOTH_PERIOD);
        }

        /// <summary>
        /// 计算 H1 周期 KD 值。
        /// </summary>
        public static Stochastic CalculateH1Stochastic(List<Candle> h1Candles)
        {
            return CalculateStochastic(h1Candles, K_PERIOD, K_SMOOTH_PERIOD, D_SMOOTH_PERIOD);
        }


        // --- 交叉检测方法 ---

        /// <summary>
        /// 检查 M5 周期是否发生了 KD 金叉 (Golden Cross)。
        /// </summary>
        public static bool IsM5KdGoldenCross(List<Candle> m5Candles)
        {
            if (m5Candles == null || m5Candles.Count < 2) return false;

            var currentStoch = CalculateM5Stochastic(m5Candles);
            var previousCandles = m5Candles.Take(m5Candles.Count - 1).ToList();
            var previousStoch = CalculateM5Stochastic(previousCandles);

            bool isCurrentlyCrossedUp = currentStoch.K > currentStoch.D;
            bool wasPreviouslyCrossedDownOrFlat = previousStoch.K <= previousStoch.D;

            return isCurrentlyCrossedUp && wasPreviouslyCrossedDownOrFlat;
        }

        /// <summary>
        /// 检查 M5 周期是否发生了 KD 死叉 (Death Cross)。
        /// </summary>
        public static bool IsM5KdDeathCross(List<Candle> m5Candles)
        {
            if (m5Candles == null || m5Candles.Count < 2) return false;

            var currentStoch = CalculateM5Stochastic(m5Candles);
            var previousCandles = m5Candles.Take(m5Candles.Count - 1).ToList();
            var previousStoch = CalculateM5Stochastic(previousCandles);

            bool isCurrentlyCrossedDown = currentStoch.K < currentStoch.D;
            bool wasPreviouslyCrossedUpOrFlat = previousStoch.K >= previousStoch.D;

            return isCurrentlyCrossedDown && wasPreviouslyCrossedUpOrFlat;
        }

        /// <summary>
        /// 检查 H1 KD K 线是否位于 D 线上方 (即金叉状态)，用于大趋势判断。
        /// </summary>
        public static bool IsH1KdTrendUp(List<Candle> h1Candles)
        {
            if (h1Candles == null || h1Candles.Count < 2) return false;

            var currentStoch = CalculateH1Stochastic(h1Candles);

            return currentStoch.K > currentStoch.D;
        }

        /// <summary>
        /// 检查 H1 KD K 线是否位于 D 线下方 (即死叉状态)，用于大趋势判断。
        /// </summary>
        public static bool IsH1KdTrendDown(List<Candle> h1Candles)
        {
            if (h1Candles == null || h1Candles.Count < 2) return false;

            var currentStoch = CalculateH1Stochastic(h1Candles);

            return currentStoch.K < currentStoch.D;
        }

        /// <summary>
        /// M5 短线做多信号检查 (多周期共振版)。
        /// 逻辑：H1 处于金叉状态 (大趋势向上) + M5 KD 已回调 (D线 <= 20) + M5 KD 形成金叉 (入场触发)。
        /// </summary>
        /// <param name="m5Candles">M5 K线列表。</param>
        /// <param name="h1Candles">H1 K线列表 (用于趋势过滤)。</param>
        /// <returns>如果共振做多条件成立，返回 true。</returns>
        public static bool IsM5BuySignalFiltered(List<Candle> m5Candles, List<Candle> h1Candles)
        {
            // 1. 检查 H1 趋势偏向 (大趋势判断: K > D)
            if (!IsH1KdTrendUp(h1Candles))
            {
                Console.WriteLine("【做多过滤】H1 KD 未处于金叉状态 (K < D)。信号被拒绝。");
                return false;
            }
            Console.WriteLine($"【做多过滤】✅ H1 趋势向上 ({CalculateH1Stochastic(h1Candles)})，允许寻找多单机会。");

            // 2. 获取 M5 KD 值 
            var currentStoch = CalculateM5Stochastic(m5Candles);

            // 3. 检查 M5 回调到位 (择时点: D线 <= OVERSOLD_LEVEL, 默认 20)
            bool isM5CooledDown = currentStoch.D <= OVERSOLD_LEVEL;

            if (!isM5CooledDown)
            {
                Console.WriteLine($"【做多过滤】M5 KD 尚未回调到位 ({currentStoch.D:F2} > {OVERSOLD_LEVEL})，等待更优价格。");
                return false;
            }
            Console.WriteLine($"【做多过滤】✅ M5 KD 已回调至超卖区 ({currentStoch.ToString()})。");

            // 4. 检查 M5 动能重新启动 (入场触发: 金叉)
            bool isGoldenCross = IsM5KdGoldenCross(m5Candles);

            if (!isGoldenCross)
            {
                Console.WriteLine("【做多过滤】M5 尚未形成金叉，动能未启动。");
                return false;
            }

            Console.WriteLine($"【做多信号】🔥 M5 金叉确认！符合 H1 趋势方向的回调做多机会。");
            return isGoldenCross;
        }

        /// <summary>
        /// M5 短线做空信号检查 (多周期共振版)。
        /// 逻辑：H1 处于死叉状态 (大趋势向下) + M5 KD 已反弹 (D线 >= 80) + M5 KD 形成死叉 (入场触发)。
        /// </summary>
        /// <param name="m5Candles">M5 K线列表。</param>
        /// <param name="h1Candles">H1 K线列表 (用于趋势过滤)。</param>
        /// <returns>如果共振做空条件成立，返回 true。</returns>
        public static bool IsM5SellSignalFiltered(List<Candle> m5Candles, List<Candle> h1Candles)
        {
            // 1. 检查 H1 趋势偏向 (大趋势判断: K < D)
            if (!IsH1KdTrendDown(h1Candles))
            {
                Console.WriteLine("【做空过滤】H1 KD 未处于死叉状态 (K > D)。信号被拒绝。");
                return false;
            }
            Console.WriteLine($"【做空过滤】✅ H1 趋势向下 ({CalculateH1Stochastic(h1Candles)})，允许寻找空单机会。");

            // 2. 获取 M5 KD 值 
            var currentStoch = CalculateM5Stochastic(m5Candles);

            // 3. 检查 M5 反弹到位 (择时点: D线 >= OVERBOUGHT_LEVEL)
            bool isM5Overheated = currentStoch.D >= OVERBOUGHT_LEVEL;

            if (!isM5Overheated)
            {
                Console.WriteLine($"【做空过滤】M5 KD 尚未反弹到位 ({currentStoch.D:F2} < {OVERBOUGHT_LEVEL})，等待更优价格。");
                return false;
            }
            Console.WriteLine($"【做空过滤】✅ M5 KD 已反弹至超买区 ({currentStoch.ToString()})。");

            // 4. 检查 M5 动能重新启动 (入场触发: 死叉)
            bool isDeathCross = IsM5KdDeathCross(m5Candles);

            if (!isDeathCross)
            {
                Console.WriteLine("【做空过滤】M5 尚未形成死叉，动能未启动。");
                return false;
            }

            Console.WriteLine($"【做空信号】🔥 M5 死叉确认！符合 H1 趋势方向的反弹做空机会。");
            return isDeathCross;
        }


        // --- 交易平仓信号方法（新增） ---

        /// <summary>
        /// 检查多单的平仓信号：M5 KD 上涨至超买区后形成死叉 (动能衰竭)。
        /// </summary>
        /// <param name="m5Candles">M5 K线列表。</param>
        /// <returns>如果应平仓 (超买死叉)，返回 true。</returns>
        public static bool IsM5CloseBuySignal(List<Candle> m5Candles)
        {
            // 1. 获取 M5 KD 值 
            var currentStoch = CalculateM5Stochastic(m5Candles);

            // 2. 检查超买区前提 (D线 >= OVERBOUGHT_LEVEL)
            bool isInOverboughtZone = currentStoch.D >= OVERBOUGHT_LEVEL;

            if (!isInOverboughtZone)
            {
                // Console.WriteLine($"[平仓过滤] M5 D线尚未达到超买区 ({currentStoch.D:F2})。");
                return false;
            }

            // 3. 检查动能衰竭 (死叉)
            bool isDeathCross = IsM5KdDeathCross(m5Candles);

            if (isDeathCross)
            {
                Console.WriteLine($"【平仓信号】🚨 多单平仓：M5 KD 在超买区 ({currentStoch.ToString()}) 形成死叉。");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查空单的平仓信号：M5 KD 下跌至超卖区后形成金叉 (动能衰竭)。
        /// </summary>
        /// <param name="m5Candles">M5 K线列表。</param>
        /// <returns>如果应平仓 (超卖金叉)，返回 true。</returns>
        public static bool IsM5CloseSellSignal(List<Candle> m5Candles)
        {
            // 1. 获取 M5 KD 值 
            var currentStoch = CalculateM5Stochastic(m5Candles);

            // 2. 检查超卖区前提 (D线 <= OVERSOLD_LEVEL)
            bool isInOversoldZone = currentStoch.D <= OVERSOLD_LEVEL;

            if (!isInOversoldZone)
            {
                // Console.WriteLine($"[平仓过滤] M5 D线尚未达到超卖区 ({currentStoch.D:F2})。");
                return false;
            }

            // 3. 检查动能衰竭 (金叉)
            bool isGoldenCross = IsM5KdGoldenCross(m5Candles);

            if (isGoldenCross)
            {
                Console.WriteLine($"【平仓信号】🚨 空单平仓：M5 KD 在超卖区 ({currentStoch.ToString()}) 形成金叉。");
                return true;
            }
            return false;
        }


        // --- 最终信号包装方法 (供策略调用) ---

        /// <summary>
        /// 封装多周期 KD 共振信号。
        /// </summary>
        /// <param name="m5Datas">M5 K线列表。</param>
        /// <param name="h1Datas">H1 K线列表。</param>
        /// <returns>"BUY"、"SELL" 或 "NO"。优先检查顺应 H1 趋势的信号。</returns>
        public static string DetectSingleKDMD5(List<Candle> m5Datas, List<Candle> h1Datas)
        {
            // 假设这是主循环调用的点，用于日志记录
            Console.WriteLine($"\n--- 检查交易信号 @ M5: {m5Datas.Last().Close:F4} | H1: {h1Datas.Last().Close:F4} ---");

            // 优先检查顺应 H1 趋势的信号
            if (IsM5BuySignalFiltered(m5Datas, h1Datas))
            {
                Console.WriteLine("【最终决策】BUY - 满足多周期共振做多条件。");
                return "BUY";
            }
            if (IsM5SellSignalFiltered(m5Datas, h1Datas))
            {
                Console.WriteLine("【最终决策】SELL - 满足多周期共振做空条件。");
                return "SELL";
            }

            Console.WriteLine("【最终决策】NO - 暂无有效共振信号。");
            return "NO";
        }

        // --- 保持原有的 Inside Bar 方法 ---

        /// <summary>
        /// 单一内包线突破策略（Single Inside Bar Breakout）
        /// </summary>
        /// <param name="candles">最近的K线序列，至少3根</param>
        /// <param name="currentPrice">当前市场价格（可以是最新tick，也可以是当前K线的收盘价）</param>
        /// <param name="minBreakoutRatio">最小突破比例</param>
        /// <returns>"BUY"、"SELL" 或 "NO"</returns>
        public static string DetectSingleInsideBars(List<Candle> candles, decimal currentPrice, decimal minBreakoutRatio = 0.2m)
        {
            if (candles == null || candles.Count < 3) return "NO";

            Candle motherBar = candles[candles.Count - 2];
            Candle insideBar = candles[candles.Count - 1];

            decimal motherRange = motherBar.High - motherBar.Low;
            decimal breakoutThreshold = motherRange * minBreakoutRatio;

            // 判断是否为 Inside Bar
            bool isInside = insideBar.High < motherBar.High && insideBar.Low > motherBar.Low;
            if (!isInside) return "NO";

            if (currentPrice > motherBar.High + breakoutThreshold) return "BUY";
            if (currentPrice < motherBar.Low - breakoutThreshold) return "SELL";

            return "NO";
        }
    }
}
