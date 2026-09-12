using System;

namespace uClient.Comm
{
    public class Quote
    {
        public Quote(string symbol, decimal bid, decimal ask, DateTime time)
        {
            this.Symbol = symbol;
            this.Bid = bid;
            this.Ask = ask;
            this.Time = time;
        }
        //
        // Summary:
        //     Trading instrument.
        public string Symbol;
        //
        // Summary:
        //     Bid.
        public decimal Bid;
        //
        // Summary:
        //     Ask.
        public decimal Ask;
        //
        // Summary:
        //     Server time.
        public DateTime Time;
        /// <summary>
        /// 异常信息
        /// </summary>
        public string exception;
    }
}
