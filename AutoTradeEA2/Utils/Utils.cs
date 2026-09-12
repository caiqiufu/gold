using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using uClient.Comm;
using static uClient.Broker.IndicatorHelper;


namespace uClient.Broker
{
    public class Utils : uClient.Comm.Utils
    {
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 最佳子区间中找到纵坐标最接近修正平均值的点
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static IDictionary<string, string> AnalysisEA(List<(int x, double y)> points)
        {
            // 定义滑动窗口大小
            int windowSize = 10;
            if (points.Count < windowSize)
            {
                windowSize = points.Count;
            }

            // 初始化存储子区间及其修正平均值的列表
            List<List<(int x, double y)>> subIntervals = new List<List<(int x, double y)>>();
            List<double> averageValues = new List<double>();

            // 遍历所有可能的滑动窗口
            for (int i = 0; i <= points.Count - windowSize; i++)
            {
                List<(int x, double y)> subInterval = points.GetRange(i, windowSize);
                double avg = CalculateAdjustedAverage(subInterval);
                subIntervals.Add(subInterval);
                averageValues.Add(avg);
            }

            // 找到包含数据最多且修正平均值最大的子区间
            double maxAverageValue = averageValues.Max();
            int maxAverageIndex = averageValues.IndexOf(maxAverageValue);

            List<(int x, double y)> bestSubInterval = subIntervals[maxAverageIndex];

            // 在最佳子区间中找到纵坐标最接近修正平均值的点
            double bestAvg = CalculateAdjustedAverage(bestSubInterval);
            var bestPoint = bestSubInterval.OrderBy(p => Math.Abs(p.y - bestAvg)).First();
            IDictionary<string, string> resut = new Dictionary<string, string>
            {
                { "area", string.Join(", ", bestSubInterval.Select(p => $"({p.x}, {p.y})")) },
                { "bestEA", bestPoint.x.ToString() },
                { "bestProfitPoint", bestPoint.y.ToString() }
            };
            return resut;
        }

        /// <summary>
        /// 计算去除极端值后的修正平均值的方法
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double CalculateAdjustedAverage(List<(int x, double y)> values)
        {
            var yValues = values.Select(v => v.y).OrderBy(y => y).ToList();
            // 去除最高和最低的各一个值
            if (yValues.Count > 2)
            {
                yValues = yValues.Skip(1).Take(yValues.Count - 2).ToList();
            }
            return yValues.Average();
        }

        /// <summary>
        /// 获取中位数对应的X
        /// </summary>
        /// <param name="dataList"></param>
        /// <returns></returns>
        public static int GetMedianX(List<(int x, double y)> dataList)
        {
            // 按 y 值排序
            var sortedDataList = dataList.OrderBy(item => item.y).ToList();

            int size = sortedDataList.Count;
            int mid = size / 2;

            // 如果 y 值的数量是奇数，返回中间的 x 值
            if (size % 2 != 0)
            {
                return sortedDataList[mid].x;
            }
            // 如果 y 值的数量是偶数，选择任意一个中间 x 值（例如，返回 mid-1 对应的 x）
            else
            {
                return sortedDataList[mid - 1].x;
            }
        }

        /// <summary>
        /// 过滤非零数据
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static (List<double>, List<double>) FilterNonZeroData(List<double> x, List<double> y)
        {
            // 检查数组长度是否一致
            if (x.Count != y.Count)
                throw new ArgumentException("Input lists must have the same length.");

            // 筛选非零数据点
            var filteredX = new List<double>();
            var filteredY = new List<double>();

            for (int i = 0; i < x.Count; i++)
            {
                if (x[i] != 0 && y[i] != 0)
                {
                    filteredX.Add(x[i]);
                    filteredY.Add(y[i]);
                }
            }

            return (filteredX, filteredY);
        }


