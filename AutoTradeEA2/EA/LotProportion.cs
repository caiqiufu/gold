using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uClient.Broker;

namespace uClient.Broker
{
    /// <summary>
    /// LOT_MODIFY
    /// 交易手数变化,根据情绪指数曲线改变交易手数,目前未实现
    /// </summary>
    public class LotProportion : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();
            throw new NotImplementedException();
        }

        public override string ToDesc()
        {
            return desc.ToString();
        }
    }
}
