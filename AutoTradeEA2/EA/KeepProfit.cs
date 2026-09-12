using System;
using System.Collections.Generic;
using System.Text;

using uClient.Broker;

namespace uClient.Broker
{
    /// <summary>
    /// KEEP_PROFIT
    /// 动态止盈,根据设置的止盈点和止盈偏差,盈利越过止盈点,即设置动态止盈值dynamicProfit=盈利-止盈偏差,在盈利回调小于动态止盈点后发出平仓指令,配置参数:keepProfit,keepProfitDiff
    /// </summary>
    public class KeepProfit : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();
            string currentOrderType = context["CurrentOrderType"].ToString();
            if (string.IsNullOrEmpty(currentOrderType))
            {
                desc.AppendFormat("策略名称[{0}]无开单,不执行该策略", this.name);
                return false;
            }
            if (!string.Equals(currentOrderType, "BUY") && !string.Equals(currentOrderType, "IN_BUY") && !string.Equals(currentOrderType, "SELL") && !string.Equals(currentOrderType, "IN_SELL"))
            {
                desc.AppendFormat("策略名称[{0}]无开单,不执行该策略", this.name);
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
            //动态止盈值
            double dynamicProfit = Convert.ToDouble(context["dynamicProfit"]);
            //止盈值
            double keepProfit = Convert.ToDouble(this.value) ;
            double keepProfitDiff = Convert.ToDouble(this.configParam["keepProfitDiff"]);
            //当前价格是否越过止盈值
            string overKeepProfit = context["overKeepProfit"].ToString();

            //当前价格是否越过止盈值
            string overKeepProfit2 = context["overKeepProfit2"].ToString();

            //最新的价格差
            double keepProfitDiffPoint = 0;
            if (string.Equals(currentOrderType,"BUY"))
            {
                keepProfitDiffPoint = Math.Round(quotePrice - currentPrice, 2);
            }
            if (string.Equals(currentOrderType, "SELL"))
            {
                keepProfitDiffPoint = Math.Round(currentPrice - quotePrice, 2);
            }

            desc.AppendFormat("动态参数[price{0}][currentOpenPrice{1}][currentOrderType{2}][keepProfit{3}][overKeepProfit{4}][keepProfitDiffPoint{5}][dynamicProfit{6}][gsMaxProfitValue{7}][gsMaxLossValue{8}][overKeepProfit2{9}];", quotePrice, currentPrice, currentOrderType, keepProfit, overKeepProfit, keepProfitDiffPoint, dynamicProfit, gsMaxProfitValue, gsMaxLossValue, overKeepProfit2, keepProfitDiff);
           
            if (string.Equals(overKeepProfit, "T") && keepProfitDiffPoint < dynamicProfit)
            {
                desc.AppendFormat($"策略名称[{this.name}]开仓[{currentOrderType}]止盈标识[{overKeepProfit}]盈利点数[{keepProfitDiffPoint}]小于动态止盈点数[{dynamicProfit}],执行该策略;");
                return true;
            }
            if (string.Equals(overKeepProfit2, "T") && keepProfitDiffPoint < keepProfit - keepProfitDiff)
            {
                desc.AppendFormat($"策略名称[{this.name}]开仓[{currentOrderType}]第二止盈标识[{overKeepProfit2}]盈利点数[{keepProfitDiffPoint}]小于止盈点数[{keepProfit - keepProfitDiff}],执行该策略;");
                return true;
            }
            desc.AppendFormat($"策略名称[{this.name}]开仓[{currentOrderType}]止盈标识[{overKeepProfit}]第二止盈标识[{overKeepProfit2}]盈利点数[{keepProfitDiffPoint}]止盈点数[{keepProfit}]第二止盈点数[{keepProfit - keepProfitDiff}],价格未过止盈或者盈利未回落,不执行该策略;");
            return false;
        }

        public override string ToDesc()
        {
            return desc.ToString();
        }
    }
    
}
