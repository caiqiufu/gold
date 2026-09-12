
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uClient.Broker;

namespace uClient.Broker
{
    /// <summary>
    /// BUY
    /// 根据总览，近1天，分时，分价数据判断是否买入
    /// </summary>
    public class BuyTradeProportion : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();
            double sum = Math.Round(eaChart.latestGoldSumValue, 2);
            double daily = Math.Round(eaChart.latestGoldDailyValue, 2);
            double hourly = Math.Round(eaChart.latestGoldHourlyValue, 2);

            double buyBeforeSum = sum;
            if (eaChart.latestGoldSumValueList.Count>=2)
            {
                buyBeforeSum = eaChart.latestGoldSumValueList[eaChart.latestGoldSumValueList.Count - 2];
            }
            double buyBeforeDaily = daily;
            if (eaChart.latestGoldDailyValueList.Count >= 2)
            {
                buyBeforeDaily = eaChart.latestGoldDailyValueList[eaChart.latestGoldDailyValueList.Count - 2];
            }
            double buyBeforeHourly = hourly;
            if (eaChart.latestGoldHourlyValueList.Count >= 2)
            {
                buyBeforeHourly = eaChart.latestGoldHourlyValueList[eaChart.latestGoldHourlyValueList.Count - 2];
            }

            double minChangeDiff = Convert.ToDouble(this.configParam["minChangeDiff"]);
            double maxDiffChangeDuration = Convert.ToDouble(context["maxDiffChangeDuration"]);


            List<double> GoldDailyValueList = eaChart.latestGoldDailyValueList.ToList();

            double openDiff = Convert.ToDouble(this.configParam["openDiff"]);
            string currentOrderType = context["CurrentOrderType"].ToString();
            if (string.IsNullOrEmpty(currentOrderType))
            {
                openDiff = 0;
            }
            else
            {
                if (string.Equals(currentOrderType, "BUY"))
                {
                    desc.AppendLine($"策略名称[{this.name}]当前订单为[currentOrderType{currentOrderType}],不执行该策略");
                    return false;
                }
            }

            double csumConfigValue = this.sumConfigValue;
            double csumConfigValue1 = this.sumConfigValue1;
            double cdailyConfigValue = this.dailyConfigValue;
            double cdailyConfigValue1 = this.dailyConfigValue1;
            double chourlyConfigValue = this.hourlyConfigValue;
            double chourlyConfigValue1 = this.hourlyConfigValue1;


            double latestGoldSumValue = 0;
            if (context.ContainsKey("latestGoldSumValue")) 
            {
                latestGoldSumValue = Convert.ToDouble(context["latestGoldSumValue"]);
            }
            double latestGoldHourlyValue = 0;
            if (context.ContainsKey("latestGoldHourlyValue"))
            {
                latestGoldHourlyValue = Convert.ToDouble(context["latestGoldHourlyValue"]);
            }
            double latestGoldDailyValue = 0;
            if (context.ContainsKey("latestGoldDailyValue"))
            {
                latestGoldDailyValue = Convert.ToDouble(context["latestGoldDailyValue"]);
            }
            context.Remove("buySumConfigValue");
            context.Add("buySumConfigValue", csumConfigValue.ToString());
            context.Remove("buyDailyConfigValue");
            context.Add("buyDailyConfigValue", cdailyConfigValue.ToString());
            context.Remove("buyHourlyConfigValue");
            context.Add("buyHourlyConfigValue", chourlyConfigValue.ToString());

            desc.AppendLine($"动态参数[sum{sum}][daily{daily}][hourly{hourly}][buyBeforeSum{buyBeforeSum}][buyBeforeDaily{buyBeforeDaily}][buyBeforeHourly{buyBeforeHourly}][latestGoldHourlyValue{latestGoldHourlyValue}][latestGoldDailyValue{latestGoldDailyValue}][currentOrderType{currentOrderType}][GoldDailyValueList{JsonConvert.SerializeObject(GoldDailyValueList)}];");
            //是否满足策略标识,满足条件return true，否则继续执行后续条件
            bool flag = false;
            if (string.IsNullOrEmpty(currentOrderType) || "CLOSE_BUY".Equals(currentOrderType) || "CLOSE_SELL".Equals(currentOrderType) || "CLOSE_ALL".Equals(currentOrderType))
            {
                if (this.dailyConfigValue > 0 && daily > 0)
                {
                    if (Math.Abs(daily - buyBeforeDaily) > minChangeDiff && eaChart.latestGoldDailyValueList.Count > maxDiffChangeDuration)
                    {
                        List<string> goldDailyValues = new List<string>();
                        for (int i = 0; i < maxDiffChangeDuration; i++)
                        {
                            double currentGoldDailyValue = eaChart.latestGoldDailyValueList[eaChart.latestGoldDailyValueList.Count - 1 - i];
                            goldDailyValues.Add(currentGoldDailyValue.ToString());
                            if (currentGoldDailyValue > cdailyConfigValue || currentGoldDailyValue < cdailyConfigValue1)
                            {
                                desc.AppendLine($"策略名称[{this.name}]策略值[currentGoldDailyValue:{currentGoldDailyValue}]不再配置区间[{cdailyConfigValue1}-{cdailyConfigValue}][goldDailyValues:{string.Join(", ", goldDailyValues)}],不执行该策略;");
                                return false;
                            }
                        }
                        context["buyBeforeDaily"] = daily.ToString();
                        desc.AppendLine($"策略名称[DAILY{this.name}]策略值[daily{daily},buyBeforeDaily{buyBeforeDaily}]变化大于[{minChangeDiff}],并且在区间{cdailyConfigValue1}-{cdailyConfigValue}之间[goldDailyValues:{string.Join(", ", goldDailyValues)}],执行该策略;");
                        return true;
                    }
                    if (Math.Abs(daily - buyBeforeDaily) <= minChangeDiff)
                    {
                        desc.AppendLine($"策略名称[DAILY{this.name}]策略值[daily{daily},buyBeforeDaily{buyBeforeDaily}]变化小于[{minChangeDiff}],不执行该策略;");
                        flag = false;
                    }
                    if (daily > cdailyConfigValue || daily < cdailyConfigValue1)
                    {
                        desc.AppendLine($"策略名称[DAILY{this.name}]策略值[daily{daily}不在配置区间内{cdailyConfigValue1}-{cdailyConfigValue}内],不执行该策略;");
                        flag = false;
                    }
                }
                else {
                    desc.AppendLine($"策略名称[DAILY{this.name}]策略值[daily{daily}不在配置区间内{cdailyConfigValue1}-{cdailyConfigValue}内],不执行该策略;");
                    flag = false;
                }
            }
            else
            {
                desc.AppendLine($"策略名称[{this.name}][dailyConfigValue{daily}][daily{dailyConfigValue}]参数不满足,不执行该策略;");
                flag = false;
            }
            return flag;
        }
        public override string ToDesc() {
            return desc.ToString();
        }
    }
}
