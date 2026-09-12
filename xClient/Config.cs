using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xClient
{
    class Config
    {
        public Broker[] Broker { set; get; }
        public String DefaultBroker { set; get; }
        public String PublishAddressOEC { set; get; }
        public String PublishAddressT4 { set; get; }
        public String DSType { set; get; }
        public String PlatForm { set; get; }
        public TradeParametre TradePara { set; get; }
    }
}
