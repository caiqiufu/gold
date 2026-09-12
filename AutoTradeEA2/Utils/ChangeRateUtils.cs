using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;

namespace uClient.Broker
{
    /*
    总结
    简单差分法 和 中心差分法：快速实现，适合连续平稳数据。
    滑动窗口法 和 指数平滑法：平滑噪声，适合含噪声的时间序列。
    多项式拟合法 和 线性回归斜率：适合捕捉趋势变化。
    小波分析：适合非平稳和复杂数据，但实现较复杂。
    选择方法时需结合数据特性（如平稳性、噪声程度）和计算效率。
    结论
    短线投机者：用滑动窗口变化率分析短期波动。
    中期交易者：用线性回归斜率法判断趋势强度。
    长期投资者：用多项式拟合法或指数平滑法捕捉趋势拐点。
    高级分析需求：用小波分析捕捉局部特征。
     */
    public class ChangeRateUtils
    {
        /// <summary>
        /// 1. 简单差分法 (Simple Difference Method)
        /// 描述：计算相邻数据点的差值，得到基本的变化率。
        /// 优点：简单高效。
        /// 缺点：无法平滑噪声。
        /// 适用性：用于检测价格快速变化的敏感时刻，但不是最佳选择。
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<double> SimpleDifference(List<double> data)
        {
            var changeRates = new List<double>();
            for (int i = 0; i < data.Count - 1; i++)
            {
                changeRates.Add(data[i + 1] - data[i]);
            }
            return changeRates;
        }

        /// <summary>
        /// 2. 中心差分法 (Central Difference Method)
        /// 描述：使用前后点的差值计算变化率，减少误差。
        /// 优点：平滑噪声，适合稳定数据。
        /// 缺点：需要舍弃首尾数据点。
        /// 适用性：适合对较短时间窗口的变化进行平滑处理。
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<double> CentralDifference(List<double> data)
        {
            var changeRates = new List<double>();
            for (int i = 1; i < data.Count - 1; i++)
            {
                changeRates.Add((data[i + 1] - data[i - 1]) / 2);
            }
            return changeRates;
        }

        /// <summary>
        /// 3. 滑动窗口变化率 (Moving Average Rate)
        /// 描述：在滑动窗口内计算平均变化率，减少随机波动。
        /// 优点：减少随机噪声。
        /// 缺点：窗口大小的选择会影响结果。
        /// 适用性：适合中期价格变化趋势的分析，是分析黄金/白银波动时的可靠方法。
        /// </summary>
        /// <param name="values"></param>
        /// <param name="windowSize"></param>
        /// <returns></returns>
        public static List<double> MovingAverageRate(List<double> data, int windowSize)
        {
            var changeRates = new List<double>();
            double lastValidAvg = 0;
            for (int i = 0; i < data.Count; i++)
            {
                if (i <= data.Count - windowSize)
                {
                    double sum = 0;
                    for (int j = i; j < i + windowSize - 1; j++)
                    {
                        sum += data[j + 1] - data[j];
                    }
                    double avg = sum / (windowSize - 1);
                    lastValidAvg = avg;
                    changeRates.Add(avg);
                }
                else
                {
                    // 无法再计算时，使用最后一个有效平均值
                    changeRates.Add(lastValidAvg);
                }
            }
            return changeRates;
        }

        /// <summary>
        /// 4. 多项式拟合法 (Polynomial Fit)
        /// 描述：对局部数据拟合多项式，并通过导数计算变化率。
        /// 优点：适合捕捉非线性趋势。
        /// 缺点：计算量较大。
        /// 适用性：适合捕捉复杂的价格变化模式，尤其在寻找趋势反转点时表现出色。
        /// </summary>
        /// <param name="data">data</param>
        /// <param name="degree">多项式阶数 (int degree),阶数越高，拟合的曲线越复杂，可以捕获更多细节，但可能会导致过拟合,推荐值:
        ///一般选择 2（拟合为二次曲线）或 3（拟合为三次曲线）。
        ///若数据波动较大，可以选择较高阶数</param>
        /// <returns></returns>
        public static List<double> CalculatePolynomialFitRates(List<double> data, int degree)
        {
            var changeRates = new List<double>();

            for (int i = 0; i <= data.Count - degree; i++)
            {
                // 获取当前窗口数据
                double[] x = Enumerable.Range(0, degree).Select(Convert.ToDouble).ToArray();
                double[] y = data.Skip(i).Take(degree).ToArray();

                // 拟合多项式
                double[] coefficients = Fit.Polynomial(x, y, degree - 1);

                // 手动计算多项式的导数系数
                double[] derivativeCoefficients = new double[coefficients.Length - 1];
                for (int j = 1; j < coefficients.Length; j++)
                {
                    derivativeCoefficients[j - 1] = j * coefficients[j];
                }

                // 使用导数计算变化率
                double changeRate = EvaluatePolynomial(derivativeCoefficients, x.Last());
                changeRates.Add(changeRate);
            }

            return changeRates;
        }

        public static double EvaluatePolynomial(double[] coefficients, double x)
        {
            double result = 0;
            for (int i = 0; i < coefficients.Length; i++)
            {
                result += coefficients[i] * Math.Pow(x, i);
            }
            return result;
        }

