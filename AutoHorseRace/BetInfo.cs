namespace AutoHorseRace
{
    /// <summary>
    /// 下注信息
    /// </summary>
    public class BetInfo
    {

        /// <summary>
        /// 下注信息的唯一标识符
        /// </summary>
        public string seq { set; get; } = string.Empty;

        /// <summary>
        /// 关联的对冲下注信息
        /// </summary>
        public string tradeRecordId { set; get; } = string.Empty;

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
        /// 下注票数
        /// </summary>
        public int stakeAmount { set; get; }

        /// <summary>
        /// 水折
        /// </summary>
        public int odds { set; get; }

        /// <summary>
        /// 票数
        /// </summary>
        public int amount { set; get; }

        /// <summary>
        /// 限额
        /// </summary>
        public int limit { set; get; }

        /// <summary>
        /// 下注类型 BET/EAT
        /// </summary>
        public string action { set; get; } = string.Empty;
        /// <summary>
        /// 状态
        /// </summary>
        public string status { set; get; } = string.Empty;
        /// <summary>
        /// 时间戳
        /// </summary>
        public string timestamp { set; get; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        public string remark { set; get; } = string.Empty;

        /// <summary>
        /// 删除时解析该字段 168600679,3,0,08-08-2026,14H,1
        /// </summary>
        public string mrParams { set; get; } = string.Empty;

        public override string ToString()
        {
            return $"BetInfo(" +
                   $"seq={seq}, " +
                   $"tradeRecordId={tradeRecordId}, " +
                   $"raceDate={raceDate}, " +
                   $"raceType={raceType}, " +
                   $"raceNo={raceNo}, " +
                   $"type={type}, " +
                   $"combo={combo}, " +
                   $"toto={toto}, " +
                   $"stakeAmount={stakeAmount}, " +
                   $"odds={odds}, " +
                   $"amount={amount}, " +
                   $"limit={limit}, " +
                   $"action={action}, " +
                   $"status={status}, " +
                   $"timestamp={timestamp}, " +
                   $"remark={remark}" +
                   $")";
        }

        /// <summary>
        /// 默认无参构造函数
        /// </summary>
        public BetInfo() { }

        /// <summary>
        /// 复制构造函数：基于已有对象创建一个全新且独立的副本
        /// </summary>
        public BetInfo(BetInfo other)
        {
            if (other == null) return;

            this.seq = other.seq;
            this.tradeRecordId = other.tradeRecordId;
            this.raceDate = other.raceDate;
            this.raceType = other.raceType;
            this.raceNo = other.raceNo;
            this.type = other.type;
            this.combo = other.combo;
            this.toto = other.toto;
            this.stakeAmount = other.stakeAmount;
            this.odds = other.odds;
            this.amount = other.amount;
            this.limit = other.limit;
            this.action = other.action;
            this.status = other.status;
            this.timestamp = other.timestamp;
            this.remark = other.remark;
        }
    }
}
