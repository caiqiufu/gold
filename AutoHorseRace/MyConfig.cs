namespace AutoHorseRace
{
    /// <summary>
    /// 个性化配置文件
    /// </summary>
    public class MyConfig
    {
        /// <summary>
        /// 账户列表
        /// </summary>
        public required Account[] Account { set; get; }

        /// <summary>
        /// 交易账户
        /// </summary>
        public required Account TradeAccount { set; get; }

        /// <summary>
        /// 数据源账户
        /// </summary>
        public Account? DSAccount { set; get; }

        /// <summary>
        /// 通知号码
        /// </summary>
        public string[]? NotifyNumber { set; get; }

        /// <summary>
        /// 通知邮件
        /// </summary>
        public string[]? NotifyEmail { set; get; }
    }
}
