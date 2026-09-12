using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uClient.Broker
{
    public class CalculateSpearmanCorrelationUtils
    {
        /*
        Spearman 相关性解释
        相关性 ρ 值	相关性强度	解释
        ρ ≈ 1.0	强正相关	黄金和白银的价格变化几乎同步上涨或下跌。
        0.5 ≤ ρ < 1.0	中等正相关	黄金和白银通常会一起变化，但不是完全同步。
        0 ≤ ρ < 0.5	弱正相关	两者价格变化方向相同，但相关性较弱。
        ρ = 0	无相关	两者价格变化无关，可能独立变化。
        -0.5 < ρ < 0	弱负相关	黄金上涨时，白银可能轻微下降，反之亦然。
        -1.0 < ρ ≤ -0.5	中等负相关	黄金和白银的价格通常相反变化。
        ρ ≈ -1.0	强负相关	黄金上涨时，白银几乎必定下跌，反之亦然。
         */
        /// <summary>
        /// 斯皮尔曼相关系数
        /// </summary>
        /// <param name="xx"></param>
        /// <param name="yy"></param>
        /// <returns></returns>
        public static double CalculateSpearmanCorrelation(List<double> xx, List<double> yy)
        {
            var (x, y) = Utils.FilterNonZeroData(xx, yy);
            // 计算斯皮尔曼相关系数
            double spearmanCorrelation = Correlation.Spearman(x, y);
            return Math.Round(spearmanCorrelation, 2);
        }
    }
}
