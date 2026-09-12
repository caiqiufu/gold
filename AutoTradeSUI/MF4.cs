using M4.Common;
using M4.Common.Classes;
using M4.Common.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Media;
using System.Threading;
using System.Windows.Forms;
using uClient.Comm;
using Log = uClient.Comm.Log;

//同一个平台的程序放到同一个命名空间下
namespace uClient.Broker
{
    public class MF4 : uClient.Comm.MF4
    {
        //平台自有参数
        /// <summary>
        /// Client 版本号: 1.0.0.1352
        /// </summary>
        public Client Client { set; get; }
        public MF4(Config config, MyConfig myConfig, Comm.Broker broker, Comm.Account account, string tradeSymbol, Log log, BackgroundWorker bgwBroberQuote, BackgroundWorker bgwOrderUpdate, DataGridView gvPositions, QuotaDisplayPanel quotaDisplayPanel, AccountDisplay accountDisplay) : base()
        {
            this.Client = new Client();
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
