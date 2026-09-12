using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp2
{
    class Broker
    {
        /**
         * 平台商名称
         */
        public string BrokerName { set; get; }

        public string DefaultAccount { set; get; }

        public string DefaultSymbol { set; get; }

        public Account LiveAccount { set; get; }

        public Account DemoAccount { set; get; }
    }
}
