using NLog;
using System;
using System.ComponentModel;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TradingAPI.MT4Server;
using uClient.Comm;

namespace uClient.Broker
{
    public class MT4 : Comm.MT4
    {
        private NLog.Logger _logger = LogManager.GetCurrentClassLogger();
            
        /// <summary>
        /// 当前开单价格
        /// </summary>
        public double _CurrentPrice = 0;

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

        public MT4()
        {
        }
        /// <summary>
        /// 更新账户余额
        /// </summary>
        public void UpdateAccountBalance()
        {
            //更新账户余额
            DBHelper.updateAccountBalance(_Broker.BrokerCode, _Account.UserCode, Math.Round(_QC.AccountBalance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin);
        }

        /// <summary>
        /// EA2的公共交易参数，作为全局变量保存
        /// </summary>
        public string _EA2_TradeRecord = "";
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
                _OC.OrderSendAsync(symbol, string.Equals(tradeType, "BUY") ? Op.Buy : Op.Sell, (double)lots, price, _Broker.TradePara.Slippage, 0, 0, "", 0, new DateTime());
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
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _DSQuote.BidDiff[0], _QC.AccountBalance.ToString()));
            }
            else
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _QC.AccountBalance.ToString()));
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
            int ticket = order.Ticket;
            string symbol = order.Symbol;
            double lots = order.Lots;
            double price = order.Type == Op.Buy ? bidPrice : askPrice;
            try
            {
                //_Log.LogInfo("发送订单时间：" + DateTime.Now.ToString("HH:mm:ss.fff"));
                _OC.OrderCloseAsync(symbol, ticket, lots, price, _Broker.TradePara.Slippage);
            }
            catch (Exception ex)
            {
                string msg = string.Format("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[" + ((_IsAutoOperationFlag == true) ? "自动平仓" : "手动平仓") + "][发送]异常信息：{0}", ex.Message);
                _logger.Error(msg + "  " + ex.StackTrace);
                _Log.LogTradeRecord(_Account, msg);
                _Log.LogInfo(ex.Message);
            }
            string tradeType = order.Type == Op.Buy ? "BUY_CLOSE" : "SELL_CLOSE";
            string message = MessageFormatter.OrderSendMessage(_IsAutoOperationFlag, tradeType);
            if (_IsAutoOperationFlag)
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _DSQuote.BidDiff[0], _QC.AccountBalance.ToString()));
            }
            else
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _QC.AccountBalance.ToString()));
            }
        }
        /// <summary>
        /// 自动加仓开单,不判断是否有单存在
        /// </summary>
        /// <param name="tradeType"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        /// <returns></returns>
        public bool AutoTradeTransaction3(int tradeType, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            bool mexecutedFlag = false;
            _SendTime = DateTime.Now;
            switch (tradeType)
            {
                case 1:
                    OpenBuyOrder(_IsAutoOperationFlag, out _SendTime, _Broker.TradePara.BuyLots);
                    mexecutedFlag = true;
                    break;
                case 2:
                    OpenSellOrder(_IsAutoOperationFlag, out _SendTime, _Broker.TradePara.SellLots);
                    mexecutedFlag = true;
                    break;
                case 3:
                    //自动平仓
                    TradingAPI.MT4Server.Order[] orders1 = myGetPosition();
                    if (orders1 != null && orders1.Length > 0)
                    {
                        foreach (var order in orders1)
                        {
                            if (order.Type == Op.Buy)
                            {
                                CloseOrder(order, _IsAutoOperationFlag, out _SendTime);
                                mexecutedFlag = true;
                            }
                        }
                    }
                    break;
                case 4:
                    //自动平仓
                    TradingAPI.MT4Server.Order[] orders2 = myGetPosition();
                    if (orders2 != null && orders2.Length > 0)
                    {
                        foreach (var order in orders2)
                        {
                            if (order.Type == Op.Sell)
                            {
                                CloseOrder(order, _IsAutoOperationFlag, out _SendTime);
                                mexecutedFlag = true;
                            }
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
        public bool AutoTradeTransaction4(string symbolType,int tradeType, bool _IsAutoOperationFlag, double lotRatio, out DateTime _SendTime)
        {
            bool mexecutedFlag = false;
            _SendTime = DateTime.Now;
            decimal goldLots = _Account.TradePara.BuyLots;
            decimal silverLots = Math.Round(_Broker.TradePara.BuyLots * Convert.ToDecimal(lotRatio),2);
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
                            if (string.Equals(symbol,_GOLD))
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
        /// 订单状态更新
        /// </summary>
        /// <param name="orderProgress"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_AutoLock"></param>
        /// <param name="_SendTime"></param>
        public override void OrderUpdate(bool _IsAutoOperationFlag, DateTime _SendTime, string _LockOrderId)
        {
            executedFlag = false;
            SoundPlayer player;
            DateTime _ReceiveTime = DateTime.Now;
            Order order = _OrderProgressEventArgs.Order;
            long diffTimeDuration = Convert.ToInt32((_ReceiveTime - _SendTime).TotalMilliseconds);
            switch (_OrderProgressEventArgs.Type)
            {
                case TradingAPI.MT4Server.ProgressType.Accepted:
                    break;
                case TradingAPI.MT4Server.ProgressType.InProcess:
                    break;
                case TradingAPI.MT4Server.ProgressType.Opened:
                    string tradeType = "";
                    if (order.Type == Op.Buy)
                    {
                        tradeType = "BUY";
                    }
                    if (order.Type == Op.Sell)
                    {
                        tradeType = "SELL";
                    }
                    _Log.LogTradeRecord(_Account, string.Format(MessageFormatter.OrderReceiveMessage(_IsAutoOperationFlag, tradeType), _ReceiveTime.ToString("HH:mm:ss.fff"), order.OpenPrice, order.Lots, diffTimeDuration, _QC.AccountBalance.ToString()));
                    string msg = string.Format(MessageFormatter.OrderReceiveMessage(_IsAutoOperationFlag, tradeType), _ReceiveTime.ToString("HH:mm:ss.fff"), order.OpenPrice, order.Lots, diffTimeDuration, _QC.AccountBalance.ToString());
                    _Log.LogInfo(string.Format("[{0}][{1}][开仓][成交][{2}]手数:{3}", order.Symbol, order.Type, order.OpenPrice, order.Lots));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\ok.wav");
                    player.PlaySync();
                    Task.Run(() => {
                        //只有EA自动开单才记录该日志
                        if (!string.IsNullOrEmpty(_EA2_TradeRecord))
                        {
                            //orderTypeDetail:openPrice:closePrice:profit
                            string[] split = _EA2_TradeRecord.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            DBHelper.saveTradeLogMT4("MT4", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", order.Ticket.ToString(), _EA2_TradeRecord, tradeType, split[0], order.Lots, order.OpenPrice, Math.Round(Convert.ToDouble(split[2]), 2), 0, 0, Math.Round(_QC.AccountBalance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin, _LockOrderId);
                            _Log.LogInfo("保存交易日志完成");
                        }
                        else
                        {
                            DBHelper.saveTradeLogMT4("MT4", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", order.Ticket.ToString(), msg, tradeType, "", order.Lots, order.OpenPrice, -99, -99, 0, Math.Round(_QC.AccountBalance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin, _LockOrderId);
                            _Log.LogInfo("保存交易日志完成");
                        }
                        //order_type:BUY_CLOSE,volume:0.3,open_price:2155.28,close_price:2158.79
                        string content = "order_type:"+ tradeType + ",volume:"+ order.Lots + ",open_price:"+ order.OpenPrice + ",close_price:";
                        DBHelper.saveNotify("TR", order.Ticket.ToString(), content, "", _Account.UserCode);
                        //更新账户余额
                        UpdateAccountBalance();
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                                _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "开", order.Type.ToString(),
                                order.OpenPrice.ToString(), order.Lots.ToString(), _QC.AccountBalance.ToString(), "成交");
                        }
                    });                    
                    break;
                case TradingAPI.MT4Server.ProgressType.Closed:
                    string tradeType1 = order.Type == Op.Buy ? "BUY_CLOSE" : "SELL_CLOSE";
                    string msg1 = string.Format(MessageFormatter.OrderReceiveMessage(_IsAutoOperationFlag, tradeType1), _ReceiveTime.ToString("HH:mm:ss.fff"), order.ClosePrice, order.Lots, diffTimeDuration, _QC.AccountBalance.ToString());
                    _Log.LogTradeRecord(_Account, string.Format(MessageFormatter.OrderReceiveMessage(_IsAutoOperationFlag, tradeType1), _ReceiveTime.ToString("HH:mm:ss.fff"), order.ClosePrice, order.Lots, diffTimeDuration, _QC.AccountBalance.ToString()));
                    _Log.LogInfo(string.Format("[{0}][{1}][平仓][成交][{2}]手数:{3}盈亏：{4}", order.Symbol, order.Type, order.ClosePrice, order.Lots, order.Profit));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\ok.wav");
                    player.PlaySync();
                    Task.Run(() => {
                        double points = order.Type == Op.Sell || order.Type == Op.SellLimit ? order.OpenPrice - _Quote.Ask : _Quote.Bid - order.OpenPrice;
                        //只有EA自动开单才记录该日志
                        if (!string.IsNullOrEmpty(_EA2_TradeRecord))
                        {
                            //orderTypeDetail:openPrice:closePrice:profit
                            //由于开仓和平仓价格无法准确从交易客户端获取(有可能人为操作多个订单),所以profit数据取自服务端
                            string[] split = _EA2_TradeRecord.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);                      
                            DBHelper.saveTradeLogMT4("MT4", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", order.Ticket.ToString(), _EA2_TradeRecord, tradeType1, split[0], order.Lots, order.OpenPrice, order.ClosePrice, Math.Round(points, 2), Math.Round(order.Profit, 2), Math.Round(_QC.AccountBalance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin, _LockOrderId);
                            _Log.LogInfo("保存交易日志完成");
                        }
                        else
                        {
                            DBHelper.saveTradeLogMT4("MT4", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", order.Ticket.ToString(), msg1, tradeType1, "", order.Lots, order.OpenPrice, order.ClosePrice, Math.Round(points, 2), Math.Round(order.Profit, 2), Math.Round(_QC.AccountBalance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin, _LockOrderId);
                            _Log.LogInfo("保存交易日志完成");
                        }
                        //order_type:BUY_CLOSE,volume:0.3,open_price:2155.28,close_price:2158.79
                        string content = "order_type:" + tradeType1 + ",volume:" + order.Lots + ",open_price:" + _CurrentPrice + ",close_price:"+ order.ClosePrice;
                        DBHelper.saveNotify("TR", order.Ticket.ToString(), content, "", _Account.UserCode);
                        _CurrentPrice = 0;
                        //更新账户余额
                        UpdateAccountBalance();
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                                _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "平", order.Type.ToString(),
                                order.ClosePrice.ToString(), order.Lots.ToString(), _QC.AccountBalance.ToString(), "成交");
                        }
                    });
                   
                    break;
                case TradingAPI.MT4Server.ProgressType.Modified:
                    break;
                case TradingAPI.MT4Server.ProgressType.PendingDeleted:
                    break;
                case TradingAPI.MT4Server.ProgressType.ClosedBy:
                    break;
                case TradingAPI.MT4Server.ProgressType.MultipleClosedBy:
                    break;
                case TradingAPI.MT4Server.ProgressType.Price:
                    string msgFormt5 = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[接收][拒绝][{0}] 耗时:{1}ms";
                    _Log.LogTradeRecord(_Account, string.Format(msgFormt5, _OrderProgressEventArgs.ToString(), diffTimeDuration));
                    _Log.LogInfo(string.Format("[委托={0}][接收][Rejected][{1}[耗时:{2}ms]]", _OrderProgressEventArgs.TempID, _OrderProgressEventArgs.ToString(), diffTimeDuration));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\alert.wav");
                    player.PlaySync();
                    Task.Run(() => {
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                                _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "开", order.Type.ToString(),
                                "", order.Lots.ToString(), _QC.AccountBalance.ToString(), "订单被拒绝" + _OrderProgressEventArgs.ToString());
                        }
                    });
                    break;
                case TradingAPI.MT4Server.ProgressType.Rejected:
                    string msgFormt6 = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[接收][拒绝][{0}] 耗时:{1}ms";
                    _Log.LogTradeRecord(_Account, string.Format(msgFormt6, _OrderProgressEventArgs.ToString(), diffTimeDuration));
                    _Log.LogInfo(string.Format("[委托={0}][接收][Rejected][{1}[耗时:{2}ms]]", _OrderProgressEventArgs.TempID, _OrderProgressEventArgs.ToString(), diffTimeDuration));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\alert.wav");
                    player.PlaySync();
                    Task.Run(() => {
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                                _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "开", order.Type.ToString(),
                                "", order.Lots.ToString(), _QC.AccountBalance.ToString(), "订单被拒绝" + _OrderProgressEventArgs.ToString());
                        }
                    });
                    break;
                case TradingAPI.MT4Server.ProgressType.Timeout:
                    _Log.LogInfo(string.Format("[委托={0}][接收][Timeout][{1}]", _OrderProgressEventArgs.TempID, _OrderProgressEventArgs.ToString()));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\alert.wav");
                    player.PlaySync();
                    Task.Run(() => {
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                                _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "开", order.Type.ToString(),
                                "", order.Lots.ToString(), _QC.AccountBalance.ToString(), "订单超时" + _OrderProgressEventArgs.ToString());
                        }
                    });
                    break;
                case TradingAPI.MT4Server.ProgressType.Exception:
                    _Log.LogInfo(string.Format("[委托={0}][接收][Exception][{1}]", _OrderProgressEventArgs.TempID, _OrderProgressEventArgs.ToString()));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\alert.wav");
                    player.PlaySync();
                    Task.Run(() => {
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                                _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "开", order.Type.ToString(),
                                "", order.Lots.ToString(), _QC.AccountBalance.ToString(), "异常" + _OrderProgressEventArgs.ToString());
                        }
                    });
                    break;
                default:
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\ok.wav");
                    player.PlaySync();
                    _Log.LogInfo(string.Format("[委托={0}][接收][{1}][{2}]", _OrderProgressEventArgs.TempID, _OrderProgressEventArgs.Type, _OrderProgressEventArgs.ToString()));
                    break;
            }
        }
    }
}
