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
using System.Diagnostics;

namespace uClient
{
    public class V4 : PlatformInf
    {
        public NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        public V4Client _Client { set; get; }
        //public V4ClientWFB _ClientWFB { set; get; }
        //public V4ClientYSG _ClientYSG { set; get; }
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
        public Account _Account { set; get; }

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
        /// 报价重连异步任务
        /// </summary>
        public BackgroundWorker _BgwV4QuoteReConnect;
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

        public string _AccountId { set; get; }
        public string _InstrumentId { set; get; }
        public string _QuotePolicyId { set; get; }
        /// <summary>
        /// 连接状态
        /// </summary>
        public bool _ConnectFlag { set; get; }
        /// <summary>
        /// 被平仓的订单列表
        /// </summary>
        public IList<Position> ClosedPosition = new List<Position>();
        /// <summary>
        /// 更新的账户和订单信息
        /// </summary>
        public TradingData _TradingData = null;
        public IList<Position> _Positions = new List<Position>();
        private Quote _lastQuote = null;
        private Quote _quote = null;
        public Quote _Quote { get { return _quote; } }
        //订单处理结果
        public OrderProgressEvent _OrderProgressEvent;
        public bool isLogin = false;
        public bool retryConnectFlag = false;
        /// <summary>
        /// 报价消息订阅
        /// </summary>
        private SubscriberSocket _Subscriber = new SubscriberSocket();
        /// <summary>
        /// 手工断开连接
        /// </summary>
        public bool _IsManullyDisconnect = true;
        /// <summary>
        /// 订单创建类型,H:主动创建的订单，C:锁仓创建的订单
        /// </summary>
        public string _OrderCreateType = "H";
        public V4(Config config, MyConfig myConfig, uClient.Comm.Broker broker, Comm.Account account, string tradeSymbol, Log log, BackgroundWorker bgwBroberQuote, BackgroundWorker bgwOrderUpdate, DataGridView gvPositions, QuotaDisplayPanel quotaDisplayPanel, AccountDisplay accountDisplay)
        {
            this._Client = new V4Client(gvPositions);
            //this._ClientWFB = new V4ClientWFB(gvPositions);
            //this._ClientYSG = new V4ClientYSG(gvPositions);
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
            this._ConnectFlag = false;
        }

