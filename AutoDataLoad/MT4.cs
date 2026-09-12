using System.ComponentModel;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using TradingAPI.MT4Server;
using uClient.Comm;

namespace uClient.Broker
{
    public class MT4 : Comm.MT4
    {

        public MT4(Config config, MyConfig myConfig, Comm.Broker broker, Account account, string tradeSymbol, Log log, BackgroundWorker bgwBroberQuote, BackgroundWorker bgwOrderUpdate, DataGridView gvPositions, QuotaDisplayPanel quotaDisplayPanel, AccountDisplay accountDisplay)
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
                _AccountDisplay.lblBlance.Text = _QC.AccountBalance.ToString();
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
            if (_QC != null && _QC.Connected)
            {
                _IsManullyDisconnect = true;
                _QC.Disconnect();
            }
            SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\disconnect.wav");
            player.Play();
        }
        public override void QCOnDisconnect(object sender, DisconnectEventArgs args)
        {
            _QuoteData.Clear();
            if (_IsManullyDisconnect)
            {
                _Log.LogInfo("手动断开连接");
            }
            else
            {
                _Log.LogInfo("系统自动断开连接");
                this._QC = null;
                Task.Run(() =>
                {
                    //if (_Account.TradePara.NotifyFlag)
                    //{
                    //string result = HttpClient.SendSMS(_MyConfig.NotifyNumber, "[" + _Account.UserCode + "]自动断开连接,请及时处理", "", " ", " ", " ", " ", " ");
                    //    EmailHelper.SendEmail(_MyConfig.NotifyEmail, "[" + _Account.UserCode + "]自动断开连接,请及时处理", "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "][" + _Account.UserCode + "]自动断开连接,请及时处理");
                    //}
                    //EmailHelper.SendEmail(_MyConfig.NotifyEmail, "[" + _Account.UserCode + "]自动断开连接,请及时处理", "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "][" + _Account.UserCode + "]自动断开连接,请及时处理");
                });
                SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\disconnect.wav");
                player.Play();
            }
        }
    }
}
