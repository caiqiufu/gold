using System.Text.Json;

namespace AutoHorseRace
{
    /// <summary>
    /// 账户信息
    /// </summary>
    public class Account
    {

        /// <summary>
        /// 交易商
        /// </summary>
        public string BrokerCode { set; get; } = string.Empty;
        /// <summary>
        /// 交易商名称
        /// </summary>
        public string BrokerName { set; get; } = string.Empty;
        /// <summary>
        /// 交易商服务器地址
        /// </summary>
        public string BrokerServer { set; get; } = string.Empty;
        /// <summary>
        /// 账户类型 Demo/Live
        /// </summary>
        public string Type { set; get; } = string.Empty;
        /// <summary>
        ///  帐号
        /// </summary>
        public string UserCode { set; get; } = string.Empty;

        /// <summary>
        ///  密码
        /// </summary>
        public string Password { set; get; } = string.Empty;
        /// <summary>
        /// Pin
        /// </summary>
        public string Pin { set; get; } = string.Empty;
        /// <summary>
        /// 平台下的交易参数
        /// </summary>
        public TradeParametre TradePara { set; get; } = new TradeParametre();
        /// <summary>
        /// 账户盈亏
        /// </summary>
        public string ProfitAndLoss { set; get; }
        /// <summary>
        /// 账户信用度
        /// </summary>
        public string AccountCredit { set; get; }
        /// <summary>
        /// 账户是否已登录
        /// </summary>
        public bool IsLogin { set; get; } = false;

        public override string ToString()
        {
            return "BrokerCode=" + BrokerCode + "," + "Type=" + Type + "," + "UserCode=" + UserCode + "," + "Password=" + Password + ",TradeParametre=" + JsonSerializer.Serialize(TradePara, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
