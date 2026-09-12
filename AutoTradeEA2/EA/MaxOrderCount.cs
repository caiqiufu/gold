
using System;
using System.Collections.Generic;
using System.Text;

namespace uClient.Broker
{
    /// <summary>
    /// MAX_ORDER_COUNT
    /// 每一个交易日总计开仓数量,超过开仓数量后不再开单
    /// 参数:maxOrderCount
    /// 返回true,未超过开仓数量,可以开仓,否则不能开仓
    /// </summary>
    public class MaxOrderCount : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();

            ///交易日最大订单数
            int maxOrderCount = Convert.ToInt16(this.configParam["maxOrderCount"]);
            //周最大亏损值
            double maxLossPoint = Convert.ToDouble(this.configParam["maxLossPoint"]);
            //连续亏损单数
            int lossOrderContinuousCount = Convert.ToInt16(this.configParam["lossOrderContinuousCount"]);
            //周总亏损单数
            int lossOrderTotalCount = Convert.ToInt16(this.configParam["lossOrderTotalCount"]);


            string analysisDataType = context["analysisDataType"].ToString();
            string noTradeDuration = this.configParam["noTradeDuration"];
            string _IsSimulateTest = context["IsSimulateTest"].ToString();
            string _TestBatchNo = context["TestBatchNo"].ToString();
            string selectedStrategy = context["SelectedStrategy"].ToString();

            string currentDateTimeStr;
            if (context.ContainsKey("CurrentDateTime"))
            {
                currentDateTimeStr = context["CurrentDateTime"].ToString();
            }
            else
            {
                currentDateTimeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            int orderCount = 0;
            double currentMaxLossPoint = 0;
            if (string.Equals(_IsSimulateTest, "T"))
            {
                orderCount = Utils.orderCountForDurationForTest(_TestBatchNo, selectedStrategy, analysisDataType, noTradeDuration, currentDateTimeStr);

            }
            else
            {
                orderCount = Utils.orderCountForDuration(analysisDataType, noTradeDuration, currentDateTimeStr);
            }

            if (string.Equals(_IsSimulateTest, "T"))
            {
                currentMaxLossPoint = Utils.getOrderCountForSumLossesWeeklyForTest(_TestBatchNo, selectedStrategy, analysisDataType, currentDateTimeStr);

            }
            else
            {
                currentMaxLossPoint = Utils.getOrderCountForSumLossesWeekly(analysisDataType, currentDateTimeStr);
            }

            desc.AppendFormat("动态参数[maxOrderCount{0}][currentDateTimeStr{1}][orderCount{2}][noTradeDuration{3}][lossOrderContinuousCount{4}][lossOrderTotalCount{5}][maxLossPoint{6}][currentMaxLossPoint{7}];", maxOrderCount, currentDateTimeStr, orderCount, noTradeDuration, lossOrderContinuousCount, lossOrderTotalCount, maxLossPoint, currentMaxLossPoint);
            if (orderCount >= maxOrderCount)
            {
                desc.AppendFormat("策略名称[{0}]当日订单数[{1}]大于设置值[{2}],限制开单,执行该策略;", this.name, orderCount, maxOrderCount);
                return true;
            }
            else
            {
                desc.AppendFormat("策略名称[{0}]当日订单数[{1}]小于设置值[{2}],不限制开单,不执行该策略;", this.name, orderCount, maxOrderCount);
            }
            if (-currentMaxLossPoint >= maxLossPoint)
            {
                desc.AppendFormat("策略名称[{0}]本周亏损值[{1}]大于设置值[{2}],限制开单,执行该策略;", this.name, currentMaxLossPoint, -maxLossPoint);
                return true;
            }
            else
            {
                desc.AppendFormat("策略名称[{0}]本周亏损值[{1}]小于设置值[{2}],不限制开单,不执行该策略;", this.name, currentMaxLossPoint, -maxLossPoint);
            }
            bool isLossOrderContinuousCount;
            if (string.Equals(_IsSimulateTest, "T"))
            {
                isLossOrderContinuousCount = Utils.orderCountForContinuousLossesWeeklyForTest(_TestBatchNo, selectedStrategy, analysisDataType, currentDateTimeStr, lossOrderContinuousCount);
            }
            else
            {
                isLossOrderContinuousCount = Utils.orderCountForContinuousLossesWeekly(analysisDataType, currentDateTimeStr, lossOrderContinuousCount);
            }
            if (isLossOrderContinuousCount)
            {
                //连续亏损
                desc.AppendFormat("策略名称[{0}]订单连续亏损单数[{1}],限制开单,执行该策略;", this.name, lossOrderContinuousCount);
                return true;
            }
            else
            {
                desc.AppendFormat("策略名称[{0}]连续亏损单量限制不满足,不执行该策略;", this.name);
            }
            bool islossOrderTotalCount;
            if (string.Equals(_IsSimulateTest, "T"))
            {
                islossOrderTotalCount = Utils.getOrderCountForTotalLossesWeeklyForTest(_TestBatchNo, selectedStrategy, analysisDataType, currentDateTimeStr, lossOrderTotalCount);
            }
            else
            {
                islossOrderTotalCount = Utils.getOrderCountForTotalLossesWeekly(analysisDataType, currentDateTimeStr, lossOrderTotalCount);
            }
            if (islossOrderTotalCount)
            {
                //连续亏损
                desc.AppendFormat("策略名称[{0}]订单总亏损单数[{1}],限制开单,执行该策略;", this.name, lossOrderTotalCount);
                return true;
            }
            else
            {
                desc.AppendFormat("策略名称[{0}]总亏损单量限制不满足,不执行该策略;", this.name);
            }
            return false;
        }
        public override string ToDesc()
        {
            return desc.ToString();
        }
    }
}
