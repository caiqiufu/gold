using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uClient.Broker
{
    public class TradeRatioCalculatorUtils
    {
        /// <summary>
        /// 计算黄金和白银仓位比例,以1Lots黄金为标准
        /// </summary>
        /// <param name="goldPrices"></param>
        /// <param name="silverPrices"></param>
        /// <returns></returns>
        public static double GetGSLotRatio(List<double> goldPrices, List<double> silverPrices)
        {
            double goldLeverage = 100;
            double silverLeverage = 5000;
            double minRatio = 0.2;  // 最小手数比例
            double maxRatio = 2; // 最大手数比例

            double goldChangeValue = 0.01;
            double silverChangeValue = 0.01;
            if (goldPrices.Count > 1)
            {
                goldChangeValue = goldPrices.Last() - goldPrices.First();
            }
            if (silverPrices.Count > 1)
            {
                silverChangeValue = silverPrices.Last() - silverPrices.First();
            }
            double rawRatio = goldLeverage * goldChangeValue / (silverLeverage * silverChangeValue);
            double boundedRatio = Math.Max(minRatio, Math.Min(maxRatio, rawRatio)); // 限制范围

            return Math.Round(boundedRatio, 2);
        }
    }
}
