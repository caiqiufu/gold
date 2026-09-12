using System;
using System.Collections.Generic;
using System.Text;


namespace uClient.Broker
{
    /// <summary>
    /// NOTRADE_DURATION
    /// 非交易指令时间,策略参数配置,参数:notradeDuration
    /// 时间格式为：04:45-06:00,20240511
    /// </summary>
    public class NoTradeDuration : StrategyEA
    {
        private StringBuilder _log = new StringBuilder();
        /// <summary>
        /// 返回策略的实时状态描述，供前端或 UI 显示
        /// </summary>
        public override string ToDesc() => _log.ToString();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            _log.Clear();
            string notradeDuration = this.configParam["NotradeDuration"].ToString();
            if (string.IsNullOrEmpty(notradeDuration))
            {
                _log.AppendFormat($"策略名称[{this.name}]未配置非执行时间,采用数据库默认配置时间;");
            }
            //string currentTime = context["CurrentTime"];
            string currentDateTime = context["CurrentDateTime"].ToString();
            if (string.IsNullOrEmpty(currentDateTime))
            {
                _log.AppendFormat($"策略名称[{this.name}]当前时间未获取,执行该策略;");
                return true;
            }
            //如果是周六就把结束时间提前15分钟
            // 1. 将字符串解析为 DateTime 对象
            if (DateTime.TryParse(currentDateTime, out DateTime dt))
            {
                // 2. 判断是否为周六 (DayOfWeek.Saturday)
                if (dt.DayOfWeek == DayOfWeek.Saturday)
                {
                    // 3. 调用工具类修改结束时间
                    // 注意：提前 15 分钟要传 -15，传正 15 会变成延后
                    notradeDuration = Utils.UpdateStartTime(notradeDuration, -15);
                    _log.AppendFormat($"策略名称[{this.name}],周六时把设置时间范围提前15分钟[{notradeDuration}];");
                }
            }
            _log.AppendFormat($"动态参数[currentDateTime{currentDateTime}][noTradeDuration{notradeDuration}];");
            //默认采用EANotradeDuration,在周六的时候需要提前15分钟,避免开仓后无法平仓
            bool flag = Utils.checkIsTradeTime(notradeDuration, currentDateTime);
            if (flag)
            {
                _log.AppendFormat($"策略名称[{this.name}]在设置时间范围[{notradeDuration}],允许交易,执行该策略;");
            }
            else
            {
                _log.AppendFormat($"策略名称[{this.name}]不在设置时间范围[{notradeDuration}],不允许交易,不执行该策略;");
                return false;
            }
            return flag;
        }
    }
}
