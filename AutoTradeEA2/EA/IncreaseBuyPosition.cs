using System;
using System.Collections.Generic;
using System.Text;
using uClient.Broker;

namespace uClient.Broker
{
    /// <summary>
    /// INCREASE_BUY_TRENDORDER
    /// 盈利到达6个点以上时,表示方向正确,并且情绪指数减少(减少点数可以配置)发出加仓指令
    /// </summary>
    public class IncreaseBuyPosition : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();

            string currentOrderType = "";
            if (context.ContainsKey("CurrentOrderType"))
            {
                currentOrderType = context["CurrentOrderType"].ToString();
            }
            if (string.IsNullOrEmpty(currentOrderType))
            {
                desc.AppendFormat("策略名称[{0}]无开单，不执行该策略", this.name);
                return false;
            }
            string CurrentOrderTypeDetail = "";
            if (context.ContainsKey("CurrentOrderTypeDetail"))
            {
                CurrentOrderTypeDetail = context["CurrentOrderTypeDetail"].ToString();

            }
            if (string.IsNullOrEmpty(CurrentOrderTypeDetail))
            {
                desc.AppendFormat("策略名称[{0}]无开单，不执行该策略", this.name);
                return false;
            }
            if (!string.Equals(CurrentOrderTypeDetail, "BUY")
                        && !string.Equals(CurrentOrderTypeDetail, "TREND_BUY")
                        && !string.Equals(CurrentOrderTypeDetail, "EVENT_BUY"))
            {
                desc.AppendFormat("策略名称[{0}]是[{1}]单，不执行该策略", this.name, CurrentOrderTypeDetail);
                return false;
            }

            //反向单标识
            string reverseProportion = this.configParam["reverseProportion"];

            if (string.Equals(reverseProportion,"F") &&!"BUY".Equals(currentOrderType))
            {
                desc.AppendFormat("策略名称[{0}]反向策略未开启,当前是[{1}]单,不是BUY单,不执行该策略", this.name, currentOrderType);
                return false;
            }
            if (string.Equals(reverseProportion, "T") && !"SELL".Equals(currentOrderType))
            {
                desc.AppendFormat("策略名称[{0}]反向单策略开启,当前是[{1}]单,不是SELL单,不执行该策略", this.name, currentOrderType);
                return false;
            }

            //设置的盈利加仓点数
            double increasePositionPoint = Convert.ToDouble(this.configParam["increasePositionPoint"]);


            //当前价格
            double price = Convert.ToDouble(context["price"]);
            //开仓价格
            double currentPrice = Convert.ToDouble(context["CurrentPrice"]);

            //盈利点数=当前价-开仓价(小于0为亏损)
            double increasePositionDiffPoint = Math.Round(price - currentPrice,2);

            desc.AppendFormat("动态参数[price{0}][currentPrice{1}][increasePositionDiffPoint{2}]", price, currentPrice, increasePositionDiffPoint);

            //是否满足策略标识
            bool flag = false;
            //正向单,多单加仓
            if (!string.Equals(reverseProportion, "T"))
            {
                if (increasePositionDiffPoint < 0)
                {
                    desc.AppendFormat("策略名称[{0}]盈利点数[increasePositionDiffPoint {1}]小于0,亏损单，不执行该策略", this.name, increasePositionDiffPoint);
                    return false;
                }
                if (!string.IsNullOrEmpty(currentOrderType))
                {
                    if (price <= 0 || currentPrice <= 0 || increasePositionDiffPoint < 0)
                    {
                        desc.AppendFormat("策略名称[{0}]当前价格[{1}]开仓价格[{2}]价格差[{3}]不满足,不执行该策略;", this.name, price, currentPrice, increasePositionDiffPoint);
                        return false;
                    }
                    if (increasePositionDiffPoint < increasePositionPoint)
                    {
                        desc.AppendFormat("策略名称[{0}]策略值[{1}]大于{2},不执行该策略;", this.name, increasePositionPoint, -increasePositionDiffPoint);
                        return false;
                    }

                    if (increasePositionDiffPoint >= increasePositionPoint)
                    {
                        desc.AppendFormat("策略名称[{0}]策略值[{1}]小于{2},执行该策略;", this.name, increasePositionPoint, -increasePositionDiffPoint);
                        return true;
                    }
                }
                else
                {
                    desc.AppendFormat("策略名称[{0}]无当前订单,不执行该策略;", this.name);
                    flag = false;
                }
            }
            else {
                //反向单,空单加仓
                if (!string.IsNullOrEmpty(currentOrderType))
                {
                    if (price <= 0 || currentPrice <= 0 || increasePositionDiffPoint < 0)
                    {
                        desc.AppendFormat("策略名称[{0}]当前价格[{1}]开仓价格[{2}]价格差[{3}]不满足,不执行该策略;", this.name, price, currentPrice, -increasePositionDiffPoint);
                        return false;
                    }
                    if (increasePositionDiffPoint < increasePositionPoint)
                    {
                        desc.AppendFormat("策略名称[{0}]策略值[{1}]大于{2},不执行该策略;", this.name, increasePositionPoint, increasePositionDiffPoint);
                        return false;
                    }

                    if (increasePositionDiffPoint >= increasePositionPoint)
                    {
                        desc.AppendFormat("策略名称[{0}]策略值[{1}]小于{2},执行该策略;", this.name, increasePositionPoint, increasePositionDiffPoint);
                        return true;
                    }
                }
                else
                {
                    desc.AppendFormat("策略名称[{0}]无当前订单,不执行该策略;", this.name);
                    flag = false;
                }
            }
            return flag;
        }
        public override string ToDesc()
        {
            return desc.ToString();
        }
        public double getSlope(EAChart eaChart, IDictionary<string, string> context, string timeDuration)
        {
            return 30;
        }
    }
    
}
