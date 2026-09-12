
namespace uClient.Comm
{
    public class MessageFormatter
    {
        /// <summary>
        /// 订单交易成功后写入日志信息
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="TradeType">交易类型（有效值: <see cref="BUY"/>, <see cref="SELL"/>）</param>
        /// <returns>"[{0}][自动买多][接收]成交价:{1} 手数:{2} 耗时:{3}ms"</returns>
        public static string OrderReceiveMessage(bool _IsAutoOperationFlag,string TradeType)
        {
            string message = "";
            if (_IsAutoOperationFlag)
            {
                if ("BUY".Equals(TradeType))
                {
                    message = "自动买多";
                }
                else if ("SELL".Equals(TradeType))
                {
                    message = "自动卖空";
                }
                else if ("BUY_CLOSE".Equals(TradeType))
                {
                    message = "自动买平";
                }
                else if ("SELL_CLOSE".Equals(TradeType))
                {
                    message = "自动卖平";
                }
            }
            else
            {
                if ("BUY".Equals(TradeType))
                {
                    message = "手动买多";
                }
                else if ("SELL".Equals(TradeType))
                {
                    message = "手动卖空";
                }
                else if ("BUY_CLOSE".Equals(TradeType))
                {
                    message = "手动买平";
                }
                else if ("SELL_CLOSE".Equals(TradeType))
                {
                    message = "手动卖平";
                }
            }
            return "[{0}]" + "["+ message + "][接收]成交价:{1} 手数:{2} 耗时:{3}ms 余额:{4}";
        }
        /// <summary>
        /// 订单交易发送后写入日志信息
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="TradeType">交易类型（有效值: <see cref="BUY"/>, <see cref="SELL"/>）</param>
        /// <returns>"[{0}][自动买多][发送]报价:{1} 手数:{2} 平台价:{3}"</returns>
        public static string OrderSendMessage(bool _IsAutoOperationFlag, string TradeType)
        {
            string message = "";
            if (_IsAutoOperationFlag)
            {
                if ("BUY".Equals(TradeType))
                {
                    message = "自动买多";
                }
                else if ("SELL".Equals(TradeType))
                {
                    message = "自动卖空";
                }
                else if("BUY_CLOSE".Equals(TradeType))
                {
                    message = "自动买平";
                }
                else if ("SELL_CLOSE".Equals(TradeType))
                {
                    message = "自动卖平";
                }
                return "[{0}]" + "[" + message + "][发送]报价:{1} 手数:{2} 跳次:{3}  余额:{4}";
            }
            else
            {
                if ("BUY".Equals(TradeType))
                {
                    message = "手动买多";
                }
                else if ("SELL".Equals(TradeType))
                {
                    message = "手动卖空";
                }
                else if ("BUY_CLOSE".Equals(TradeType))
                {
                    message = "手动买平";
                }
                else if ("SELL_CLOSE".Equals(TradeType))
                {
                    message = "手动卖平";
                }
                return "[{0}]" + "[" + message + "][发送]报价:{1} 手数:{2}  余额:{3}";
            }
            
        }
    }
}
