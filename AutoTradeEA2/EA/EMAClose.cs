
using System;
using System.Collections.Generic;
using System.Text;

namespace uClient.Broker
{
    /// <summary>
    /// CLOSE_EMA
    /// EMA 趋势已反转,执行平仓
    /// 返回true,趋势已反转,执行平仓
    /// </summary>
    public class EMAClose : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();

            string currentOrderType = context["CurrentOrderType"].ToString();
            string _IsSimulateTest = context["IsSimulateTest"].ToString();
            string EMAATRPeriod = this.configParam["EMAATRPeriod"];
            int EMAperiod = Convert.ToInt32(this.configParam["EMAPeriod"]);
            int ATRperiod = Convert.ToInt32(this.configParam["ATRPeriod"]);
            string analysisDataType = context["analysisDataType"].ToString();

            decimal currentPrice = decimal.Parse(context["price"].ToString());
            int trendCount = Convert.ToInt32(this.configParam["TrendCount"]);

            string currentDateTimeStr;
            if (context.ContainsKey("CurrentDateTime"))
            {
                currentDateTimeStr = context["CurrentDateTime"].ToString();
            }
            else
            {
                currentDateTimeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            string strategyResult = "NO TREND";
            double strategyBarPrice = 0;
            double strategyCurrentPrice = 0;

            IDictionary<string, string> datas = null;
            if (string.Equals(_IsSimulateTest, "T"))
            {
                datas = DSHelper.getStrategyResultTest("TREND_EMA_ATR", "XAUUSD", EMAATRPeriod, currentDateTimeStr, trendCount);
                if (datas != null)
                {
                    strategyResult = datas["result_status"];
                    strategyBarPrice = Convert.ToDouble(datas["bar_price"]);
                    strategyCurrentPrice = Convert.ToDouble(datas["current_price"]);
                }
            }
            else
            {
                datas = DSHelper.getStrategyResult("TREND_EMA_ATR", "XAUUSD", EMAATRPeriod, currentDateTimeStr, trendCount);
                if (datas != null)
                {
                    strategyResult = datas["result_status"];
                    strategyBarPrice = Convert.ToDouble(datas["bar_price"]);
                    strategyCurrentPrice = Convert.ToDouble(datas["current_price"]);
                }
            }

            desc.AppendFormat($"当前趋势[{strategyResult}][currentOrderType{currentOrderType}]");
            desc.AppendFormat("动态参数[EMAATRPeriod{0}][EMAperiod{1}][ATRperiod{2}][currentDateTimeStr{3}][strategyResult{4}][strategyBarPrice{5}][strategyCurrentPrice{6}][quotePrice{7}];", EMAATRPeriod, EMAperiod, ATRperiod, currentDateTimeStr, strategyResult, strategyBarPrice, strategyCurrentPrice, currentPrice);

            context["StrategyResultC"] = strategyResult;

            //是否满足策略标识
            bool flag = false;
            if (string.Equals(currentOrderType, "BUY") || string.Equals(currentOrderType, "SELL"))
            {
                if (string.Equals(currentOrderType, "BUY") && (string.Equals(strategyResult, "SELL TREND") || string.Equals(strategyResult, "NO TREND")))
                {
                    desc.AppendFormat("策略名称[{0}]趋势行情已变化[currentOrderType{1}][strategyResult{2}],执行该策略", this.name, currentOrderType, strategyResult);
                    flag = true;
                    context["StrategyResultC"] = "SELL TREND";
                }
                if (string.Equals(currentOrderType, "SELL") && (string.Equals(strategyResult, "BUY TREND") || string.Equals(strategyResult, "NO TREND")))
                {
                    desc.AppendFormat("策略名称[{0}]趋势行情已变化[currentOrderType{1}][strategyResult{2}],执行该策略", this.name, currentOrderType, strategyResult);
                    flag = true;
                    context["StrategyResultC"] = "BUY TREND";
                }
            }
            else
            {
                desc.AppendFormat("策略名称[{0}]无当前订单,不执行该策略;", this.name);
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
