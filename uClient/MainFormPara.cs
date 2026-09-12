
namespace uClient.Comm
{
    public class MainFormPara
    {
        /// <summary>
        /// Form编号 PlatformCode_BrokerCode
        /// </summary>
        public string FormNo { set; get; }
        /// <summary>
        /// 窗口名称
        /// </summary>
        public string FormName { set; get; }
        /// <summary>
        /// 窗口类型 （有效值: <see cref="MF4,V4,MT4"/>）
        /// </summary>
        public string FormType { set; get; }
        /// <summary>
        /// 平台类型（有效值: <see cref="MF4,V4,MT4"/>）
        /// </summary>
        public string PlatformCode { set; get; }
        /// <summary>
        /// 交易商编码
        /// </summary>
        public string BrokerCode { set; get; }
        /// <summary>
        /// 交易商名称
        /// </summary>
        public string BrokerName { set; get; }

        public string toString() {
            return "FormNo=" + FormNo + "," + "FormName=" + FormName + "," + "FormType=" + FormType + "," + "PlatformCode=" + PlatformCode + "," + "BrokerCode=" + BrokerCode + "," + "BrokerName=" + BrokerName + ",";
        }
    }
}