        /// <summary>
        /// 开单方向 
        /// 返回值大于0黄金上涨,白银下跌并且变化斜率一致, 黄金卖出,白银买入
        /// 返回值小于0黄金下跌,白银上涨并且黄金斜率比白银小, 黄金买入,白银卖出
        /// 黄金斜率 > 0 且白银斜率 < 0：黄金涨，白银跌。
        /// 黄金斜率 < 0 且白银斜率 > 0：黄金跌，白银涨。
        /// 3: 黄金上涨,白银下跌并且变化斜率不一致, 黄金卖出,白银买入
        /// 2: 黄金上涨,白银下跌并且黄金斜率比白银大, 黄金卖出,白银买入
        /// 1: 黄金斜率比白银大, 黄金卖出,白银买入
        /// -1: 黄金斜率比白银小,黄金买入,白银卖出
        /// -2: 黄金下跌,白银上涨并且黄金斜率比白银小, 黄金买入,白银卖出
        /// -3: 黄金下跌,白银上涨并且变化斜率一致, 黄金买入,白银卖出
        /// </summary>
        /// <param name="goldPrices"></param>
        /// <param name="silverPrices"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string DetermineTrendWithRegression(double chunkSize, double slope, List<double> goldPrices, List<double> silverPrices)
        {
            //变化斜率阀值
            //double slopeThreshold = 0.3;
            if (goldPrices.Count != silverPrices.Count || goldPrices.Count == 0)
            {
                //throw new ArgumentException("Gold and silver prices must have the same non-zero length");
                return "0";
            }
            double goldSlope = SlopeUtils.CalculateSlope(goldPrices);
            goldSlope = Math.Round(goldSlope, 3);
            double silverSlope = SlopeUtils.CalculateSlope(silverPrices);
            silverSlope = Math.Round(silverSlope, 3);
            //_logger.Info("goldPrices:" + string.Join(",", goldPrices) + ",silverPrices:" + string.Join(",", silverPrices));
            //_logger.Info("goldSlope:"+ goldSlope+ ",silverSlope:"+ silverSlope);
            //goldSlope < 0 黄金下跌时出现偏离,继续卖出黄金, goldSlope > 0 黄金上涨时出现偏离,继续买入黄金
            //1:黄金卖出,-1:黄金买入
            if (goldSlope != 0 && silverSlope != 0)
            {
                //长时间变化,假设黄金上涨完成,开始下跌
                if (chunkSize > 30)
                {

                    if (goldSlope < -slope)
                    {
                        return "-1";
                    }
                    if (goldSlope > slope)
                    {
                        return "1";
                    }
                }
                //短时间变化,假设黄金持续上涨
                if (chunkSize <= 30)
                {
                    if (goldSlope < -slope)
                    {
                        return "1";
                    }
                    if (goldSlope > slope)
                    {
                        return "-1";
                    }
                }
            }
            return "0";
        }
        public static string ListOfListsToString(List<List<double>> listOfLists)
        {
            List<string> rowStrings = new List<string>();

            foreach (var sublist in listOfLists)
            {
                string row = string.Join(", ", sublist);
                rowStrings.Add("[" + row + "]");
            }

            return "[" + string.Join(", ", rowStrings) + "]";
        }

        // =========================================================================
        // MARK: - 方法一: LINQ GroupBy (AggregateCandlesLinQ) - 极致聚合效率 O(N)
        // =========================================================================

        /// <summary>
        /// [高效聚合 + 严格连续性] 将基础K线列表高效地聚合成指定周期。
        /// 如果基础K线缺失，则该目标K线不会生成。
        /// </summary>
        /// <param name="baseCandles">基础 K线数据列表（例如 M1）。</param>
        /// <param name="baseMinutes">基础K线周期分钟数（如 M1->1, M5->5, H1->60）。</param>
        /// <param name="targetMinutes">目标 K线周期，以分钟为单位 (例如：5 -> M5, 60 -> H1, 1440 -> D1)。</param>
        /// <returns>聚合后的K线数据列表（升序）。</returns>
        public static List<Candle> AggregateCandlesStrictLinQ(List<Candle> baseCandles, int baseMinutes, int targetMinutes)
        {
            if (baseCandles == null || !baseCandles.Any() || baseMinutes <= 0 || targetMinutes <= 0)
                return new List<Candle>();

            // 每个目标K线需要多少根基础K线
            int barsPerTarget = targetMinutes / baseMinutes;
            if (barsPerTarget < 1)
                throw new ArgumentException("目标周期必须大于等于基础周期的整数倍");

            // 升序排序，确保连续性检查
            baseCandles = baseCandles.OrderBy(c => c.Time).ToList();

            // 分组 Key：计算每根K线所属目标周期起始时间
            var grouped = baseCandles.GroupBy(c =>
            {
                if (targetMinutes >= 1440 && targetMinutes % 1440 == 0)
                    return c.Time.Date; // D1 及以上按日期聚合

                long totalMinutes = (long)c.Time.TimeOfDay.TotalMinutes;
                long bucketStartMinutes = (totalMinutes / targetMinutes) * targetMinutes;
                return c.Time.Date.AddMinutes(bucketStartMinutes);
            });

            var result = new List<Candle>();

            foreach (var g in grouped.OrderBy(g => g.Key))
            {
                var slice = g.OrderBy(c => c.Time).ToList();

                // 严格连续性检查
                bool isContinuous = true;
                for (int i = 1; i < slice.Count; i++)
                {
                    if ((slice[i].Time - slice[i - 1].Time).TotalMinutes != baseMinutes)
                    {
                        isContinuous = false;
                        break;
                    }
                }

                // 仅当基础K线连续且数量达到目标K线所需时才生成
                if (isContinuous && slice.Count >= barsPerTarget)
                {
                    // 取前 barsPerTarget 根K线做聚合
                    var aggregateSlice = slice.Take(barsPerTarget).ToList();

                    result.Add(new Candle
                    {
                        Time = aggregateSlice[0].Time,
                        Open = aggregateSlice[0].Open,
                        Close = aggregateSlice.Last().Close,
                        High = aggregateSlice.Max(c => c.High),
                        Low = aggregateSlice.Min(c => c.Low),
                        Volume = aggregateSlice.Sum(c => c.Volume)
                    });
                }
            }

            return result;
        }


