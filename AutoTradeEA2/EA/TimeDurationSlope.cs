
using System;
using System.Collections.Generic;
using System.Text;

namespace uClient.Broker
{
    /// <summary>
    /// TIME_DURATION_SLOPE
    /// 在设置的时间区间内,不同策略需要满足不同的的斜率采后才能执行策略
    /// 参数:timeDuration,slope
    /// </summary>
    public class TimeDurationSlope : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();
            double slopeConfigValue = Convert.ToDouble(this.configParam["slope"]);
            if (slopeConfigValue > 0)
            {
                double sumSlope = getSumSlope(eaChart,this.value);
                context.Remove("sumSlope");
                context.Add("sumSlope", sumSlope.ToString());
                _Log.LogInfo("总览斜率为：" + sumSlope);
                double dailySlope = getDailySlope(eaChart, this.value);
                context.Remove("dailySlope");
                context.Add("dailySlope", dailySlope.ToString());
                _Log.LogInfo("近1天斜率为：" + dailySlope);
                double hourlySlope = getHourlySlope(eaChart, this.value);
                context.Remove("hourlySlope");
                context.Add("hourlySlope", hourlySlope.ToString());
                _Log.LogInfo("分时斜率为：" + hourlySlope);


                if (sumSlope >= slopeConfigValue)
                {
                    desc.AppendFormat("策略名称[总览斜率]策略值[{0}]大于设置[{1}],执行该策略;", sumSlope, slopeConfigValue);
                }
                else
                {
                    desc.AppendFormat("策略名称[总览斜率]策略值[{0}]小于设置[{1}],不执行该策略;", sumSlope, slopeConfigValue);
                    return false;
                }
                if (dailySlope >= slopeConfigValue)
                {
                    desc.AppendFormat("策略名称[近1天斜率]策略值[{0}]大于设置[{1}],执行该策略;", dailySlope, slopeConfigValue);
                }
                else
                {
                    desc.AppendFormat("策略名称[近1天斜率]策略值[{0}]小于设置[{1}],不执行该策略;", dailySlope, slopeConfigValue);
                    return false;
                }
                if (hourlySlope >= slopeConfigValue)
                {
                    desc.AppendFormat("策略名称[分时斜率]策略值[{0}]大于设置[{1}],执行该策略;", hourlySlope, slopeConfigValue);
                }
                else
                {
                    desc.AppendFormat("策略名称[分时斜率]策略值[{0}]小于设置[{1}],不执行该策略;", hourlySlope, slopeConfigValue);
                    return false;
                }
            }
            else
            {
                desc.AppendFormat("策略名称[{0}]区间策略值[{1}]不大于0,不执行该策略;", this.name, slopeConfigValue);
            }           
            return true;
        }

        public override string ToDesc()
        {
            return desc.ToString();
        }
        
        public double getSumSlope(EAChart eaChart, string timeDuration)
        {
            int count = Convert.ToInt32(timeDuration);
            List<IDictionary<string, string>> goldSumChartData = eaChart.goldSumChartData;
            if (goldSumChartData != null && goldSumChartData.Count >= count)
            {
                int chartCount = goldSumChartData.Count;
                double[] axisX = new double[count];
                double[] axisY = new double[count];
                for (int i = 0; i < count; i++)
                {
                    IDictionary<string, string> data = goldSumChartData[chartCount - 1 - i];
                    double buyValume = Convert.ToDouble(data["buy_valume"]);
                    double sellValume = Convert.ToDouble(data["sell_valume"]);
                    double p;
                    if (buyValume == 0 && sellValume == 0)
                    {
                        p = 0;
                    }
                    else
                    {
                        p = 100 * buyValume / (buyValume + sellValume);
                        p = double.Parse(Math.Round(p, 2).ToString());
                    }
                    axisY[i] = p;
                    axisX[i] = i + 1;//x坐标按照步长1模拟计算
                }
                //return Math.Abs(Slope.get_K(axisX, axisY));
                return SlopeUtils.get_K(axisX, axisY);
            }
            return 90;
        }
        public double getDailySlope(EAChart eaChart, string timeDuration)
        {
            int count = Convert.ToInt32(timeDuration);
            List<IDictionary<string, string>> goldDailyChartData = eaChart.goldDailyChartData;
            if (goldDailyChartData != null && goldDailyChartData.Count >= count)
            {
                int chartCount = goldDailyChartData.Count;
                double[] axisX = new double[count];
                double[] axisY = new double[count];
                for (int i = 0; i < count; i++)
                {
                    IDictionary<string, string> data = goldDailyChartData[chartCount - 1 - i];
                    double buyValume = Convert.ToDouble(data["buy_valume"]);
                    double sellValume = Convert.ToDouble(data["sell_valume"]);
                    double p;
                    if (buyValume == 0 && sellValume == 0)
                    {
                        p = 0;
                    }
                    else
                    {
                        p = 100 * buyValume / (buyValume + sellValume);
                        p = double.Parse(Math.Round(p, 2).ToString());
                    }
                    axisY[i] = p;
                    axisX[i] = i + 1;//x坐标按照步长1模拟计算
                }
                return SlopeUtils.get_K(axisX, axisY);
            }
            return 90;
        }
        public double getHourlySlope(EAChart eaChart, string timeDuration)
        {
            int count = Convert.ToInt32(timeDuration);
            List<IDictionary<string, string>> goldHourlyChartData = eaChart.goldHourlyChartData;
            if (goldHourlyChartData != null && goldHourlyChartData.Count >= count)
            {
                int chartCount = goldHourlyChartData.Count;
                double[] axisX = new double[count];
                double[] axisY = new double[count];
                for (int i = 0; i < count; i++)
                {
                    IDictionary<string, string> data = goldHourlyChartData[chartCount - 1 - i];
                    double buyValume = Convert.ToDouble(data["buy_valume"]);
                    double sellValume = Convert.ToDouble(data["sell_valume"]);
                    double p;
                    if (buyValume == 0 && sellValume == 0)
                    {
                        p = 0;
                    }
                    else
                    {
                        p = 100 * buyValume / (buyValume + sellValume);
                        p = double.Parse(Math.Round(p, 2).ToString());
                    }
                    axisY[i] = p;
                    axisX[i] = i + 1;//x坐标按照步长1模拟计算
                }
                return SlopeUtils.get_K(axisX, axisY);
            }
            return 90;
        }
    }
}
