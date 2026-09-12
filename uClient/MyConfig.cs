
namespace uClient.Comm
{
    public class MyConfig
    {
        public uClient.Comm.Account[] Account { set; get; }
        /// <summary>
        /// 平台列表,Key = BrokerCode_Type
        /// </summary>
        //public Dictionary<String, Account> AccountList = new Dictionary<String, Account>();

        ///通知号码
        public string[] NotifyNumber { set; get; }

        ///通知邮件
        public string[] NotifyEmail { set; get; }

        /// <summary>
        /// 特殊号码,需要特殊处理逻辑
        /// </summary>
        public string[] SpecialNumber  { set; get; }
        /// <summary>
        /// 回测按钮权限
        /// </summary>
        public bool Permission_Button_SimulatorTest { set; get;}        
    }
}
