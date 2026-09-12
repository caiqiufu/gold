using System;
using System.Collections.Generic;
using System.Text;
using uClient.Broker;

namespace uClient.Broker
{
    /// <summary>
    /// CLOSE_BUY
    /// 根据总览，近1天，分时，分价数据判断是否买入单平仓
    /// </summary>
    public class CloseBuyTradeProportion : StrategyEA
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
            double daily = eaChart.latestGoldDailyValue;

            context["closeBuyDailyConfigValue"] = this.dailyConfigValue.ToString();
            context["closeBuyDailyConfigValue1"] = this.dailyConfigValue1.ToString();

            string values = string.Join(",", eaChart.latestGoldDailyValueList);


            //发送指令时的值
            string latestGoldDailyValue = context["latestGoldDailyValue"].ToString();

            double buyBeforeDaily = Convert.ToDouble(latestGoldDailyValue);

            double minChangeDiff = Convert.ToDouble(this.configParam["minChangeDiff"]);
            double maxDiffChangeDuration = Convert.ToDouble(context["maxDiffChangeDuration"]);
            desc.AppendFormat("动态参数[daily{0}][buyBeforeDaily{1}][values{2}][currentOrderType{3}];", daily, buyBeforeDaily,values, currentOrderType);

            if (this.dailyConfigValue > 0 && this.dailyConfigValue1 > 0 && eaChart.latestGoldDailyValueList.Count > maxDiffChangeDuration)
            {
                List<string> goldDailyValues = new List<string>();
                for (int i = 0; i < maxDiffChangeDuration; i++)
                {
                    double currentGoldDailyValue = eaChart.latestGoldDailyValueList[eaChart.latestGoldDailyValueList.Count - 1 - i];
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
        public override string ToDesc() {
            return desc.ToString();
        }
    }
}
