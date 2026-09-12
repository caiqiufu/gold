namespace AutoHorseRace
{
    /// <summary>
    /// 结算汇总信息类，用于表示单场比赛中各项投注（Bet）与吃票（Eat）的资金明细及盈亏情况。
    /// </summary>
    public class SettledSummaryInfo
    {
        /// <summary>
        /// 场次编号（例如："1" 表示第一场）
        /// </summary>
        public string raceNo { get; set; } = string.Empty;

        /// <summary>
        /// 下注（Bet）的本金/注数金额
        /// </summary>
        public string betStake { get; set; } = string.Empty;

        /// <summary>
        /// 下注（Bet）的实际收付金额（带括号通常代表负数/支出）
        /// </summary>
        public string betAmount { get; set; } = string.Empty;

        /// <summary>
        /// 下注（Bet）产生的税费或抽水
        /// </summary>
        public string betTax { get; set; } = string.Empty;

        /// <summary>
        /// 下注（Bet）退款金额
        /// </summary>
        public string betRefund { get; set; } = string.Empty;

        /// <summary>
        /// 吃票（Eat）的本金/注数金额
        /// </summary>
        public string eatStake { get; set; } = string.Empty;

        /// <summary>
        /// 吃票（Eat）的实际收付金额
        /// </summary>
        public string eatAmount { get; set; } = string.Empty;

        /// <summary>
        /// 吃票（Eat）产生的税费或抽水
        /// </summary>
        public string eatTax { get; set; } = string.Empty;

        /// <summary>
        /// 吃票（Eat）退款金额
        /// </summary>
        public string eatRefund { get; set; } = string.Empty;

        /// <summary>
        /// 当前场次的最终净盈亏
        /// </summary>
        public string winLoss { get; set; } = string.Empty;
    }
}
