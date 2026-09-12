namespace AutoHorseRace
{
    /// <summary>
    /// 赛马交易或对冲记录明细类，用于表示单笔下注/吃票的详细信息、赔率、税费及执行状态。
    /// </summary>
    public class RaceBettingRecordInfo
    {
        /// <summary>
        /// 是否为子单/分流单标志
        /// </summary>
        public bool isSub { get; set; }

        /// <summary>
        /// 场次
        /// </summary>
        public string raceNo { get; set; } = string.Empty;

        /// <summary>
        /// 玩法或大盘类型（例如："Q" 表示连赢）
        /// </summary>
        public string type { get; set; } = string.Empty;

        /// <summary>
        /// 涉及的马匹组合（例如："3 & 4"）
        /// </summary>
        public string horses { get; set; } = string.Empty;

        /// <summary>
        /// 基础下注金额
        /// </summary>
        public string stake { get; set; } = string.Empty;

        /// <summary>
        /// 百分比或成交比例系数（例如："89"）
        /// </summary>
        public string pct { get; set; } = string.Empty;

        /// <summary>
        /// 赔率或金额限额（例如："700"）
        /// </summary>
        public string limit { get; set; } = string.Empty;

        /// <summary>
        /// 税费或抽水比例（例如："0.06"）
        /// </summary>
        public string tax { get; set; } = string.Empty;

        /// <summary>
        /// 下注（Bet）结算赔付金额（带括号代表负数/亏损）
        /// </summary>
        public string betPayout { get; set; } = string.Empty;

        /// <summary>
        /// 赔率（若无则显示 "-"）
        /// </summary>
        public string odds { get; set; } = string.Empty;

        /// <summary>
        /// 吃票（Eat）结算赔付金额
        /// </summary>
        public string eatPayout { get; set; } = string.Empty;

        /// <summary>
        /// 当前动作的最终净盈亏
        /// </summary>
        public string winLoss { get; set; } = string.Empty;

        /// <summary>
        /// 操作动作类型（例如："赌"）
        /// </summary>
        public string action { get; set; } = string.Empty;

        /// <summary>
        /// 动作执行的具体时间戳（例如："31-07-2026 20:00:04"）
        /// </summary>
        public string actDate { get; set; } = string.Empty;

        /// <summary>
        /// 实际操作类型
        /// </summary>
        public string actType { get; set; } = string.Empty;

        /// <summary>
        /// 实际操作涉及的马匹组合
        /// </summary>
        public string actHorses { get; set; } = string.Empty;

        /// <summary>
        /// 实际操作的下注金额
        /// </summary>
        public string actStake { get; set; } = string.Empty;

        /// <summary>
        /// 动作执行状态
        /// </summary>
        public string actStatus { get; set; } = string.Empty;
    }
}
