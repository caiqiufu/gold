namespace AutoHorseRace
{
    /// <summary>
    /// 赔率信息
    /// </summary>
    public class TotoInfo
    {

        /// <summary>
        /// 下注信息的唯一标识符
        /// </summary>
        public string seq { set; get; } = string.Empty;
        /// <summary>
        /// 场次
        /// </summary>
        public string race { set; get; } = string.Empty;

        /// <summary>
        /// 场次日期
        /// </summary>
        public string raceDate { set; get; } = string.Empty;

        /// <summary>
        /// 场次类型
        /// </summary>
        public string raceType { set; get; } = string.Empty;

        /// <summary>
        /// 类型，连赢Q/位置PQ
        /// </summary>
        public string type { set; get; } = string.Empty;

        /// <summary>
        /// 组合
        /// </summary>
        public string combo { set; get; } = string.Empty;

        /// <summary>
        /// 彩池赔率
        /// </summary>
        public double toto { set; get; }

        /// <summary>
        /// 热度
        /// </summary>
        public double level { set; get; }

        /// <summary>
        /// 赔率键值，方便查找
        /// </summary>
        public string key { set; get; } = string.Empty;

    }
}
