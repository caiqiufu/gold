using NLog;
using System;
using System.Collections.Generic;
using System.Media;
using System.Windows.Forms;
using uClient.Comm;
using V4;
using System.ComponentModel;
using Quote = uClient.Comm.Quote;
using Position = uClient.Comm.Position;
using System.Threading;
using System.Xml;
using System.Threading.Tasks;
using NetMQ.Sockets;
using NetMQ;

namespace uClient.Broker
{
    public class V4 : uClient.V4
    {
        public uClient.Broker.V4ClientYGS ClientYGS { set; get; }
        public uClient.Broker.V4ClientWFB ClientWFB { set; get; }
        public V4(Config config, uClient.Comm.Broker broker, Comm.Account account, string tradeSymbol, Log log, BackgroundWorker bgwBroberQuote, BackgroundWorker bgwOrderUpdate, DataGridView gvPositions, QuotaDisplayPanel quotaDisplayPanel, AccountDisplay accountDisplay) : base()
        {
            this._Client = new V4Client(gvPositions);
            this._Config = config;
            this._Broker = broker;
            this._Account = account;
            this._TradeSymbol = tradeSymbol;
            this._Log = log;
            this._BgwBroberQuote = bgwBroberQuote;
            this._BgwOrderUpdate = bgwOrderUpdate;
            this._GvPositions = gvPositions;
            this._QuotaDisplayPanel = quotaDisplayPanel;
            this._AccountDisplay = accountDisplay;
            this._ConnectFlag = false;
        }
    }
}
