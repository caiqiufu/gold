using System;
using System.Collections.Generic;
using System.Text;

namespace uClient.Broker
{
    /// <summary>
    /// CLOSE_ORDER_DAILY
    /// 每天平仓,避免跳开
    /// </summary>
    public class CloseOrderForDailyEnd : StrategyEA
    {
        private StringBuilder _log = new StringBuilder();
        /// <summary>
        /// 返回策略的实时状态描述，供前端或 UI 显示
        /// </summary>
        public override string ToDesc() => _log.ToString();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            _log.Clear();
            string currentOrderType = context["CurrentOrderType"].ToString();
            string currentDateTime = context["CurrentDateTime"].ToString();
            string notradeDuration = this.configParam["NotradeDuration"].ToString();

            _log.AppendFormat($"动态参数[currentOrderType{currentOrderType}][currentDateTime{currentDateTime}];");

            if (string.IsNullOrEmpty(currentOrderType))
            {
                _log.AppendFormat($"策略名称[{this.name}]无开单,不执行该策略");
                return false;
            }
            if (string.IsNullOrEmpty(currentDateTime))
            {
                _log.AppendFormat($"策略名称[{this.name}]当前时间未获取,执行该策略;");
                return false;
            }

            if (string.Equals(currentOrderType, "SELL") || string.Equals(currentOrderType, "BUY"))
            {
                // 提前5分钟,避免由于quota采集时间间隔导致未完成平仓
                notradeDuration = Utils.UpdateStartTime(notradeDuration, -5);
                bool flag = checkIsDailyCloseTime(notradeDuration, currentDateTime);
                if (flag)
                {
                    _log.AppendFormat($"策略名称[{this.name}][currentDateTime:{currentDateTime}]时间已超过平仓时间[{notradeDuration}],执行该策略;");
                    return true;
                }
                else
                {
                    _log.AppendFormat($"策略名称[{this.name}][currentDateTime:{currentDateTime}]时间未超过平仓时间[{notradeDuration}],不执行该策略;");
                    return false;
                }
            }
            else
            {
                _log.AppendFormat($"策略名称[{this.name}]未开仓,不执行日平仓策略;");
                return false;
            }
        }

        /// <summary>
        /// 判断当前时间是否在 Notrade 时间区间内（如 04:45-06:15）
        /// </summary>
        public bool checkIsDailyCloseTime(string EANotradeDuration, string currentTime)
        {
            if (!DateTime.TryParse(currentTime, out DateTime current))
                return false;

            string[] parts = EANotradeDuration.Split('-');

            if (!TimeSpan.TryParse(parts[0], out TimeSpan start))
                return false;

            if (!TimeSpan.TryParse(parts[1], out TimeSpan end))
                return false;

            TimeSpan now = current.TimeOfDay;

            // 判断是否在区间内（包含边界）
            return now >= start && now <= end;
        }
    }
}
