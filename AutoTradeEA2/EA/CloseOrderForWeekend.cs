using System;
using System.Collections.Generic;
using System.Text;

namespace uClient.Broker
{
    /// <summary>
    /// CLOSE_ORDER_WEEKEND
    /// 周末平仓,不持单过周末和节假日,策略参数配置非交易日期,周末平仓后,把context参数也清空
    /// </summary>
    public class CloseOrderForWeekEnd : StrategyEA
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
                if (DateTime.TryParse(currentDateTime, out DateTime dt))
                {
                    // 2. 判断是否为周六 (DayOfWeek.Saturday)
                    if (dt.DayOfWeek == DayOfWeek.Saturday)
                    {
                        // 3. 调用工具类修改结束时间
                        // 注意：提前 15 分钟要传 -15，传正 15 会变成延后
                        // 提前5分钟,避免由于quota采集时间间隔导致未完成平仓
                        notradeDuration = Utils.UpdateStartTime(notradeDuration, -5);
                        _log.AppendFormat($"策略名称[{this.name}],周六时把设置时间范围提前5分钟[{notradeDuration}];");
                    }
                }

                bool flag = checkIsSatCloseTime(notradeDuration, currentDateTime);
                if (flag)
                {
                    _log.AppendFormat($"策略名称[{this.name}][currentDateTime:{currentDateTime}]时间已超过周六[{notradeDuration}],执行该策略;");
                    return true;
                }
                else
                {
                    _log.AppendFormat($"策略名称[{this.name}][currentDateTime:{currentDateTime}]时间未超过周六[{notradeDuration}],不执行该策略;");
                    return false;
                }
            }
            else
            {
                _log.AppendFormat($"策略名称[{this.name}]未开仓,不执行周末平仓策略;");
                return false;
            }
        }
        /// <summary>
        /// 在周六早上根据Notrade平仓(开仓时间已经提前15分钟停止)
        /// </summary>
        public bool checkIsSatCloseTime(string EANotradeDuration, string currentTime)
        {
            DateTime currentTimedt = DateTime.Parse(currentTime); // 当前时间, 数据采集时间
            DayOfWeek dayOfWeek = currentTimedt.DayOfWeek;

            // 解析 EANotradeDuration，例如 "04:45-06:15"
            string[] parts = EANotradeDuration.Split('-');
            DateTime startTime = DateTime.Parse(parts[0]);

            if (dayOfWeek == DayOfWeek.Saturday && currentTimedt.TimeOfDay >= startTime.TimeOfDay)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
