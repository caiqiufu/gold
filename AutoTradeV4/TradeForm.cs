
using uClient.Comm;

namespace uClient.Broker
{
    public partial class TradeForm : uClient.TradeForm
    {
        
        public TradeForm(MainFormPara mainFormPara) : base()
        {
            if (mainFormPara == null)
            {
                mainFormPara = new MainFormPara();
                mainFormPara.PlatformCode = "V4";
                mainFormPara.BrokerCode = "YSG";
                mainFormPara.BrokerName = "佑生";
                mainFormPara.FormType = "V4";
                mainFormPara.FormNo = "V4_YSG";
            }
            _MainFormPara = mainFormPara;
            CheckForIllegalCrossThreadCalls = false;
        }
    }
}