        /// <summary>
        /// 5. 指数平滑法 (Exponential Smoothing)
        /// 描述：加权当前和历史变化率，近期数据权重大，适合处理噪声数据。
        /// 优点：减少随机波动，突出趋势。
        /// 缺点：需要选择合适的平滑因子
        /// 适用性：适合长期价格趋势的分析，例如投资者对黄金和白银的长期持仓策略分析。
        /// </summary>
        /// <param name="values"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static List<double> ExponentialSmoothingRate(List<double> data, double alpha)
        {
            var changeRates = new List<double>();
            double previousRate = 0;

            for (int i = 1; i < data.Count; i++)
            {
                double rate = data[i] - data[i - 1];
                double smoothedRate = alpha * rate + (1 - alpha) * previousRate;
                changeRates.Add(smoothedRate);
                previousRate = smoothedRate;
            }

            return changeRates;
        }

        /// <summary>
        /// 6. 线性回归斜率法 (Linear Regression Slope)
        /// 描述：通过滑动窗口内的线性回归模型计算斜率，斜率表示变化率。
        /// 优点：可以捕捉趋势变化率。
        /// 缺点：对局部数据敏感。
        /// 适用性：适合短期趋势和中期趋势分析，用于判断当前价格走势的强弱。
        /// </summary>
        /// <param name="values"></param>
        /// <param name="windowSize"></param>
        /// <returns></returns>
        public static List<double> LinearRegressionSlope(List<double> data, int windowSize)
        {
            var changeRates = new List<double>();

            for (int i = 0; i <= data.Count - windowSize; i++)
            {
                double[] x = Enumerable.Range(0, windowSize).Select(Convert.ToDouble).ToArray();
                double[] y = data.Skip(i).Take(windowSize).ToArray();

                double xMean = x.Average();
                double yMean = y.Average();

                double numerator = x.Zip(y, (xi, yi) => (xi - xMean) * (yi - yMean)).Sum();
                double denominator = x.Sum(xi => Math.Pow(xi - xMean, 2));

                double slope = numerator / denominator;
                changeRates.Add(slope);
            }
            return changeRates;
        }
        /// <summary>
        /// 整体变化率
        /// 优点
        /// 简单直接：支持直接输入数组区间，方便函数调用和测试。
        /// 错误处理：对输入长度和起始价格异常情况进行了保护性检查。
        /// 高扩展性：可以轻松扩展到其他金融数据的整体变化率计算。
        /// 适用场景
        /// 短期价格趋势分析：计算短时间内黄金价格涨跌幅。
        /// 交易策略测试：在历史数据中批量评估区间的涨跌趋势。
        /// 金融建模：作为更复杂分析模型的基础计算模块。
        /// </summary>
        /// <param name="priceRange"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double CalculateOverallChangeRate(List<double> priceRange)
        {
            // 校验输入有效性
            if (priceRange == null || priceRange.Count < 2)
            {
                throw new ArgumentException("价格数组区间为空或长度不足。");
            }

            // 获取起始价格和结束价格
            double startPrice = priceRange[0];
            double endPrice = priceRange[priceRange.Count - 1];

            // 确保起始价格不为零，避免除零异常
            if (startPrice == 0)
            {
                throw new ArgumentException("起始价格不能为零。");
            }

            // 计算整体变化率
            double overallChangeRate = (endPrice - startPrice) / startPrice;
            return overallChangeRate;
        }
        /// <summary>
        /// 计算平滑的平均变化值
        /// 范围：通常取值在 (0, 1] 范围内：
        ///α→1：当前值权重高，平滑效果较弱，敏感于价格变化。
        ///α→0：历史值权重高，平滑效果更强，响应较慢。
        /// </summary>
        /// <param name="prices"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double CalculateSmoothedAverageChange(List<double> pricesNoFilter, double alpha)
        {
            // 剔除值为0的元素，您也可以根据需求增加其他极端值剔除逻辑
            var prices = pricesNoFilter.Where(p => p != 0).ToList();

            if (prices == null || prices.Count < 2)
                throw new ArgumentException("价格列表至少需要两个值进行计算。");

            // 存储变化率
            List<double> changes = new List<double>();

            // 计算逐项变化率
            for (int i = 1; i < prices.Count; i++)
            {
                double change = prices[i] - prices[i - 1];
                changes.Add(change);
            }

            // 平滑变化率
            List<double> smoothedChanges = SmoothChanges(changes, alpha);

            // 计算平滑变化率的平均值
            double averageChange = smoothedChanges.Average();

            return averageChange;
        }
        /// <summary>
        /// 对变化率进行指数加权平滑
        /// </summary>
        /// <param name="changes"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public static List<double> SmoothChanges(List<double> changes, double alpha)
        {
            List<double> smoothed = new List<double>();
            double previousSmoothed = changes[0]; // 初始化为第一个变化率

            foreach (var change in changes)
            {
                // 指数平滑公式
                double currentSmoothed = alpha * change + (1 - alpha) * previousSmoothed;
                smoothed.Add(currentSmoothed);
                previousSmoothed = currentSmoothed;
            }

            return smoothed;
        }

        /// <summary>
        /// 计算平均变化值
        /// </summary>
        /// <param name="prices"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double CalculateAverageChange(List<double> prices)
        {
            // 剔除值为0的元素
            var filteredPrices = prices.Where(p => p != 0).ToList();

            // 如果数据少于2个点，无法计算变化值
            if (filteredPrices.Count < 2)
            {
                throw new InvalidOperationException("数据点太少，无法计算变化值");
            }

            // 计算连续两点之间的变化值
            double totalChange = 0;
            for (int i = 1; i < filteredPrices.Count; i++)
            {
                totalChange += Math.Abs(filteredPrices[i] - filteredPrices[i - 1]);
            }

            // 计算平均变化值
            double averageChange = totalChange / (filteredPrices.Count - 1);
            return averageChange;
        }
    }
}
