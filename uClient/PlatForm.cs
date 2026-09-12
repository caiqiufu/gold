using System;
using System.Collections.Generic;

namespace uClient.Comm
{
    public class Platform
    {
        /// <summary>
        /// 平台编码，主要用于分支判断,取值Enum.Platform
        /// MF4 = 1,
        /// MT4 = 2,
        /// V4 = 3,
        /// MT5 = 4
        /// </summary>
        public int PlatformNo { set; get; }
        /// <summary>
        /// 平台编码
        /// </summary>
        public string PlatformCode { set; get; }
        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { set; get; }
        /// <summary>
        /// 平台下的交易参数
        /// </summary>
        public TradeParametre TradePara { set; get; }
        /// <summary>
        /// 平台下的交易商列表
        /// </summary>
        public List<uClient.Comm.Broker> Broker = new List<Broker>();
        /// <summary>
        /// 平台下的交易商列表
        /// </summary>
        public Dictionary<String, Broker> BrokerList = new Dictionary<String, Broker>();
        public Broker GetBrokerByName(string BrokerName)
        {
            if (Broker!=null&& Broker.Count>0)
            {
                foreach (uClient.Comm.Broker b in this.Broker)
                {
                    if (string.Equals(BrokerName,b.BrokerName))
                    {
                        return b;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 平台下的默认交易商
        /// </summary>
        public String DefaultBrokerCode { set; get; }
    }
}
