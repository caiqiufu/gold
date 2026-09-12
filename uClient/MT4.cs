using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TradingAPI.MT4Server;

namespace uClient.Comm
{
    public class MT4 : PlatformInf
    {
        private NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 数据源
        /// </summary>
        public CustomQuotePanel _DSQuote { set; get; }
        /// <summary>
        /// 报价
        /// </summary>
        //public CustomMF4QuotePanel _Quote { set; get; }
        public QuotePanelData _QuoteData { set; get; }
        /// <summary>
        /// 配置文件
        /// </summary>
        public Config _Config { set; get; }
        /// <summary>
        /// 个性配置文件
        /// </summary>
        public MyConfig _MyConfig { set; get; }
        /// <summary>
        /// 策略配置
        /// </summary>
        public StrategyConfig _StrategyConfig { set; get; }
        /// <summary>
        /// 当前选中的合约
        /// </summary>
        public string _TradeSymbol { set; get; }
        /// <summary>
        /// 当前选中的代理商
        /// </summary>
        public uClient.Comm.Broker _Broker { set; get; }
        /// <summary>
        /// 当前选中的账户
        /// </summary>
        public uClient.Comm.Account _Account { set; get; }

        /// <summary>
        /// 是否自动交易
        /// </summary>
        //private bool _IsAutoOperationFlag = false;
        /// <summary>
        /// 订单发送时间
        /// </summary>
        //private DateTime _SendTime;
        /// <summary>
        /// 平台商报价刷新后台异步任务
        /// </summary>
        public BackgroundWorker _BgwBroberQuote;
        /// <summary>
        /// 订单刷新后台异步任务
        /// </summary>
        public BackgroundWorker _BgwOrderUpdate;
        /// <summary>
        /// 订单列表
        /// </summary>
        public DataGridView _GvPositions;
        /// <summary>
        /// 报价跳次显示面板
        /// </summary>
        public QuotaDisplayPanel _QuotaDisplayPanel;
        public AccountDisplay _AccountDisplay;
        /// <summary>
        /// 记录日志
        /// </summary>
        public Log _Log { set; get; }


        /// <summary>
        /// QuoteClient
        /// </summary>
        public QuoteClient _QC { set; get; }
        /// <summary>
        /// OrderClient
        /// </summary>
        public OrderClient _OC { set; get; }

        /// <summary>
        /// 报价更新
        /// </summary>
        public QuoteEventArgs _lastQuote = null;
        public QuoteEventArgs _quote = null;
        public QuoteEventArgs _Quote { get { return _quote; } }
        public OrderProgressEventArgs _OrderProgressEventArgs;
        public QuoteEventArgs _QuoteEventArgs;
        /// <summary>
        /// 手工断开连接
        /// </summary>
        public bool _IsManullyDisconnect = false;
        /// <summary>
        /// 订单创建类型,H:主动创建的订单，C:锁仓创建的订单
        /// </summary>
        public string _OrderCreateType = "";

        /// <summary>
        /// 当前开单价格
        /// </summary>
        public double _CurrentPrice = 0;

        /// <summary>
        /// EA2的公共交易参数，作为全局变量保存
        /// </summary>
        public string _EA2_TradeRecord = "";

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
            DBHelper.updateAccountBalance(_Broker.BrokerCode, _Account.UserCode, Math.Round(_QC.AccountBalance, 2), Math.Round(_QC.AccountEquity, 2), Math.Round(_QC.AccountMargin, 2), Math.Round(_QC.AccountFreeMargin, 2));
        }

