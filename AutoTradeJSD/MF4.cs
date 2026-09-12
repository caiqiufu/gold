using M4.Client;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using uClient.Comm;

namespace uClient.Broker
{
    public class MF4 : Comm.MF4
    {
        //平台自有参数
        /// <summary>
        /// Client 版本号: 1.0.0.1400
        /// </summary>
        public Client Client { set; get; }

        //: base() 在 C# 中，base() 关键字通常用于在派生类的构造函数中调用其基类的构造函数。它有助于确保基类的成员能够正确初始化
        public MF4(Config config, MyConfig myConfig, uClient.Comm.Broker broker, uClient.Comm.Account account, string tradeSymbol, uClient.Comm.Log log, BackgroundWorker bgwBroberQuote, BackgroundWorker bgwOrderUpdate, DataGridView gvPositions, QuotaDisplayPanel quotaDisplayPanel, AccountDisplay accountDisplay) 
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
        /// <summary>
        /// 由于新老版本不一致,老版本需要覆盖父类方法
        /// </summary>
        /// <returns></returns>
        public override IList<M4.Common.Classes.Position> myGetPosition()
        {
            IList<M4.Common.Classes.Position> orders = null;
            try
            {
                if (isConnect())
                {
                    orders = _Client.OpenPositions;
                }
                else
                {
                    _Log.LogInfo("服务器未连接，不能获取持仓信息");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                _Log.LogInfo(ex.Message);
            }
            return orders;
        }
    }
}
