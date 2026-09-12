using System;
using System.Collections.Generic;
using System.Text;

namespace uClient.Broker
{
    /// <summary>
    /// OPEN_AI_KLINE
    /// AI根据K线判断下一条k线涨跌反向
    /// </summary>
    public class AIKLineDetector : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();

            string _IsSimulateTest = context["IsSimulateTest"].ToString();
            double AIProbability = Convert.ToDouble(context["slope"].ToString());
            string PredictedDirection = context["PredictedDirection"].ToString();
            double PredictedProbability = Convert.ToDouble(context["PredictedProbability"].ToString());
            string currentOrderType = context["CurrentOrderType"].ToString();
            double quotePrice = Convert.ToDouble(context["price"].ToString());
            string currentDateTimeStr;
            if (context.ContainsKey("CurrentDateTime"))
            {
                currentDateTimeStr = context["CurrentDateTime"].ToString();
            }
            else
            {
                currentDateTimeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            string strategyResult = "NO";


            if (string.IsNullOrEmpty(currentOrderType) || "CLOSE_BUY".Equals(currentOrderType) || "CLOSE_SELL".Equals(currentOrderType) || "CLOSE_ALL".Equals(currentOrderType))
            {
                if (PredictedProbability > AIProbability && string.Equals(PredictedDirection, "LONG"))
                {
                    strategyResult = "BUY";
                }
                if (1 - PredictedProbability > AIProbability && string.Equals(PredictedDirection, "SHORT"))
                {
                    strategyResult = "SELL";
                }
            }

            desc.AppendFormat("动态参数[currentOrderType{0}][currentDateTimeStr{1}][strategyResult{2}][direction{3}][probability{4}][AIProbability{5}];", currentOrderType, currentDateTimeStr, strategyResult, PredictedDirection, PredictedProbability, AIProbability);

            context["StrategyResultAIKLine"] = strategyResult;

            if (string.IsNullOrEmpty(strategyResult) || string.Equals(strategyResult, "NO"))
            {
                desc.AppendFormat("策略名称[{0}]AI推理结果概率[probability=" + PredictedProbability + "]小于等于[" + AIProbability + "],不执行该策略", this.name);
                return false;
            }

            if (string.Equals(strategyResult, "BUY") || string.Equals(strategyResult, "SELL"))
            {
                desc.AppendFormat("策略名称[{0}]AI推理结果[probability=" + PredictedProbability + "]大于[" + AIProbability + "]交易方向[" + PredictedDirection + "][" + strategyResult + "],执行该策略", this.name);
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
