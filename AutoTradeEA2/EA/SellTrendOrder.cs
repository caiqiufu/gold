using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uClient.Broker;

namespace uClient.Broker
{
    /// <summary>
    /// SELL_TRENDORDER
    /// 卖出趋势单,根据情绪指数值极端偏向时,即绝大多数人都认为是卖出,即执行卖出指令
    /// </summary>
    public class SellTrendOrder : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();
            double sum = eaChart.latestGoldSumValue;
            double daily = eaChart.latestGoldDailyValue;
            double hourly = eaChart.latestGoldHourlyValue;

            double buyBeforeSum = sum;
            if (eaChart.latestGoldSumValueList.Count >= 2)
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

            double trendOrder = Convert.ToDouble(this.configParam["trendOrder"]);
            double trendOrder1 = Convert.ToDouble(this.configParam["trendOrder1"]);

            double minChangeDiff = Convert.ToDouble(this.configParam["minChangeDiff"]);

            string continuousOrder = this.configParam["continuousOrder"];
            string analysisDataType = this.configParam["analysisDataType"];

            //double maxChangeDiff = Convert.ToDouble(this.configParam["maxChangeDiff"]);
            //double maxDiffChangeDuration = Convert.ToDouble(this.configParam["maxDiffChangeDuration"]);
            //double openDiff = Convert.ToDouble(this.configParam["openDiff"]);

            string currentOrderType = context["CurrentOrderType"].ToString();

            desc.AppendFormat("动态参数[sum{0}][daily{1}][hourly{2}][buyBeforeSum{3}][buyBeforeDaily{4}][buyBeforeHourly{5}][latestGoldHourlyValue{6}][latestGoldDailyValue{7}];", sum, daily, hourly, buyBeforeSum, buyBeforeDaily, buyBeforeHourly, latestGoldHourlyValue, latestGoldDailyValue);
            //是否满足策略标识
            bool flag = false;
            if (!string.IsNullOrEmpty(currentOrderType))
            {
                if (string.Equals(continuousOrder, "F"))
                {

                    if (trendOrder > 0 && daily > 0)
                    {
                        if (Math.Abs(daily - buyBeforeDaily) <= minChangeDiff)
                        {
                            desc.AppendFormat("策略名称[DAILY{0}]策略值[{1}]变化小于{2},不执行该策略;", this.name, daily + "," + buyBeforeDaily, minChangeDiff);
                            flag = false;
                        }
                        if (Math.Abs(daily - buyBeforeDaily) > minChangeDiff)
                        {
                            context["sellTrendBeforeDaily"] = daily.ToString();
                            desc.AppendFormat("策略名称[DAILY{0}]策略值[{1}]变化大于{2};", this.name, daily + "," + buyBeforeDaily, minChangeDiff);
                            desc.AppendFormat("策略名称[DAILY{0}]策略值[{1}]小于设置[{2}],执行该策略;", this.name, daily, trendOrder);
                            return true;
                        }
                    }
                }
                if (string.Equals(continuousOrder, "T"))
                {
                    desc.AppendFormat("策略名称[{0}]策略配置[{1}],开连续单;", this.name, "true");

                    if (string.Equals(currentOrderType, "CLOSE_SELL") && Math.Abs(daily - buyBeforeDaily) <= minChangeDiff)
                    {
                        desc.AppendFormat("策略名称[{0}],当前已是[{1}],不执行该策略;", this.name, currentOrderType);
                        flag = false;
                    }
                    if (trendOrder > 0 && daily > 0 && daily <= trendOrder)
                    {
                        desc.AppendFormat("策略名称[DAILY{0}]策略值[{1}]小于设置[{2}],执行该策略;", this.name, daily, trendOrder);
                        return true;
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
    }
}
