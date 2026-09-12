using MathNet.Numerics.LinearRegression;
using System;
using System.Collections.Generic;


namespace uClient.Broker
{
    public class SlopeUtils
    {
        /*
        *****注意数据类型
        *参数   count              数据个数 数组行(列)的个数 数组的行列数目相等
        *参数   dataCol_X[count]   数据的列数据
        *参数   dataRow_Y[count]   数据的行数据
        *返回值 k                  斜率 

        *使用注意 需要重定义数据类型
        *typedef unsigned char  uint8;
        *typedef unsigned int   uint16;
        *typedef unsigned long  uint32;
        */
        public static double get_K(double[] dataCol_X , double[] dataRow_Y)
        {
            double k = 0;//斜率
            double aveCol_X = 0;//列的平均值x
            double aveRow_Y = 0;//行的平均值y
            double sum_XY = 0;//行列的总和xy
            double sumRow_Y = 0;//行的总和y
            double sumCol_X = 0;//列的总和x
            double sumCol_X2 = 0;//列的总和x^2	
            int count = dataCol_X.Length;
            for (int i = 0; i < count; i++)
            {
                sumCol_X += dataCol_X[i];//求列x的总和
                sumRow_Y += dataRow_Y[i];//求行y的总和
                sumCol_X2 += dataCol_X[i] * dataCol_X[i];//求x^2的总和
                sum_XY += (dataCol_X[i] * dataRow_Y[i]);//求xy的总和
            }

            aveCol_X = 1.0 * sumCol_X / count;//求平均值
            aveRow_Y = 1.0 * sumRow_Y / count;

            k = (sum_XY - aveCol_X * aveRow_Y * count) / //根据公式求斜率
                (sumCol_X2 - aveCol_X * aveCol_X * count);

            //return k;
            return double.Parse(k.ToString("0.00"));
        }
        /// <summary>
        /// 计算价格变化百分比的斜率
        /// </summary>
        /// <param name="prices"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double CalculatePriceSlope(List<double> prices)
        {
            if (prices == null || prices.Count < 2)
                throw new ArgumentException("Price list must contain at least two elements.");

            // 计算价格变动的百分比
            List<double> percentChanges = new List<double>();
            for (int i = 1; i < prices.Count; i++)
            {
                percentChanges.Add((prices[i] - prices[i - 1]) / prices[i - 1] * 100);
            }

            int N = percentChanges.Count;
            if (N < 2)
                return 0; // 变动数据不足，斜率定义为0

            // x: 时间索引，y: 价格变动百分比
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            for (int i = 0; i < N; i++)
            {
                sumX += i;
                sumY += percentChanges[i];
                sumXY += i * percentChanges[i];
                sumX2 += i * i;
            }

            // 计算斜率
            double slope = (N * sumXY - sumX * sumY) / (N * sumX2 - sumX * sumX);
            return slope;
        }

        public static double CalculateSlope(List<double> prices)
        {
            int n = prices.Count;
            double[] x = new double[n]; // x 轴时间索引
            double[] y = prices.ToArray(); // y 轴价格数据

            for (int i = 0; i < n; i++)
            {
                x[i] = i;
            }
            (double intercept, double slope) = SimpleRegression.Fit(x, y);
            return slope; // 斜率
        }

        /// <summary>
        /// 计算斜率
        /// 一阶差分法：对于每个时间点，计算曲线的斜率或变化率，即 Δy/Δx。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<double> CalculateRateOfChange(List<double> data)
        {
            List<double> rateOfChange = new List<double>();
            for (int i = 1; i < data.Count; i++)
            {
                rateOfChange.Add((data[i] - data[i - 1]) / data[i - 1]);
            }
            return rateOfChange;
        }
    }
}
