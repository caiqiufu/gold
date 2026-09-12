namespace AutoHorseRace
{
    /// <summary>
    /// 已结算交易历史记录信息类，用于映射单条已结算盘口和账单明细数据。
    /// </summary>
    public class SettledHistoryInfo
    {
        /// <summary>
        /// 结算日期描述（例如："31 07月 2026 星期五"）
        /// </summary>
        public string date { get; set; } = string.Empty;

        /// <summary>
        /// 赛事地点或场次场地名称（例如："香港 海外-古活"）
        /// </summary>
        public string location { get; set; } = string.Empty;

        /// <summary>
        /// 赔率所属类型或地区（例如："香港"）
        /// </summary>
        public string oddsType { get; set; } = string.Empty;

        /// <summary>
        /// 总投注/下注金额
        /// </summary>
        public string stake { get; set; } = string.Empty;

        /// <summary>
        /// 抽水或税费金额
        /// </summary>
        public string tax { get; set; } = string.Empty;

        /// <summary>
        /// 盈亏金额（带括号通常代表负数/亏损）
        /// </summary>
        public string winLoss { get; set; } = string.Empty;
        /// <summary>
        /// 赛马日期（例如："31-07-2026"）
        /// </summary>
        public string raceDate { get; set; } = string.Empty;

        /// <summary>
        /// 赛事/大盘分类标识（例如："171H"）
        /// </summary>
        public string raceType { get; set; } = string.Empty;
    }
}