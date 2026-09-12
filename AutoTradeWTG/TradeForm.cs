using uClient;
using uClient.Comm;

namespace WTG.Broker
{
    public partial class TradeForm : uClient.TradeForm
    {
        //MF4 平台 beign
        public new MF4 _MF4;

        public TradeForm(MainFormPara mainFormPara) : base()
        {
            if (mainFormPara == null)
            {
                mainFormPara = new MainFormPara();
                mainFormPara.PlatformCode = "MF4";
                mainFormPara.BrokerCode = "EPM";
                mainFormPara.BrokerName = "环球贸易";
                mainFormPara.FormType = "MF4";
                mainFormPara.FormNo = "";
            }
            _MainFormPara = mainFormPara;
            CheckForIllegalCrossThreadCalls = false;
        }
        /// <summary>
        /// Client 中针对不同的平台商配置了不同的验证程序，该方法重载后，才能加载Client中的配置文件
        /// </summary>
        public override void GeneratePlatformObj()
        {
            switch (_Platform.PlatformNo)
            {
                case (int)uClient.Comm.Enum.Platform.MF4:
                    _MF4 = new WTG.Broker.MF4(_Config, _MyConfig, _Broker, _Account, _TradeSymbol, _Log, bgwBroberQuote, bgwOrderUpdate, base.gvPositions, quotaDisplayPanel, accountDisplay);
                    _MF4._QuoteData = new QuotePanelData(_TradeSymbol, 0.1);
                    _MF4._DSQuote = _DSQuote;
                    _MF4._StrategyConfig = _StrategyConfig;
                    base._MF4 = _MF4;
                    break;
                case (int)uClient.Comm.Enum.Platform.MT4:
                    _MT4 = new MT4(_Config, _MyConfig, _Broker, _Account, _TradeSymbol, _Log, bgwBroberQuote, bgwOrderUpdate, base.gvPositions, quotaDisplayPanel, accountDisplay);
                    _MT4._QuoteData = new QuotePanelData(_TradeSymbol, 0.1);
                    _MT4._DSQuote = _DSQuote;
                    _MT4._StrategyConfig = _StrategyConfig;
                    base._MT4 = _MT4;
                    break;
                case (int)uClient.Comm.Enum.Platform.V4:
                    _V4 = new uClient.V4(_Config, _MyConfig, _Broker, _Account, _TradeSymbol, _Log, bgwBroberQuote, bgwOrderUpdate, base.gvPositions, quotaDisplayPanel, accountDisplay);
                    _V4._QuoteData = new QuotePanelData(_TradeSymbol, 0.1);
                    _V4._DSQuote = _DSQuote;
                    _V4._StrategyConfig = _StrategyConfig;
                    base._V4 = _V4;
                    break;
            }
        }
    }
}
