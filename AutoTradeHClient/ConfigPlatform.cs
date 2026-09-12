using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uClient.Comm;

namespace uClient.Comm
{
    class ConfigPlatform
    {
        /// <summary>
        /// 平台ID
        /// </summary>
        public int Platform { set; get; }
        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { set; get; }
        /// <summary>
        /// 是否自动锁单
        /// </summary>
        public bool AutoLock { set; get; }
        /// <summary>
        /// 平台下默认的交易参数
        /// </summary>
        public TradeParametre TradePara { set; get; }
        /// <summary>
        /// 代理商列表，直接采用程序加载
        /// </summary>
        public uClient.Comm.Broker[] Broker { set; get; }
        /// <summary>
        /// 平台下的代理商列表
        /// </summary>
        Dictionary<String, Broker> BrokerList = new Dictionary<String, Broker>();
        /// <summary>
        /// 该平台下默认的代理商
        /// </summary>
        public String DefaultBroker { set; get; }
    }
}
