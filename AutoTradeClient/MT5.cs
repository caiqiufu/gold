using mtapi.mt5;
using System;
using System.ComponentModel;
using System.Media;
using System.Threading;
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
        /// 黄金白银变化率订单操作
        /// 1)根据指定的symbolType开仓
        /// 2)所有订单全部同时平仓
        /// </summary>
        /// <param name="symbolType">_GOLD/_SILVER</param>
        /// <param name="tradeType"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="lotRatio"></param>
        /// <param name="_SendTime"></param>
        /// <returns></returns>
        public bool AutoTradeTransaction4(string symbolType, int tradeType, bool _IsAutoOperationFlag, double lotRatio, out DateTime _SendTime)
        {
            bool mexecutedFlag = false;
            _SendTime = DateTime.Now;
            decimal goldLots = _Broker.TradePara.BuyLots;
            decimal silverLots = Math.Round(_Broker.TradePara.BuyLots * Convert.ToDecimal(lotRatio), 2);
            //var quote = _QC.GetQuote(_TradeSymbol);
            switch (tradeType)
            {
                case 1:
                    if (string.Equals(_GOLD, symbolType))
                    {
                        OpenGSOrder(_IsAutoOperationFlag, goldLots, _GOLD, _GOLD_ASK_PRICE, "BUY", out _SendTime);
                    }
                    if (string.Equals(_SILVER, symbolType))
                    {
                        OpenGSOrder(_IsAutoOperationFlag, silverLots, _SILVER, _SILVER_ASK_PRICE, "SELL", out _SendTime);
                    }

                    mexecutedFlag = true;
                    break;
                case 2:
                    if (string.Equals(_GOLD, symbolType))
                    {
                        OpenGSOrder(_IsAutoOperationFlag, goldLots, _GOLD, _GOLD_BID_PRICE, "SELL", out _SendTime);
                    }
                    if (string.Equals(_SILVER, symbolType))
                    {
                        OpenGSOrder(_IsAutoOperationFlag, silverLots, _SILVER, _SILVER_BID_PRICE, "BUY", out _SendTime);
                    }
                    mexecutedFlag = true;
                    break;
                case 3:
                    //自动平仓
                    Order[] orders1 = myGetPosition();
                    if (orders1 != null && orders1.Length > 0)
                    {
                        foreach (var order in orders1)
                        {
                            string symbol = order.Symbol;
                            if (string.Equals(symbol, _GOLD))
                            {
                                CloseGSOrder(order, _IsAutoOperationFlag, _GOLD_BID_PRICE, _GOLD_ASK_PRICE, out _SendTime);
                            }
                            if (string.Equals(symbol, _SILVER))
                            {
                                CloseGSOrder(order, _IsAutoOperationFlag, _SILVER_BID_PRICE, _SILVER_ASK_PRICE, out _SendTime);
                            }
                            mexecutedFlag = true;
                            Thread.Sleep(3);
                        }
                    }
                    break;
                default:
                    _Log.LogInfo("订单类型[" + tradeType + "]未定义");
                    break;
            }
            return mexecutedFlag;
        }

        /// <summary>
        /// GS 开仓
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="lots"></param>
        /// <param name="symbol"></param>
        /// <param name="price"></param>
        /// <param name="tradeType">BUY|SELL</param>
        /// <param name="_SendTime"></param>
        public void OpenGSOrder(bool _IsAutoOperationFlag, decimal lots, string symbol, double price, string tradeType, out DateTime _SendTime)
        {
            _SendTime = DateTime.Now;
            try
            {
                //_Log.LogInfo("发送订单时间：" + DateTime.Now.ToString("HH:mm:ss.fff"));
                _QC.OrderSendAsync(1, symbol, (double)lots, price, string.Equals(tradeType, "BUY") ? OrderType.Buy : OrderType.Sell, 0, 0, ulong.Parse(_Broker.TradePara.Slippage.ToString()), fillPolicy: FillPolicy.Any);
            }
            catch (Exception ex)
            {
                string msg = string.Format("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[" + ((_IsAutoOperationFlag == true) ? "自动买多" : "手动买多") + "][发送]异常信息：{0}", ex.Message);
                _logger.Error(msg + "  " + ex.StackTrace);
                _Log.LogTradeRecord(_Account, msg);
                _Log.LogInfo(ex.Message);
            }
            string message = MessageFormatter.OrderSendMessage(_IsAutoOperationFlag, string.Equals(tradeType, "BUY") ? "BUY" : "SELL");
            if (_IsAutoOperationFlag)
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _DSQuote.BidDiff[0], _QC.Account.Balance.ToString()));
            }
            else
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _QC.Account.Balance.ToString()));
            }
        }

        /// <summary>
        /// GS 平仓
        /// </summary>
        /// <param name="order"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="bidPrice"></param>
        /// <param name="askPrice"></param>
        /// <param name="_SendTime"></param>
        public void CloseGSOrder(Order order, bool _IsAutoOperationFlag, double bidPrice, double askPrice, out DateTime _SendTime)
        {
            _SendTime = DateTime.Now;
            long ticket = order.Ticket;
            string symbol = order.Symbol;
            double lots = order.Lots;
            double price = order.OrderType == OrderType.Buy ? _Quote.Bid : _Quote.Ask;
            try
            {
                _QC.OrderCloseAsync(1, ticket, symbol, price, lots, order.OrderType, ulong.Parse(_Broker.TradePara.Slippage.ToString()));
            }
            catch (Exception ex)
            {
                string msg = string.Format("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[" + ((_IsAutoOperationFlag == true) ? "自动平仓" : "手动平仓") + "][发送]异常信息：{0}", ex.Message);
                _logger.Error(msg + "  " + ex.StackTrace);
                _Log.LogTradeRecord(_Account, msg);
                _Log.LogInfo(ex.Message);
            }
            string tradeType = order.OrderType == OrderType.Buy ? "BUY_CLOSE" : "SELL_CLOSE";
            string message = MessageFormatter.OrderSendMessage(_IsAutoOperationFlag, tradeType);
            if (_IsAutoOperationFlag)
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _DSQuote.BidDiff[0], _QC.Account.Balance.ToString()));
            }
            else
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _QC.Account.Balance.ToString()));
            }
        }
    }
}
