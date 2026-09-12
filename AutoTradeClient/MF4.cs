using M4.Client;
using M4.Common;
using M4.Common.Classes;
using M4.Common.Enums;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using uClient.Comm;
using Log = uClient.Comm.Log;
using Position = uClient.Comm.Position;

namespace uClient.Broker
{
    public class MF4 : PlatformInf
    {
        public NLog.Logger _logger = LogManager.GetCurrentClassLogger();
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
        //public bool _IsAutoOperationFlag = false;
        /// <summary>
        /// 订单发送时间
        /// </summary>
        //public DateTime _SendTime;
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

        //平台自有参数
        /// <summary>
        /// Client
        /// </summary>
        public Client _Client { set; get; }
        /// <summary>
        /// 被平仓的订单列表
        /// </summary>
        public IList<M4.Common.Classes.Position> ClosedPosition = new List<M4.Common.Classes.Position>();
        /// <summary>
        /// 系统通知消息
        /// </summary>
        public SystemMessage _SystemMessage;
        /// <summary>
        /// 报价更新
        /// </summary>
        public QuoteUpdatedEventArgs _QuoteUpdatedEventArgs;
        public M4.Common.Classes.Quote _lastQuote = null;
        public M4.Common.Classes.Quote _quote = null;
        public M4.Common.Classes.Quote _Quote { get { return _quote; } }
        /// <summary>
        /// 手工断开连接
        /// </summary>
        public bool _IsManullyDisconnect = true;
        /// <summary>
        /// 订单创建类型,H:主动创建的订单，C:锁仓创建的订单
        /// </summary>
        public string _OrderCreateType = "H";

        /// <summary>
        /// 当前开单价格
        /// </summary>
        public double _CurrentPrice = 0;


