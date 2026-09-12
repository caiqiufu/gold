
namespace uClient.Comm
{
    public class TradeParametre
    {
        public decimal SellOpen { set; get; }
        public decimal BuyOpen { set; get; }
        public decimal SellClose { set; get; }
        public decimal BuyClose { set; get; }
        public decimal SellLots { set; get; }
        public decimal BuyLots { set; get; }
        public int Slippage { set; get; }
        public decimal Stoploss { set; get; }
        /// <summary>
        /// 是否自动锁单
        /// </summary>
        public bool AutoLock { set; get; }
        /// <summary>
        /// 独立锁单，勾选后该窗口会自动根据配置时间生成锁单
        /// </summary>
        public bool IndLockFlag { set; get; }
        /// <summary>
        /// 该窗口是否只作为锁单平仓窗口，不开单
        /// </summary>
        public bool OnlyCloseFlag { set; get; }
        /// <summary>
        /// 锁单窗口
        /// </summary>
        public string AutoLockForm { set; get; }
        /// <summary>
        /// 锁单点数
        /// </summary>
        public decimal AutoLockPoint { set; get; }
        /// <summary>
        /// 锁单时间
        /// </summary>
        public decimal AutoLockTimeDuration { set; get; }
        /// <summary>
        /// 平仓时间
        /// </summary>
        public string AutoCloseTimeDuration { set; get; }
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
        public string NotifyType { set; get; }
        /// <summary>
        /// 平台服务器所在时区，默认为UTC+8
        /// </summary>
        public double Timezone { set; get; }

        /// <summary>
        /// 自动加仓
        /// </summary>
        public bool IncreaseFlag { set; get; }

        /// <summary>
        /// 自动加仓手数
        /// </summary>
        public decimal IncreaseLots { set; get; }

        /// <summary>
        /// 自动交易,启动交易窗口后,如果该值为true,则执行自动交易
        /// </summary>
        public bool AutoTrade { set; get; }

        /// <summary>
        /// 价格检查
        /// </summary>
        public bool QuotaCheckFlag { set; get; }
        /// <summary>
        /// 价格检查值差
        /// </summary>
        public decimal QuotaCheckValue { set; get; }
        /// <summary>
        /// 事件单
        /// </summary>
        public bool EventFlag { set; get; }
        /// <summary>
        /// 事件策略配置参数
        /// 时间间隔(s),跳次大小,跳次总数,正跳次数:3,3,10,6
        /// </summary>
        public string EventConfig { set; get; }

        /// <summary>
        /// EA策略开平仓
        /// </summary>
        public bool EAFlag { set; get; }
    }
}
