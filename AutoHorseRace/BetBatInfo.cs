namespace AutoHorseRace
{
    /// <summary>
    /// 吃票赌票信息
    /// </summary>
    public class EatBetInfo
    {

        /// <summary>
        /// 下注信息的唯一标识符
        /// </summary>
        public string seq { set; get; } = string.Empty;

        /// <summary>
        /// 场次日期
        /// </summary>
        public string raceDate { set; get; } = string.Empty;

        /// <summary>
        /// 场次类型 赛马大盘类型
        /// </summary>
        public string raceType { set; get; } = string.Empty;

        /// <summary>
        /// 当前场次
        /// </summary>
        public string raceNo { set; get; } = string.Empty;

        /// <summary>
        /// 类型，连赢 Q/位置QP
        /// </summary>
        public string type { set; get; } = string.Empty;

        /// <summary>
        /// 组合
        /// </summary>
        public string combo { set; get; } = string.Empty;

        /// <summary>
        /// 赔率
        /// </summary>
        public double toto { set; get; }

        /// <summary>
        /// 已确认吃票数量
        /// </summary>
        public double eatExecutedAmount { set; get; }

        /// <summary>
        /// 吃票水折,最新的水折记录
        /// </summary>
        public double eatOdds { set; get; }

        /// <summary>
        /// 吃票限额
        /// </summary>
        public double eatLimit { set; get; }
        /// <summary>
        /// 待确认吃票数量
        /// </summary>
        public double eatPendingAmount { set; get; }


        /// <summary>
        /// 已确认下注数量
        /// </summary>
        public double betExecutedAmount { set; get; }

        /// <summary>
        /// 下注水折,最新水折记录
        /// </summary>
        public double betOdds { set; get; }

        /// <summary>
        /// 下注限额
        /// </summary>
        public double betLimit { set; get; }

        /// <summary>
        /// 待确认下注数量
        /// </summary>
        public double betPendingAmount { set; get; }

        /// <summary>
        /// 总吃票数
        /// </summary>
        public double totalEatAmount { set; get; }

        /// <summary>
        /// 总赌票数
        /// </summary>
        public double totalBetAmount { set; get; }

    }
}
