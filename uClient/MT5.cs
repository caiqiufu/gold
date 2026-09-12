using mtapi.mt5;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using Test;
using Order = mtapi.mt5.Order;

namespace uClient.Comm
{
    public class MT5 : PlatformInf
    {
        public NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

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
        /// MT5API
        /// </summary>
        public MT5API _QC { set; get; }

        /// <summary>
        /// 报价更新
        /// </summary>
        public mtapi.mt5.Quote _lastQuote = null;

        public mtapi.mt5.Quote _quote = null;
        public mtapi.mt5.Quote _Quote { get { return _quote; } }

        public mtapi.mt5.Quote _QuoteEventArgs;

        public OrderProgress _OrderProgressEventArgs;

        public OrderUpdate _OrderUpdateEventArgs;

        /// <summary>
        /// 手工断开连接
        /// </summary>
        public bool _IsManullyDisconnect = false;
        /// <summary>
        /// 订单创建类型,H:主动创建的订单，C:锁仓创建的订单
        /// </summary>
        public string _OrderCreateType = "";

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
        /// 当前开单价格
        /// </summary>
        public double _CurrentPrice = 0;
        /// <summary>
        /// EA2的公共交易参数，作为全局变量保存
        /// </summary>
        public string _EA2_TradeRecord = "";

        /// <summary>
        /// MT5初始化
        /// </summary>
        /// <param name="config"></param>
        /// <param name="myConfig"></param>
        /// <param name="broker"></param>
        /// <param name="account"></param>
        /// <param name="tradeSymbol"></param>
        /// <param name="log"></param>
        /// <param name="bgwBroberQuote"></param>
        /// <param name="bgwOrderUpdate"></param>
        /// <param name="gvPositions"></param>
        /// <param name="quotaDisplayPanel"></param>
        /// <param name="accountDisplay"></param>
        public MT5(Config config, MyConfig myConfig, Comm.Broker broker, Account account, string tradeSymbol, Log log, BackgroundWorker bgwBroberQuote, BackgroundWorker bgwOrderUpdate, DataGridView gvPositions, QuotaDisplayPanel quotaDisplayPanel, AccountDisplay accountDisplay)
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
        /// 更新账户余额
        /// </summary>
        public void UpdateAccountBalance()
        {
            //更新账户余额
            DBHelper.updateAccountBalance(_Broker.BrokerCode, _Account.UserCode, Math.Round(_QC.Account.Balance, 2), Math.Round(_QC.AccountEquity, 2), Math.Round(_QC.AccountMargin, 2), Math.Round(_QC.AccountFreeMargin, 2));
        }

        /// <summary>
        /// MT5初始化
        /// </summary>
        public MT5()
        {
        }
        /// <summary>
        /// MT5连接
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
                _QC = new MT5API(ulong.Parse(_Account.UserCode), _Account.Password, _Broker.IP(_Account.Type), int.Parse(_Broker.Port(_Account.Type)));
                if (_QC != null)
                {
                    _QC.OnConnectProgress += OnConnectProgress;
                    _QC.OnQuote += QCOnQuote;
                    _QC.Connect();
                    _IsManullyDisconnect = false;
                    //_QC.OnOrderProgress += QCOnOrderProgress;
                    _QC.OnOrderUpdate += OnOrderUpdate;
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
        /// 检查平台连接状态
        /// </summary>
        public override void CheckPlatformConnectStatus()
        {

        }
        /// <summary>
        /// 平仓
        /// </summary>
        /// <param name="position"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void CloseOrder(Position position, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            throw new NotImplementedException();
        }