        /// <summary>
        /// 自动补齐 M1 缺失K线（严格按1分钟递增）
        /// 使用前一根收盘价平滑填充
        /// </summary>
        /// <param name="source">原始M1数据（必须按时间升序）</param>
        /// <returns>补齐后的完整M1序列</returns>
        public static List<Candle> FillMissingM1Candles(List<Candle> source)
        {
            if (source == null || source.Count < 2)
                return source;

            var result = new List<Candle>();
            result.Add(source[0]);

            for (int i = 1; i < source.Count; i++)
            {
                var prev = result.Last();
                var current = source[i];

                var diff = (int)(current.Time - prev.Time).TotalMinutes;

                // 如果出现断层
                if (diff > 1)
                {
                    for (int m = 1; m < diff; m++)
                    {
                        var fillTime = prev.Time.AddMinutes(m);

                        var filled = new Candle
                        {
                            Time = fillTime,
                            Open = prev.Close,
                            High = prev.Close,
                            Low = prev.Close,
                            Close = prev.Close,
                            Volume = 0,
                            IsFilled = true
                        };

                        result.Add(filled);
                    }
                }

                result.Add(current);
            }

            return result;
        }

        // =========================================================================
        // MARK: - 方法二: 优化的循环实现 (AggregateLoopOptimized) - 兼顾功能与效率 O(N)
        // =========================================================================

        /// <summary>
        /// 辅助方法：将时间对齐到目标周期的起始点。
        /// Helper method: Aligns time to the start of the target period.
        /// </summary>
        private static DateTime AlignTime(DateTime time, int periodMinutes)
        {
            // D1 周期或以上，对齐到当天 00:00:00
            if (periodMinutes >= 1440 && periodMinutes % 1440 == 0)
            {
                return time.Date;
            }

            // 日内周期，对齐到最近的周期起始时间
            long totalMinutes = (long)time.TimeOfDay.TotalMinutes;
            long alignedMinutes = (totalMinutes / periodMinutes) * periodMinutes;

            return time.Date.AddMinutes(alignedMinutes);
        }

        /// <summary>
        /// [功能优化] 使用优化的循环和指针将 K线聚合成指定周期。
        /// **效率较高 O(N)**，并支持缺失数据填充和部分周期控制。
        /// </summary>
        /// <param name="candles">输入 1-分钟 K线。</param>
        /// <param name="periodMinutes">目标周期（分钟）。</param>
        /// <param name="fillMissing">是否填充缺失的 K线（i.e., fill time gaps）。</param>
        /// <param name="includePartial">是否包含末尾不完整的周期。</param>
        /// <returns>聚合后的 K线列表。</returns>
        public static List<Candle> AggregateLoopOptimized(
            List<Candle> candles,
            int periodMinutes,
            bool fillMissing = false,
            bool includePartial = true)
        {
            var result = new List<Candle>();
            if (candles == null || !candles.Any()) return result;

            // 确保数据已排序
            var ordered = candles.OrderBy(c => c.Time).ToList();

            // 数据的实际截止时间 (最后一个 K 线结束的时间)
            var lastM1EndTime = ordered.Last().Time.AddMinutes(1);

            // **指针/索引优化**：记录当前处理到原始列表的哪个位置
            int currentIndex = 0;

            // 对齐起始时间
            var startTime = AlignTime(ordered.First().Time, periodMinutes);

            // 循环遍历，直到超过数据的实际末尾时间
            while (startTime < lastM1EndTime)
            {
                var endTime = startTime.AddMinutes(periodMinutes);

                bool isPartialPeriod = endTime > lastM1EndTime;

                if (isPartialPeriod && !includePartial)
                {
                    break;
                }

                // **关键优化点**：使用索引循环代替 Where(c => c.Time >= startTime && c.Time < endTime).ToList()
                int periodStart = currentIndex;
                int periodEnd = currentIndex;

                // 找到周期内 K 线的结束位置
                while (periodEnd < ordered.Count && ordered[periodEnd].Time < endTime)
                {
                    periodEnd++;
                }

                // 周期内 K 线的数量
                int count = periodEnd - periodStart;

                if (count > 0)
                {
                    // 合并 K 线
                    var firstCandle = ordered[periodStart];
                    var lastCandle = ordered[periodEnd - 1]; // 注意索引

                    var agg = new Candle
                    {
                        Time = startTime,
                        Open = firstCandle.Open,
                        Close = lastCandle.Close,
                        High = ordered.GetRange(periodStart, count).Max(c => c.High),
                        Low = ordered.GetRange(periodStart, count).Min(c => c.Low),
                        Volume = ordered.GetRange(periodStart, count).Sum(c => c.Volume)
                    };
                    result.Add(agg);

                    // 更新索引到已处理 K 线的下一个位置
                    currentIndex = periodEnd;
                }
                else if (fillMissing && !isPartialPeriod) // 只有在不是最后一个不完整周期时才填充
                {
                    // 用前一根的收盘价补齐缺失的 K 线 (平盘)
                    var prevClose = result.Count > 0 ? result.Last().Close : 0;
                    var agg = new Candle
                    {
                        Time = startTime,
                        Open = prevClose,
                        High = prevClose,
                        Low = prevClose,
                        Close = prevClose,
                        Volume = 0 // 填充的 K线，成交量为 0
                    };
                    result.Add(agg);
                }

                startTime = endTime; // 移动到下一个周期
            }

            return result;
        }

