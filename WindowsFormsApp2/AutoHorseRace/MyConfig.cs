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
        /// 通知号码
        /// </summary>
        public required string[] NotifyNumber { set; get; }

        /// <summary>
        /// 通知邮件
        /// </summary>
        public required string[] NotifyEmail { set; get; }
    }
}
