using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using uClient.Broker;

namespace uClient.Broker
{
    /// <summary>
    /// KEEP_LOSS
    /// 动态止损,采用动态止损方式,即动态提升止损点
    /// </summary>
    public class KeepLoss : StrategyEA
    {
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
            
            double quotePrice = Convert.ToDouble(context["price"]);
            double currentPrice = Convert.ToDouble(context["CurrentPrice"]);
            if (currentPrice == 0)
            {
                desc.AppendFormat("策略名称[{0}]开仓价格未获取，不执行该策略", this.name);
                return false;
            }

            if (quotePrice == 0 || string.Equals(context["price"], "0"))
            {
                desc.AppendFormat("策略名称[{0}]当前价格未获取，不执行该策略", this.name);
                return false;
            }

            double gsMaxProfitValue = Convert.ToDouble(context["gsMaxProfitValue"]);
            double gsMaxLossValue = Convert.ToDouble(context["gsMaxLossValue"]);
            //动态止损值,该值为正数
            double dynamicLost = Math.Round(Convert.ToDouble(context["dynamicLost"]), 2);
            //最新的价格差
            double keepProfitDiffPoint = 0;
            if (string.Equals(currentOrderType, "BUY"))
            {
                keepProfitDiffPoint = Math.Round(quotePrice - currentPrice, 2);
            }
            if (string.Equals(currentOrderType, "SELL"))
            {
                keepProfitDiffPoint = Math.Round(currentPrice - quotePrice, 2);
            }

            desc.AppendFormat("动态参数[quotePrice{0}][currentPrice{1}][dynamicLost{2}][currentOrderType{3}][gsMaxProfitValue{4}][gsMaxLossValue{5}];", quotePrice, currentPrice, dynamicLost, currentOrderType, gsMaxProfitValue, gsMaxLossValue);
            
            if (string.Equals(currentOrderType, "BUY") || string.Equals(currentOrderType, "SELL"))
            {            
                if (string.Equals(currentOrderType, "BUY"))
                {
                    if (-keepProfitDiffPoint > dynamicLost)
                    {
                        desc.AppendFormat("策略名称[BUY{0}]策略值[{1}-{2}={3}]大于动态止损值[{4}],执行该策略;", this.name, quotePrice, currentPrice, -keepProfitDiffPoint, dynamicLost);
                        return true;
                    }
                    else 
                    {
                        desc.AppendFormat("策略名称[BUY{0}]策略值[{1}-{2}={3}]小于动态止损值[{4}],不执行该策略;", this.name, quotePrice, currentPrice, -keepProfitDiffPoint, dynamicLost);
                        return false;
                    }                   
                }
                if (string.Equals(currentOrderType, "SELL"))
                {
                    if (-keepProfitDiffPoint > dynamicLost)
                    {
                        desc.AppendFormat("策略名称[SELL{0}]策略值[{1}-{2}={3}]大于等于动态止损值[{4}],执行该策略;", this.name, currentPrice, quotePrice, -keepProfitDiffPoint, dynamicLost);
                        return true;
                    }
                    else 
                    {
                        desc.AppendFormat("策略名称[SELL{0}]策略值[{1}-{2}={3}]小于等于动态止损值[{4}],不执行该策略;", this.name, currentPrice, quotePrice, -keepProfitDiffPoint, dynamicLost);
                        return false;
                    }                    
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
