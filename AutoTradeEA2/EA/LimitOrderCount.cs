
using System;
using System.Collections.Generic;
using System.Text;

namespace uClient.Broker
{
    /// <summary>
    /// LIMIT_ORDER_COUNT
    /// 参数:maxOrderCount
    /// 如果一天单量超过指定单量,就修改参数[maxDiffChangeDuration=2] 配置参数:当前为固定值3
    /// 返回true,订单数量超过设定值
    /// </summary>
    public class LimitOrderCount : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();

            string analysisDataType = this.configParam["analysisDataType"];
            int maxOrderCount = Convert.ToInt16(this.configParam["maxOrderCount"]);
            string noTradeDuration = this.configParam["noTradeDuration"];
            string _IsSimulateTest = this.configParam["IsSimulateTest"];
            string _TestBatchNo = this.configParam["TestBatchNo"];

            string currentDateTimeStr;
            if (context.ContainsKey("CurrentDateTime"))
            {
                currentDateTimeStr = context["CurrentDateTime"].ToString();
            }
            else
            {
                currentDateTimeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            int orderCount = Utils.orderCountForDuration(analysisDataType, noTradeDuration, currentDateTimeStr);

            desc.AppendFormat("动态参数[maxOrderCount{0}][currentDateTimeStr{1}][analysisDataType{2}][noTradeDuration{3}];", maxOrderCount, currentDateTimeStr, analysisDataType, noTradeDuration);

            if (orderCount > maxOrderCount)
            {
                desc.AppendFormat("策略名称[{0}]当前订单数[{1}]大于设置值[{2}],执行该策略", this.name, orderCount, maxOrderCount);
                return true;
            }
            else
            {
                desc.AppendFormat("策略名称[{0}]当前订单数[{1}]小于设置值[{2}],不执行该策略", this.name, orderCount, maxOrderCount);
                return false;
            }
        }

        public override string ToDesc()
        {
            return desc.ToString();
        }
    }
}
