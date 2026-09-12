using mtapi.mt5;
using System;
using System.ComponentModel;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using uClient.Comm;

namespace uClient.Broker
{
    public class MT5 : uClient.Comm.MT5
    {
        public MT5(Config config, MyConfig myConfig, Comm.Broker broker, Account account, string tradeSymbol, Log log, BackgroundWorker bgwBroberQuote, BackgroundWorker bgwOrderUpdate, DataGridView gvPositions, QuotaDisplayPanel quotaDisplayPanel, AccountDisplay accountDisplay) : base()
        {
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
        /// <summary>
        /// 平台报价更新
        /// </summary>
        public override void UpdateQuote()
        {
            if (isConnect() && _Quote != null)
            {
                Comm.Utils.UpdateQuoteDisplay(_QuotaDisplayPanel, _QuoteData, (decimal)_Quote.Bid, (decimal)_Quote.Ask);
            }
        }
        /// <summary>
        /// 更新帐户资金信息
        /// </summary>
        public override void UpdateAccountCaptial()
        {
            if (isConnect())
            {
                _AccountDisplay.lblBlance.Text = _QC.Account.Balance.ToString();
            }
            else
            {
                _AccountDisplay.lblBlance.Text = "8888.88";
            }
        }
    }
}
