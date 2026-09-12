using System;

namespace uClient.Comm
{
    public class StrategySymbol
    {
        public String SymbolName { set; get; }
        public StrategyDetail BuyClose { set; get; }
        public StrategyDetail SellClose { set; get; }
        public TradeParametre TradePara { set; get; }
    }
}
