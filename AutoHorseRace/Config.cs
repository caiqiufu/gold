using Newtonsoft.Json;
using System.Globalization;

namespace AutoHorseRace
{
    /// <summary>
    /// 全局配置文件
    /// </summary>
    public class Config
    {
        /// <summary>
        ///EA信息地址,包括写入/获取EA策略,自动交易信息,系统配置信息,回写交易日志信息
        /// </summary>
        public string EAInfoAddress = string.Empty;

        /// <summary>
        ///EA服务地址
        /// </summary>
        public string EAServerAddress = string.Empty;

        /// <summary>
        /// 读水号地址
        /// </summary>
        public string DSServerAddress = string.Empty;

        /// <summary>
        /// 默认数据库地址
        /// </summary>
        public string DefaultDBAddress = string.Empty;

        /// <summary>
        /// 默认的当前赛马类型 
        /// H / 3H：通常代表 Hong Kong (香港) 赛区或特定的本地盘口分类。
        /// S / 3S：通常代表 Singapore(新加坡) 赛区或沙地（Sand）赛事。
        /// M / 3M：通常代表 Malaysia(马来西亚) 赛区。
        /// </summary>
        public string CurrentRaceTypeDesc = string.Empty;

        /// <summary>
        /// 当前赛马类型
        /// </summary>
        public string CurrentRaceType = string.Empty;

        /// <summary>
        /// 默认当前赛马场次
        /// </summary>
        public string CurrentRaceNo = string.Empty;

        /// <summary>
        /// 当前的开赛时间,后台返回值,例如: 09:40am 或者03:40pm
        /// </summary>
        private string _currentRaceTimeDesc = string.Empty;

        public string CurrentRaceTimeDesc
        {
            get => _currentRaceTimeDesc;
            set
            {
                _currentRaceTimeDesc = value;
                if (!string.IsNullOrEmpty(_currentRaceTimeDesc) &&
                    DateTime.TryParseExact(_currentRaceTimeDesc.Trim(), "hh:mmtt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTime))
                {
                    CurrentRaceTime = parsedTime.ToString("HH:mm");
                }
                else
                {
                    CurrentRaceTime = string.Empty;
                }
            }
        }

        /// <summary>
        /// 当前的开赛时间,配置在数据库,格式为HH:mm,例如: 09:40am 或者03:40pm
        /// </summary>
        public string CurrentRaceTime = string.Empty;

        /// <summary>
        /// 当前的开赛日期,配置在数据库,格式为dd-MM-yyyy,例如: 17-07-2026
        /// </summary>
        public string CurrentRaceDate = string.Empty;

        /// <summary>
        /// 自动交易在开赛前多长时间开赛，单位分钟
        /// </summary>
        public int AutoTradeStartTimeDuration = 30;

        /// <summary>
        /// 自动交易在开赛前多长时间结束，单位秒
        /// </summary>
        public int AutoTradeEndTimeDuration = 30;

        /// <summary>
        /// 自动交易时间间隔，单位秒
        /// </summary>
        public int AutoTradeInterval = 30;

        /// <summary>
        /// 是否自动跳转到下一场比赛,默认false,表示不自动跳转
        /// </summary>
        public bool AutoToNext = false;

        /// <summary>
        /// 是否自动删除未成交订单,默认false,表示不自动删除
        /// </summary>
        public bool AutoDeletePendingOrder = false;

        /// <summary>
        /// 自动下吃注
        /// </summary>
        public bool AutoClosePosition = false;

        /// <summary>
        /// 自动刷新下注,默认false,表示不自动刷新
        /// </summary>
        public bool AutoRefreshBetting = false;
        /// <summary>
        /// Q 自动下注标志,默认false,表示不自动下注
        /// </summary>
        public bool QAutoBettingFlag = false;
        /// <summary>
        /// Q下注金额,默认10,表示每次下注10元
        /// </summary>
        public int QStakeAmount = 10;
        /// <summary>
        /// 下注水折起始位
        /// </summary>
        public int QStartOdds = 0;

        /// <summary>
        /// 下注水折截至位
        /// </summary>
        public int QEndOdds = 0;
        /// <summary>
        /// 最小限额数量
        /// </summary>
        public int QMinLimit = 0;

        /// <summary>
        /// QP 自动下注标志,默认false,表示不自动下注
        /// </summary>
        public bool QPAutoBettingFlag = false;
        /// <summary>
        /// Q下注金额,默认10,表示每次下注10元
        /// </summary>
        public int QPStakeAmount = 10;
        /// <summary>
        /// 下注水折起始位
        /// </summary>
        public int QPStartOdds = 0;

        /// <summary>
        /// 下注水折截至位
        /// </summary>
        public int QPEndOdds = 0;
        /// <summary>
        /// 最小限额数量
        /// </summary>
        public int QPMinLimit = 0;
        /// <summary>
        /// Q赌票水折差
        /// </summary>
        public int QBetSpread = 0;
        /// <summary>
        /// QP赌票水折差
        /// </summary>
        public int QPBetSpread = 0;

        /// <summary>
        /// Q吃票水折差
        /// </summary>
        public int QEatSpread = 0;

        /// <summary>
        /// QP吃票水折差
        /// </summary>
        public int QPEatSpread = 0;

        /// <summary>
        /// 当天的赛事信息列表,从服务器读取数据
        /// </summary>
        public List<RaceInfo> RaceInfoList = new List<RaceInfo>();

        public RaceInfo CurrentRaceInfo = new RaceInfo();

        /// <summary>
        /// 系统配置参数,从服务器读取数据
        /// </summary>
        public Dictionary<String, String> SysConfig = new Dictionary<String, String>();

        /// <summary>
        /// Q允许的下注马位,1-2,1-3,1-5
        /// </summary>
        public string QCombos = string.Empty;
        /// <summary>
        /// QP允许的下注马位,1-2,1-3,1-5
        /// </summary>
        public string QPCombos = string.Empty;
        /// <summary>
        /// Q满足下注条件但是票数小于该值时就降低一个水位下注
        /// </summary>
        public int QLowerThreshold = 0;

        /// <summary>
        /// QP满足下注条件但是票数小于该值时就降低一个水位下注
        /// </summary>
        public int QPLowerThreshold = 0;

        // 删除：public ConcurrentDictionary<string, EatBetInfo> LocalAllOrders { get; } = new ConcurrentDictionary<string, EatBetInfo>();

        [JsonIgnore]  // 防止 SaveConfig()/LoadConfigFile() 里的 JsonConvert 序列化/反序列化尝试处理这个运行时对象
        public TradeStateStore TradeStateStore { get; } = new TradeStateStore();
    }
}