        /// <summary>
        /// MT4 连接
        /// </summary>
        public override void Connect()
        {
            if (_Account.UserCode == null || _Account.UserCode == "")
            {
                _Log.LogInfo("账号不能为空");
                MessageBox.Show("账号不能为空");
                return;
            }
            if (_Account.Password == null || _Account.Password == "")
            {
                _Log.LogInfo("密码不能为空");
                MessageBox.Show("密码不能为空");
                return;
            }
            if (_Broker.IP(_Account.Type) == null || _Broker.IP(_Account.Type) == "")
            {
                _Log.LogInfo("IP不能为空");
                MessageBox.Show("IP不能为空");
                return;
            }
            if (_Broker.Port(_Account.Type) == null || _Broker.Port(_Account.Type) == "")
            {
                _Log.LogInfo("Port不能为空");
                MessageBox.Show("Port不能为空");
                return;
            }
            try
            {
                if (_QC != null && _QC.Connected)
                {
                    _QC.Disconnect();
                }
                _Log.LogInfo(string.Format("平台商：{0} 服务器地址:{1} 端口:{2} 交易品种:{3} 账号:{4}", _Broker.BrokerName, _Broker.IP(_Account.Type), _Broker.Port(_Account.Type), _TradeSymbol, _Account.UserCode));
                /**
                string loginIdPath = null;
                switch (_Broker.TradePara.MT4LoginIdServer)
                {
                    case "L":
                        //_QC.LoginIdPath = "http://18.116.232.37:5556/loginid";//本地测试验证服务
                        loginIdPath = "http://18.116.232.37:5556/loginid";//本地测试验证服务;
                        break;
                    case "O":
                        //_QC.LoginIdPath = "http://95.217.201.27:7700/loginid";//MT4 陈帅哥验证服务
                        loginIdPath = "http://95.217.201.27:7700/loginid";//本地测试验证服务;
                        break;
                    case "R":
                        //_QC.LoginIdPath = "";//俄罗斯验证服务器
                        break;
                }**/

                _QC = new QuoteClient(Convert.ToInt32(_Account.UserCode), _Account.Password, _Broker.IP(_Account.Type), Convert.ToInt32(_Broker.Port(_Account.Type)));

                //_QC.LoginIdExPath = "116.202.157.162:7700/loginid";//俄罗斯验证服务器


                if (_QC != null)
                {
                    //会导致在没有开单情况下不能连接成功
                    //_QC = QuoteClient.GetQuoteClient(Convert.ToInt32(_Account.UserCode), _Account.Password, _Broker.IP(_Account.Type), Convert.ToInt32(_Broker.Port(_Account.Type)), loginIdPath);                
                    _QC.OnConnect += QCOnConnect;
                    _QC.OnDisconnect += QCOnDisconnect;
                    _QC.OnQuote += QCOnQuote;
                    //_MT4._QC.OnOrderUpdate += MT4Qc_OnOrderUpdate;
                    _QC.CalculateTradeProps = true;
                    _QC.Connect();
                    _IsManullyDisconnect = false;
                    _OC = new OrderClient(_QC);
                    _OC.OnOrderProgress += QCOnOrderProgress;
                    _ManullyDisconnectSMSCount = 0;
                }
                else
                {
                    _Log.LogInfo("平台链接失败");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                _Log.LogInfo(ex.Message);
            }
        }
        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void QCOnConnect(object sender, ConnectEventArgs args)
        {
            if (args.Exception != null)
            {
                _Log.LogInfo("OnConnect Exception" + args.Exception.Message);
                return;
            }
            _Log.LogInfo("OnConnect");
            SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\connect.wav");
            //简单播放一遍
            //player.Play();
            //循环播放
            //player.PlayLooping();
            //另起线程播放
            player.Play();
            _Log.LogInfo("登录成功");
            SymbolSubscription(_TradeSymbol);
            //Task.Delay(2000).ContinueWith(_ =>
            //{
            //    SymbolSubscription(_TradeSymbol);
            //});
        }
        public virtual void QCOnDisconnect(object sender, DisconnectEventArgs args)
        {
            _Log.LogInfo("DisConnect");
            UpdatePositionGrid();
            _QuoteData.Clear();
            UpdateQuote();
            if (_IsManullyDisconnect)
            {
                _Log.LogInfo("手动断开连接");
            }
            else
            {
                _Log.LogInfo("自动断开连接");
                Task.Run(() =>
                {
                    //if (_Account.TradePara.NotifyFlag)
                    //{
                    //string result = HttpClient.SendSMS(_MyConfig.NotifyNumber, "[" + _Account.UserCode + "]自动断开连接,请及时处理", "", " ", " ", " ", " ", " ");
                    //    EmailHelper.SendEmail(_MyConfig.NotifyEmail, "[龙知易][平台断连][" + _Broker.BrokerName+ "][" + _Account.UserCode + "]自动断开连接,请及时处理", "["+ DateTime.Now.ToString("HH:mm:ss.fff") + "][" + _Account.UserCode + "]自动断开连接,请及时处理");
                    //}
                    //EmailHelper.SendEmail(_MyConfig.NotifyEmail, "[龙知易][平台断连][" + _Broker.BrokerName + "][" + _Account.UserCode + "]自动断开连接,请及时处理", "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "][" + _Account.UserCode + "]自动断开连接,请及时处理");
                });
                SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\disconnect.wav");
                player.Play();
                //int i = 0;
                //while (i < 10 && !isConnect())
                //{
                //    i++;
                //    player.Play();
                //    Thread.Sleep(3000);
                //}
            }
        }
        /// <summary>
        /// 报价更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void QCOnQuote(object sender, QuoteEventArgs args)
        {
            _QuoteEventArgs = args;
            _quote = args;
            if (_BgwBroberQuote.IsBusy != true)
            {
                try
                {
                    if (string.Equals(_TradeSymbol, args.Symbol))
                    {
                        _BgwBroberQuote.RunWorkerAsync();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _logger.Error(ex.Message);
                }

            }
        }
        /// <summary>
        /// 订单状态更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void QCOnOrderProgress(object sender, OrderProgressEventArgs args)
        {
            _OrderProgressEventArgs = args;
            if (_BgwOrderUpdate.IsBusy != true)
            {
                _BgwOrderUpdate.CancelAsync();
                _BgwOrderUpdate.RunWorkerAsync();
            }
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnConnect(object sender, ConnectEventArgs args)
        {
            if (args.Exception != null)
            {
                _Log.LogInfo("OnConnect Exception" + args.Exception.Message);
                return;
            }
            _Log.LogInfo("OnConnect");
            _Log.LogInfo(string.Format("平台商：{0} 服务器地址:{1} 端口:{2} 交易品种:{3}", _Broker.BrokerName, _Broker.IP(_Account.Type), _Broker.Port(_Account.Type), _TradeSymbol));
            SoundPlayer player = new SoundPlayer(Application.StartupPath + "\\wav\\connect.wav");
            //简单播放一遍
            player.Play();
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
                _Log.LogInfo("DisConnect");
                SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\disconnect.wav");
                player.Play();
                UpdateQuote();
                UpdatePositionGrid();
                _QuoteData.Clear();
            }
        }
        /// <summary>
        /// 更新持仓列表
        /// </summary>
        public override void UpdatePositionGrid()
        {
            if (_GvPositions != null)
            {
                if (_GvPositions.InvokeRequired)
                {
                    _GvPositions.Invoke(new Action(UpdatePositionGrid));
                }
                else
                {
                    Order[] orders = myGetPosition();
                    if (_Quote != null && orders != null && orders.Length > 0)
                    {
                        foreach (var order in orders)
                        {
                            double points = order.Type == Op.Sell || order.Type == Op.SellLimit ? order.OpenPrice - _Quote.Ask : _Quote.Bid - order.OpenPrice;
                            bool IsExistOrder = false;
                            for (int i = 0; i < _GvPositions.Rows.Count; i++)
                            {
                                if (order.Ticket.ToString().Equals(_GvPositions.Rows[i].Cells[1].Value.ToString()))
                                {
                                    IsExistOrder = true;
                                    _GvPositions.Rows[i].Cells[6].Value = points.ToString("f2");
                                    _GvPositions.Rows[i].Cells[7].Value = order.Profit;
                                    //TimeSpan ts = _QC.ServerTime - order.OpenTime;
                                    double timezone = _Account.TradePara.Timezone;
                                    TimeSpan ts = DateTime.Now - order.OpenTime.AddHours(timezone);
                                    string h = ts.Hours.ToString().PadLeft(2, '0');
                                    string m = ts.Minutes.ToString().PadLeft(2, '0');
                                    string s = ts.Seconds.ToString().PadLeft(2, '0');
                                    _GvPositions.Rows[i].Cells[8].Value = h + ":" + m + ":" + s;
                                }
                            }
                            if (!IsExistOrder)
                            {
                                //TimeSpan ts = _QC.ServerTime - order.OpenTime;
                                double timezone = _Account.TradePara.Timezone;
                                TimeSpan ts = DateTime.Now - order.OpenTime.AddHours(timezone);
                                string h = ts.Hours.ToString().PadLeft(2, '0');
                                string m = ts.Minutes.ToString().PadLeft(2, '0');
                                string s = ts.Seconds.ToString().PadLeft(2, '0');
                                //窗口被关闭，异步方法会报错
                                if (_GvPositions.ColumnCount > 0)
                                {
                                    _GvPositions.Rows.Add(false, order.Ticket, order.Symbol, order.Type == Op.Sell ? "卖出" : "买入", order.Lots, order.OpenPrice, points.ToString("f2"), order.Profit, h + ":" + m + ":" + s);
                                }
                            }
                        }
                        for (int j = 0; j < _GvPositions.Rows.Count; j++)
                        {
                            bool IsDeleteOrder = true;
                            foreach (var order in orders)
                            {
                                if (order.Ticket.ToString().Equals(_GvPositions.Rows[j].Cells[1].Value.ToString()))
                                {
                                    IsDeleteOrder = false;
                                }
                            }
                            if (IsDeleteOrder)
                            {
                                _GvPositions.Rows.RemoveAt(_GvPositions.Rows[j].Index);
                            }
                        }
                    }
                    else
                    {
                        _GvPositions.Rows.Clear();
                    }
                    if (_GvPositions.Rows.Count > 0)
                    {
                        _GvPositions.Rows[0].Selected = true;
                        _GvPositions.Rows[0].Cells[0].Value = true;
                    }
                }
            }
        }
        /// <summary>
        /// 平台报价更新
        /// </summary>
        public override void UpdateQuote()
        {
            if (isConnect() && _Quote != null)
            {
                Utils.UpdateQuoteDisplay(_QuotaDisplayPanel, _QuoteData, (decimal)_Quote.Bid, (decimal)_Quote.Ask);
            }
            else
            {
                Utils.ClearBrokerDiff(_QuotaDisplayPanel);
            }
        }
        /// <summary>
        /// 更新帐户资金信息
        /// </summary>
        public override void UpdateAccountCaptial()
        {
            if (isConnect())
            {
                if (_AccountDisplay.lblBlance.InvokeRequired)
                {
                    _AccountDisplay.lblBlance.Invoke(new Action(UpdateAccountCaptial));
                }
                else
                {
                    _AccountDisplay.lblBlance.Text = Math.Round(_QC.AccountBalance, 2).ToString();
                    _AccountDisplay.lblEquity.Text = Math.Round(_QC.AccountEquity, 2).ToString();
                    _AccountDisplay.lblMargin.Text = Math.Round(_QC.AccountMargin, 2).ToString();
                    _AccountDisplay.lblFreeMargin.Text = Math.Round(_QC.AccountFreeMargin, 2).ToString();
                }
            }
        }
        int _ManullyDisconnectSMSCount = 0;
        public override void CheckPlatformConnectStatus()
        {
            if (!isConnect() && !_IsManullyDisconnect)
            {
                executedFlag = false;
                if (_ManullyDisconnectSMSCount < 3)
                {
                    Task.Run(() =>
                    {
                        _ManullyDisconnectSMSCount++;
                        //if (_Account.TradePara.NotifyFlag)
                        //{
                        //string result = HttpClient.SendSMS(_MyConfig.NotifyNumber, "["+ _Broker.BrokerName + "]["+_Account.UserCode+"][平台]自动断开连接,请及时处理", "", " ", " ", " ", " ", " ");
                        //    EmailHelper.SendEmail(_MyConfig.NotifyEmail, "[龙知易][平台断连][" + _Broker.BrokerName + "][" + _Account.UserCode + "][平台]自动断开连接,请及时处理", "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "][" + _Account.UserCode + "][平台]自动断开连接,请及时处理");
                        //}
                        //EmailHelper.SendEmail(_MyConfig.NotifyEmail, "[龙知易][平台断连][" + _Broker.BrokerName + "][" + _Account.UserCode + "][平台]自动断开连接,请及时处理", "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "][" + _Account.UserCode + "][平台]自动断开连接,请及时处理");
                    });
                }
            }
        }
        /// <summary>
        /// 获取持仓信息
        /// </summary>
        /// <returns></returns>
        public Order[] myGetPosition()
        {
            Order[] orders = null;
            try
            {
                if (isConnect())
                {
                    orders = _QC.GetOpenedOrders();
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

        public override void NewQuote()
        {
            QuoteEventArgs quote = _QuoteEventArgs;
            if (_lastQuote == null)
            {
                _lastQuote = quote;
                _quote = quote;
            }
            else
            {
                _quote = quote;
                //string symbol,decimal newBidPrice,decimal oldBidPrice,decimal newAskPrice,decimal oldAskPrice
                _QuoteData.NewQuoute(_TradeSymbol, (decimal)_quote.Bid, (decimal)_lastQuote.Bid, (decimal)_quote.Ask, (decimal)_lastQuote.Ask);
                _lastQuote = _quote;
            }
        }

        /// <summary>
        /// 买多开仓
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        public override void OpenBuyOrder(bool _IsAutoOperationFlag, out DateTime _SendTime, decimal lots = 0.1m)
        {
            double price = _QC.GetQuote(_TradeSymbol).Ask;
            _SendTime = DateTime.Now;
            try
            {
                //_Log.LogInfo("发送订单时间：" + DateTime.Now.ToString("HH:mm:ss.fff"));
                _OC.OrderSendAsync(_TradeSymbol, Op.Buy, (double)lots, price, _Broker.TradePara.Slippage, 0, 0, "", 0, new DateTime());
            }
            catch (Exception ex)
            {
                string msg = string.Format("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[" + ((_IsAutoOperationFlag == true) ? "自动买多" : "手动买多") + "][发送]异常信息：{0}", ex.Message);
                _logger.Error(msg + "  " + ex.StackTrace);
                _Log.LogTradeRecord(_Account, msg);
                _Log.LogInfo(ex.Message);
            }
            string message = MessageFormatter.OrderSendMessage(_IsAutoOperationFlag, "BUY");
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
        /// 卖空开仓
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        public override void OpenSellOrder(bool _IsAutoOperationFlag, out DateTime _SendTime, decimal lots = 0.1m)
        {
            _SendTime = DateTime.Now;
            double price = _QC.GetQuote(_TradeSymbol).Bid;
            try
            {
                //_Log.LogInfo("发送订单时间：" + DateTime.Now.ToString("HH:mm:ss.fff"));
                _OC.OrderSendAsync(_TradeSymbol, Op.Sell, (double)lots, price, _Broker.TradePara.Slippage, 0, 0, "", 0, new DateTime());
            }
            catch (Exception ex)
            {
                string msg = string.Format("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[" + ((_IsAutoOperationFlag == true) ? "自动卖空" : "手动卖空") + "][发送]异常信息：{0}", ex.Message);
                _logger.Error(msg + "  " + ex.StackTrace);
                _Log.LogTradeRecord(_Account, msg);
                _Log.LogInfo(ex.Message);
            }
            string message = MessageFormatter.OrderSendMessage(_IsAutoOperationFlag, "SELL");
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
        /// 平仓
        /// </summary>
        /// <param name="order"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        public void CloseOrder(Order order, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            _SendTime = DateTime.Now;
            int ticket = order.Ticket;
            string symbol = order.Symbol;
            double lots = order.Lots;
            var quote = _QC.GetQuote(_TradeSymbol);
            double price = order.Type == Op.Buy ? quote.Bid : quote.Ask;
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
        public void CloseOrder(Order order, double closeLots, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            _SendTime = DateTime.Now;
            int ticket = order.Ticket;
            string symbol = order.Symbol;
            if (closeLots > order.Lots)
            {
                closeLots = order.Lots;
            }
            double lots = closeLots;
            var quote = _QC.GetQuote(_TradeSymbol);
            double price = order.Type == Op.Buy ? quote.Bid : quote.Ask;
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
        public bool executedFlag = false;
        /// <summary>
        /// 自动交易
        /// </summary>
        /// <param name="chkBuyOpen"></param>
        /// <param name="chkSellOpen"></param>
        /// <param name="chkBuyClose"></param>
        /// <param name="chkSellClose"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        /// <returns></returns>
        public bool AutoTradeTransaction(bool chkBuyOpen, bool chkSellOpen, bool chkBuyClose, bool chkSellClose, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            bool mexecutedFlag = false;
            _SendTime = DateTime.Now;
            //自动买多开仓
            if (chkBuyOpen && (_DSQuote.BidDiff[0] >= (double)_Broker.TradePara.BuyOpen))
            {
                OpenBuyOrder(_IsAutoOperationFlag, out _SendTime, _Broker.TradePara.BuyLots);
                mexecutedFlag = true;
            }
            //自动卖空开仓
            if (chkSellOpen && (_DSQuote.BidDiff[0] <= (double)(-_Broker.TradePara.SellOpen)))
            {
                OpenSellOrder(_IsAutoOperationFlag, out _SendTime, _Broker.TradePara.SellLots);
                mexecutedFlag = true;
            }
            //自动平仓
            TradingAPI.MT4Server.Order[] orders = myGetPosition();
            if (orders != null && orders.Length > 0)
            {
                foreach (var order in orders)
                {
                    if ((chkBuyClose && order.Type == Op.Buy && _DSQuote.BidDiff[0] <= (double)-_Broker.TradePara.BuyClose) ||
                        (chkSellClose && order.Type == Op.Sell && _DSQuote.BidDiff[0] >= (double)_Broker.TradePara.SellClose))
                    {
                        _IsAutoOperationFlag = true;
                        CloseOrder(order, _IsAutoOperationFlag, out _SendTime);
                        mexecutedFlag = true;
                    }
                }
            }
            return mexecutedFlag;
        }
        public bool AutoTradeTransaction2(int tradeType, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            bool mexecutedFlag = false;
            _SendTime = DateTime.Now;
            switch (tradeType)
            {
                case 1:
                    if (_QC.GetOpenedOrders() == null || _QC.GetOpenedOrders().Length == 0)
                    {
                        OpenBuyOrder(_IsAutoOperationFlag, out _SendTime, _Broker.TradePara.BuyLots);
                        mexecutedFlag = true;
                    }
                    else
                    {
                        _Log.LogInfo("已有单，不能自动重复开单");
                    }
                    break;
                case 2:
                    if (_QC.GetOpenedOrders() == null || _QC.GetOpenedOrders().Length == 0)
                    {
                        OpenSellOrder(_IsAutoOperationFlag, out _SendTime, _Broker.TradePara.SellLots);
                        mexecutedFlag = true;
                    }
                    else
                    {
                        _Log.LogInfo("已有单，不能自动重复开单");
                    }
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
        public bool AutoTradeTransaction4(int tradeType, double lots, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            bool mexecutedFlag = false;
            _SendTime = DateTime.Now;
            decimal tblots = _Broker.TradePara.BuyLots;
            if (tblots >= 1)
            {
                tblots = Math.Round(tblots * (decimal)lots, 2);
            }
            decimal tslots = _Broker.TradePara.SellLots;
            if (tslots >= 1)
            {
                tslots = Math.Round(tslots * (decimal)lots, 2);
            }
            switch (tradeType)
            {
                case 1:
                case 5:
                    OpenBuyOrder(_IsAutoOperationFlag, out _SendTime, tblots);
                    mexecutedFlag = true;
                    break;
                case 2:
                case 6:
                    OpenSellOrder(_IsAutoOperationFlag, out _SendTime, tslots);
                    mexecutedFlag = true;
                    break;
                case 3:
                case 7:
                    //自动平仓
                    TradingAPI.MT4Server.Order[] orders1 = myGetPosition();
                    if (orders1 != null && orders1.Length > 0)
                    {
                        foreach (var order in orders1)
                        {
                            if (order.Type == Op.Buy)
                            {
                                CloseOrder(order, (double)tblots, _IsAutoOperationFlag, out _SendTime);
                                mexecutedFlag = true;
                            }
                        }
                    }
                    break;
                case 4:
                case 8:
                    //自动平仓
                    TradingAPI.MT4Server.Order[] orders2 = myGetPosition();
                    if (orders2 != null && orders2.Length > 0)
                    {
                        foreach (var order in orders2)
                        {
                            if (order.Type == Op.Sell)
                            {
                                CloseOrder(order, (double)tslots, _IsAutoOperationFlag, out _SendTime);
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
                    player.Play();
                    Task.Run(() =>
                    {
                        //只有EA自动开单才记录该日志
                        if (!string.IsNullOrEmpty(_EA2_TradeRecord))
                        {
                            //orderTypeDetail:openPrice:closePrice:profit
                            string[] split = _EA2_TradeRecord.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            DBHelper.saveTradeLogMT4("MT4", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", order.Ticket.ToString(), _EA2_TradeRecord, tradeType, split[0], order.Lots, order.OpenPrice, Math.Round(Convert.ToDouble(split[2]), 2), 0, 0, Math.Round(_QC.AccountBalance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin, _LockOrderId);
                            _Log.LogInfo("保存交易日志完成");
                            //order_type:BUY_CLOSE,volume:0.3,open_price:2155.28,close_price:2158.79
                            string content = "order_type:" + tradeType + ",volume:" + order.Lots + ",open_price:" + order.OpenPrice + ",close_price:";
                            DBHelper.saveNotify("TR", order.Ticket.ToString(), content, "", _Account.UserCode);
                        }
                        else
                        {

                            DBHelper.saveTradeLogMT4("MT4", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", order.Ticket.ToString(), msg, tradeType, "", order.Lots, order.OpenPrice, -99, -99, 0, Math.Round(_QC.AccountBalance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin, _LockOrderId);
                            _Log.LogInfo("保存交易日志完成");
                        }
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
                    _Log.LogTradeRecord(_Account, msg1);
                    _Log.LogInfo(string.Format("[{0}][{1}][平仓][成交][{2}]手数:{3}盈亏：{4}", order.Symbol, order.Type, order.ClosePrice, order.Lots, order.Profit));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\ok.wav");
                    player.Play();
                    Task.Run(() =>
                    {

                        double points = order.Type == Op.Sell || order.Type == Op.SellLimit ? order.OpenPrice - _Quote.Ask : _Quote.Bid - order.OpenPrice;
                        //只有EA自动开单才记录该日志
                        if (!string.IsNullOrEmpty(_EA2_TradeRecord))
                        {
                            //orderTypeDetail:openPrice:closePrice:profit
                            //由于开仓和平仓价格无法准确从交易客户端获取(有可能人为操作多个订单),所以profit数据取自服务端
                            string[] split = _EA2_TradeRecord.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            DBHelper.saveTradeLogMT4("MT4", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", order.Ticket.ToString(), _EA2_TradeRecord, tradeType1, split[0], order.Lots, order.OpenPrice, order.ClosePrice, Math.Round(points, 2), Math.Round(order.Profit, 2), Math.Round(_QC.AccountBalance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin, _LockOrderId);
                            _Log.LogInfo("保存交易日志完成");

                            //order_type:BUY_CLOSE,volume:0.3,open_price:2155.28,close_price:2158.79
                            string content = "order_type:" + tradeType1 + ",volume:" + order.Lots + ",open_price:" + _CurrentPrice + ",close_price:" + order.ClosePrice;
                            DBHelper.saveNotify("TR", order.Ticket.ToString(), content, "", _Account.UserCode);
                        }
                        else
                        {
                            DBHelper.saveTradeLogMT4("MT4", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", order.Ticket.ToString(), msg1, tradeType1, "", order.Lots, order.OpenPrice, order.ClosePrice, Math.Round(points, 2), Math.Round(order.Profit, 2), Math.Round(_QC.AccountBalance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin, _LockOrderId);
                            _Log.LogInfo("保存交易日志完成");
                        }

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
                    string msg2 = string.Format(msgFormt5, _OrderProgressEventArgs.ToString(), diffTimeDuration);
                    _Log.LogTradeRecord(_Account, msg2);
                    _Log.LogInfo(string.Format("[委托={0}][接收][Rejected][{1}[耗时:{2}ms]]", _OrderProgressEventArgs.TempID, _OrderProgressEventArgs.ToString(), diffTimeDuration));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\alert.wav");
                    player.Play();
                    Task.Run(() =>
                    {
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                                _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "开", order.Type.ToString(),
                                "", order.Lots.ToString(), _QC.AccountBalance.ToString(), "订单被拒绝" + _OrderProgressEventArgs.ToString());
                        }
                        string myTradeType = order.Type == Op.Buy ? "BUY" : "SELL";
                        DBHelper.saveTradeLogMT4("MT4", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", order.Ticket.ToString(), msg2, myTradeType, "", order.Lots, order.OpenPrice, -99, -99, 0, Math.Round(_QC.AccountBalance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin, _LockOrderId);
                        _Log.LogInfo("保存交易日志完成");
                    });
                    break;
                case TradingAPI.MT4Server.ProgressType.Rejected:
                    string msgFormt6 = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[接收][拒绝][{0}] 耗时:{1}ms";
                    string msg3 = string.Format(msgFormt6, _OrderProgressEventArgs.ToString(), diffTimeDuration);
                    _Log.LogTradeRecord(_Account, msg3);
                    _Log.LogInfo(string.Format("[委托={0}][接收][Rejected][{1}[耗时:{2}ms]]", _OrderProgressEventArgs.TempID, _OrderProgressEventArgs.ToString(), diffTimeDuration));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\alert.wav");
                    player.Play();
                    Task.Run(() =>
                    {
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                                _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "开", order.Type.ToString(),
                                "", order.Lots.ToString(), _QC.AccountBalance.ToString(), "订单被拒绝" + _OrderProgressEventArgs.ToString());
                        }
                        string myTradeType = order.Type == Op.Buy ? "BUY" : "SELL";
                        DBHelper.saveTradeLogMT4("MT4", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", order.Ticket.ToString(), msg3, myTradeType, "", order.Lots, order.OpenPrice, -99, -99, 0, Math.Round(_QC.AccountBalance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin, _LockOrderId);
                        _Log.LogInfo("保存交易日志完成");
                    });
                    break;
                case TradingAPI.MT4Server.ProgressType.Timeout:
                    _Log.LogInfo(string.Format("[委托={0}][接收][Timeout][{1}]", _OrderProgressEventArgs.TempID, _OrderProgressEventArgs.ToString()));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\alert.wav");
                    player.Play();
                    Task.Run(() =>
                    {
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
                    player.Play();
                    Task.Run(() =>
                    {
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
                    player.Play();
                    _Log.LogInfo(string.Format("[委托={0}][接收][{1}][{2}]", _OrderProgressEventArgs.TempID, _OrderProgressEventArgs.Type, _OrderProgressEventArgs.ToString()));
                    break;
            }
        }
        /// <summary>
        /// 根据订单号获取订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public Order GetOrder(string orderId)
        {
            Order[] orders = this.myGetPosition();
            if (orders != null && orders.Length > 0)
            {
                foreach (Order order in orders)
                {
                    if (order.Ticket == Convert.ToInt32(orderId))
                    {
                        return order;
                    }
                }
                _logger.Error("订单号[" + orderId + "]不存在");
                _Log.LogInfo("订单号[" + orderId + "]不存在");
                return null;
            }
            else
            {
                _logger.Error("订单号[" + orderId + "]不存在");
                _Log.LogInfo("订单号[" + orderId + "]不存在");
                return null;
            }
        }
        /// <summary>
        /// 黄金,采用的是万州symbol
        /// </summary>
        public string _GOLD = "";
        /// <summary>
        /// 白银,采用的是万州symbol
        /// </summary>
        public string _SILVER = "";
        /// <summary>
        /// 当前黄金BID价格
        /// </summary>
        public double _GOLD_BID_PRICE = 0;
        /// <summary>
        /// 当前黄金ASK价格
        /// </summary>
        public double _GOLD_ASK_PRICE = 0;
        /// <summary>
        /// 当前白银BID价格
        /// </summary>
        public double _SILVER_BID_PRICE = 0;
        /// <summary>
        /// 当前白ASK银价格
        /// </summary>
        public double _SILVER_ASK_PRICE = 0;
        /// <summary>
        /// 订阅行情
        /// </summary>
        /// <param name="symbol"></param>
        public override void SymbolSubscription(string symbol)
        {
            if (_QC != null)
            {
                string[] symbols = _QC.Symbols;
                if (symbols != null && symbols.Length > 0)
                {
                    _Log.LogInfo("产品列表:");
                    string ss = "";
                    foreach (string s in symbols)
                    {
                        ss = ss + s + ",";
                    }
                    _Log.LogInfo(ss);
                    if (ss.Contains(symbol))
                    {
                        _QC.Subscribe(symbol);
                        _Log.LogInfo("订阅平台行情:" + symbol);
                        var info = _QC.GetSymbolInfo(symbol);
                        _QuoteData = new QuotePanelData(symbol, (double)info.Point);
                        //增加白银数据
                        if (!string.IsNullOrEmpty(_GOLD) && ss.Contains("," + _GOLD + ","))
                        {
                            _QC.Subscribe(_GOLD);
                            _Log.LogInfo("订阅GOLD:" + _GOLD);
                        }
                        if (!string.IsNullOrEmpty(_SILVER) && ss.Contains("," + _SILVER + ","))
                        {
                            _QC.Subscribe(_SILVER);
                            _Log.LogInfo("订阅SILVER:" + _SILVER);
                        }
                    }
                    else
                    {
                        _Log.LogInfo("平台行情[" + symbol + "]不存在");
                    }
                }
                else
                {
                    _Log.LogInfo("未获取平台行情");
                }
            }
        }
        /// <summary>
        /// 是否已连接
        /// </summary>
        /// <returns></returns>
        public override bool isConnect()
        {
            if (this._QC == null || !this._QC.Connected)
            {
                //_Log.LogInfo("平台商没有连接或未初始化:[_QC=" + _QC + "][_Quote=" + _Quote + "][IsConnected=" +(_QC==null? "_QC=null": this._QC.Connected?"true":"false") + "]");
                return false;
            }
            else
            {
                return true;
            }
        }
        public void KeepConnectedStatus(Func<bool> Connect)
        {
            object syncRoot = new object();
            this.Connect();
            this._QC.OnConnect += (status, message) =>
            {
                if (_QC != null && _QC.Connected)
                {
                    return;
                }
                lock (syncRoot)
                {
                    if (_QC.Connected)
                        return;
                    _Log.LogInfo("失去连接");
                    for (int i = 1; i <= 10; i++)
                    {
                        _Log.LogInfo($"正在第 {i} 次重试");
                        if (Connect())
                        {
                            SpinWait.SpinUntil(() => _QC.Connected);
                            _Log.LogInfo("重新登录成功");
                            break;
                        }
                    }
                }
            };
        }
        /// <summary>
        /// 获取当前价格
        /// </summary>
        /// <param name="AutoLockTradeSide"></param>
        /// <returns></returns>
        public override double GetCurrentPrice(string AutoLockTradeSide)
        {
            return "BUY".Equals(AutoLockTradeSide) || "CLOSE_SELL".Equals(AutoLockTradeSide) ? _Quote.Ask : _Quote.Bid;
        }



        /// <summary>
        /// 由于参数类型不一致，不能重写的方法
        /// </summary>

        public override IList<Position> GetPosition()
        {
            throw new NotImplementedException();
        }

        public override void CloseOrder(Position position, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            throw new NotImplementedException();
        }
    }
}
