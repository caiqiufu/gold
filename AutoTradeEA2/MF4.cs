
using System.ComponentModel;
using System.Media;
using System.Windows.Forms;
using uClient.Comm;

namespace uClient.Broker
{
    public class MF4 : Comm.MF4
    {
        /// <summary>
        /// 策略配置
        /// </summary>
        public new EAStrategyConfig _StrategyConfig { set; get; }
        public MF4(Config config, MyConfig myConfig, Comm.Broker broker, Account account, string tradeSymbol, Log log, BackgroundWorker bgwBroberQuote, BackgroundWorker bgwOrderUpdate, DataGridView gvPositions, QuotaDisplayPanel quotaDisplayPanel, AccountDisplay accountDisplay)
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
                Comm.Utils.UpdateQuoteDisplay(_QuotaDisplayPanel, _QuoteData, _Quote.BidPrice, _Quote.AskPrice);
            }

        }
        /// <summary>
        /// 更新帐户资金信息
        /// </summary>
        public override void UpdateAccountCaptial()
        {
            if (isConnect())
            {
                _AccountDisplay.lblBlance.Text = _Client.AccountInfo.Balance.ToString();
            }
            else
            {
                _AccountDisplay.lblBlance.Text = "8888.88";
            }
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public override void DisConnect()
        {
            if (isConnect())
            {
                _IsManullyDisconnect = true;
                _Client.Logout();
            }
            SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\disconnect.wav");
            player.Play();
            _Log.LogInfo("DisConnect");
        }
    }
}
