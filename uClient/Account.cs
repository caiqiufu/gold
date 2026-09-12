namespace uClient.Comm
{
    public class Account
    {
        /// <summary>
        /// 交易商
        /// </summary>
        public string BrokerCode { set; get; }
        /// <summary>
        /// 账户类型 Demo/Live
        /// </summary>
        public string Type { set; get; }
        /// <summary>
        ///  帐号
        /// </summary>
        public string UserCode { set; get; }

        /// <summary>
        ///  密码
        /// </summary>
        public string Password { set; get; }
        /// <summary>
        /// 平台下的交易参数
        /// </summary>
        public TradeParametre TradePara { set; get; }

        public string toString()
        { 
            return "BrokerCode="+ BrokerCode+","+ "Type=" + Type + "," + "UserCode=" + UserCode + "," + "Password=" + Password + ",";
        }

    }
}
