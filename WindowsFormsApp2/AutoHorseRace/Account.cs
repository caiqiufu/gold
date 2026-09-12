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
        public required string BrokerCode { set; get; }
        /// <summary>
        /// 交易商名称
        /// </summary>
        public required string BrokerName { set; get; }
        /// <summary>
        /// 账户类型 Demo/Live
        /// </summary>
        public required string Type { set; get; }
        /// <summary>
        ///  帐号
        /// </summary>
        public required string UserCode { set; get; }

        /// <summary>
        ///  密码
        /// </summary>
        public required string Password { set; get; }
        /// <summary>
        /// Pin
        /// </summary>
        public required string Pin { set; get; }
        /// <summary>
        /// 平台下的交易参数
        /// </summary>
        public required TradeParametre TradePara { set; get; }

        public string toString()
        {
            return "BrokerCode=" + BrokerCode + "," + "Type=" + Type + "," + "UserCode=" + UserCode + "," + "Password=" + Password + ",TradeParametre=" + JsonSerializer.Serialize(TradePara, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
