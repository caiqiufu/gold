using System;
using System.Collections.Generic;
using System.Text;
using uClient.Broker;

namespace uClient.Broker
{
    /// <summary>
    /// CLOSE_SELL_TRENDORDER
    /// 买出趋势单平仓
    /// </summary>
    public class CloseSellTrendOrder : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();
            string currentOrderType = context["CurrentOrderType"].ToString();
            if (string.IsNullOrEmpty(currentOrderType) || string.Equals(currentOrderType, "CLOSE_BUY") || string.Equals(currentOrderType, "CLOSE_SELL"))
            {
                desc.AppendFormat("策略名称[{0}]无开单，不执行该策略", this.name);
                return false;
            }
            double sum = eaChart.latestGoldSumValue;
            double daily = eaChart.latestGoldDailyValue;
            double hourly = eaChart.latestGoldHourlyValue;


            double trendOrder = Convert.ToDouble(this.configParam["trendOrder"]);
            double trendOrder1 = Convert.ToDouble(this.configParam["trendOrder1"]);

            desc.AppendLine($"动态参数[currentOrderType{currentOrderType}]");

            if (context.ContainsKey("CurrentOrderTypeDetail"))
            {
                string CurrentOrderTypeDetail = context["CurrentOrderTypeDetail"].ToString();
                if (string.Equals(CurrentOrderTypeDetail, "TREND_SELL"))
                {
                    if (trendOrder1 > 0)
                    {
                        if (daily >= trendOrder1 && daily < 100)
                        {
                            desc.AppendFormat("策略名称[DAILY{0}]策略值[{1}]大于设置[{2}],执行该策略;", this.name, daily, trendOrder1);
                            return true;
                        }
                    }
                    desc.AppendFormat("策略名称[{0}]策略值[{1}]小于设置[{2}],不执行该策略;", this.name, sum + "," + daily +","+ hourly, trendOrder1);
                    return false;
                }
                else
                {
                    desc.AppendFormat("策略名称[{0}]不是趋势开仓,不执行趋势平仓策略;", this.name);
                    return false;
                }
            }
            desc.AppendFormat("策略名称[{0}]未开仓,不执行趋势平仓策略;", this.name);
            return false;
        }

        public override string ToDesc()
        {
            return desc.ToString();
        }
    }
}