        public void Connect1()
        {
            var exam = new Examples();
            //exam.MarketOrder();

            var api = new MT5API(107448, "!Qf9aac7", "93.90.192.241", 1950);
            api.OnConnectProgress += OnConnectProgress;
            api.Connect();
            Console.WriteLine("Connected");
            Console.WriteLine("AccountBalance=" + api.Account.Balance);
            Console.WriteLine("AccountEquity=" + api.AccountEquity);
            Console.WriteLine("AccountMargin=" + api.AccountMargin);
            Console.WriteLine("AccountFreeMargin=" + api.AccountFreeMargin);
            api.OnOrderProgress += QCOnOrderProgress;
            api.OnOrderUpdate += OnOrderUpdate;
            Console.WriteLine("================================");

            string symbol = "XAUUSD.";
            api.Subscribe(symbol);
            api.OnQuote += QCOnQuote;
            var quote = api.GetQuote(symbol);
            Console.WriteLine("quote.ask" + quote.Ask);
            var positions = api.GetOpenedOrders();
            foreach (var position in positions)
            {
                Console.WriteLine($"Ticket N {position.Ticket}, Lots = {position.Lots}, OrderType = {position.OrderType}");
                api.OrderCloseAsync(1, position.Ticket, symbol, api.GetQuote(symbol).Bid, position.Lots, OrderType.Buy, 100);
            }

            api.OrderSendAsync(1, symbol, 0.1, double.NaN, OrderType.Sell, fillPolicy: FillPolicy.Any);
            Console.WriteLine("===Sell Done=============================");
            Task.Delay(1000).Wait();
            positions = api.GetOpenedOrders();
            long ticket = -1;
            foreach (var position in positions)
            {
                Console.WriteLine($"Ticket N {position.Ticket}, Lots = {position.Lots}, OrderType = {position.OrderType}");
                ticket = position.Ticket;
            }
            api.OrderCloseAsync(1, ticket, symbol, api.GetQuote(symbol).Bid, 0.01, OrderType.Buy, 100);
            Console.WriteLine("===Close Done=============================");
        }

