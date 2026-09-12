
namespace uClient.Comm
{
    public class Enum
    {
        public static class MarketOrderStatus
        {
            public static string Accepted = "Market Order Accepted,市价单被接纳";
            public static string Rejected = "Market Order Rejected,市价单被拒绝";
        }
        public static class PendingOrderStatus
        {
            public static string Accepted = "Pending Order Accepted,挂单被接纳";
            public static string Rejected = "Pending Order Rejected,挂单被拒绝";
        }
        public static class LiquidationOrderStatus
        {
            public static string Accepted = "Liquidation Accepted,平仓被接纳";
            public static string Rejected = "Liquidation Rejected,平仓被拒绝";
        }

        public static string Contract = "合約,Contract";
        /// <summary>
        /// 市价单,挂单
        /// </summary>
        public static string OrderType = "订单类型,Order Type";
        /// <summary>
        /// 市价平仓,挂单平仓
        /// </summary>
        public static string LiquidationType = "平仓类型,Liquidation Type";
        /// <summary>
        /// 开仓
        /// </summary>
        public static string Open = "Open";
        /// <summary>
        /// 平仓
        /// </summary>
        public static string Close = "Close";
        /// <summary>
        /// 卖出/买进
        /// </summary>
        public static string TradeSide = "卖出/买进,Sell/Buy";
        public static string Buy = "买进,Buy";
        public static string Sell = "卖出,Sell";
        public static string Lots = "手数,Lots";
        public static string Price = "执行价格,Execution Price";
        public static string ServerPositionRef = "编号,交易编号,Ref.";
        public static string Reason = "原因,Reason";
        public static string ExecutionTime = "执行时间,平仓时间,Execution Time,Liquidation Time";
        public static string Profit = "盈亏,Profit/Loss";
        public static string OpenPositionReference = "开仓编号,Open Position Reference";

        public enum Platform
        {
            MF4 = 1,
            MT4 = 2,
            V4 = 3,
            MT5 = 4
        }
    }
}
