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
                mainFormPara.PlatformCode = "MT4";
                mainFormPara.BrokerCode = "";
                mainFormPara.BrokerName = "";
                mainFormPara.FormType = "MT4";
                mainFormPara.FormNo = "";
            }
            _MainFormPara = mainFormPara;
            CheckForIllegalCrossThreadCalls = false;
        }
    }
}

