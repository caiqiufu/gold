using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uClient.Broker
{
    public class StrategyConfigTestData
    {
        /// <summary>
        /// 生成该策略的参数
        /// </summary>
         public string paras { set; get; }
        /// <summary>
        /// 策略列表测试数据
        /// </summary>
        public IList<Dictionary<string, EAEntity>> EAEntityList = new List<Dictionary<string, EAEntity>>();
    }
}