        /// <summary>
        /// 在回测模式下，将当前时间回退指定的K线周期（分钟），以确保指标基于已收线的K线计算。
        /// 
        /// 此方法实现了以下逻辑:
        /// // 将时间回退指定的周期
        /// currentDateTime = currentDateTime.Subtract(TimeSpan.FromMinutes(adjustmentMinutes));
        /// 
        /// // 更新时间字符串，供 K 线数据获取使用
        /// currentDateTimeStr = currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        /// </summary>
        /// <param name="currentDateTimeStr">当前的日期时间字符串 (格式: yyyy-MM-dd HH:mm:ss)。</param>
        /// <param name="adjustmentMinutes">指定要从当前时间回退的时间间隔（以分钟为单位，例如：5 表示 M5 周期，60 表示 H1 周期）。</param>
        /// <returns>调整后的日期时间字符串。</returns>
        public static string AdjustSimulatedTime(string currentDateTimeStr, int adjustmentMinutes)
        {
            if (!DateTime.TryParse(currentDateTimeStr, out DateTime currentDateTime))
            {
                // 如果时间字符串格式错误，记录错误并返回原始字符串，防止崩溃
                // Console.WriteLine($"[ERROR] 无法解析日期时间字符串: {currentDateTimeStr}");
                return currentDateTimeStr;
            }

            // 将输入的分钟数转换为 TimeSpan
            TimeSpan adjustmentPeriod = TimeSpan.FromMinutes(adjustmentMinutes);

            // 将时间回退指定的周期
            currentDateTime = currentDateTime.Subtract(adjustmentPeriod);

            // 返回更新后的时间字符串
            return currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 盈利点数计算
        /// </summary>
        /// <param name="_Context"></param>
        /// <returns></returns>
        public static IDictionary<string, Object> ClosePositionsInProfit(IDictionary<string, Object> _Context)
        {
            string result = _Context["CurrentOrderDetail"].ToString();
            //正常开仓价
            double openPrice = Math.Round(Convert.ToDouble(_Context["openPrice"]), 2);
            //普通加仓价
            double inOpenPrice = 0;
            if (_Context.ContainsKey("inOpenPrice"))
            {
                inOpenPrice = Math.Round(Convert.ToDouble(_Context["inOpenPrice"]), 2);
            }

            //马丁加仓完成标识
            bool MartingaleAddPosition1 = Convert.ToBoolean(_Context["MartingaleAddPosition1"]);
            bool MartingaleAddPosition2 = Convert.ToBoolean(_Context["MartingaleAddPosition2"]);
            bool MartingaleAddPosition3 = Convert.ToBoolean(_Context["MartingaleAddPosition3"]);
            //马丁加仓手数
            double MartingaleAddPositionLot1 = Math.Round(Convert.ToDouble(_Context["MartingaleAddPositionLot1"]), 2);
            double MartingaleAddPositionLot2 = Math.Round(Convert.ToDouble(_Context["MartingaleAddPositionLot2"]), 2);
            double MartingaleAddPositionLot3 = Math.Round(Convert.ToDouble(_Context["MartingaleAddPositionLot3"]), 2);
            //分批平仓手数
            double ManagePositionTakeProfitLot1 = Math.Round(Convert.ToDouble(_Context["ManagePositionTakeProfitLot1"]), 2);
            double ManagePositionTakeProfitLot2 = Math.Round(Convert.ToDouble(_Context["ManagePositionTakeProfitLot2"]), 2);
            double MartingaleAddPosition1Price = 0;
            double MartingaleAddPosition2Price = 0;
            double MartingaleAddPosition3Price = 0;
            if (_Context.ContainsKey("AddPositionLevel"))
            {
                int AddPositionLevel = Convert.ToInt32(_Context["AddPositionLevel"]);
                string AddPosition = "M" + AddPositionLevel.ToString();

                if (MartingaleAddPosition1)
                {
                    MartingaleAddPosition1Price = Math.Round(Convert.ToDouble(_Context["openPrice" + AddPosition]), 2);
                }

                if (MartingaleAddPosition2)
                {
                    MartingaleAddPosition2Price = Math.Round(Convert.ToDouble(_Context["openPrice" + AddPosition]), 2);
                }

                if (MartingaleAddPosition3)
                {
                    MartingaleAddPosition3Price = Math.Round(Convert.ToDouble(_Context["openPrice" + AddPosition]), 2);
                }
            }
            double closePriceP1 = 0;
            if (_Context.ContainsKey("closePriceP1"))
            {
                closePriceP1 = Math.Round(Convert.ToDouble(_Context["closePriceP1"]), 2);
            }
            double closePriceP2 = 0;
            if (_Context.ContainsKey("closePriceP2"))
            {
                closePriceP2 = Math.Round(Convert.ToDouble(_Context["closePriceP2"]), 2);
            }
            //有分批平仓,最后平仓价格
            double closePriceAll = 0;
            if ((closePriceP1 > 0 || closePriceP2 > 0) && _Context.ContainsKey("closePrice"))
            {
                closePriceAll = Math.Round(Convert.ToDouble(_Context["closePrice"]), 2);
            }
            //平仓价
            double closePrice = 0;
            if (_Context.ContainsKey("closePrice"))
            {
                closePrice = Math.Round(Convert.ToDouble(_Context["closePrice"]), 2);
            }
            double gsMaxProfitValue = Math.Round(Convert.ToDouble(_Context["gsMaxProfitValue"]), 2);
            double gsMaxLossValue = Math.Round(Convert.ToDouble(_Context["gsMaxLossValue"]), 2);

            //指令明细:orderTypeDetail:openPrice:closePrice:profit
            string orderTypeDetailRecord = "";
            //盈利
            double profit = 0;
            List<string> openPriceList = new List<string>();
            List<string> closePriceList = new List<string>();
            //全部平仓
            if (result.Contains(":CLOSE_BUY:") || result.Contains(":CLOSE_SELL:"))
            {
                if (result.Contains(":CLOSE_BUY:"))
                {
                    profit = closePrice - openPrice;
                    openPriceList.Add(openPrice.ToString());
                    closePriceList.Add(closePrice.ToString());
                    if (inOpenPrice > 0)
                    {
                        profit = profit + closePrice - inOpenPrice;
                        openPriceList.Add(inOpenPrice.ToString());
                        closePriceList.Add(closePrice.ToString());
                    }
                    if (MartingaleAddPosition1Price > 0)
                    {
                        profit = profit + (closePrice - MartingaleAddPosition1Price) * MartingaleAddPositionLot1;
                        openPriceList.Add(MartingaleAddPosition1Price.ToString());
                        closePriceList.Add(closePrice.ToString());
                    }
                    if (MartingaleAddPosition2Price > 0)
                    {
                        profit = profit + (closePrice - MartingaleAddPosition2Price) * MartingaleAddPositionLot2;
                        openPriceList.Add(MartingaleAddPosition2Price.ToString());
                        closePriceList.Add(closePrice.ToString());
                    }
                    if (MartingaleAddPosition3Price > 0)
                    {
                        profit = profit + (closePrice - MartingaleAddPosition3Price) * MartingaleAddPositionLot3;
                        openPriceList.Add(MartingaleAddPosition3Price.ToString());
                        closePriceList.Add(closePrice.ToString());
                    }
                    orderTypeDetailRecord = "CLOSE_BUY" + ":" + string.Join("|", openPriceList) + ":" + string.Join("|", closePriceList) + ":" + Math.Round(profit, 2) + ":" + gsMaxProfitValue + ":" + gsMaxLossValue;
                }
                if (result.Contains(":CLOSE_SELL:"))
                {
                    profit = -(closePrice - openPrice);
                    openPriceList.Add(openPrice.ToString());
                    closePriceList.Add(closePrice.ToString());
                    if (inOpenPrice > 0)
                    {
                        profit = profit - (closePrice - inOpenPrice);
                        openPriceList.Add(inOpenPrice.ToString());
                        closePriceList.Add(closePrice.ToString());
                    }
                    if (MartingaleAddPosition1Price > 0)
                    {
                        profit = profit - (closePrice - MartingaleAddPosition1Price) * MartingaleAddPositionLot1;
                        openPriceList.Add(MartingaleAddPosition1Price.ToString());
                        closePriceList.Add(closePrice.ToString());
                    }
                    if (MartingaleAddPosition2Price > 0)
                    {
                        profit = profit - (closePrice - MartingaleAddPosition2Price) * MartingaleAddPositionLot2;
                        openPriceList.Add(MartingaleAddPosition2Price.ToString());
                        closePriceList.Add(closePrice.ToString());
                    }
                    if (MartingaleAddPosition3Price > 0)
                    {
                        profit = profit - (closePrice - MartingaleAddPosition3Price) * MartingaleAddPositionLot3;
                        openPriceList.Add(MartingaleAddPosition3Price.ToString());
                        closePriceList.Add(closePrice.ToString());
                    }
                    orderTypeDetailRecord = "CLOSE_SELL" + ":" + string.Join("|", openPriceList) + ":" + string.Join("|", closePriceList) + ":" + Math.Round(profit, 2) + ":" + gsMaxProfitValue + ":" + gsMaxLossValue;
                }
            }
            //1)所有加仓完成后再会平仓
            //2)假设启动部分平仓后不会再有加仓
            //部分平仓
            if (result.Contains(":P_CLOSE_BUY:") || result.Contains(":P_CLOSE_SELL:"))
            {
                if (result.Contains(":P_CLOSE_BUY:"))
                {
                    if (closePriceP1 > 0)
                    {
                        if (MartingaleAddPosition1Price > 0)
                        {
                            profit = profit - ((closePrice - MartingaleAddPosition1Price) * ManagePositionTakeProfitLot1 - (closePriceP1 - openPrice) * ManagePositionTakeProfitLot1) * MartingaleAddPositionLot1;
                        }
                        else if (MartingaleAddPosition2Price > 0)
                        {
                            profit = profit - ((closePrice - MartingaleAddPosition2Price) * ManagePositionTakeProfitLot1 - (closePriceP1 - openPrice) * ManagePositionTakeProfitLot1) * MartingaleAddPositionLot2;
                        }
                        else if (MartingaleAddPosition3Price > 0)
                        {
                            profit = profit - ((closePrice - MartingaleAddPosition3Price) * ManagePositionTakeProfitLot1 - (closePriceP1 - openPrice) * ManagePositionTakeProfitLot1) * MartingaleAddPositionLot3;
                        }
                        else
                        {
                            profit = profit - ((closePrice - openPrice) * ManagePositionTakeProfitLot1 - (closePriceP1 - openPrice) * ManagePositionTakeProfitLot1);
                        }
                    }
                    if (closePriceP2 > 0)
                    {
                        if (MartingaleAddPosition1Price > 0)
                        {
                            profit = profit - ((closePrice - MartingaleAddPosition1Price) * ManagePositionTakeProfitLot2 - (closePriceP2 - openPrice) * ManagePositionTakeProfitLot2) * MartingaleAddPositionLot1;
                        }
                        else if (MartingaleAddPosition2Price > 0)
                        {
                            profit = profit - ((closePrice - MartingaleAddPosition2Price) * ManagePositionTakeProfitLot2 - (closePriceP2 - openPrice) * ManagePositionTakeProfitLot2) * MartingaleAddPositionLot2;
                        }
                        else if (MartingaleAddPosition3Price > 0)
                        {
                            profit = profit - ((closePrice - MartingaleAddPosition3Price) * ManagePositionTakeProfitLot2 - (closePriceP2 - openPrice) * ManagePositionTakeProfitLot2) * MartingaleAddPositionLot3;
                        }
                        else
                        {
                            profit = profit - ((closePrice - openPrice) * ManagePositionTakeProfitLot2 - (closePriceP2 - openPrice) * ManagePositionTakeProfitLot2);
                        }
                    }
                }
                if (result.Contains(":P_CLOSE_SELL:"))
                {
                    if (closePriceP1 > 0)
                    {
                        if (MartingaleAddPosition1Price > 0)
                        {
                            profit = profit + ((closePrice - MartingaleAddPosition1Price) * ManagePositionTakeProfitLot1 - (closePriceP1 - openPrice) * ManagePositionTakeProfitLot1) * MartingaleAddPositionLot1;
                        }
                        else if (MartingaleAddPosition2Price > 0)
                        {
                            profit = profit + ((closePrice - MartingaleAddPosition2Price) * ManagePositionTakeProfitLot1 - (closePriceP1 - openPrice) * ManagePositionTakeProfitLot1) * MartingaleAddPositionLot2;
                        }
                        else if (MartingaleAddPosition3Price > 0)
                        {
                            profit = profit + ((closePrice - MartingaleAddPosition3Price) * ManagePositionTakeProfitLot1 - (closePriceP1 - openPrice) * ManagePositionTakeProfitLot1) * MartingaleAddPositionLot3;
                        }
                        else
                        {
                            profit = profit + ((closePrice - openPrice) * ManagePositionTakeProfitLot1 - (closePriceP1 - openPrice) * ManagePositionTakeProfitLot1);
                        }
                    }
                    if (closePriceP2 > 0)
                    {
                        if (MartingaleAddPosition1Price > 0)
                        {
                            profit = profit + ((closePrice - MartingaleAddPosition1Price) * ManagePositionTakeProfitLot2 - (closePriceP2 - openPrice) * ManagePositionTakeProfitLot2) * MartingaleAddPositionLot1;
                        }
                        else if (MartingaleAddPosition2Price > 0)
                        {
                            profit = profit + ((closePrice - MartingaleAddPosition2Price) * ManagePositionTakeProfitLot2 - (closePriceP2 - openPrice) * ManagePositionTakeProfitLot2) * MartingaleAddPositionLot2;
                        }
                        else if (MartingaleAddPosition3Price > 0)
                        {
                            profit = profit + ((closePrice - MartingaleAddPosition3Price) * ManagePositionTakeProfitLot2 - (closePriceP2 - openPrice) * ManagePositionTakeProfitLot2) * MartingaleAddPositionLot3;
                        }
                        else
                        {
                            profit = profit + ((closePrice - openPrice) * ManagePositionTakeProfitLot2 - (closePriceP2 - openPrice) * ManagePositionTakeProfitLot2);
                        }
                    }
                }
            }
            if (result.Contains(":BUY:") || result.Contains(":BUY_K:") || result.Contains(":BUY_H:"))
            {
                orderTypeDetailRecord = "BUY" + ":" + openPrice + ":-99.00" + ":-99.00";
            }
            else if (result.Contains(":IN_BUY:"))
            {
                orderTypeDetailRecord = "BUY" + ":" + inOpenPrice + ":-99.00" + ":-99.00";
            }
            else if (result.Contains(":M1_BUY:"))
            {
                orderTypeDetailRecord = "BUY" + ":" + MartingaleAddPosition1Price + ":-99.00" + ":-99.00";
            }
            else if (result.Contains(":M2_BUY:"))
            {
                orderTypeDetailRecord = "BUY" + ":" + MartingaleAddPosition2Price + ":-99.00" + ":-99.00";
            }
            else if (result.Contains(":M3_BUY:"))
            {
                orderTypeDetailRecord = "BUY" + ":" + MartingaleAddPosition3Price + ":-99.00" + ":-99.00";
            }
            if (result.Contains(":SELL:") || result.Contains(":SELL_K:") || result.Contains(":SELL_H:"))
            {
                orderTypeDetailRecord = "SELL" + ":" + openPrice + ":-99.00" + ":-99.00";
            }
            else if (result.Contains(":IN_SELL:"))
            {
                orderTypeDetailRecord = "SELL" + ":" + inOpenPrice + ":-99.00" + ":-99.00";
            }
            else if (result.Contains(":M1_SELL:"))
            {
                orderTypeDetailRecord = "SELL" + ":" + MartingaleAddPosition1Price + ":-99.00" + ":-99.00";
            }
            else if (result.Contains(":M2_SELL:"))
            {
                orderTypeDetailRecord = "SELL" + ":" + MartingaleAddPosition2Price + ":-99.00" + ":-99.00";
            }
            else if (result.Contains(":M3_SELL:"))
            {
                orderTypeDetailRecord = "SELL" + ":" + MartingaleAddPosition3Price + ":-99.00" + ":-99.00";
            }

            IDictionary<string, Object> profitResult = new Dictionary<string, Object>();
            profitResult.Add("profit", Math.Round(profit, 2));
            profitResult.Add("orderTypeDetailRecord", orderTypeDetailRecord);
            return profitResult;
        }

        /// <summary>
        /// 将 Candle 列表转换为易读的表格字符串，适合写入日志
        /// </summary>
        public static string ToLogCandleString(List<Candle> candles, string title = "Candle List")
        {
            if (candles == null || candles.Count == 0)
                return $"{title}: [Empty]";

            var sb = new StringBuilder();
            sb.AppendLine($"--- {title} (Count: {candles.Count}) ---");
            // 表头
            sb.AppendLine("Time                | Open      | High      | Low       | Close     | Volume");
            sb.AppendLine("--------------------------------------------------------------------------------");

            foreach (var c in candles)
            {
                // 使用插值字符串和对齐格式 (例如 :F2 表示两位小数，-19 表示占位并左对齐)
                sb.AppendLine($"{c.Time:yyyy-MM-dd HH:mm:ss} | {c.Open,-9:F2} | {c.High,-9:F2} | {c.Low,-9:F2} | {c.Close,-9:F2} | {c.Volume}");
            }

            return sb.ToString();
        }

        public static string ToLogKDJString(List<KdResult> kdjs, string title = "KDJ List")
        {
            if (kdjs == null || kdjs.Count == 0)
                return $"{title}: [Empty]";

            var sb = new StringBuilder();
            sb.AppendLine($"--- {title} (Count: {kdjs.Count}) ---");
            // 表头
            sb.AppendLine("Time                | K      | D      | J ");
            sb.AppendLine("--------------------------------------------------------------------------------");

            foreach (var c in kdjs)
            {
                // 使用插值字符串和对齐格式 (例如 :F2 表示两位小数，-19 表示占位并左对齐)
                sb.AppendLine($"{c.dateTime:yyyy-MM-dd HH:mm:ss} | {c.K,-9:F2} | {c.D,-9:F2} | {c.J,-9:F2}");
            }

            return sb.ToString();
        }
        /// <summary>
        /// 把KDJ字符串转换成数组
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<KdResult> ParseKDJ(string input)
        {
            var list = new List<KdResult>();

            var matches = Regex.Matches(input,
                @"dateTime=(?<time>[\d\-:\s]+),K=(?<k>[\d\.]+),D=(?<d>[\d\.]+)");

            foreach (Match m in matches)
            {
                decimal k = decimal.Parse(m.Groups["k"].Value);
                decimal d = decimal.Parse(m.Groups["d"].Value);

                list.Add(new KdResult
                {
                    dateTime = m.Groups["time"].Value,
                    K = k,
                    D = d,
                    J = 3 * k - 2 * d   // 常规J值公式
                });
            }

            return list;
        }

        /// <summary>
        /// 记录 KDJ 策略的执行全量详细信息
        /// </summary>
        /// <param name="kdHistory">KDJ 计算历史序列</param>
        /// <param name="config">当前周期的配置参数</param>
        /// <param name="strategyResult">策略决策结果 (BUY/SELL/NONE)</param>
        /// <param name="period">周期名称</param>
        /// <param name="currentDateTimeStr">当前 K 线时间</param>
        public static void LogKdjReport(List<KdResult> kdHistory, PeriodConfig config, string strategyResult, string period, string currentDateTimeStr)
        {
            if (kdHistory == null || kdHistory.Count < 2) return;

            var current = kdHistory[kdHistory.Count - 1];
            var previous = kdHistory[kdHistory.Count - 2];

            // 1. 格式化 KDJ 历史序列
            // C# 7.3 环境下使用 Select 构建字符串列表后再 Join
            var historyRows = kdHistory.Select((kd, idx) =>
                $"[{idx}] {kd.dateTime} | K:{kd.K:F2} D:{kd.D:F2} J:{kd.J:F2} | " +
                $"SlopeK:{kd.SlopeK:F2} | Gap:{Math.Abs(kd.K - kd.D):F2} | " +
                $"Cross:{(kd.IsGoldenCross ? "GOLDEN" : (kd.IsDeathCross ? "DEATH" : "NONE"))}"
            );
            string allKdStr = string.Join("\n    ", historyRows);

            // 2. 使用 StringBuilder 组装 (高性能)
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("==================== KDJ Strategy Execution Report ====================");
            sb.AppendLine($"[Execution Time]   : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"[Current Bar Time] : {currentDateTimeStr}");
            sb.AppendLine($"[Target Period]    : {period}");
            sb.AppendLine("-------------------- Market Data History --------------------");
            sb.AppendLine($"    {allKdStr}");
            sb.AppendLine("-------------------- Decision Metadata ----------------------");
            sb.AppendLine($"[Config Threshold] : Slope > {config.SlopeThreshold}, OB:{config.Overbought}, OS:{config.Oversold}");
            sb.AppendLine($"[Current Metrics]  : K={current.K:F2}, Slope={current.SlopeK:F2}, PrevSlope={previous.SlopeK:F2}, Gap={Math.Abs(current.K - current.D):F2}");
            sb.AppendLine($"[Strategy Result]  : {strategyResult}");
            sb.AppendLine("=======================================================================");

            Console.WriteLine(sb.ToString());
        }



        /// <summary>
        /// 全量输出 K 线列表到日志中，不进行任何截断
        /// </summary>
        /// <param name="candles">K 线列表</param>
        /// <param name="listName">列表名称 (如 M5Candles)</param>
        public static void LogAllCandles(List<Candle> candles, string listName)
        {
            if (candles == null || candles.Count == 0)
            {
                Console.WriteLine($"[{listName}] 列表为空，无数据可记录。");
                return;
            }

            // 预估容量：每行约 100 字符，防止 StringBuilder 频繁扩容
            StringBuilder sb = new StringBuilder(candles.Count * 100);

            sb.AppendLine($"\n>>> BEGIN FULL CANDLE DUMP: {listName} (Total: {candles.Count}) <<<");

            // 表头
            sb.AppendLine("Index | Time                | Open       | High       | Low        | Close      | Vol      | Filled");
            sb.AppendLine(new string('-', 95));

            for (int i = 0; i < candles.Count; i++)
            {
                var c = candles[i];
                // 使用 string.Format 进行严格对齐
                sb.AppendLine(string.Format("{0,5} | {1,-19} | {2,10:F2} | {3,10:F2} | {4,10:F2} | {5,10:F2} | {6,8:F0} | {7}",
                    i,
                    c.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    c.Open,
                    c.High,
                    c.Low,
                    c.Close,
                    c.Volume,
                    c.IsFilled ? "Y" : "N"));

                // 关键点：如果数据量达到上万级别，建议每 1000 行写入一次日志组件，防止单个字符串过大
                if (i > 0 && i % 1000 == 0)
                {
                    Console.WriteLine(sb.ToString());
                    sb.Clear();
                }
            }

            sb.AppendLine($">>> END FULL CANDLE DUMP: {listName} <<< \n");
            Console.WriteLine(sb.ToString());
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}