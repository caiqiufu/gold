namespace AutoHorseRace
{
    /// <summary>
    /// 赛事信息
    /// </summary>
    public class RaceInfo
    {

        /// <summary>
        /// 下注信息的唯一标识符
        /// </summary>
        public string seq { set; get; } = string.Empty;

        /// <summary>
        /// 赛事国家
        /// </summary>
        public string country { set; get; } = string.Empty;

        /// <summary>
        /// 赛事名称
        /// </summary>
        public string name { set; get; } = string.Empty;

        /// <summary>
        /// 赛事显示名称
        /// </summary>
        public string displayName { set; get; } = string.Empty;

        /// <summary>
        /// 赛事分类
        /// </summary>
        public string category { set; get; } = string.Empty;

        /// <summary>
        /// 场次日期
        /// </summary>
        public string raceDate { set; get; } = string.Empty;

        /// <summary>
        /// 场次类型 赛马大盘类型
        /// </summary>
        public string raceType { set; get; } = string.Empty;

        /// <summary>
        /// 默认的当前赛马场次
        /// </summary>
        public string defaultRc { set; get; } = string.Empty;

        /// <summary>
        /// 可以选择的赛马场次
        /// </summary>
        public List<IDictionary<string, object>> raceAvailableRaces { get; set; } = new List<IDictionary<string, object>>();

        /// <summary>
        ///当前赛事时间信息
        /// </summary>
        public string currRaceTimeInfo { set; get; } = string.Empty;
    }
}
