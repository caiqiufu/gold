namespace uClient.Comm
{
    /// <summary>
    /// 交易商类
    /// </summary>
    public class Broker
    {
        /// <summary>
        /// 交易商编码
        /// </summary>
        public string BrokerCode { set; get; }
        /// <summary>
        /// 交易商名称
        /// </summary>
        public string BrokerName { set; get; }
        /// <summary>
        /// 平台编码 
        /// </summary>
        public int PlatformNo { set; get; }
        /// <summary>
        /// 平台编码
        /// </summary>
        public string PlatformCode { set; get; }

        /// <summary>
        /// 当前账户打开的窗口编码
        /// </summary>
        public string FormNo { set; get; }
        /// <summary>
        /// 交易品种
        /// </summary>
        public string[] Symbol { set; get; }
        /// <summary>
        /// DemoIP
        /// </summary>
        public string DemoIP { set; get; }
        public string IP(string accountType)
        {
            return string.Equals(accountType, "Demo") ? DemoIP : LiveIP;
        }
        /// <summary>
        /// DemoPort
        /// </summary>
        public string DemoPort { set; get; }
        public string Port(string accountType)
        {
            return string.Equals(accountType, "Demo") ? DemoPort : LivePort;
        }
        /// <summary>
        /// 交易商连接对应的DemoPathLine
        /// </summary>
        public string DemoPathLine { set; get; }
        public string PathLine(string accountType)
        {
            return string.Equals(accountType, "Demo") ? DemoPathLine : LivePathLine;
        }
        /// <summary>
        /// DemoIP
        /// </summary>
        public string LiveIP { set; get; }
        /// <summary>
        /// DemoPort
        /// </summary>
        public string LivePort { set; get; }
        /// <summary>
        /// 交易商连接对应的DemoPathLine
        /// </summary>
        public string LivePathLine { set; get; }
        /// <summary>
        /// 交易商下的默认账号类型
        /// </summary>
        public string DefaultAccountType { set; get; }
        /// <summary>
        /// 交易商下的默认交易品类
        /// </summary>
        public string DefaultSymbol { set; get; }
        /// <summary>
        /// 交易商的实盘账号
        /// </summary>
        public Account LiveAccount { set; get; }
        /// <summary>
        /// 交易商的模拟账号
        /// </summary>
        public Account DemoAccount { set; get; }
        /// <summary>
        /// 交易商的交易配置信息，覆盖平台商的配置信息
        /// </summary>
        public TradeParametre TradePara { set; get; }

        public string toString()
        {
            string symbols = DefaultSymbol;
            if (Symbol!=null && Symbol.Length >0)
            {
                symbols = string.Join("|", Symbol);
            }
            string str = "BrokerCode=" + BrokerCode + "," + "BrokerName=" + BrokerName + "," + "FormNo=" + FormNo + "," + "LiveIP=" + LiveIP + "," + "DemoIP=" + DemoIP + "," + "symbols="+ symbols+",";
            if (LiveAccount!=null)
            {
                str = str + LiveAccount.toString() + ",";
            }
            if (DemoAccount != null)
            {
                str = str + DemoAccount.toString() + ",";
            }
            return str;
        }
    }
}
