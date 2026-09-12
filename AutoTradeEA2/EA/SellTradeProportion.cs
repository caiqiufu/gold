
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using uClient.Broker;

namespace uClient.Broker
{
    /// <summary>
    /// SELL
    /// 根据总览，近1天，分时，分价数据判断是否卖出
    /// </summary>
    public class SellTradeProportion : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();
            double bsum = eaChart.latestGoldSumValue;
            double bdaily =  eaChart.latestGoldDailyValue;
            double bhourly =  eaChart.latestGoldHourlyValue;

            double sum = Math.Round(100 - bsum,2);
            double daily = Math.Round(100 - bdaily, 2);
            double hourly = Math.Round(100 - bhourly, 2);

            double buyBeforeSum = sum;
            if (eaChart.latestGoldSumValueList.Count >= 2)
            {
                buyBeforeSum = double.Parse(Math.Round(100 - eaChart.latestGoldSumValueList[eaChart.latestGoldSumValueList.Count - 2], 2).ToString());
            }
            double buyBeforeDaily = daily;
            if (eaChart.latestGoldDailyValueList.Count >= 2)
            {
                buyBeforeDaily = double.Parse(Math.Round(100 - eaChart.latestGoldDailyValueList[eaChart.latestGoldDailyValueList.Count - 2], 2).ToString());
            }
            double buyBeforeHourly = hourly;
            if (eaChart.latestGoldHourlyValueList.Count >= 2)
            {
                buyBeforeHourly = double.Parse(Math.Round(100 - eaChart.latestGoldHourlyValueList[eaChart.latestGoldHourlyValueList.Count - 2], 2).ToString());
            }

