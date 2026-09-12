using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4FixExampleQFn
{
    public class AppSettings
    {
        public string Firm { get; set; } = "";
        public string User { get; set; } = "";
        public string Account { get; set; } = "";
        public string ExchangeId { get; set; } = "";
        public string ContractId { get; set; } = "";
        public string MarketId { get; set; } = "";
        public string UseStunnel { get; set; } = "N";
    }
}
