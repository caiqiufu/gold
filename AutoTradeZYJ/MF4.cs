using System.ComponentModel;
using System.Windows.Forms;
using uClient.Comm;
namespace uClient.Broker
{
    public class MF4 : uClient.Comm.MF4
    {
        //平台自有参数
        /// <summary>
        /// Client
        /// </summary>
        public uClient.Broker.Client Client { set; get; }
        public MF4(Config config, MyConfig myConfig, Comm.Broker broker, Comm.Account account, string tradeSymbol, Log log, BackgroundWorker bgwBroberQuote, BackgroundWorker bgwOrderUpdate, DataGridView gvPositions, QuotaDisplayPanel quotaDisplayPanel, AccountDisplay accountDisplay) : base()
        {
            this.Client = new uClient.Broker.Client();
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