        public MF4(Config config, MyConfig myConfig, uClient.Comm.Broker broker, uClient.Comm.Account account, string tradeSymbol, Log log, BackgroundWorker bgwBroberQuote, BackgroundWorker bgwOrderUpdate, DataGridView gvPositions, QuotaDisplayPanel quotaDisplayPanel, AccountDisplay accountDisplay)
        {
            this._Client = new Client();
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

        public MF4()
        {
            this._Client = new Client();
        }

        public override void Connect()
        {
            if (_Account.UserCode == null || _Account.UserCode == "")
            {
                MessageBox.Show("账号不能为空");
                return;
            }
            if (_Account.Password == null || _Account.Password == "")
            {
                MessageBox.Show("密码不能为空");
                return;
            }
            int pathIndex = 0;
            if (_Client != null && _Client.IsConnected)
            {
                _Client.Logout();
            }
            if (string.IsNullOrEmpty(_Broker.IP(_Account.Type)) || string.IsNullOrEmpty(_Broker.Port(_Account.Type)))
            {
                //_MF4.KeepConnectedStatus(() => _MF4._Client.Login(_Account.UserCode, _Account.Password, "Live".Equals(_Account.Type), pathIndex, out string message2));
                if (!_Client.Login(_Account.UserCode, _Account.Password, "Live".Equals(_Account.Type), pathIndex, out string message1))
                {
                    _logger.Error("登录失败,异常信息：" + message1);
                    _Log.LogInfo("登录失败,异常信息：" + message1);
                }
            }
            else if (!string.IsNullOrEmpty(_Broker.IP(_Account.Type)) && !string.IsNullOrEmpty(_Broker.Port(_Account.Type)))
            {
                _Log.LogInfo(string.Format("平台商：{0} 服务器地址:{1} 端口:{2} 交易品种:{3} 账号:{4}", _Broker.BrokerName, _Broker.IP(_Account.Type), _Broker.Port(_Account.Type), _TradeSymbol, _Account.UserCode));
                //_MF4.KeepConnectedStatus(() => _MF4._Client.Login(_Account.UserCode, _Account.Password, "Live".Equals(_Account.Type), _Account.IP, Convert.ToInt32(_Account.Port), out string message4));
                if (!_Client.Login(_Account.UserCode, _Account.Password, "Live".Equals(_Account.Type), _Broker.IP(_Account.Type), Convert.ToInt32(_Broker.Port(_Account.Type)), out string message2))
                {
                    _logger.Error("登录失败,异常信息：" + message2);
                    _Log.LogInfo("登录失败,异常信息：" + message2);
                }
                else
                {
                    _Log.LogInfo("登录成功");
                }
            }
            else
            {
                _logger.Error("登录失败,异常信息：IP,端口,或者线路未选择");
                _Log.LogInfo("登录失败,异常信息：IP,端口,或者线路未选择");
            }
            if (isConnect())
            {
                OnConnect();
                SymbolSubscription(_TradeSymbol);
                //_Client.DataProvider.Subscribe(_TradeSymbol);
                _Client.QuoteUpdated += OnQuote;
                _Client.ConnectionStatusChanged += ConnectionStatusChanged;
                //返回所有订单交易信息
                _Client.SystemMessageReceived += SystemMessageReceived;
                _IsManullyDisconnect = false;
            }
        }
        /// <summary>
        /// 更新报价
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnQuote(Client sender, QuoteUpdatedEventArgs e)
        {
            _QuoteUpdatedEventArgs = e;
            if (_BgwBroberQuote.IsBusy != true)
            {
                _BgwBroberQuote.RunWorkerAsync();
            }
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
        public override void DisConnect()
        {
            if (isConnect())
            {
                _Client.Logout();
            }
            SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\disconnect.wav");
            player.Play();
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
                _Log.LogInfo("系统自动断开连接");
                //Task.Run(() =>
                //{
                //    if (_Account.TradePara.NotifyFlag)
                //    {
                //        //HttpClient.SendSMS(_MyConfig.NotifyNumber, "MF4系统自动断开连接", "", " ", " ", " ", " ", " ");
                //        EmailHelper.SendEmail(_MyConfig.NotifyEmail, "[龙知易][平台断连][" + _Broker.BrokerName + "][" + _Account.UserCode + "][平台]自动断开连接,请及时处理", "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "][" + _Account.UserCode + "][平台]自动断开连接,请及时处理");
                //    }
                //});
                //player.Play();
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
        /// 连接状态改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ConnectionStatusChanged(Client sender, ConnectionStatusChangedEventArgs e)
        {
            if (e.Status == M4.Interfaces.UserCode.ConnectionStatus.Connected)
            {
                _Log.LogInfo("ConnectionStatusChanged:Connected");
                OnConnect();
            }
            else if (e.Status == M4.Interfaces.UserCode.ConnectionStatus.Disconnected)
            {
                _Log.LogInfo("ConnectionStatusChanged:Disconnected");
                DisConnect();
            }
            else if (e.Status == M4.Interfaces.UserCode.ConnectionStatus.ConnectionLost)
            {
                _Log.LogInfo("ConnectionStatusChanged:ConnectionLost");
                DisConnect();
            }
            else if (e.Status == M4.Interfaces.UserCode.ConnectionStatus.DisconnectedByServer)
            {
                _Log.LogInfo("ConnectionStatusChanged:DisconnectedByServer");
                DisConnect();
            }
        }
        /// <summary>
        /// 系统返回消息，主要用于订单处理结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SystemMessageReceived(Client sender, SystemMessageReceivedEventArgs e)
        {
            _SystemMessage = e.Message;
            //_logger.Error("_SystemMessage="+ $"[{e.Message.MessageTime}] [{e.Message.MessageType}] {e.Message.Title} {(string.IsNullOrEmpty(e.Message.Message) ? string.Join(" ", e.Message.MessageItems) : e.Message.Message)}");
            if (_BgwOrderUpdate.IsBusy != true)
            {
                _BgwOrderUpdate.RunWorkerAsync();
            }
        }
        public IList<M4.Common.Classes.Position> myGetPosition()
        {
            IList<M4.Common.Classes.Position> orders = null;
            try
            {
                if (isConnect())
                {
                    //新版本对应函数
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
        /// <summary>
        /// 更新帐户资金信息
        /// </summary>
        public override void UpdateAccountCaptial()
        {
            if (isConnect())
            {
                _AccountDisplay.lblBlance.Text = _Client.AccountInfo.Balance.ToString();
                _AccountDisplay.lblEquity.Text = _Client.AccountInfo.TotalEquity.ToString();
                _AccountDisplay.lblMargin.Text = _Client.AccountInfo.UsedMargin.ToString();
                _AccountDisplay.lblFreeMargin.Text = _Client.AccountInfo.UsableMargin.ToString();
            }
        }

        /// <summary>
        /// 更新持仓列表
        /// 盈亏计算 蔡秋伏 caiqiufu 20210114
        /// 卖出价=Bid，买入价=Ask
        /// 卖出单：
        /// point = 开仓价-买入价(Ask)
        /// profit = point*100
        /// 买入单：
        /// point = 卖出价(Bid)-开仓价
        /// profit = point*100
        /// </summary>
        public override void UpdatePositionGrid()
        {
            IList<M4.Common.Classes.Position> orders = myGetPosition();
            if (_Quote != null && orders != null && orders.Count > 0)
            {
                foreach (var order in orders)
                {
                    double points = order.BuySell == 0 ? (double)(order.OpenPrice - _Quote.AskPrice) : (double)(_Quote.BidPrice - order.OpenPrice);
                    bool IsExistOrder = false;
                    for (int i = 0; i < _GvPositions.Rows.Count; i++)
                    {
                        if (order.ServerPositionRef.ToString().Equals(_GvPositions.Rows[i].Cells[1].Value.ToString()))
                        {
                            IsExistOrder = true;
                            _GvPositions.Rows[i].Cells[6].Value = points.ToString("f2");
                            _GvPositions.Rows[i].Cells[7].Value = order.Profit;
                            //gvPositions.Rows[i].Cells[7].Value = points*100;
                            TimeSpan ts = _Client.ServerTime - order.CreationTime;
                            string h = ts.Hours.ToString().PadLeft(2, '0');
                            string m = ts.Minutes.ToString().PadLeft(2, '0');
                            string s = ts.Seconds.ToString().PadLeft(2, '0');
                            _GvPositions.Rows[i].Cells[8].Value = h + ":" + m + ":" + s;
                        }
                    }
                    if (!IsExistOrder)
                    {
                        TimeSpan ts = _Client.ServerTime - order.CreationTime;
                        string h = ts.Hours.ToString().PadLeft(2, '0');
                        string m = ts.Minutes.ToString().PadLeft(2, '0');
                        string s = ts.Seconds.ToString().PadLeft(2, '0');
                        TradeSide tside = order.BuySell;
                        //窗口被关闭，异步方法会报错
                        if (_GvPositions.ColumnCount > 0)
                        {
                            _GvPositions.Rows.Add(false, order.ServerPositionRef, order.Symbol, tside == TradeSide.Sell ? "卖出" : "买入", order.Lot, order.OpenPrice, points.ToString("f2"), order.Profit, h + ":" + m + ":" + s, order.CreationTime);
                        }
                    }
                }
                for (int j = 0; j < _GvPositions.Rows.Count; j++)
                {
                    bool IsDeleteOrder = true;
                    foreach (var order in orders)
                    {
                        if (order.ServerPositionRef.ToString().Equals(_GvPositions.Rows[j].Cells[1].Value.ToString()))
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

        /// <summary>
        /// 平台报价更新
        /// </summary>
        public override void UpdateQuote()
        {
            if (!_BgwBroberQuote.CancellationPending)
            {
                if (isConnect())
                {
                    uClient.Comm.Utils.UpdateQuoteDisplay(_QuotaDisplayPanel, _QuoteData, _Quote.BidPrice, _Quote.AskPrice);
                }
                else
                {
                    uClient.Comm.Utils.ClearBrokerDiff(_QuotaDisplayPanel);
                }
            }
        }
        /// <summary>
        /// 开仓
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        public override void OpenBuyOrder(bool _IsAutoOperationFlag, out DateTime _SendTime, decimal lots = 0.1m)
        {
            //Quote quote = _Client.DataProvider.GetQuote(_TradeSymbol);
            _SendTime = DateTime.Now;
            try
            {
                //_Log.LogInfo("发送订单时间：" + DateTime.Now.ToString("HH:mm:ss.fff"));
                _Client.PlaceOrder(_Quote, TradeSide.Buy, _Broker.TradePara.Slippage, lots);
            }
            catch (Exception ex)
            {
                string msg = string.Format("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[" + ((_IsAutoOperationFlag == true) ? "自动买多" : "手动买多") + "][发送]异常信息：{0}", ex.Message);
                _logger.Error(msg + "  " + ex.StackTrace);
                _Log.LogTradeRecord(_Account, msg);
                _Log.LogInfo(ex.Message);
            }
            decimal price = _Quote.AskPrice;
            string message = MessageFormatter.OrderSendMessage(_IsAutoOperationFlag, "BUY");
            if (_IsAutoOperationFlag)
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _DSQuote.BidDiff[0], _Client.AccountInfo.Balance.ToString()));
            }
            else
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _Client.AccountInfo.Balance.ToString()));
            }
        }
        /// <summary>
        /// 卖空开仓
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        public override void OpenSellOrder(bool _IsAutoOperationFlag, out DateTime _SendTime, decimal lots = 0.1m)
        {
            //Quote quote = _Client.DataProvider.GetQuote(_TradeSymbol);            
            _SendTime = DateTime.Now;
            try
            {
                //_Log.LogInfo("发送订单时间：" + DateTime.Now.ToString("HH:mm:ss.fff"));
                _Client.PlaceOrder(_Quote, TradeSide.Sell, _Broker.TradePara.Slippage, lots);
            }
            catch (Exception ex)
            {
                string msg = string.Format("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[" + ((_IsAutoOperationFlag == true) ? "自动卖空" : "手动卖空") + "][发送]异常信息：{0}", ex.Message);
                _logger.Error(msg + "  " + ex.StackTrace);
                _Log.LogTradeRecord(_Account, msg);
                _Log.LogInfo(ex.Message);
            }
            decimal price = _Quote.BidPrice;
            string message = MessageFormatter.OrderSendMessage(_IsAutoOperationFlag, "SELL");
            if (_IsAutoOperationFlag)
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _DSQuote.BidDiff[0], _Client.AccountInfo.Balance.ToString()));
            }
            else
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _Client.AccountInfo.Balance.ToString()));
            }
        }
        /// <summary>
        /// 平仓交易
        /// </summary>
        /// <param name="position"></param>
        /// <param name="price"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        public void CloseOrder(M4.Common.Classes.Position position, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            decimal lots = position.Lot;
            _SendTime = DateTime.Now;
            try
            {
                //_Log.LogInfo("发送订单时间：" + DateTime.Now.ToString("HH:mm:ss.fff"));
                _Client.LiquidateOrder(position, _Broker.TradePara.Slippage, lots);
            }
            catch (Exception ex)
            {
                string msg = string.Format("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[" + ((_IsAutoOperationFlag == true) ? "自动平仓" : "手动平仓") + "][发送]异常信息：{0}", ex.Message);
                _logger.Error(msg + "  " + ex.StackTrace);
                _Log.LogTradeRecord(_Account, msg);
                _Log.LogInfo(ex.Message);
            }
            string tradeType = position.BuySell == TradeSide.Buy ? "BUY_CLOSE" : "SELL_CLOSE";
            M4.Common.Classes.Quote quote = _Client.DataProvider.GetQuote(_TradeSymbol);
            decimal price = position.BuySell == TradeSide.Buy ? quote.BidPrice : quote.AskPrice;
            string message = MessageFormatter.OrderSendMessage(_IsAutoOperationFlag, tradeType);
            if (_IsAutoOperationFlag)
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _DSQuote.BidDiff[0], _Client.AccountInfo.Balance.ToString()));
            }
            else
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _Client.AccountInfo.Balance.ToString()));
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
            _ReTryOrderNum = 0;
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
            IList<M4.Common.Classes.Position> orders = myGetPosition();
            if (orders != null && orders.Count > 0)
            {
                foreach (var position in orders)
                {
                    if (chkBuyClose && position.BuySell == TradeSide.Buy && _DSQuote.BidDiff[0] <= (double)-_Broker.TradePara.BuyClose ||
                         chkSellClose && position.BuySell == TradeSide.Sell && _DSQuote.BidDiff[0] >= (double)_Broker.TradePara.SellClose)
                    {
                        CloseOrder(position, _IsAutoOperationFlag, out _SendTime);
                        mexecutedFlag = true;
                        ClosedPosition.Add(position);
                    }
                }
            }
            return mexecutedFlag;
        }
        //EA2的公共交易参数，作为全局变量保存
        public string _EA2_TradeRecord = "";
        /// <summary>
        /// 自动开单，自动锁单, 如果已有单,则不执行开单操作
        /// </summary>
        /// <param name="tradeType">1:openbuy,2:opensell,3:closebuy,4:closesell</param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        /// <returns></returns>
        public bool AutoTradeTransaction2(int tradeType, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            _ReTryOrderNum = 0;
            bool mexecutedFlag = false;
            _SendTime = DateTime.Now;
            switch (tradeType)
            {
                case 1:
                    if (_Client.OpenPositions == null || _Client.OpenPositions.Count == 0)
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
                    if (_Client.OpenPositions == null || _Client.OpenPositions.Count == 0)
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
                    IList<M4.Common.Classes.Position> orders1 = myGetPosition();
                    if (orders1 != null && orders1.Count > 0)
                    {
                        foreach (var position in orders1)
                        {
                            if (position.BuySell == TradeSide.Buy)
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
                    IList<M4.Common.Classes.Position> orders2 = myGetPosition();
                    if (orders2 != null && orders2.Count > 0)
                    {
                        foreach (var position in orders2)
                        {
                            if (position.BuySell == TradeSide.Sell)
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
            return mexecutedFlag;
        }

        /// <summary>
        /// 自动加仓开单,不判断是否有单存在
        /// </summary>
        /// <param name="tradeType">1:openbuy,2:opensell,3:closebuy,4:closesell</param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        /// <returns></returns>
        public bool AutoTradeTransaction3(int tradeType, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            _ReTryOrderNum = 0;
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
                    IList<M4.Common.Classes.Position> orders1 = myGetPosition();
                    if (orders1 != null && orders1.Count > 0)
                    {
                        foreach (var position in orders1)
                        {
                            if (position.BuySell == TradeSide.Buy)
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
                    IList<M4.Common.Classes.Position> orders2 = myGetPosition();
                    if (orders2 != null && orders2.Count > 0)
                    {
                        foreach (var position in orders2)
                        {
                            if (position.BuySell == TradeSide.Sell)
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
            return mexecutedFlag;
        }

        public int _ReTryOrderNum = 0;
        /// <summary>
        /// 订单状态更新
        /// </summary>
        /// <param name="systemMessage"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        public override void OrderUpdate(bool _IsAutoOperationFlag, DateTime _SendTime, string _LockOrderId)
        {
            executedFlag = false;
            SoundPlayer player;
            DateTime _ReceiveTime = DateTime.Now;
            long diffTimeDuration = Convert.ToInt32((_ReceiveTime - _SendTime).TotalMilliseconds);
            if (_SystemMessage != null)
            {
                string orderType = _SystemMessage.Title;
                Dictionary<string, object> dic = uClient.Comm.Utils.GetSystemMessageReceived(_SystemMessage);
                string tradeSide = dic["TradeSide"].ToString();
                //开仓被接纳
                if (Comm.Enum.MarketOrderStatus.Accepted.Contains(orderType))
                {
                    _CurrentPrice = Double.Parse(dic["Price"].ToString());
                    if (Comm.Enum.Buy.Contains(tradeSide))
                    {
                        _Log.LogTradeRecord(_Account, string.Format(MessageFormatter.OrderReceiveMessage(_IsAutoOperationFlag, "BUY"), _ReceiveTime.ToString("HH:mm:ss.fff"), dic["Price"], dic["Lots"], diffTimeDuration, _Client.AccountInfo.Balance.ToString()));
                    }
                    else
                    {
                        _Log.LogTradeRecord(_Account, string.Format(MessageFormatter.OrderReceiveMessage(_IsAutoOperationFlag, "SELL"), _ReceiveTime.ToString("HH:mm:ss.fff"), dic["Price"], dic["Lots"], diffTimeDuration, _Client.AccountInfo.Balance.ToString()));
                    }
                    _Log.LogInfo(string.Format("[{0}][{1}][开仓][成交][{2}]手数:{3}", _TradeSymbol, tradeSide, dic["Price"], dic["Lots"]));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\ok.wav");
                    player.Play();
                    string tradeType = Comm.Enum.Buy.Contains(tradeSide) ? "BUY" : "SELL";
                    string myTradeType = tradeType;
                    Task.Run(() =>
                    {
                        //只有EA自动开单才记录该日志
                        if (!string.IsNullOrEmpty(_EA2_TradeRecord))
                        {
                            //orderTypeDetail:openPrice:closePrice:profit
                            string[] split = _EA2_TradeRecord.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            DBHelper.saveTradeLogMF4("MF4", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", dic["ServerPositionRef"].ToString(), _EA2_TradeRecord, myTradeType, split[0], Convert.ToDouble(dic["Lots"]), Convert.ToDouble(dic["Price"]), Math.Round(Convert.ToDouble(split[2]), 2), Math.Round(Convert.ToDouble(split[3]), 2), Math.Round((double)_Client.AccountInfo.Balance, 2), (double)_Client.AccountInfo.TotalEquity, (double)_Client.AccountInfo.UsedMargin, (double)_Client.AccountInfo.UsableMargin, _LockOrderId);
                        }
                        else
                        {
                            DBHelper.saveTradeLogMF4("MF4", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", dic["ServerPositionRef"].ToString(), _EA2_TradeRecord, myTradeType, "", Convert.ToDouble(dic["Lots"]), Convert.ToDouble(dic["Price"]), -99, -99, Math.Round((double)_Client.AccountInfo.Balance, 2), (double)_Client.AccountInfo.TotalEquity, (double)_Client.AccountInfo.UsedMargin, (double)_Client.AccountInfo.UsableMargin, _LockOrderId);
                        }
                        //order_type:BUY_CLOSE,volume:0.3,open_price:2155.28,close_price:2158.79
                        string content = "order_type:" + myTradeType + ",volume:" + dic["Lots"] + ",open_price:" + _CurrentPrice + ",close_price:";
                        DBHelper.saveNotify("TR", dic["ServerPositionRef"].ToString(), content, "", _Account.UserCode);
                        UpdateAccountBalance();
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                                _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "开", tradeSide,
                                dic["Price"].ToString(), dic["Lots"].ToString(), _Client.AccountInfo.Balance.ToString(), "成交");
                        }
                    });
                }
                //平仓被接纳
                if (Comm.Enum.LiquidationOrderStatus.Accepted.Contains(orderType))
                {
                    string tradeType = Comm.Enum.Sell.Contains(tradeSide) ? "BUY_CLOSE" : "SELL_CLOSE";
                    _Log.LogTradeRecord(_Account, string.Format(MessageFormatter.OrderReceiveMessage(_IsAutoOperationFlag, tradeType), _ReceiveTime.ToString("HH:mm:ss.fff"), dic["Price"], dic["Lots"], diffTimeDuration, _Client.AccountInfo.Balance.ToString()));
                    _Log.LogInfo(string.Format("[{0}][{1}][平仓][成交][{2}]手数:{3}盈亏：{4}", _TradeSymbol, dic["LiquidationType"], dic["ExecutionTime"], dic["Lots"], dic["Profit"]));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\ok.wav");
                    player.Play();
                    string myTradeType = tradeType;
                    Task.Run(() =>
                    {
                        //只有EA自动开单才记录该日志
                        if (!string.IsNullOrEmpty(_EA2_TradeRecord))
                        {
                            //orderTypeDetail:openPrice:closePrice:profit
                            //由于开仓和平仓价格无法准确从交易客户端获取(有可能人为操作多个订单),所以profit数据取自服务端
                            string[] split = _EA2_TradeRecord.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            DBHelper.saveTradeLogMF4("MF4", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", dic["ServerPositionRef"].ToString(), _EA2_TradeRecord, myTradeType, split[0], Convert.ToDouble(dic["Lots"]), Math.Round(Convert.ToDouble(split[1]), 2), Math.Round(Convert.ToDouble(dic["Price"]), 2), Math.Round(Convert.ToDouble(split[3]), 2), Math.Round((double)_Client.AccountInfo.Balance, 2), (double)_Client.AccountInfo.TotalEquity, (double)_Client.AccountInfo.UsedMargin, (double)_Client.AccountInfo.UsableMargin, _LockOrderId);
                        }
                        else
                        {
                            DBHelper.saveTradeLogMF4("MF4", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", dic["ServerPositionRef"].ToString(), _EA2_TradeRecord, myTradeType, "", Convert.ToDouble(dic["Lots"]), -99, Convert.ToDouble(dic["Price"]), Convert.ToDouble(dic["Profit"]), Math.Round((double)_Client.AccountInfo.Balance, 2), (double)_Client.AccountInfo.TotalEquity, (double)_Client.AccountInfo.UsedMargin, (double)_Client.AccountInfo.UsableMargin, _LockOrderId);
                        }
                        //order_type:BUY_CLOSE,volume:0.3,open_price:2155.28,close_price:2158.79
                        string content = "order_type:" + myTradeType + ",volume:" + dic["Lots"] + ",open_price:" + _CurrentPrice + ",close_price:" + dic["Price"];
                        DBHelper.saveNotify("TR", dic["ServerPositionRef"].ToString(), content, "", _Account.UserCode);
                        _CurrentPrice = 0;
                        UpdateAccountBalance();
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                                _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "平", string.Equals(tradeType, "BUY_CLOSE") ? "买平" : "卖平",
                                dic["Price"].ToString(), dic["Lots"].ToString(), _Client.AccountInfo.Balance.ToString(), "成交");
                        }
                    });
                }
                //开仓被拒绝
                if (Comm.Enum.MarketOrderStatus.Rejected.Contains(orderType))
                {
                    string msgFormt = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[接收][拒绝][{0}][耗时:{1}ms]";
                    _Log.LogTradeRecord(_Account, string.Format(msgFormt, dic["Reason"], diffTimeDuration));
                    _Log.LogInfo(string.Format("[委托={0}][接收][拒绝][{1}][耗时:{2}ms]", dic["OrderType"], dic["Reason"], diffTimeDuration));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\alert.wav");
                    player.Play();
                    if (string.Equals(_OrderCreateType, "C") && _IsAutoOperationFlag)
                    {
                        if (_ReTryOrderNum < 3)
                        {
                            _Log.LogInfo("[" + tradeSide + "]失败,重试次数[" + (_ReTryOrderNum + 1) + "]");
                            _ReTryOrderNum++;
                            //延迟5s再重试
                            Thread.Sleep(5000);
                            if (Comm.Enum.Buy.Contains(tradeSide))
                            {
                                _Log.LogInfo("重新开仓买入");
                                executedFlag = false;
                                OpenBuyOrder(_IsAutoOperationFlag, out _SendTime, _Broker.TradePara.BuyLots);
                            }
                            else if (Comm.Enum.Sell.Contains(tradeSide))
                            {
                                _Log.LogInfo("重新开仓卖出");
                                executedFlag = false;
                                OpenSellOrder(_IsAutoOperationFlag, out _SendTime, _Broker.TradePara.BuyLots);
                            }
                            else
                            {
                                _Log.LogInfo("[" + tradeSide + "]订单需手动处理");
                            }
                        }
                    }
                    Task.Run(() =>
                    {
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                                _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "开", orderType,
                                dic["Price"].ToString(), dic["Lots"].ToString(), _Client.AccountInfo.Balance.ToString(), "订单被拒绝");
                        }
                    });
                }
                //平仓被拒绝
                if (Comm.Enum.LiquidationOrderStatus.Rejected.Contains(orderType))
                {
                    string msgFormt = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[接收][拒绝][{0}][耗时:{1}ms]";
                    _Log.LogTradeRecord(_Account, string.Format(msgFormt, dic["Reason"], diffTimeDuration));
                    _Log.LogInfo(string.Format("[委托={0}][接收][拒绝][{1}][耗时:{2}ms]", dic["LiquidationType"], dic["Reason"], diffTimeDuration));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\alert.wav");
                    player.Play();
                    if (string.Equals(_OrderCreateType, "C") && _IsAutoOperationFlag)
                    {
                        if (_ReTryOrderNum < 3)
                        {
                            _Log.LogInfo("[" + tradeSide + "]失败,重试次数[" + (_ReTryOrderNum + 1) + "]");
                            _ReTryOrderNum++;
                            IList<M4.Common.Classes.Position> orders1 = myGetPosition();
                            if (orders1 != null && orders1.Count > 0)
                            {
                                foreach (var position in orders1)
                                {
                                    _Log.LogInfo("重新平仓");
                                    executedFlag = false;
                                    CloseOrder(position, _IsAutoOperationFlag, out _SendTime);
                                }
                            }
                        }
                    }
                    Task.Run(() =>
                    {
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                                _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "平", orderType,
                                dic["Price"].ToString(), dic["Lots"].ToString(), _Client.AccountInfo.Balance.ToString(), "订单被拒绝" + dic["Reason"].ToString());
                        }
                    });
                }
                //挂单被拒绝
                if (Comm.Enum.PendingOrderStatus.Rejected.Contains(orderType))
                {
                    string msgFormt = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "]" + "[接收][拒绝][{0}][耗时:{1}ms]";
                    _Log.LogTradeRecord(_Account, string.Format(msgFormt, dic["Reason"], diffTimeDuration));
                    _Log.LogInfo(string.Format("[委托={0}][接收][拒接][{1}][耗时:{2}ms]", dic["LiquidationType"], dic["Reason"], diffTimeDuration));
                    player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\alert.wav");
                    player.Play();
                    Task.Run(() =>
                    {
                        if (_Account.TradePara.NotifyFlag)
                        {
                            HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                                _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), "手动挂单", orderType,
                                dic["Price"].ToString(), dic["Lots"].ToString(), _Client.AccountInfo.Balance.ToString(), "订单被拒绝" + dic["Reason"].ToString());
                        }
                    });
                }
            }
        }
        /// <summary>
        /// 更新账户余额
        /// </summary>
        public void UpdateAccountBalance()
        {
            //更新账户余额
            DBHelper.updateAccountBalance(_Broker.BrokerCode, _Account.UserCode, Math.Round((double)_Client.AccountInfo.Balance, 2), Math.Round((double)_Client.AccountInfo.TotalEquity, 2), Math.Round((double)_Client.AccountInfo.UsedMargin, 2), Math.Round((double)_Client.AccountInfo.UsableMargin, 2));
        }


        /// <summary>
        /// 根据订单号获取订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public M4.Common.Classes.Position GetOrder(string orderId)
        {
            M4.Common.Classes.Position o = null;
            IList<M4.Common.Classes.Position> orders = this.myGetPosition();
            if (orders != null && orders.Count > 0)
            {
                foreach (M4.Common.Classes.Position order in orders)
                {
                    if (order.ServerPositionRef.Equals(orderId))
                    {
                        o = order;
                    }
                }
            }
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
            var info = this._Client.DataProvider.GetInstrumentBySymbol(symbol);
            if (info == null)
            {
                List<Instrument> symbols = this._Client.DataProvider.Instruments;
                foreach (Instrument ins in symbols)
                {
                    _Log.LogInfo("交易商行情列表:" + ins.Symbol);
                }
                _Log.LogInfo("请设置正确的品类后重新订阅");
            }
            else
            {
                this._QuoteData = new QuotePanelData(symbol, (double)info.Point);
                this._Client.DataProvider.Subscribe(symbol);
                _Log.LogInfo("订阅交易商行情:" + symbol);
            }
        }
        /// <summary>
        /// 是否已连接
        /// </summary>
        /// <returns></returns>
        public override bool isConnect()
        {
            if (this._Client == null || !this._Client.IsConnected)
            {
                //_Log.LogInfo("平台商没有连接或未初始化:[_Client="+ _Client + "][_Quote="+ _Quote + "][IsConnected="+ _Client.IsConnected + "]");
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 获取平仓订单
        /// </summary>
        /// <param name="ServerPositionRef"></param>
        /// <returns></returns>
        public M4.Common.Classes.Position GetClosedPosition(string ServerPositionRef)
        {
            foreach (M4.Common.Classes.Position postion in this.ClosedPosition)
            {
                if (postion.ServerPositionRef.Equals(ServerPositionRef))
                {
                    return postion;
                }
            }
            return null;
        }
        public int count = 0;
        public void KeepConnectedStatus(Func<bool> login)
        {
            object syncRoot = new object();
            _Client.ConnectionStatusChanged += (status, message) =>
            {
                if (_Client.IsConnected)
                    return;
                lock (syncRoot)
                {
                    if (_Client.IsConnected)
                        return;
                    _Log.LogInfo("失去连接");
                    while (count < 3)
                    {
                        _Log.LogInfo($"正在第 {count} 次重试");
                        if (login())
                        {
                            count = 0;
                            SpinWait.SpinUntil(() => _Client.IsConnected);
                            _Log.LogInfo("重新登录成功");
                            break;
                        }
                        count++;
                    }
                }
            };
        }
        public override void NewQuote()
        {
            M4.Common.Classes.Quote quote = _QuoteUpdatedEventArgs.Quote;
            if (_lastQuote == null)
            {
                _lastQuote = quote;
                _quote = quote;
            }
            else
            {
                _quote = quote;
                //string symbol,decimal newBidPrice,decimal oldBidPrice,decimal newAskPrice,decimal oldAskPrice
                _QuoteData.NewQuoute(_QuoteData.Symbol, _quote.BidPrice, _lastQuote.BidPrice, _quote.AskPrice, _lastQuote.AskPrice);
                _lastQuote = _quote;
            }
        }
        /// <summary>
        /// 获取当前价格
        /// </summary>
        /// <param name="AutoLockTradeSide"></param>
        /// <returns></returns>
        public override double GetCurrentPrice(string AutoLockTradeSide)
        {
            return "BUY".Equals(AutoLockTradeSide) || "CLOSE_SELL".Equals(AutoLockTradeSide) ? (double)_Quote.AskAdjusted : (double)_Quote.BidAdjusted;
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

        /// <summary>
        /// 判断是否在可交易时间范围内
        /// </summary>
        /// <param name="tradeTimeWindow"></param>
        /// <returns></returns>
        public bool IsTradeTimeWindow(string tradeTimeWindow)
        {
            string[] times = tradeTimeWindow.Split('-');
            string _strWorkingDayAM = times[0];//工作时间上午08:30
            string _strWorkingDayPM = times[1];//工作时间上午16:30
            TimeSpan dspWorkingDayAM = DateTime.Parse(_strWorkingDayAM).TimeOfDay;
            TimeSpan dspWorkingDayPM = DateTime.Parse(_strWorkingDayPM).TimeOfDay;
            TimeSpan dspNow = DateTime.Now.TimeOfDay;
            if (dspNow > dspWorkingDayAM && dspNow < dspWorkingDayPM)
            {
                return true;
            }
            _Log.LogInfo(string.Format("交易策略[{0}]:[{1}]不满足", "交易窗口时间", tradeTimeWindow));
            return false;
        }

        public override void CheckPlatformConnectStatus()
        {
            if (!isConnect() && !_IsManullyDisconnect)
            {
                executedFlag = false;
                Task.Run(() =>
                {
                    //if (_Account.TradePara.NotifyFlag)
                    //{
                    //HttpClient.SendSMS(_MyConfig.NotifyNumber, "MF4系统自动断开连接,请及时处理", "", " ", " ", " ", " ", " ");
                    //    EmailHelper.SendEmail(_MyConfig.NotifyEmail, "[龙知易][平台断连][" + _Broker.BrokerName + "][" + _Account.UserCode + "][平台]自动断开连接,请及时处理", "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "][" + _Account.UserCode + "][平台]自动断开连接,请及时处理");
                    //}
                    //EmailHelper.SendEmail(_MyConfig.NotifyEmail, "[龙知易][平台断连][" + _Broker.BrokerName + "][" + _Account.UserCode + "][平台]自动断开连接,请及时处理", "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "][" + _Account.UserCode + "][平台]自动断开连接,请及时处理");
                });
            }
        }
    }
}
