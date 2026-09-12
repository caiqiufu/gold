using System;
using System.Collections.Generic;
using System.Text;
using uClient.Broker;

namespace uClient.Broker
{
    /// <summary>
    /// CLOSE_SELL
    /// 根据总览，近1天，分时，分价数据判断是否卖出单平仓
    /// </summary>
    public class CloseSellTradeProportion : StrategyEA
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
            double daily = 100 - eaChart.latestGoldDailyValue;
            context["closeSellDailyConfigValue"] = this.dailyConfigValue.ToString();
            context["closeSellDailyConfigValue1"] = this.dailyConfigValue1.ToString();

            string values = string.Join(",", eaChart.latestGoldDailyValueList);

            //发送指令时的值
            string latestGoldDailyValue = context["latestGoldDailyValue"].ToString();

            double buyBeforeDaily = 100 - Convert.ToDouble(latestGoldDailyValue);


            double minChangeDiff = Convert.ToDouble(this.configParam["minChangeDiff"]);
            double maxDiffChangeDuration = Convert.ToDouble(context["maxDiffChangeDuration"]);


            context["daily"] = daily.ToString();

            desc.AppendFormat("动态参数[daily{0}][buyBeforeDaily{1}][latestGoldDailyValue{2}][values{3}][currentDateTime{4}];",  daily, buyBeforeDaily, latestGoldDailyValue, values, currentOrderType);

            if (this.dailyConfigValue > 0 && this.dailyConfigValue1 > 0 && eaChart.latestGoldDailyValueList.Count > maxDiffChangeDuration)
            {
                List<string> goldDailyValues = new List<string>();
                for (int i = 0; i < maxDiffChangeDuration; i++)
                {
                    double currentGoldDailyValue = Math.Round(100 - eaChart.latestGoldDailyValueList[eaChart.latestGoldDailyValueList.Count - 1 - i], 2);
                    goldDailyValues.Add(currentGoldDailyValue.ToString());
                    if (currentGoldDailyValue < this.dailyConfigValue && currentGoldDailyValue > this.dailyConfigValue1)
                    {
                        desc.AppendFormat("策略名称[{0}]策略值[{1}]不在平仓区间[{2}-{3}]中,不执行该策略;", this.name, currentGoldDailyValue, this.dailyConfigValue, dailyConfigValue1);
                        return false;
                    }
                    else
                    {
                        desc.AppendFormat("策略名称[{0}]策略值[{1}]在平仓区间[{2}-{3}]中,继续检查下一个值;", this.name, currentGoldDailyValue, this.dailyConfigValue, dailyConfigValue1);
                    }
                }
                desc.AppendFormat("策略名称[{0}]策略值[goldDailyValues{1}]在平仓区间[{2}-{3}]中,执行该策略;", this.name, string.Join(", ", goldDailyValues), this.dailyConfigValue, dailyConfigValue1);
                return true;
            }
            return false;
        }

        public override string ToDesc()
        {
            return desc.ToString();
        }
    }
}
