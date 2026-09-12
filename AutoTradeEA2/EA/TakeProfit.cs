using System;
using System.Collections.Generic;
using System.Text;
using uClient.Broker;

namespace uClient.Broker
{
    /// <summary>
    /// TAKE_PROFIT
    /// 止盈,参数:takeProfit
    /// </summary>
    public class TakeProfit : StrategyEA
    {
        //生成操作指令：{symbol}:{price}:{optType}:{lotProportion}
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();
            string currentOrderType = context["CurrentOrderType"].ToString();
            if (string.IsNullOrEmpty(currentOrderType))
            {
                desc.AppendFormat("策略名称[{0}]无开单，不执行该策略", this.name);
                return false;
            }
            if (!string.Equals(currentOrderType, "BUY") && !string.Equals(currentOrderType, "IN_BUY") && !string.Equals(currentOrderType, "SELL") && !string.Equals(currentOrderType, "IN_SELL"))
            {
                desc.AppendFormat("策略名称[{0}]不是开仓单，不执行该策略", this.name);
                return false;
            }

            double currentPrice = Convert.ToDouble(context["CurrentPrice"]);
            //止盈值
            double keepProfit = Convert.ToDouble(this.value);

            string CurrentOrderDetail = context["CurrentOrderDetail"].ToString();
            double quotePrice = Convert.ToDouble(context["price"]);
            if (quotePrice == 0 || string.Equals(context["price"], "0"))
            {
                desc.AppendFormat("策略名称[{0}]当前价格未获取，不执行该策略", this.name);
                return false;
            }

            desc.AppendFormat("动态参数[quotePrice{0}][currentPrice{1}][currentOrderType{2}][keepProfit{3}];", quotePrice, currentPrice, currentOrderType, keepProfit);

            if (string.Equals(currentOrderType, "BUY") || string.Equals(currentOrderType, "SELL")) {
                if (currentPrice == 0)
                {
                    desc.AppendFormat("策略名称[{0}]开仓价格未获取，不执行该策略", this.name);
                    return false;
                }
                double diff = Math.Round(quotePrice - currentPrice,2);

                if (string.Equals(currentOrderType, "BUY") && diff >= keepProfit)
                {
                    desc.AppendFormat("策略名称[BUY{0}]策略值[{1}-{2}={3}]大于等于设置[{4}],执行该策略;", this.name, quotePrice, currentPrice, diff, keepProfit);
                    return true;
                }
                if (string.Equals(currentOrderType, "SELL") && -diff >= keepProfit)
                {
                    desc.AppendFormat("策略名称[SELL{0}]策略值[{1}-{2}={3}]大于等于设置[{4}],执行该策略;", this.name, currentPrice, quotePrice, -diff, keepProfit);
                    return true;
                }

                if (string.Equals(currentOrderType, "BUY"))
                {
                    desc.AppendFormat("策略名称[BUY{0}]策略值[{1}-{2}={3}]小于等于设置[{4}],不执行该策略;", this.name, quotePrice, currentPrice, diff, keepProfit);
                }
                if (string.Equals(currentOrderType, "SELL"))
                {
                    desc.AppendFormat("策略名称[SELL{0}]策略值[{1}-{2}={3}]小于等于设置[{4}],不执行该策略;", this.name, currentPrice, quotePrice, -diff, keepProfit);
                }
            }
            return false;
        }

        public override string ToDesc()
        {
            return desc.ToString();
        }
    }
}