        private void QCOnQuote(MT5API api, mtapi.mt5.Quote quote)
        {
            _QuoteEventArgs = quote;
            _quote = quote;
            if (_BgwBroberQuote.IsBusy != true)
            {
                try
                {
                    if (string.Equals(_TradeSymbol, quote.Symbol))
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
        /// 订单最终状态更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="update"></param>
        private void OnOrderUpdate(MT5API sender, OrderUpdate update)
        {
            _OrderUpdateEventArgs = update;
            if (_BgwOrderUpdate != null && _BgwOrderUpdate.IsBusy != true)
            {
                _BgwOrderUpdate.CancelAsync();
                _BgwOrderUpdate.RunWorkerAsync();
            }
        }
        /// <summary>
        /// 订单过程状态更新
        /// </summary>
        /// <param name="api"></param>
        /// <param name="progress"></param>
        private void QCOnOrderProgress(MT5API api, OrderProgress progress)
        {
            _OrderProgressEventArgs = progress;
            if (_BgwOrderUpdate.IsBusy != true)
            {
                _BgwOrderUpdate.CancelAsync();
                _BgwOrderUpdate.RunWorkerAsync();
            }

        }
        /// <summary>
        /// 连接状态更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnConnectProgress(MT5API sender, ConnectEventArgs args)
        {
            if (args.Exception != null)
            {
                _Log.LogInfo("OnConnect Exception" + args.Exception.Message);
                return;
            }
            if (args.Progress == ConnectProgress.Connected)
            {
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
            }
            if (args.Progress == ConnectProgress.Disconnect)
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
                    SoundPlayer player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\disconnect.wav");
                    player.Play();
                }
            }
        }
        /// <summary>
        /// 断开连接
        /// </summary>
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
        /// 获取当前价格
        /// </summary>
        /// <param name="AutoLockTradeSide"></param>
        /// <returns></returns>
        public override double GetCurrentPrice(string AutoLockTradeSide)
        {
            return "BUY".Equals(AutoLockTradeSide) || "CLOSE_SELL".Equals(AutoLockTradeSide) ? _Quote.Ask : _Quote.Bid;
        }
        /// <summary>
        /// 获取最新仓位信息,子类可以重写
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override IList<Position> GetPosition()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 判断连接状态
        /// </summary>
        /// <returns></returns>
        public override bool isConnect()
        {
            if (this._QC == null || !this._QC.Connected)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 更新报价到跳次列表
        /// </summary>
        public override void NewQuote()
        {
            mtapi.mt5.Quote quote = _QuoteEventArgs;
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
        ///开仓买入
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        /// <param name="lots"></param>
        public override void OpenBuyOrder(bool _IsAutoOperationFlag, out DateTime _SendTime, decimal lots = 0.1M)
        {
            double price = _QC.GetQuote(_TradeSymbol).Ask;
            _SendTime = DateTime.Now;
            try
            {

                int reqID = _QC.GetRequestId();
                //_QC.OrderSend(_TradeSymbol, (double)lots, price, OrderType.Buy, 0, 0, ulong.Parse(_Broker.TradePara.Slippage.ToString()),null,0);
                _QC.OrderSendAsync(reqID, _TradeSymbol, (double)lots, price, OrderType.Buy, 0, 0, ulong.Parse(_Broker.TradePara.Slippage.ToString()), fillPolicy: FillPolicy.Any);
                //api.OrderSendAsync(id, "EURUSD.DEMO", 0.01, ask, OrderType.Buy, 0, 0, 100, null, 0, FillPolicy.FillOrKill);
                //api.OrderSend(symbol, 0.01, api.GetQuote(symbol).Ask, OrderType.Buy, 0, 0, 1000, null, 0);
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
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _DSQuote.BidDiff[0], _QC.Account.Balance.ToString()));
            }
            else
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _QC.Account.Balance.ToString()));
            }
        }
        /// <summary>
        /// 开仓卖出
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        /// <param name="lots"></param>
        public override void OpenSellOrder(bool _IsAutoOperationFlag, out DateTime _SendTime, decimal lots = 0.1M)
        {
            _SendTime = DateTime.Now;
            double price = _QC.GetQuote(_TradeSymbol).Bid;
            try
            {
                //_QC.OrderSend(_TradeSymbol, (double)lots, price, OrderType.Sell, 0, 0, ulong.Parse(_Broker.TradePara.Slippage.ToString()), null, 0);
                int reqID = _QC.GetRequestId();
                _QC.OrderSendAsync(reqID, _TradeSymbol, (double)lots, price, OrderType.Sell, 0, 0, ulong.Parse(_Broker.TradePara.Slippage.ToString()), fillPolicy: FillPolicy.Any);
                //api.OrderSendAsync(id, "EURUSD.DEMO", 0.01, ask, OrderType.Buy, 0, 0, 100, null, 0, FillPolicy.FillOrKill);
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
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _DSQuote.BidDiff[0], _QC.Account.Balance.ToString()));
            }
            else
            {
                _Log.LogTradeRecord(_Account, string.Format(message, _SendTime.ToString("HH:mm:ss.fff"), price, lots, _QC.Account.Balance.ToString()));
            }
        }
        /// <summary>
        ///平仓
        /// </summary>
        /// <param name="order"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        public void CloseOrder(Order order, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            _SendTime = DateTime.Now;
            long ticket = order.Ticket;
            string symbol = order.Symbol;
            double lots = order.Lots;
            var quote = _QC.GetQuote(_TradeSymbol);
            double price = order.OrderType == OrderType.Buy ? quote.Bid : quote.Ask;
            try
            {
                Task.Run(() =>
                {
                    int reqID = _QC.GetRequestId();
                    //_QC.OrderClose(ticket, symbol, price, lots, order.OrderType, ulong.Parse(_Broker.TradePara.Slippage.ToString()));
                    _QC.OrderCloseAsync(reqID, ticket, symbol, price, lots, order.OrderType, ulong.Parse(_Broker.TradePara.Slippage.ToString()));
                    //api.OrderCloseAsync(reqID, 2873358, symbol, api.GetQuote(symbol).Bid, 0.04, OrderType.Buy, 100);
                    //api.OrderClose(item.Ticket, item.Symbol, api.GetQuote(symbol).Bid, item.Lots, item.OrderType, deviation: 1000);
                });
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

        /// <summary>
        ///指定手数平仓
        /// </summary>
        /// <param name="order"></param>
        /// <param name="closeLots"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        public void CloseOrder(Order order, double closeLots, bool _IsAutoOperationFlag, out DateTime _SendTime)
        {
            _SendTime = DateTime.Now;
            long ticket = order.Ticket;
            string symbol = order.Symbol;
            if (closeLots > order.Lots)
            {
                closeLots = order.Lots;
            }
            double lots = closeLots;
            var quote = _QC.GetQuote(_TradeSymbol);
            double price = order.OrderType == OrderType.Buy ? quote.Bid : quote.Ask;
            try
            {
                Task.Run(() =>
                {
                    int reqID = _QC.GetRequestId();
                    //_QC.OrderClose(ticket, symbol, price, lots, order.OrderType, ulong.Parse(_Broker.TradePara.Slippage.ToString()));
                    _QC.OrderCloseAsync(reqID, ticket, symbol, price, lots, order.OrderType, ulong.Parse(_Broker.TradePara.Slippage.ToString()));
                    //api.OrderCloseAsync(reqID, 2873358, symbol, api.GetQuote(symbol).Bid, 0.04, OrderType.Buy, 100);
                    //api.OrderClose(item.Ticket, item.Symbol, api.GetQuote(symbol).Bid, item.Lots, item.OrderType, deviation: 1000);
                });
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

        /// <summary>
        /// 订单是否执行中
        /// </summary>
        public bool executedFlag = false;
        /// <summary>
        /// 订单最终状态更新
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        /// <param name="_LockOrderId"></param>
        public override void OrderUpdate(bool _IsAutoOperationFlag, DateTime _SendTime, string _LockOrderId)
        {
            executedFlag = false;
            SoundPlayer player;
            DateTime _ReceiveTime = DateTime.Now;
            Order order = _OrderUpdateEventArgs.Order;
            long diffTimeDuration = Convert.ToInt32((_ReceiveTime - _SendTime).TotalMilliseconds);
            if (_OrderUpdateEventArgs.Type == UpdateType.MarketOpen)
            {
                string tradeType = "";
                if (order.OrderType == OrderType.Buy)
                {
                    tradeType = "BUY";
                }
                if (order.OrderType == OrderType.Sell)
                {
                    tradeType = "SELL";
                }
                _Log.LogTradeRecord(_Account, string.Format(MessageFormatter.OrderReceiveMessage(_IsAutoOperationFlag, tradeType), _ReceiveTime.ToString("HH:mm:ss.fff"), order.OpenPrice, order.Lots, diffTimeDuration, _QC.Account.Balance.ToString()));
                string msg = string.Format(MessageFormatter.OrderReceiveMessage(_IsAutoOperationFlag, tradeType), _ReceiveTime.ToString("HH:mm:ss.fff"), order.OpenPrice, order.Lots, diffTimeDuration, _QC.Account.Balance.ToString());
                _Log.LogInfo(string.Format("[{0}][{1}][开仓][成交][{2}]手数:{3}", order.Symbol, order.OrderType, order.OpenPrice, order.Lots));
                player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\ok.wav");
                player.Play();
                Task.Run(() =>
                {
                    //只有EA自动开单才记录该日志
                    if (!string.IsNullOrEmpty(_EA2_TradeRecord))
                    {
                        //orderTypeDetail:openPrice:closePrice:profit
                        string[] split = _EA2_TradeRecord.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                        DBHelper.saveTradeLogMT4("MT5", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", order.Ticket.ToString(), _EA2_TradeRecord, tradeType, split[0], order.Lots, order.OpenPrice, Math.Round(Convert.ToDouble(split[2]), 2), 0, 0, Math.Round(_QC.Account.Balance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin, _LockOrderId);
                        _Log.LogInfo("保存交易日志完成");
                        //order_type:BUY_CLOSE,volume:0.3,open_price:2155.28,close_price:2158.79
                        string content = "order_type:" + tradeType + ",volume:" + order.Lots + ",open_price:" + order.OpenPrice + ",close_price:";
                        DBHelper.saveNotify("TR", order.Ticket.ToString(), content, "", _Account.UserCode);
                    }
                    else
                    {
                        DBHelper.saveTradeLogMT4("MT5", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", order.Ticket.ToString(), msg, tradeType, "", order.Lots, order.OpenPrice, -99, -99, 0, Math.Round(_QC.Account.Balance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin, _LockOrderId);
                        _Log.LogInfo("保存交易日志完成");
                    }
                    //更新账户余额
                    UpdateAccountBalance();
                    if (_Account.TradePara.NotifyFlag)
                    {
                        HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                            _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "开", order.OrderType.ToString(),
                            order.OpenPrice.ToString(), order.Lots.ToString(), _QC.Account.Balance.ToString(), "成交");
                    }
                });
            }
            else if (_OrderUpdateEventArgs.Type == UpdateType.MarketClose)
            {
                string tradeType1 = order.OrderType == OrderType.Buy ? "BUY_CLOSE" : "SELL_CLOSE";
                string msg1 = string.Format(MessageFormatter.OrderReceiveMessage(_IsAutoOperationFlag, tradeType1), _ReceiveTime.ToString("HH:mm:ss.fff"), order.ClosePrice, order.CloseLots, diffTimeDuration, _QC.Account.Balance.ToString());
                _Log.LogTradeRecord(_Account, msg1);
                _Log.LogInfo(string.Format("[{0}][{1}][平仓][成交][{2}]手数:{3}盈亏：{4}", order.Symbol, order.OrderType, order.ClosePrice, order.CloseLots, order.Profit));
                player = new System.Media.SoundPlayer(Application.StartupPath + "\\wav\\ok.wav");
                player.Play();
                Task.Run(() =>
                {
                    double points = order.OrderType == OrderType.Sell || order.OrderType == OrderType.SellLimit ? order.OpenPrice - _Quote.Ask : _Quote.Bid - order.OpenPrice;
                    //只有EA自动开单才记录该日志
                    if (!string.IsNullOrEmpty(_EA2_TradeRecord))
                    {
                        //orderTypeDetail:openPrice:closePrice:profit
                        //由于开仓和平仓价格无法准确从交易客户端获取(有可能人为操作多个订单),所以profit数据取自服务端
                        string[] split = _EA2_TradeRecord.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                        DBHelper.saveTradeLogMT4("MT5", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", order.Ticket.ToString(), _EA2_TradeRecord, tradeType1, split[0], order.CloseLots, order.OpenPrice, order.ClosePrice, Math.Round(points, 2), Math.Round(order.Profit, 2), Math.Round(_QC.Account.Balance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin, _LockOrderId);
                        _Log.LogInfo("保存交易日志完成");

                        //order_type:BUY_CLOSE,volume:0.3,open_price:2155.28,close_price:2158.79
                        string content = "order_type:" + tradeType1 + ",volume:" + order.CloseLots + ",open_price:" + _CurrentPrice + ",close_price:" + order.ClosePrice;
                        DBHelper.saveNotify("TR", order.Ticket.ToString(), content, "", _Account.UserCode);
                    }
                    else
                    {
                        DBHelper.saveTradeLogMT4("MT5", _Broker.BrokerCode, _Broker.BrokerName, _Account.UserCode, _IsAutoOperationFlag ? "Y" : "N", order.Ticket.ToString(), msg1, tradeType1, "", order.CloseLots, order.OpenPrice, order.ClosePrice, Math.Round(points, 2), order.Profit, Math.Round(_QC.Account.Balance, 2), _QC.AccountEquity, _QC.AccountMargin, _QC.AccountFreeMargin, _LockOrderId);
                        _Log.LogInfo("保存交易日志完成");
                    }
                    _CurrentPrice = 0;
                    //更新账户余额
                    UpdateAccountBalance();
                    if (_Account.TradePara.NotifyFlag)
                    {
                        HTTPHelper.sendNotify(_Account.TradePara.NotifyType, _MyConfig.NotifyNumber, _MyConfig.NotifyEmail, _Broker.BrokerName,
                            _Account.UserCode, _ReceiveTime.ToString("HH:mm:ss"), (_IsAutoOperationFlag ? "自" : "手") + "平", order.OrderType.ToString(),
                            order.ClosePrice.ToString(), order.CloseLots.ToString(), _QC.Account.Balance.ToString(), "成交");
                    }
                });
            }
            else
            {
                _Log.LogInfo($"_OrderUpdateEventArgs.Type{_OrderUpdateEventArgs.Type}不能识别");
            }
        }

        /// <summary>
        /// 自动交易,需要判断是否有单,平仓时要判断订单类型
        /// </summary>
        /// <param name="tradeType"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        /// <returns></returns>
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
                    Order[] orders1 = myGetPosition();
                    if (orders1 != null && orders1.Length > 0)
                    {
                        foreach (var order in orders1)
                        {
                            if (order.OrderType == OrderType.Buy)
                            {
                                CloseOrder(order, _IsAutoOperationFlag, out _SendTime);
                                mexecutedFlag = true;
                            }
                        }
                    }
                    break;
                case 4:
                    //自动平仓
                    Order[] orders2 = myGetPosition();
                    if (orders2 != null && orders2.Length > 0)
                    {
                        foreach (var order in orders2)
                        {
                            if (order.OrderType == OrderType.Sell)
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
        /// 指定交易手数
        /// </summary>
        /// <param name="tradeType"></param>
        /// <param name="lots"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        /// <returns></returns>
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
                    Order[] orders1 = myGetPosition();
                    if (orders1 != null && orders1.Length > 0)
                    {
                        foreach (var order in orders1)
                        {
                            if (order.OrderType == OrderType.Buy)
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
                    Order[] orders2 = myGetPosition();
                    if (orders2 != null && orders2.Length > 0)
                    {
                        foreach (var order in orders2)
                        {
                            if (order.OrderType == OrderType.Sell)
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
                    if (order.Ticket == Convert.ToInt64(orderId))
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
        /// 品类订阅
        /// </summary>
        /// <param name="symbol"></param>
        public override void SymbolSubscription(string symbol)
        {
            if (_QC != null)
            {
                //Dictionary<string, SymbolInfo> symbols = _QC.Symbols.Infos;
                IDictionary<string, SymbolInfo> symbols = _QC.Symbols.Infos;
                if (symbols != null && symbols.Count > 0)
                {
                    _Log.LogInfo("产品列表:");
                    string ss = "";
                    foreach (var s in symbols)
                    {
                        ss = ss + s.Key + ",";
                    }
                    _Log.LogInfo(ss);
                    if (ss.Contains(symbol))
                    {
                        _QC.Subscribe(symbol);
                        _Log.LogInfo("订阅平台行情:" + symbol);
                        var info = symbols[symbol];
                        _QuoteData = new QuotePanelData(symbol, (double)info.Points);
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
        /// 更新账户余额信息
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
                    _AccountDisplay.lblBlance.Text = Math.Round(_QC.Account.Balance, 2).ToString();
                    _AccountDisplay.lblEquity.Text = Math.Round(_QC.AccountEquity, 2).ToString();
                    _AccountDisplay.lblMargin.Text = Math.Round(_QC.AccountMargin, 2).ToString();
                    _AccountDisplay.lblFreeMargin.Text = Math.Round(_QC.AccountFreeMargin, 2).ToString();
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
        /// <summary>
        /// 更新仓位列表
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
                            double points = order.OrderType == OrderType.Sell ? order.OpenPrice - _Quote.Ask : _Quote.Bid - order.OpenPrice;
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
                                    _GvPositions.Rows.Add(false, order.Ticket, order.Symbol, order.OrderType == OrderType.Sell ? "卖出" : "买入", order.Lots, order.OpenPrice, points.ToString("f2"), order.Profit, h + ":" + m + ":" + s);
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
        /// 更新报价
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
    }
}
