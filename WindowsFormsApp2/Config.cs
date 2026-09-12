using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    class Config
    {
        public Broker[] Broker { set; get; }
        public String DefaultBroker { set; get; }
        public String PublishAddress { set; get; }
        public TradeParametre TradePara { set; get; }
    }
}
