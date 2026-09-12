using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xClient
{
    class TradeParametre
    {
        public decimal SellOpen { set; get; }
        public decimal BuyOpen { set; get; }
        public decimal SellClose { set; get; }
        public decimal BuyClose { set; get; }
        public decimal SellLots { set; get; }
        public decimal BuyLots { set; get; }
        public decimal Slippage { set; get; }
        public decimal Stoploss { set; get; }
    }
}
