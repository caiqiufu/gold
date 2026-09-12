using System;
using System.Collections.Generic;
using System.Text;
using uClient.Broker;

namespace uClient.Broker
{
    /// <summary>
    /// BUY_TRENDORDER
    /// 买入趋势单,根据情绪指数值极端偏向时,即绝大多数人都认为是买入,即执行买入指令
    /// </summary>
    public class BuyTrendOrder : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();
            double sum = double.Parse(Math.Round(100 - eaChart.latestGoldSumValue, 2).ToString());
            double daily =  double.Parse(Math.Round(100 - eaChart.latestGoldDailyValue, 2).ToString());
            double hourly =  double.Parse(Math.Round(100 - eaChart.latestGoldHourlyValue, 2).ToString());


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

            double trendOrder = Convert.ToDouble(this.configParam["trendOrder"]);
            double trendOrder1 = Convert.ToDouble(this.configParam["trendOrder1"]);

            double minChangeDiff = Convert.ToDouble(this.configParam["minChangeDiff"]);

            string continuousOrder = this.configParam["continuousOrder"];
            string analysisDataType = this.configParam["analysisDataType"];

            string currentOrderType = context["CurrentOrderType"].ToString();

            desc.AppendFormat("动态参数[sum{0}][daily{1}][hourly{2}][buyBeforeSum{3}][buyBeforeDaily{4}][buyBeforeHourly{5}][latestGoldHourlyValue{6}][latestGoldDailyValue{7}][currentOrderType{8}];", sum, daily, hourly, buyBeforeSum, buyBeforeDaily, buyBeforeHourly, latestGoldHourlyValue, latestGoldDailyValue, currentOrderType);
            //是否满足策略标识
            bool flag = false;
            if (!string.IsNullOrEmpty(currentOrderType))
            {
                if (string.Equals(continuousOrder, "F"))
                {
                    desc.AppendFormat("策略名称[{0}]策略配置[{1}],开连续单;", this.name, "false");
                    if (trendOrder > 0 && daily > 0)
                    {
                        if (Math.Abs(daily - buyBeforeDaily) <= minChangeDiff)
                        {
                            desc.AppendFormat("策略名称[DAILY{0}]策略值[{1}]变化小于{2},不执行该策略;", this.name, daily + "," + buyBeforeDaily, minChangeDiff);
                            flag = false;
                        }
                        if (Math.Abs(daily - buyBeforeDaily) > minChangeDiff && daily <= trendOrder)
                        {
                            context["buyTrendBeforeDaily"] = daily.ToString();
                            desc.AppendFormat("策略名称[DAILY{0}]策略值[{1}]变化大于{2};", this.name, daily + "," + buyBeforeDaily, minChangeDiff);
                            desc.AppendFormat("策略名称[DAILY{0}]策略值[{1}]小于设置[{2}],执行该策略;", this.name, daily, trendOrder);
                            return true;
                        }
                    }
                }
                       
                if (string.Equals(continuousOrder, "T"))
                {
                    desc.AppendFormat("策略名称[{0}]策略配置[{1}],开连续单;", this.name, "true");
                    if (string.Equals(currentOrderType, "CLOSE_BUY") && Math.Abs(daily - buyBeforeDaily) <= minChangeDiff)
                    {
                        desc.AppendFormat("策略名称[{0}],当前已是[{1}],不执行该策略;", this.name, currentOrderType);
                        flag = false;
                    }
                    if (trendOrder > 0 && daily > 0 && daily <= trendOrder)
                    {
                        desc.AppendFormat("策略名称[DAILY{0}]策略值[{1}]小于设置[{2}],执行该策略;", this.name, daily, trendOrder);
                        return true;
                    }
                    else
                    {
                        desc.AppendFormat("策略名称[DAILY{0}]策略值[{1}]大于设置[{2}],不执行该策略;", this.name, daily, trendOrder);
                    }
                }
            }
            else
            {
                desc.AppendFormat("策略名称[{0}]无当前订单,不执行该策略;", this.name);
                flag = false;
            }
            desc.AppendFormat("策略名称[{0}]不满足条件,不执行该策略;", this.name);
            return flag;
        }

        public override string ToDesc()
        {
            return desc.ToString();
        }
        public double getSlope(EAChart eaChart, IDictionary<string, string> context,string timeDuration) {
            return 30;
        }
    }
}
