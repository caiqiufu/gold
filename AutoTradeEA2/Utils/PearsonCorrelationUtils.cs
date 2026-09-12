using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uClient.Broker
{
    public class PearsonCorrelationUtils
    {
        /*         
         r=1：完全正相关，即一个变量增加时，另一个变量严格线性递增。
         𝑟=−1
         r=−1：完全负相关，即一个变量增加时，另一个变量严格线性递减。
         𝑟=0
         r=0：无线性相关性，即变量之间没有线性关系（但可能存在其他类型的关系，例如非线性关系）。
         */
        /// <summary>
        /// 皮尔逊相关系数
        /// 通过相关系数来衡量两条曲线变化率的相关性，数值范围为-1到1，越接近1表示两者变化一致性越高
        /// 黄金和白银价格的变动通常具有较强的相关性，历史数据显示：
        /// 短期波动 可能在 0.3 ~ 0.8 之间（中等或较强正相关）。
        ///长期趋势 相关性可能高达 0.8 ~ 0.95（强正相关）。
        ///但在极端情况下（比如市场操控、经济政策影响），相关性可能下降到 0.1 ~ 0.3。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double PearsonCorrelation(List<double> xx, List<double> yy)
        {
            if (xx.Count != yy.Count) throw new ArgumentException("Data sets must have the same length");

            var (x, y) = Utils.FilterNonZeroData(xx, yy);

            double meanX = x.Average();
            double meanY = y.Average();
            double sumXY = 0, sumX2 = 0, sumY2 = 0;

            for (int i = 0; i < x.Count; i++)
            {
                sumXY += (x[i] - meanX) * (y[i] - meanY);
                sumX2 += System.Math.Pow(x[i] - meanX, 2);
                sumY2 += Math.Pow(y[i] - meanY, 2);
            }

            return sumXY / Math.Sqrt(sumX2 * sumY2);
        }
    }
}
