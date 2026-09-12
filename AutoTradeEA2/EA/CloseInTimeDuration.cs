
using ICSharpCode.NRefactory.CSharp.Refactoring;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uClient.Comm;

namespace uClient.Broker
{
    /// <summary>
    /// CLOSE_IN_TIMEDURATION
    /// 开仓单在开仓5-10分钟之间随机平仓
    /// </summary>
    public class CloseInTimeDuration : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();

            string commandCreateTime = context["CommandCreateTime"].ToString();
            string currentOrderType = context["CurrentOrderType"].ToString();

            if (string.IsNullOrEmpty(currentOrderType) || string.Equals(currentOrderType, "CLOSE_BUY") || string.Equals(currentOrderType, "CLOSE_SELL"))
            {
                desc.AppendFormat("策略名称[{0}]无开单，不执行该策略", this.name);
                return false;
            }
            string currentDateTimeStr;
            if (context.ContainsKey("CurrentDateTime"))
            {
                currentDateTimeStr = context["CurrentDateTime"].ToString();
            }
            else
            {
                currentDateTimeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            desc.AppendLine($"动态参数[currentOrderType{currentOrderType}][commandCreateTime{commandCreateTime}]");

            // 解析时间字符串
            DateTime currentDateTime = DateTime.ParseExact(currentDateTimeStr, "yyyy-MM-dd HH:mm:ss", null);
            DateTime createTime = DateTime.ParseExact(commandCreateTime, "yyyy-MM-dd HH:mm:ss", null);

            // 计算时间差（秒）
            double diffSeconds = (currentDateTime - createTime).TotalSeconds;

            // 生成随机数（300~600）
            Random random = new Random();
            int randomThreshold = random.Next(300, 601);
            //是否满足策略标识
            bool flag = false;
            string strategyResult = "";
            if (string.Equals(currentOrderType, "BUY") || string.Equals(currentOrderType, "SELL"))
            {
                if (diffSeconds > randomThreshold && string.Equals(currentOrderType, "BUY"))
                {
                    strategyResult = "CLOSE_BUY";
                }
                if (diffSeconds > randomThreshold && string.Equals(currentOrderType, "SELL"))
                {
                    strategyResult = "CLOSE_SELL";
                }
                flag = true;
            }
            else 
            {
                flag = false;
            }

            desc.AppendFormat("动态参数[currentOrderType{0}][commandCreateTime{1}][currentDateTimeStr{2}][randomThreshold{3}][strategyResult{4}];", currentOrderType, commandCreateTime, currentDateTimeStr, randomThreshold, strategyResult);

            context["StrategyResultInTimeDurationClose"] = strategyResult;

            return flag;
        }
        public override string ToDesc()
        {
            return desc.ToString();
        }       
    }
}