            double latestGoldSumValue = 0;
            if (context.ContainsKey("latestGoldSumValue"))
            {
                latestGoldSumValue = double.Parse(Math.Round(100 - Convert.ToDouble(context["latestGoldSumValue"]), 2).ToString());
            }
            double latestGoldHourlyValue = 0;
            if (context.ContainsKey("latestGoldHourlyValue"))
            {
                latestGoldHourlyValue = double.Parse(Math.Round(100 - Convert.ToDouble(context["latestGoldHourlyValue"]), 2).ToString());
            }
            double latestGoldDailyValue = 0;
            if (context.ContainsKey("latestGoldDailyValue"))
            {
                latestGoldDailyValue = double.Parse(Math.Round(100 - Convert.ToDouble(context["latestGoldDailyValue"]), 2).ToString());
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
            if (string.Equals(currentOrderType, "SELL"))
            {
                desc.AppendFormat("策略名称[{0}]当前订单为[currentOrderType{1}]，不执行该策略", this.name, currentOrderType);
                return false;
            }
            double csumConfigValue = this.sumConfigValue;
            double csumConfigValue1 = this.sumConfigValue1;
            double cdailyConfigValue = this.dailyConfigValue;
            double cdailyConfigValue1 = this.dailyConfigValue1;
            double chourlyConfigValue = this.hourlyConfigValue;
            double chourlyConfigValue1 = this.hourlyConfigValue1;
            //double cnoTradeConfigValue = this.noTradeConfigValue;

            //在止盈止损后,调整开仓情绪指数值,当前该功能未使用openDiff值默认为0
            if (context.ContainsKey("CurrentOrderTypeDetail"))
            {
                if (string.Equals(context.ContainsKey("CurrentOrderTypeDetail"), "CLOSE_TAKE_PROFIT"))
                {
                    csumConfigValue1 = csumConfigValue1 + openDiff;
                    cdailyConfigValue1 = cdailyConfigValue1 + openDiff;
                    chourlyConfigValue1 = chourlyConfigValue1 + openDiff;
                }
                if (string.Equals(context.ContainsKey("CurrentOrderTypeDetail"), "CLOSE_STOP_LOSS"))
                {
                    csumConfigValue = csumConfigValue - openDiff;
                    cdailyConfigValue = cdailyConfigValue - openDiff;
                    chourlyConfigValue = chourlyConfigValue - openDiff;
                }
            }


            context.Remove("sellSumConfigValue");
            context.Add("sellSumConfigValue", csumConfigValue.ToString());
            context.Remove("sellDailyConfigValue");
            context.Add("sellDailyConfigValue", cdailyConfigValue.ToString());
            context.Remove("sellHourlyConfigValue");
            context.Add("sellHourlyConfigValue", chourlyConfigValue.ToString());

            desc.AppendFormat("动态参数[sum{0}][daily{1}][hourly{2}][buyBeforeSum{3}][buyBeforeDaily{4}][buyBeforeHourly{5}][latestGoldHourlyValue{6}][latestGoldDailyValue{7}][currentOrderType{8}][GoldDailyValueList{9}];", sum, daily, hourly, buyBeforeSum, buyBeforeDaily, buyBeforeHourly, latestGoldHourlyValue, latestGoldDailyValue, currentOrderType, JsonConvert.SerializeObject(GoldDailyValueList));
            if (daily==39.49)
            {
                Console.WriteLine(daily);
            }
            //是否满足策略标识
            bool flag = false;
            if (string.IsNullOrEmpty(currentOrderType) || "CLOSE_BUY".Equals(currentOrderType) || "CLOSE_SELL".Equals(currentOrderType) || "CLOSE_ALL".Equals(currentOrderType))
            {
                if (this.dailyConfigValue > 0 && daily > 0 )
                {
                    if (Math.Abs(daily - buyBeforeDaily) > minChangeDiff && eaChart.latestGoldDailyValueList.Count > maxDiffChangeDuration)
                    {

                        List<string> goldDailyValues = new List<string>();
                        for (int i = 0; i < maxDiffChangeDuration; i++)
                        {
                            double currentGoldDailyValue = Math.Round(100 - eaChart.latestGoldDailyValueList[eaChart.latestGoldDailyValueList.Count - 1 - i], 2);
                            goldDailyValues.Add(currentGoldDailyValue.ToString());
                            if (currentGoldDailyValue > cdailyConfigValue || currentGoldDailyValue < cdailyConfigValue1)
                            {
                                desc.AppendFormat("策略名称[{0}]策略值[currentGoldDailyValue:{1}]不在配置区间[{2}-{3}][goldDailyValues:{4}],不执行该策略;", this.name, currentGoldDailyValue, cdailyConfigValue1, cdailyConfigValue, string.Join(", ", goldDailyValues));
                                return false;
                            }
                        }
                        context["sellBeforeDaily"] = daily.ToString();
                        desc.AppendFormat("策略名称[DAILY{0}]策略值[daily{1},buyBeforeDaily{2}]变化大于[{3}],并且在区间{4}-{5}之间[goldDailyValues:{6}],执行该策略;", this.name, daily, buyBeforeDaily, minChangeDiff, cdailyConfigValue1, cdailyConfigValue, string.Join(", ", goldDailyValues));
                        return true;
                    }
                    if (Math.Abs(daily - buyBeforeDaily) <= minChangeDiff)
                    {
                        desc.AppendFormat("策略名称[[DAILY{0}]策略值[daily{1},buyBeforeDaily{2}]变化小于[{3}],不执行该策略;", this.name, daily, buyBeforeDaily, minChangeDiff);
                        flag = false;
                    }
                    if (daily > cdailyConfigValue || daily < cdailyConfigValue1)
                    {
                        desc.AppendFormat("策略名称[DAILY{0}]策略值[daily{1}不在配置区间内{2}-{3}内],不执行该策略;", this.name, daily, cdailyConfigValue1, cdailyConfigValue);
                        flag = false;
                    }
                }
                else
                {
                    desc.AppendFormat("策略名称[{0}][dailyConfigValue{1}][daily{2}]参数不满足,不执行该策略;", this.name, dailyConfigValue, daily);
                    flag = false;
                }
            }
            else 
            {
                desc.AppendFormat("策略名称[{0}][currentOrderType{1}]已开仓,不执行该策略;", this.name, currentOrderType);
                flag = false;
            }
            return flag;
        }

        public override string ToDesc()
        {
            return desc.ToString();
        }
    }
}
