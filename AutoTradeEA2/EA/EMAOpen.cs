using System;
using System.Collections.Generic;
using System.Text;

namespace uClient.Broker
{
    /// <summary>
    /// OPEN_EMA
    /// EMA 判断趋势,大于EMA价格买入,小于EMA价格卖出
    /// 参数:BUY TREND:买入趋势,SELL TREND卖出趋势,NO TREND:无交易趋势
    /// 返回true,未超过开仓数量,可以开仓,否则不能开仓
    /// </summary>
    public class EMAOpen : StrategyEA
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
            int M1KCount = IndicatorHelper.CalculateRequiredCount("H4", "M1", 2, EMAperiod, ATRperiod);
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
            desc.AppendFormat("动态参数[EMAATRPeriod{0}][EMAperiod{1}][ATRperiod{2}][currentDateTimeStr{3}][strategyResult{4}][strategyBarPrice{5}][strategyCurrentPrice{6}][quotePrice{7}];", EMAATRPeriod, EMAperiod, ATRperiod, currentDateTimeStr, strategyResult, strategyBarPrice, strategyCurrentPrice, currentPrice);

            desc.AppendFormat($"当前趋势[{strategyResult}][currentOrderType{currentOrderType}]");

            context["StrategyResultO"] = strategyResult;

            //是否满足策略标识
            bool flag = false;
            if (string.IsNullOrEmpty(currentOrderType) || "CLOSE_BUY".Equals(currentOrderType) || "CLOSE_SELL".Equals(currentOrderType) || "CLOSE_ALL".Equals(currentOrderType))
            {
                if (string.IsNullOrEmpty(strategyResult) || string.Equals(strategyResult, "NO TREND"))
                {
                    desc.AppendFormat("策略名称[{0}]不是趋势行情[strategyResult{1}],不执行该策略", this.name, strategyResult);
                    flag = false;
                }
                if (string.Equals(strategyResult, "BUY TREND"))
                {
                    //quotePrice > strategyPrice 表示BUY趋势已经反转
                    //desc.AppendFormat("策略名称[{0}]趋势行情{1},[当前价格{2}]<[趋势价格{3}],行情反转,不执行该策略", this.name, strategyResult, quotePrice, strategyBarPrice);
                    desc.AppendFormat("策略名称[{0}]趋势行情{1},执行该策略", this.name, strategyResult);
                    flag = true;
                }

                if (string.Equals(strategyResult, "SELL TREND"))
                {
                    // quotePrice < strategyPrice 表示SELL趋势已经反转
                    //desc.AppendFormat("策略名称[{0}]趋势行情{1},[当前价格{2}]>[趋势价格{3}],行情反转,不执行该策略", this.name, strategyResult, quotePrice, strategyBarPrice);
                    desc.AppendFormat("策略名称[{0}]趋势行情{1},执行该策略", this.name, strategyResult);
                    flag = true;
                }
            }
            else
            {
                desc.AppendFormat("策略名称[{0}][currentOrderType:{1}]不满足开仓条件,不执行该策略;", this.name, currentOrderType);
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
