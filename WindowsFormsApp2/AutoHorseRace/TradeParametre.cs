namespace AutoHorseRace
{
    /// <summary>
    /// 扩展参数
    /// </summary>
    public class TradeParametre
    {
        /// <summary>
        /// 是否自动锁单
        /// </summary>
        public bool AutoLock { set; get; }
        /// <summary>
        /// 锁单时间
        /// </summary>
        public decimal AutoLockTimeDuration { set; get; }
        /// <summary>
        /// 平仓时间
        /// </summary>
        public required string AutoCloseTimeDuration { set; get; }
        /// <summary>
        /// 自动选中
        /// </summary>
        public bool AutoChecked { set; get; }
        /// <summary>
        /// 发送通知
        /// </summary>
        public bool NotifyFlag { set; get; }

        /// <summary>
        /// 提醒方式 1:SMS,2:Email,3:All
        /// </summary>
        public required string NotifyType { set; get; }
        /// <summary>
        /// 平台服务器所在时区，默认为UTC+8
        /// </summary>
        public double Timezone { set; get; }
        /// <summary>
        /// 自动交易,启动交易窗口后,如果该值为true,则执行自动交易
        /// </summary>
        public bool AutoTrade { set; get; }
    }
}
