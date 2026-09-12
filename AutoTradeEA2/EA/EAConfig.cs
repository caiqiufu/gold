using System.Collections.Generic;


namespace uClient.Broker
{
    /// <summary>
    /// EA 配置数据
    /// 策略包含两种类型
    /// 1)实时执行策略,该策略执行后判断是否生成指令
    /// 2)定时执行策略,该策略执行后判断是否需要修改策略参数
    /// </summary>
    public class EAConfig
    {
        /// <summary>
        /// 1)实时执行策略EA信息
        /// </summary>
        public List<StrategyEA> EAList = new List<StrategyEA>();
        /// <summary>
        /// 2)定时执行策略EA信息
        /// </summary>
        public List<StrategyEA> ParaEAList = new List<StrategyEA>();
        /// <summary>
        /// EA列表，Code=EACode
        /// </summary>
        public Dictionary<string, StrategyEA> EA = new Dictionary<string, StrategyEA>();

        
    }
}
