using System.ComponentModel;
using System.Windows.Forms;
using uClient;
using uClient.Comm;

namespace WTG.Broker
{
    public class MF4 : uClient.Comm.MF4
    {
        //平台自有参数
        /// <summary>
        /// Client
        /// </summary>
        public WTG.Broker.Client Client { set; get; }
        public MF4(Config config, MyConfig myConfig, uClient.Comm.Broker broker, Account account, string tradeSymbol, Log log, BackgroundWorker bgwBroberQuote, BackgroundWorker bgwOrderUpdate, DataGridView gvPositions,QuotaDisplayPanel quotaDisplayPanel, AccountDisplay accountDisplay) : base()
        {
            this.Client = new WTG.Broker.Client();
            base._Client = this.Client;
            this._Config = config;
            this._MyConfig = myConfig;
            this._Broker = broker;
            this._Account = account;
            this._TradeSymbol = tradeSymbol;
            this._Log = log;
            this._BgwBroberQuote = bgwBroberQuote;
            this._BgwOrderUpdate = bgwOrderUpdate;
            this._GvPositions = gvPositions;
            this._QuotaDisplayPanel = quotaDisplayPanel;
            this._AccountDisplay = accountDisplay;
        }
    }
}
