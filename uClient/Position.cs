using System;

namespace uClient.Comm
{
    public class Position
    {
        public decimal Profit { get; set; }
        public decimal Commission { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LiquidatedTime { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal OrigPrice { get; set; }
        public string Ticket { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal ClosePrice { get; set; }
        public string ContractName { get; set; }
        public string Symbol { get; set; }
        public string BuySell { get; set; }
        public decimal Lot { get; set; }
        public decimal Amount { get; set; }

        public string toString()
        {
            return string.Format("Profit={0},CurrentPrice={1},Ticket={2},OpenPrice={3},ClosePrice={4},BuySell={5}", Profit, CurrentPrice, Ticket, OpenPrice, ClosePrice, BuySell);
        }
    }
}
