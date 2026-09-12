using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xClient
{
    class StrategySymbol
    {
        public String SymbolName { set; get; }
        public StrategyDetails BuyClose { set; get; }
        public StrategyDetails SellClose { set; get; }
        public TradeParametre TradePara { set; get; }
    }
}