        /// <summary>
        /// 连接
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
            if (!string.IsNullOrEmpty(_Broker.IP(_Account.Type)) && !string.IsNullOrEmpty(_Broker.Port(_Account.Type)))
            {
                try
                {
                    _Log.LogInfo(string.Format("平台商：{0} 服务器地址:{1} 端口:{2} 交易品种:{3} 账号:{4}", _Broker.BrokerName, _Broker.IP(_Account.Type), _Broker.Port(_Account.Type), _TradeSymbol, _Account.UserCode));
                    string accountType = "";
                    if (string.Equals(_Broker.BrokerCode, "YSG"))
                    {
                        accountType = string.Equals(_Account.Type, "Live") ? "YSG" : "DEM";
                    }
                    if (string.Equals(_Broker.BrokerCode, "WFB"))
                    {
                        accountType = string.Equals(_Account.Type, "Live") ? "WFB" : "WFB";
                    }
                    _ConnectFlag = _Client.Login1(_Account.UserCode, _Account.Password, accountType, _Broker.IP(_Account.Type), Convert.ToInt32(_Broker.Port(_Account.Type)));
                    if (_ConnectFlag)
                    {
                        isLogin = true;
                        _Log.LogInfo("登录成功");
                    }
                    else
                    {
                        isLogin = false;
                        _Log.LogInfo("登录失败");
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("登录失败,异常信息:" + e.Message);
                    _Log.LogInfo("登录失败,异常信息:" + e.Message);
                }
            }
            else
            {
                _logger.Error("登录失败,异常信息：IP,端口,或者线路未选择");
                _Log.LogInfo("登录失败,异常信息：IP,端口,或者线路未选择");
            }
            if (isConnect() && !retryConnectFlag)
            {
                _Log.LogInfo("启动报价获取");
                Task.Run(() =>
                {
                    GetSelfQuote();
                });               
            }
            OnConnect();
            SymbolSubscription(_TradeSymbol);
            _TradingData = GetOpenedOrders();
            _IsManullyDisconnect = false;
        }
        /// <summary>
        /// 登录成功后初始化数据
        /// </summary>
        public void OnConnect()
        {
            _Log.LogInfo("OnConnect");
            SoundPlayer player = new SoundPlayer(Application.StartupPath + "\\wav\\connect.wav");
            //简单播放一遍
            player.Play();
            //循环播放
            //player.PlayLooping();
            //另起线程播放
            //player.PlaySync();
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        public override void DisConnect()
        {
            if (isConnect())
            {
                _IsManullyDisconnect = true;
                _ConnectFlag = false;
                _Client.Logout();
            }
            _Log.LogInfo("DisConnect");
            isLogin = false;
            SoundPlayer player = new SoundPlayer(Application.StartupPath + "\\wav\\disconnect.wav");
            player.Play();
            UpdatePositionGrid();
            _QuoteData.Clear();
            UpdateQuote();
            if (_IsManullyDisconnect)
            {
                _Log.LogInfo("手动断开连接");
            }
            else
            {
                _Log.LogInfo("系统自动断开连接");
                Task.Run(() =>
                {
                    if (_Account.TradePara.NotifyFlag)
                    {
                        HTTPHelper.SendSMS(_MyConfig.NotifyNumber, "MT4系统自动断开连接", "", " ", " ", " ", " ", " ");
                    }                  
                });
                player.Play();
            }
        }
        public void UpdatePositionGridAsny()
        {
            UpdatePositionGrid();
        }
        /// <summary>
        /// 更新持仓列表
        /// </summary>
        public override void UpdatePositionGrid()
        {
            if (_TradingData != null)
            {
                _Client.GetUpdatedPositionList(_TradingData, _Quote.Ask, _Quote.Bid);
                IList<uClient.Comm.Position> orders = _TradingData.positions;
                if (orders != null && orders.Count > 0 && !_PositionLockFlag)
                {
                    _Positions.Clear();
                    foreach (var order in orders)
                    {
                        _Positions.Add(order);
                        decimal points = Comm.Enum.Buy.Contains(order.BuySell) ? (order.CurrentPrice - order.OpenPrice) : (order.OpenPrice - order.CurrentPrice);
                        bool IsExistOrder = false;
                        for (int i = 0; i < _GvPositions.Rows.Count; i++)
                        {
                            if (order.Ticket.ToString().Equals(_GvPositions.Rows[i].Cells[1].Value.ToString()))
                            {
                                IsExistOrder = true;
                                _GvPositions.Rows[i].Cells[6].Value = points.ToString("f2");
                                _GvPositions.Rows[i].Cells[7].Value = order.Profit;
                                TimeSpan ts = DateTime.Now - order.CreationTime;
                                string h = ts.Hours.ToString().PadLeft(2, '0');
                                string m = ts.Minutes.ToString().PadLeft(2, '0');
                                string s = ts.Seconds.ToString().PadLeft(2, '0');
                                _GvPositions.Rows[i].Cells[8].Value = h + ":" + m + ":" + s;
                            }
                        }
                        if (!IsExistOrder)
                        {
                            TimeSpan ts = DateTime.Now - order.CreationTime;
                            string h = ts.Hours.ToString().PadLeft(2, '0');
                            string m = ts.Minutes.ToString().PadLeft(2, '0');
                            string s = ts.Seconds.ToString().PadLeft(2, '0');
                            //窗口被关闭，异步方法会报错
                            if (_GvPositions.ColumnCount > 0)
                            {
                                _GvPositions.Rows.Add(false, order.Ticket, order.Symbol, Comm.Enum.Sell.Contains(order.BuySell) ? "卖出" : "买入", order.Lot.ToString("f2"), order.OpenPrice, points.ToString("f2"), order.Profit.ToString("f2"), h + ":" + m + ":" + s);
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
        /// <summary>
        /// 获取开仓信息
        /// </summary>
        /// <returns></returns>
        public TradingData GetOpenedOrders()
        {
            string res = _Client.TradingDataString();
            //Console.WriteLine("res="+ res);
            return _Client.GetOpenedOrders(res);
        }
        /// <summary>
        /// 平台报价更新
        /// </summary>
        public override void UpdateQuote()
        {
            if (_Quote != null)
            {
                uClient.Comm.Utils.UpdateQuoteDisplay(_QuotaDisplayPanel, _QuoteData, _Quote.Bid, _Quote.Ask);
            }
            else
            {
                uClient.Comm.Utils.ClearBrokerDiff(_QuotaDisplayPanel);
            }
        }
        public override void UpdateAccountCaptial()
        {
            if (_TradingData != null && !isCloseLock)
            {
                _AccountDisplay.lblBlance.Text = _TradingData.AccountBalance.ToString();
                _AccountDisplay.lblEquity.Text = _TradingData.AccountEquity.ToString();
                _AccountDisplay.lblMargin.Text = _TradingData.AccountMargin.ToString();
                _AccountDisplay.lblFreeMargin.Text = _TradingData.AccountFreeMargin.ToString();
            }
        }

        public override IList<Position> GetPosition()
        {
            IList<uClient.Comm.Position> positions = new List<uClient.Comm.Position>();
            try
            {
                if (_GvPositions != null)
                {

                    for (int i = 0; i < _GvPositions.Rows.Count; i++)
                    {
                        uClient.Comm.Position order = new uClient.Comm.Position();
                        positions.Add(order);
                        order.Ticket = _GvPositions.Rows[i].Cells[1].Value.ToString();
                        order.BuySell = _GvPositions.Rows[i].Cells[3].Value.ToString() == "买入" ? "Buy" : "Sell";
                        order.Lot = Convert.ToDecimal(_GvPositions.Rows[i].Cells[4].Value.ToString());
                        order.OpenPrice = Convert.ToDecimal(_GvPositions.Rows[i].Cells[5].Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                _Log.LogInfo(ex.Message);
            }
            return positions;
        }

        /// <summary>
        /// 订单状态更新
        /// </summary>
        /// <param name="orderProgress"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_AutoLock"></param>
        /// <param name="_SendTime"></param>
        public override void OrderUpdate(bool _IsAutoOperationFlag, DateTime _SendTime,string _OrderId)
        {
            executedFlag = false;
            SoundPlayer player;
            _SendTime = _OrderProgressEvent.SendTime;
            DateTime _ReceiveTime = _OrderProgressEvent.ReceiveTime;
            _Log.LogInfo("交易日志:"+_OrderProgressEvent.toString());
            Position position = _OrderProgressEvent.postion;
            long diffTimeDuration = Convert.ToInt32((_ReceiveTime - _SendTime).TotalMilliseconds);
            switch (_OrderProgressEvent.Type)
            {
                case ProgressType.Accepted:
                    break;
                case ProgressType.InProcess:
                    break;
                case ProgressType.Opened:
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\ok.wav");
                    player.Play();
                    //int i = 0;
                    //while ((_TradingData == null || _TradingData.positions.Count == 0) && i<3)
                    //{
                    //    _TradingData = GetOpenedOrders();
                    //    i++;
                    //}
                    _TradingData = GetOpenedOrders();
                    decimal OpenPrice = _TradingData.positions.Count == 0 ? 0 : _TradingData.positions[_TradingData.positions.Count - 1].OpenPrice;
                    if (Comm.Enum.Buy.Contains(position.BuySell))
                    {
                        _Log.LogTradeRecord(_Account, string.Format(MessageFormatter.OrderReceiveMessage(_IsAutoOperationFlag, "BUY"), _ReceiveTime.ToString("HH:mm:ss.fff"), OpenPrice, position.Lot, diffTimeDuration, _TradingData.AccountBalance.ToString()));
                    }
                    if (Comm.Enum.Sell.Contains(position.BuySell))
                    {
                        _Log.LogTradeRecord(_Account, string.Format(MessageFormatter.OrderReceiveMessage(_IsAutoOperationFlag, "SELL"), _ReceiveTime.ToString("HH:mm:ss.fff"), OpenPrice, position.Lot, diffTimeDuration, _TradingData.AccountBalance.ToString()));
                    }
                    _Log.LogInfo(string.Format("[{0}][{1}][开仓][成交][{2}]手数:{3}", position.Symbol, position.BuySell, position.OpenPrice, position.Lot));                 
                    Task.Run(() => {
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.SendSMS(_MyConfig.NotifyNumber, _Account.UserCode, _Account.Type, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "开", position.BuySell, Convert.ToString(OpenPrice), Convert.ToString(position.Lot) + " " + _TradingData.AccountBalance.ToString());
                        }                       
                    });
                    break;
                case ProgressType.Closed:
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\ok.wav");
                    player.Play();
                    _TradingData = GetOpenedOrders();
                    string tradeType = Comm.Enum.Buy.Contains(position.BuySell) ? "BUY_CLOSE" : "SELL_CLOSE";
                    _Log.LogTradeRecord(_Account, string.Format(MessageFormatter.OrderReceiveMessage(_IsAutoOperationFlag, tradeType), _ReceiveTime.ToString("HH:mm:ss.fff"), position.ClosePrice, position.Lot, diffTimeDuration, _TradingData.AccountBalance.ToString()));
                    _Log.LogInfo(string.Format("[{0}][{1}][平仓][成交][{2}]手数:{3}盈亏：{4}", position.Symbol, position.BuySell, position.ClosePrice, position.Lot, position.Profit));                   
                    _TradingData = GetOpenedOrders();
                    Task.Run(() => {
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.SendSMS(_MyConfig.NotifyNumber, _Account.UserCode, _Account.Type, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "平", position.BuySell, Convert.ToString(position.OpenPrice), Convert.ToString(position.Lot) + " " + _TradingData.AccountBalance.ToString());
                        }                        
                    });
                    break;
                case ProgressType.Modified:
                    break;
                case ProgressType.PendingDeleted:
                    break;
                case ProgressType.ClosedBy:
                    break;
                case ProgressType.MultipleClosedBy:
                    break;
                case ProgressType.Price:
                    break;
                case ProgressType.Rejected:
                    string msgFormt5 = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[接收][拒绝][{0}] 耗时:{1}ms";
                    _Log.LogTradeRecord(_Account, string.Format(msgFormt5, _OrderProgressEvent.Exception.Message.ToString(), diffTimeDuration));
                    _Log.LogInfo(string.Format("[委托={0}][接收][Rejected][{1}] 耗时:{2}ms", _OrderProgressEvent.TempID, _OrderProgressEvent.Exception.Message.ToString(), diffTimeDuration));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\alert.wav");
                    player.Play();
                    Task.Run(() => {
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.SendSMS(_MyConfig.NotifyNumber, _Account.UserCode, _Account.Type, _ReceiveTime.ToString("HH:mm:ss"), position.BuySell, "订单被拒绝", _OrderProgressEvent.Exception.Message.ToString(), " ");
                        }                       
                    });
                    break;
                case ProgressType.Timeout:
                    _Log.LogInfo(string.Format("[委托={0}][接收][Timeout][{1}]", _OrderProgressEvent.TempID, _OrderProgressEvent.Exception.Message.ToString()));
                    player = new SoundPlayer(Application.StartupPath + "\\wav\\alert.wav");
                    player.Play();
                    Task.Run(() => {
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.SendSMS(_MyConfig.NotifyNumber, _Account.UserCode, _Account.Type, _ReceiveTime.ToString("HH:mm:ss"), position.BuySell, "订单超时", _OrderProgressEvent.Exception.Message.ToString(), "");
                        }                      
                    });
                    break;
                case ProgressType.Exception:
                    _Log.LogInfo(string.Format("[委托={0}][接收][Exception][{1}]", _OrderProgressEvent.TempID, _OrderProgressEvent.Exception.Message.ToString()));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\alert.wav");
                    player.Play();
                    Task.Run(() => {
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.SendSMS(_MyConfig.NotifyNumber, _Account.UserCode, _Account.Type, _ReceiveTime.ToString("HH:mm:ss"), position.BuySell, "Exception", _OrderProgressEvent.Exception.Message.ToString(), "");
                        }                       
                    });
                    break;
                default:
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\ok.wav");
                    player.Play();
                    _Log.LogInfo(string.Format("[委托={0}][接收][{1}][{2}]", _OrderProgressEvent.TempID, _OrderProgressEvent.Type, _OrderProgressEvent.Exception.Message.ToString()));
                    break;
            }
        }

        public XmlDocument xml = new XmlDocument();
        DateTime startTime = DateTime.Now;
        private bool _IsAutoOperationFlag;
        private DateTime _SendTime;
        private decimal _Lots;
        private Position _Position;
        public void OpenBuyOrder1(bool isAutoOperationFlag, out DateTime _SendTime, decimal lots = 0.1m)
        {
            _IsAutoOperationFlag = isAutoOperationFlag;
            _SendTime = DateTime.Now;
            _Lots = lots;
            Action a = OpenBuyOrderAsny;
            a.BeginInvoke(null, null);

        }
        public void OpenBuyOrderAsny()
        {
            OpenBuyOrder(_IsAutoOperationFlag, out this._SendTime, _Lots);
        }
        public void OpenSellOrder1(bool isAutoOperationFlag, out DateTime _SendTime, decimal lots = 0.1m)
        {
            _IsAutoOperationFlag = isAutoOperationFlag;
            _SendTime = DateTime.Now;
            _Lots = lots;
            Action a = OpenSellOrderAsny;
            a.BeginInvoke(null, null);
        }
        public void OpenSellOrderAsny()
        {
            OpenSellOrder(_IsAutoOperationFlag, out this._SendTime, _Lots);
        }
        public void CloseOrder1(Position position, bool isAutoOperationFlag, out DateTime _SendTime)
        {
            _Position = position;
            _IsAutoOperationFlag = isAutoOperationFlag;
            _SendTime = DateTime.Now;
            Action a = CloseOrderAsny;
            a.BeginInvoke(null, null);
        }
        public void CloseOrderAsny()
        {
            CloseOrder(_Position, _IsAutoOperationFlag, out _SendTime);
        }
        /// <summary>
        /// 开仓
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="symbol"></param>
        /// <param name="lots"></param>
        /// <param name="price"></param>
        /// <param name="_SendTime"></param>
        public override void OpenBuyOrder(bool _IsAutoOperationFlag, out DateTime _SendTime, decimal lots = 0.1m)
        {
            //_Client.HeartPeerQuote1();
            _Client.HeartPeerQuote1();
            decimal price = _Quote.Ask;
            //decimal price = (decimal)_Client.GetAskPrice1();
            _SendTime = DateTime.Now;
            try
            {
                //_OrderProgressEvent = _Client.OpenBuyOrder1((double)price, Convert.ToDouble(_Broker.TradePara.Slippage) / 100, (double)lots);
                _OrderProgressEvent = _Client.OpenBuyOrder1((double)price, Convert.ToDouble(_Broker.TradePara.Slippage) / 100, (double)lots);
                if (_BgwOrderUpdate.IsBusy != true)
                {
                    _BgwOrderUpdate.RunWorkerAsync();
                }
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
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _DSQuote.BidDiff[0], ""));
            }
            else
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, ""));
            }
        }
        /// <summary>
        /// 卖空开仓
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="symbol"></param>
        /// <param name="lots"></param>
        /// <param name="price"></param>
        /// <param name="_SendTime"></param>
        public override void OpenSellOrder(bool _IsAutoOperationFlag, out DateTime _SendTime, decimal lots = 0.1m)
        {
            //_Client.HeartPeerQuote1();
            _Client.HeartPeerQuote1();
            decimal price = _Quote.Bid;
            //decimal price = (decimal)_Client.GetBidPrice1();
            //decimal price = 0;
            _SendTime = DateTime.Now;
            try
            {
                //_OrderProgressEvent = _Client.OpenSellOrder1((double)price, Convert.ToDouble(_Broker.TradePara.Slippage), (double)lots);
                _OrderProgressEvent = _Client.OpenSellOrder1((double)price, Convert.ToDouble(_Broker.TradePara.Slippage) / 100, (double)lots);
                if (_BgwOrderUpdate.IsBusy != true)
                {
                    _BgwOrderUpdate.RunWorkerAsync();
                }
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
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _DSQuote.BidDiff[0], ""));
            }
            else
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots,""));
            }
        }
        bool isCloseLock = false;
        /// <summary>
        /// 平仓
        /// </summary>
        /// <param name="position"></param>
        /// <param name="price"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        public override void CloseOrder(Position position, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            //_Client.HeartPeerQuote1();
            _Client.HeartPeerQuote1();
            _SendTime = DateTime.Now;
            position.Symbol = _InstrumentId;
            bool isBuy = uClient.Comm.Enum.Buy.Contains(position.BuySell);
            string tradeType = isBuy ? "BUY_CLOSE" : "SELL_CLOSE";
            decimal price = isBuy ? _quote.Bid : _quote.Ask;
            //decimal price = isBuy ? (decimal)_Client.GetBidPrice1() : (decimal)_Client.GetAskPrice1();
            try
            {
                //_OrderProgressEvent = _Client.CloseOrder1(position, (double)price, 0);
                _OrderProgressEvent = _Client.CloseOrder1(position, (double)price, 0);
                if (_BgwOrderUpdate.IsBusy != true)
                {
                    _BgwOrderUpdate.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[" + ((_IsAutoOperationFlag == true) ? "自动平仓" : "手动平仓") + "][发送]异常信息：{0}", ex.Message);
                _logger.Error(msg + "  " + ex.StackTrace);
                _Log.LogTradeRecord(_Account, msg);
                _Log.LogInfo(ex.Message);
            }
            string message = MessageFormatter.OrderSendMessage(_IsAutoOperationFlag, tradeType);
            if (_IsAutoOperationFlag)
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, position.Lot, _DSQuote.BidDiff[0], ""));
            }
            else
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, position.Lot, ""));
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
            if (!mexecutedFlag)
            {
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
                IList<Position> orders = GetPosition();
                if (orders != null && orders.Count > 0)
                {
                    isCloseLock = true;
                    foreach (var position in orders)
                    {
                        if (chkBuyClose && Comm.Enum.Buy.Contains(position.BuySell) && _DSQuote.BidDiff[0] <= (double)-_Broker.TradePara.BuyClose ||
                             chkSellClose && Comm.Enum.Sell.Contains(position.BuySell) && _DSQuote.BidDiff[0] >= (double)_Broker.TradePara.SellClose)
                        {
                            CloseOrder(position, _IsAutoOperationFlag, out _SendTime);
                            mexecutedFlag = true;
                            ClosedPosition.Add(position);
                        }
                    }
                    isCloseLock = false;
                }
            }             
            return mexecutedFlag;
        }
        //EA2的公共交易参数，作为全局变量保存
        public string _EA2_TradeRecord = "";
        public bool AutoTradeTransaction2(int tradeType, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            bool mexecutedFlag = false;
            _SendTime = DateTime.Now;
            if (!executedFlag)
            {
                switch (tradeType)
                {
                    case 1:
                        if (_Client._GvPositions == null || _Client._GvPositions.Rows.Count == 0)
                        {
                            OpenBuyOrder(_IsAutoOperationFlag, out _SendTime, _Broker.TradePara.BuyLots);
                            mexecutedFlag = true;
                        }
                        else {
                            _Log.LogInfo("已有单，不能自动重复开单");
                        }
                        break;
                    case 2:
                        if (_Client._GvPositions == null || _Client._GvPositions.Rows.Count == 0)
                        {
                            OpenSellOrder(_IsAutoOperationFlag, out _SendTime, _Broker.TradePara.BuyLots);
                            mexecutedFlag = true;
                        }
                        else
                        {
                            _Log.LogInfo("已有单，不能自动重复开单");
                        }
                        break;
                    case 3:
                        //自动平仓
                        IList<Position> orders1 = GetPosition();
                        if (orders1 != null && orders1.Count > 0)
                        {
                            foreach (var position in orders1)
                            {
                                if (Comm.Enum.Buy.Contains(position.BuySell))
                                {                                   
                                    CloseOrder(position, _IsAutoOperationFlag, out _SendTime);
                                    mexecutedFlag = true;
                                    ClosedPosition.Add(position);
                                }
                            }
                        }
                        break;
                    case 4:
                        //自动平仓
                        IList<Position> orders2 = GetPosition();
                        if (orders2 != null && orders2.Count > 0)
                        {
                            foreach (var position in orders2)
                            {
                                if (Comm.Enum.Sell.Contains(position.BuySell))
                                {                                   
                                    CloseOrder(position, _IsAutoOperationFlag, out _SendTime);
                                    mexecutedFlag = true;
                                    ClosedPosition.Add(position);
                                }
                            }
                        }
                        break;
                    default:
                        _Log.LogInfo("订单类型[" + tradeType + "]未定义");
                        break;
                }
            }               
            return mexecutedFlag;
        }

        private bool _PositionLockFlag = false;
        /// <summary>
        /// 根据订单号获取订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public Position GetOrder(string orderId)
        {
            _PositionLockFlag = true;
            uClient.Comm.Position o = null;
            IList<uClient.Comm.Position> orders = this._Positions;
            if (orders != null && orders.Count > 0)
            {
                foreach (Position order in orders)
                {
                    if (order.Ticket.Equals(orderId))
                    {
                        o = order;
                    }
                }
            }
            _PositionLockFlag = false;
            if (o == null)
            {
                _logger.Error("订单号[" + orderId + "]不存在");
                _Log.LogInfo("订单号[" + orderId + "]不存在");
            }
            return o;
        }
        /// <summary>
        /// 订阅行情
        /// </summary>
        /// <param name="symbol"></param>
        public override void SymbolSubscription(string symbol)
        {
            this._QuoteData = new QuotePanelData(symbol, 0.1);
            _Log.LogInfo("订阅平台行情" + symbol);
        }
        /// <summary>
        /// 是否已连接
        /// </summary>
        /// <returns></returns>
        public override bool isConnect()
        {
            return _ConnectFlag;
        }
        /// <summary>
        /// 获取平仓订单
        /// </summary>
        /// <param name="Ticket"></param>
        /// <returns></returns>
        public Position GetClosedPosition(string Ticket)
        {
            foreach (Position postion in this.ClosedPosition)
            {
                if (postion.Ticket.Equals(Ticket))
                {
                    return postion;
                }
            }
            return null;
        }
        /// <summary>
        /// 获取当前价格
        /// </summary>
        /// <param name="AutoLockTradeSide"></param>
        /// <returns></returns>
        public override double GetCurrentPrice(string AutoLockTradeSide)
        {
            return "BUY".Equals(AutoLockTradeSide) || "CLOSE_SELL".Equals(AutoLockTradeSide) ? (double)_Quote.Ask : (double)_Quote.Bid;
        }
        private bool V4QuoteConnection()
        {
            try
            {
                if (_Config.PublishAddressV4 == null || _Config.PublishAddressV4 == "")
                {
                    MessageBox.Show(string.Format("{0}服务器地址未配置", _Config.DSType));
                    return false;
                }
                _Subscriber.Options.TcpKeepalive = true;
                _Subscriber.Options.TcpKeepaliveIdle = new TimeSpan(5, 0, 0);
                _Subscriber.Options.TcpKeepaliveInterval = new TimeSpan(0, 0, 1);
                _Subscriber.Connect(_Config.PublishAddressV4);
                _Subscriber.Subscribe("");
                _Log.LogInfo(string.Format("连接到报价{0}服务器：{1}", "V4", _Config.PublishAddressV4));
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message + "  " + ex.StackTrace);
                _Log.LogInfo(string.Format("连接到{0}服务器时错误：{1}", _Config.DSType, ex.Message));
            }
            return false;
        }
        public void GetExtendQuote()
        {
            _Log.LogInfo("获取外部报价");
            string PreReceiveMsg = "";
            while (true)
            {
                if (_ConnectFlag)
                {
                    string results = _Subscriber.ReceiveFrameString();
                    //Console.WriteLine("results=" + results);
                    if (results.Equals(PreReceiveMsg))
                    {
                        continue;
                    }
                    else
                    {
                        PreReceiveMsg = results;
                    }
                    string[] split = results.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    //参数说明：Gold 1893.4 1893.6 145158
                    //_logger.Info("OnMarketQuote :" + split[0] + " " + split[1] + " " + split[2] + " " + split[3]);
                    //_Log.LogInfo("接收到消息时间："+DateTime.Now.ToString("HH:mm:ss.fff"));
                    Quote quote = new Quote(split[0], decimal.Parse(split[1]), decimal.Parse(split[2]), DateTime.Now);
                    if (quote != null)
                    {
                        _quote = quote;
                        if (_BgwBroberQuote.IsBusy != true)
                        {
                            _BgwBroberQuote.RunWorkerAsync();
                        }
                    }
                }
                Thread.Sleep(3);
            }
        }
        public void GetSelfQuote()
        {
            _Log.LogInfo("获取本账号报价");
            DateTime startTime = DateTime.Now;
            bool isExecuting = false;
            while (true)
            {
                if (_ConnectFlag)
                {
                    if ((DateTime.Now - startTime).TotalMinutes > 18)
                    {
                        _Log.LogInfo("报价重新连接开始");
                        //bool res = _Client.ConnectQuote1();
                        DateTime s = DateTime.Now;
                        //_Client.LoginQuote1();
                        _Client.LoginQuote1();
                        DateTime e = DateTime.Now;
                        //_Client.HeartPeerQuote1();
                        _Log.LogInfo("报价重新连接完成");
                        _Log.LogInfo("连接消耗时间:" + (e - s).TotalMilliseconds);
                        startTime = DateTime.Now;
                        //if (res)
                        //{
                        //    _Client.LoginQuote1();
                        //    _Log.LogInfo("报价重新连接完成...");
                        //}
                    }
                    if (!isExecuting)
                    {
                        isExecuting = true;
                        //Quote quote = _Client.GetQuote1();
                        Quote quote = null;
                        //Stopwatch sw = new Stopwatch();
                        //sw.Start();
                        quote = _Client.GetQuote1();
                        //sw.Stop();
                        //_Log.LogInfo("获取报价消耗时间:" + sw.ElapsedMilliseconds);                     
                        //Quote quote = _Client.GetQuote2();
                        //Quote quote = _Client.GetDisplayQuote1();
                        if (quote != null)
                        {
                            _quote = quote;
                            if (_BgwBroberQuote.IsBusy != true)
                            {
                                _BgwBroberQuote.RunWorkerAsync();
                            }
                        }
                        isExecuting = false;
                    }
                }
                Thread.Sleep(3);
            }
        }
        public override void NewQuote()
        {
            if (_quote != null)
            {
                if (_lastQuote == null)
                {
                    _lastQuote = _quote;
                }
                else
                {
                    _QuoteData.NewQuoute(_QuoteData.Symbol, (decimal)_quote.Bid, (decimal)_lastQuote.Bid, (decimal)_quote.Ask, (decimal)_lastQuote.Ask);
                    _lastQuote = _quote;
                }
            }
        }
        public void HeartPeer()
        {
            //_Client.HeartPeer1();
            //_Client.HeartPeerQuote1();
            _Client.HeartPeerQuote1();
        }
        /// <summary>
        /// 在主线程中执行该方法
        /// </summary>
        /// <param name="action"></param>
        protected void RenderUI(Action action)
        {
            this._GvPositions.Invoke(new Action(delegate ()
            {
                action();
            }));
        }
        //如果你的后台线程在更新一个UI控件的状态后不需要等待，而是要继续往下处理，那么你就应该使用BeginInvoke来进行异步处理。
        protected void AsynRenderUI(Control control, Action action)
        {
            control.BeginInvoke(new Action(delegate ()
            {
                action();
            }));
        }
        /// <summary>
        /// 交易策略判断
        /// </summary>
        /// <returns></returns>
        public bool IsStrategyVerify()
        {
            return true;
        }

        public override void CheckPlatformConnectStatus()
        {
            if (!isConnect() && !_IsManullyDisconnect)
            {
                executedFlag = false;
                Task.Run(() => {
                    if (_Account.TradePara.NotifyFlag)
                    {
                        HTTPHelper.SendSMS(_MyConfig.NotifyNumber, "V4系统自动断开连接,请及时处理", "", " ", " ", " ", " ", " ");
                    }                    
                });
            }
        }
    }
}
