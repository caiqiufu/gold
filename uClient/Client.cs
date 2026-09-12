using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using M4.Common;
using M4.Common.Classes;
using M4.Common.Enums;
using M4.IBroker;
using M4.Interfaces;
using M4.Interfaces.UserCode;
using mFinance.Settings;

///
/// 新版本程序,适合SUI版本
///



namespace M4.Client{
	/// <summary>
	/// 账户更新事件参数
	/// </summary>
	public  class AccountUpdatedEventArgs : EventArgs {
		/// <summary>
		/// 账户信息
		/// </summary>
		public AccountInfo AccountInfo { get; }

		public AccountUpdatedEventArgs(AccountInfo accountInfo) {
			AccountInfo = accountInfo ?? throw new ArgumentNullException(nameof(accountInfo));
		}
	}

	/// <summary>
	/// 账户更新事件委托
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void AccountUpdatedEventHandler(Client sender, AccountUpdatedEventArgs e);

	/// <summary>
	/// 连接状态改变事件参数
	/// </summary>
	public  class ConnectionStatusChangedEventArgs : EventArgs {
		/// <summary>
		/// 更新后的连接状态
		/// </summary>
		public ConnectionStatus Status { get; }

		/// <summary>
		/// 消息
		/// </summary>
		public string Message { get; }

		public ConnectionStatusChangedEventArgs(ConnectionStatus status, string message) {
			Status = status;
			Message = message;
		}
	}

	/// <summary>
	/// 连接状态改变事件委托
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void ConnectionStatusChangedEventHandler(Client sender, ConnectionStatusChangedEventArgs e);

	/// <summary>
	/// 系统消息事件参数
	/// </summary>
	public  class SystemMessageReceivedEventArgs : EventArgs {
		/// <summary>
		/// 系统消息
		/// </summary>
		public SystemMessage Message { get; }

		public SystemMessageReceivedEventArgs(SystemMessage message) {
			Message = message ?? throw new ArgumentNullException(nameof(message));
		}
	}

	/// <summary>
	/// 系统消息事件委托
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void SystemMessageReceivedEventHandler(Client sender, SystemMessageReceivedEventArgs e);

	/// <summary>
	/// 仓位更新事件参数
	/// </summary>
	public  class PositionUpdatedEventArgs : EventArgs {
		/// <summary>
		/// 当前仓位
		/// </summary>
		public Position Position { get; }

		public PositionUpdatedEventArgs(Position position) {
			Position = position ?? throw new ArgumentNullException(nameof(position));
		}
	}

	/// <summary>
	/// 仓位更新事件委托
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void PositionUpdatedEventHandler(Client sender, PositionUpdatedEventArgs e);

	/// <summary>
	/// 报价更新事件参数
	/// </summary>
	public  class QuoteUpdatedEventArgs : EventArgs {
		/// <summary>
		/// 当前报价
		/// </summary>
		public Quote Quote { get; }

		public QuoteUpdatedEventArgs(Quote quote) {
			Quote = quote ?? throw new ArgumentNullException(nameof(quote));
		}
	}

	/// <summary>
	/// 报价更新事件委托
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void QuoteUpdatedEventHandler(Client sender, QuoteUpdatedEventArgs e);

	/// <summary>
	/// M4客户端
	/// </summary>
	public class Client : IDisposable {
		public readonly System.Timers.Timer _timer = new System.Timers.Timer();
		public bool _isDisposed;

		/// <summary>
		/// 核心接口（所有接口集合）
		/// </summary>
		public ICore Core { get; set; }

		/// <summary>
		/// 运行时代理接口
		/// </summary>
		public IBroker.IBroker Broker => Core.Broker;

		/// <summary>
		/// 数据接口
		/// </summary>
		public IDataProvider DataProvider => Core.DataProvider;

		/// <summary>
		/// 是否连接到服务器
		/// </summary>
		public bool IsConnected => DataProvider.IsConnected;

		/// <summary>
		/// 当前账户信息
		/// </summary>
		public AccountInfo AccountInfo => Broker.AccountInfo;

		/// <summary>
		/// 当前连接状态
		/// </summary>
		public ConnectionStatus ConnectionStatus { get; set; }

		/// <summary>
		/// 当前服务器时间
		/// </summary>
		public DateTime ServerTime {
			get => Broker.CurrentServerTime;
			set => Broker.CurrentServerTime = value;
		}

		/// <summary>
		/// 当前报价列表
		/// </summary>
		public IList<Quote> Quotes => DataProvider.LatestQuotesList;

		/// <summary>
		/// ?
		/// </summary>
		public IList<Instrument> Instruments => DataProvider.Instruments;

		/// <summary>
		/// 当前持有仓位列表
		/// </summary>
		public virtual IList<Position> OpenPositions => UpdatePositions(Broker.OpenPositionsNew);

		/// <summary>
		/// 账户信息更新事件
		/// </summary>
		public event AccountUpdatedEventHandler AccountUpdated;

		/// <summary>
		/// 连接状态改变事件
		/// </summary>
		public event ConnectionStatusChangedEventHandler ConnectionStatusChanged;

		/// <summary>
		/// 系统消息事件
		/// </summary>
		public event SystemMessageReceivedEventHandler SystemMessageReceived;

		/// <summary>
		/// 报价更新事件
		/// </summary>
		public event QuoteUpdatedEventHandler QuoteUpdated;

		/// <summary>
		/// 持有仓位更新事件
		/// </summary>
		public event PositionUpdatedEventHandler OpenPositionUpdated;

		/// <summary />
		public Client() 
		{
			//所有之类会调用该构造方法,针对每个平台的初始化由之类实现
            //Init();
        }
		public virtual void Init()
		{
			Core = CreateCore();
			RegisterEvents();
			ConnectionStatus = ConnectionStatus.Disconnected;
		}

		public static ICore CreateCore() {
			var core = (ICore)new Core.Core();
			core.Settings.CheckAndInitialiseDefaults();
			core.IsSaveSettingRequired = false;
			core.CurrentCluture = new CultureInfo(core.Settings.CurrentCulture);
			var versionInfo = FileVersionInfo.GetVersionInfo(Application.StartupPath + "\\SUI_mF4.exe");
			core.Settings.DisclaimerVersion = new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart).ToString();
			core.Settings.FxServerClientVersion = AssemblyName.GetAssemblyName("FxServerClient.dll").Version;
			return core;
		}

		public virtual void RegisterEvents() {
			Broker.ServerTimeReceived += (sender, e) => ServerTime = e.Value.ServerTime;
			Broker.AccountUpdate += (sender, e) => OnAccountUpdated(AccountInfo);
			DataProvider.ConnectionStatusChanged += (status, message) => OnConnectionStatusChanged(status, message);
			DataProvider.SystemMessageReceived += (sender, e) => {
				foreach (var message in e.Value)
					OnSystemMessageReceived(message);
			};
			DataProvider.NewQuote += quote => OnQuoteUpdated(quote);
			Broker.OpenPositionsUpdated += (sender, e) => {
				foreach (var openPosition in OpenPositions)
					OnOpenPositionUpdated(openPosition);
			};
			_timer.AutoReset = true;
			_timer.Interval = 1000;//要分析该数值的作用
			_timer.Elapsed += (sender, e) => ServerTime = ServerTime.AddSeconds(1.0);
			_timer.Start();
		}

		public void OnConnectionStatusChanged(ConnectionStatus status, string message) {
			ConnectionStatus = status;
			ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(status, message));
		}

		public void OnSystemMessageReceived(SystemMessage message) {
			SystemMessageReceived?.Invoke(this, new SystemMessageReceivedEventArgs(message));
		}

		public void OnAccountUpdated(AccountInfo accountInfo) {
			AccountUpdated?.Invoke(this, new AccountUpdatedEventArgs(accountInfo));
		}

		public void OnQuoteUpdated(Quote quote) {
			QuoteUpdated?.Invoke(this, new QuoteUpdatedEventArgs(quote));
		}

		public void OnOpenPositionUpdated(Position position) {
			OpenPositionUpdated?.Invoke(this, new PositionUpdatedEventArgs(position));
		}

		/// <summary>
		/// 登录
		/// </summary>
		/// <param name="userName">用户名</param>
		/// <param name="password">密码</param>
		/// <param name="isLive">是否真实账户，true表示真实账户，false表示模拟账户</param>
		/// <param name="message">登录失败时服务器返回的消息</param>
		/// <param name="timeout">登录超时多少毫秒后返回失败</param>
		/// <returns></returns>
		public bool Login(string userName, string password, bool isLive, out string message, int timeout = 10 * 1000) {
			return Login(userName, password, isLive, 0, out message, timeout);
		}

		/// <summary>
		/// 登录
		/// </summary>
		/// <param name="userName">用户名</param>
		/// <param name="password">密码</param>
		/// <param name="isLive">是否真实账户，true表示真实账户，false表示模拟账户</param>
		/// <param name="pathIndex">服务器路径（0~?）</param>
		/// <param name="message">登录失败时服务器返回的消息</param>
		/// <param name="timeout">登录超时多少毫秒后返回失败</param>
		/// <returns></returns>
		public bool Login(string userName, string password, bool isLive, int pathIndex, out string message, int timeout = 10 * 1000) {
			if (string.IsNullOrEmpty(userName))
				throw new ArgumentException($"'{nameof(userName)}' cannot be null or empty", nameof(userName));
			if (string.IsNullOrEmpty(password))
				throw new ArgumentException($"'{nameof(password)}' cannot be null or empty", nameof(password));

			if (timeout < 0)
				return LoginCore(userName, password, isLive, out message, pathIndex);
			string msg = message = null;
			var task = Task.Run(() => LoginCore(userName, password, isLive, out msg, pathIndex));
			int index = Task.WaitAny(task, Task.Delay(timeout));
			message = msg;
			return task.Result;
		}

		/// <summary>
		/// 登录
		/// </summary>
		/// <param name="userName">用户名</param>
		/// <param name="password">密码</param>
		/// <param name="isLive">是否真实账户，true表示真实账户，false表示模拟账户</param>
		/// <param name="ip">服务器IP</param>
		/// <param name="port">服务器端口</param>
		/// <param name="message">登录失败时服务器返回的消息</param>
		/// <param name="timeout">登录超时多少毫秒后返回失败</param>
		/// <returns></returns>
		public bool Login(string userName, string password, bool isLive, string ip, int port, out string message, int timeout = 10 * 1000) {
			if (string.IsNullOrEmpty(userName))
				throw new ArgumentException($"'{nameof(userName)}' cannot be null or empty", nameof(userName));
			if (string.IsNullOrEmpty(password))
				throw new ArgumentException($"'{nameof(password)}' cannot be null or empty", nameof(password));
			if (string.IsNullOrEmpty(ip))
				throw new ArgumentException($"'{nameof(ip)}' cannot be null or empty", nameof(ip));

			if (timeout < 0)
				return LoginCore(userName, password, isLive, out message, 0, ip, port);
			string msg = message = null;
			var task = Task.Run(() => LoginCore(userName, password, isLive, out msg, 0, ip, port));
			int index = Task.WaitAny(task, Task.Delay(timeout));
			message = msg;
			return task.Result;
		}

		public bool LoginCore(string userName, string password, bool isLive, out string message, int pathIndex = 0, string ip = null, int port = 0) {
			var userLogin = new FxServerAdapter.UserLogin {
				userName = userName,
				password = password,
				ic_ok = isLive ? "0" : "1",
				isHexDecode = FxConstants.HexDecode
			};
			var environment = isLive ? EnvironmentSettings.Live : EnvironmentSettings.Demo;
			if (!string.IsNullOrEmpty(ip)) {
				environment.ManualIP = ip;
				environment.ManualPort = port;
			}
			string result = null;
			DataProvider.Login(userLogin, pathIndex, environment, s => result = s);
			message = result;
			return string.IsNullOrEmpty(result);
		}

		/// <summary>
		/// 登出
		/// </summary>
		public void Logout() {
			Broker.SendLogoutMessages(false, Broker.Account.Login);
			DataProvider.Logout();
		}

		public IList<Position> UpdatePositions(IList<Position> positions) {
			foreach (var position in positions)
				UpdatePosition(position);
			return positions;
		}

		public virtual void UpdatePosition(Position position) {
			string liqRef = position.ServerPositionRef + "|";
			var order = Broker.OrdersNew.Where(o => o.LiquidationRef.StartsWith(liqRef) && o.OrderType == TradeType.Limit && o.Status == OrderStatus.Received).FirstOrDefault();
			var order2 = Broker.OrdersNew.Where(o => o.LiquidationRef.StartsWith(liqRef) && o.OrderType == TradeType.Stop && o.Status == OrderStatus.Received).FirstOrDefault();
			if (!(order is null)) {
				position.LimitOrderRef = order.ServerOrderRef;
				position.LimitOrderPrice = order.RequestedPrice;
				position.LimitOrderGoodTill = order.GoodTill;
				position.LimitOrderAmount = order.RequestedAmount;
			}
			else {
				position.LimitOrderPrice = 0M;
				position.LimitOrderRef = string.Empty;
				position.LimitOrderGoodTill = TimeInForce.Unknown;
				position.LimitOrderAmount = 0M;
			}
			if (order2 != null) {
				position.StopOrderRef = order2.ServerOrderRef;
				position.StopOrderPrice = order2.RequestedPrice;
				position.StopOrderGoodTill = order2.GoodTill;
				position.StopOrderAmount = order2.RequestedAmount;
				position.TrailingStopPips = (order2.TrailingStopPips > 0) ? order2.TrailingStopPips : 0;
			}
			else {
				position.StopOrderPrice = 0M;
				position.StopOrderRef = string.Empty;
				position.StopOrderGoodTill = TimeInForce.Unknown;
				position.StopOrderAmount = 0M;
				position.TrailingStopPips = 0;
			}
			var quotes = Quotes;
			position.Profit = position.GetFloatingProfit(Broker.Account, quotes, quotes.First(t => t.Symbol == position.Symbol));
		}

		/// <summary>
		/// 通过市场下单
		/// </summary>
		/// <param name="quote">报价</param>
		/// <param name="side">交易方向</param>
		/// <param name="slippage">滑点</param>
		/// <param name="lot">手数</param>
		public virtual void PlaceOrder(Quote quote, TradeSide side, int slippage, decimal lot) {
			if (quote is null)
				throw new ArgumentNullException(nameof(quote));
			var instrument = GetInstrumentBySymbol(quote.Symbol);
			var order = CreateOrder(instrument, side, TradeType.Market, false, slippage, 0, 0);
			order.CurrentPrice = CalculatePriceToOpenTrade(quote, side, lot);
			order.PriceTag = quote.PriceTag;
			order.QuoteID = quote.QuoteID;
			order.isUnrestrictedMarket = FxConstants.DefaultTradingMode == 2;
			order.Quantity = lot * instrument.ContractSize;
			Broker.PlaceOrder(order);
		}

		/// <summary>
		/// 通过指定价格下单
		/// </summary>
		/// <param name="symbol">合约符号</param>
		/// <param name="side">交易方向</param>
		/// <param name="price">价格</param>
		/// <param name="slippage">滑点</param>
		/// <param name="lot">手数</param>
		/// <param name="orderType">订单类型（有效值: <see cref="TradeType.Stop"/>, <see cref="TradeType.Limit"/>）</param>
		/// <param name="timeInForce">期限</param>
		public virtual void PlaceOrder(string symbol, TradeSide side, decimal price, decimal lot, TradeType orderType, TimeInForce timeInForce) {
			if (string.IsNullOrEmpty(symbol))
				throw new ArgumentException($"'{nameof(symbol)}' cannot be null or empty", nameof(symbol));
			if (orderType != TradeType.Stop && orderType != TradeType.Limit)
				throw new ArgumentOutOfRangeException(nameof(orderType));

			var instrument = GetInstrumentBySymbol(symbol);
			var order = CreateOrder(instrument, side, TradeType.Market, false, 0, 0, 0);
			order.OrderType = orderType;
			order.Slippage = 0;
			order.GoodTill = timeInForce;
			order.Price = price;
			order.CurrentPrice = price;
			order.Quantity = lot * instrument.ContractSize;
			Broker.PlaceOrder(order);
		}

		public static Order CreateOrder(Instrument instrument, TradeSide tradeSide, TradeType tradeType, bool bsd, int slippage, int slOrderPips, int tpOrderPips) {
			var order = new Order {
				Symbol = instrument.Symbol,
				ContractName = instrument.ContractName,
				ContractSize = instrument.ContractSize,
				BuySell = tradeSide,
				Side = tradeSide,
				Type = tradeType,
				Status = OrderStatus.New,
				RequestedAmount = instrument.MinTradeLot * instrument.ContractSize,
				RequestedLot = instrument.MinTradeLot,
				BSD = bsd,
				Slippage = slippage,
				SLOrderPips = slOrderPips,
				TPOrderPips = tpOrderPips,
				ServerOrderRef = string.Empty,
				OCORefs = new List<string>(),
				GoodTill = TimeInForce.Unknown,
				OrderType = tradeType,
				OrderID = string.Empty,
				OcoLimitGoodTill = TimeInForce.Unknown,
				OcoStopGoodTill = TimeInForce.Unknown,
				OcoStopPrice = -1,
				TrailingStopPips = -1,
				OcoLimitPrice = -1
			};
			return order;
		}

		/// <summary>
		/// 通过市场价清算订单
		/// </summary>
		/// <param name="position">持有仓位</param>
		/// <param name="slippage">滑点</param>
		/// <param name="lot">手数</param>
		public void LiquidateOrder(Position position, int slippage, decimal lot) {
			LiquidateOrder(position, Quotes.First(t => t.Symbol == position.Symbol), slippage, lot);
		}

		/// <summary>
		/// 通过市场价清算订单
		/// </summary>
		/// <param name="position">持有仓位</param>
		/// <param name="quote">报价</param>
		/// <param name="slippage">滑点</param>
		/// <param name="lot">手数</param>
		public void LiquidateOrder(Position position, Quote quote, int slippage, decimal lot) {
			if (position is null)
				throw new ArgumentNullException(nameof(position));
			if (quote is null)
				throw new ArgumentNullException(nameof(quote));

			var instrument = GetInstrumentBySymbol(position.Symbol);
			string positionRef = position.ServerPositionRef;
			decimal amount = lot * instrument.ContractSize;
			decimal price = CalculatePriceToCloseTrade(quote, position.BuySell, lot);
			Broker.ClosePosition(positionRef, amount, price, slippage, amount, position.OpenPrice, position.BuySell, quote.PriceTag, quote.QuoteID ?? string.Empty, FxConstants.DefaultTradingMode == 2);
		}

		/// <summary>
		/// 通过指定点数清算订单
		/// </summary>
		/// <param name="position">持有仓位</param>
		/// <param name="stopPricePips">止损点数</param>
		/// <param name="limitPricePips">止盈点数</param>
		/// <param name="stopLot">止损手数</param>
		/// <param name="limitLot">止盈手数</param>
		/// <param name="stopTimeInForce">止损期限</param>
		/// <param name="limitTimeInForce">止盈期限</param>
		public void LiquidateOrder(Position position, int stopPricePips, int limitPricePips, decimal stopLot, decimal limitLot, TimeInForce stopTimeInForce, TimeInForce limitTimeInForce) {
			LiquidateOrder(position, Quotes.First(t => t.Symbol == position.Symbol), stopPricePips, limitPricePips, stopLot, limitLot, stopTimeInForce, limitTimeInForce);
		}

		/// <summary>
		/// 通过指定点数清算订单
		/// </summary>
		/// <param name="position">持有仓位</param>
		/// <param name="quote">报价</param>
		/// <param name="stopPricePips">止损点数</param>
		/// <param name="limitPricePips">止盈点数</param>
		/// <param name="stopLot">止损手数</param>
		/// <param name="limitLot">止盈手数</param>
		/// <param name="stopTimeInForce">止损期限</param>
		/// <param name="limitTimeInForce">止盈期限</param>
		public void LiquidateOrder(Position position, Quote quote, int stopPricePips, int limitPricePips, decimal stopLot, decimal limitLot, TimeInForce stopTimeInForce, TimeInForce limitTimeInForce) {
			if (position is null)
				throw new ArgumentNullException(nameof(position));
			if (quote is null)
				throw new ArgumentNullException(nameof(quote));

			var instrument = GetInstrumentBySymbol(position.Symbol);
			decimal pipValue = GlobalHelper.GetPipValue(instrument.Digits);
			decimal openPrice = TradingCalculator.GetPriceToCloseTrade(position.BuySell, quote.BSD, quote.AskPriceAdjusted, quote.BidPriceAdjusted);
			decimal stopPrice = 0;
			decimal limitPrice = 0;
			if (stopLot != 0)
				stopPrice = (stopPricePips * pipValue) + openPrice;
			else if (limitLot != 0)
				limitPrice = (limitPricePips * pipValue) + openPrice;
			else
				throw new ArgumentOutOfRangeException();
			LiquidateOrder(position, stopPrice, limitPrice, stopLot, limitLot, stopTimeInForce, limitTimeInForce);
		}

		/// <summary>
		/// 通过指定价格清算订单
		/// </summary>
		/// <param name="position">持有仓位</param>
		/// <param name="stopPrice">止损价格</param>
		/// <param name="limitPrice">止盈价格</param>
		/// <param name="stopLot">止损手数</param>
		/// <param name="limitLot">止盈手数</param>
		/// <param name="stopTimeInForce">止损期限</param>
		/// <param name="limitTimeInForce">止盈期限</param>
		public virtual void LiquidateOrder(Position position, decimal stopPrice, decimal limitPrice, decimal stopLot, decimal limitLot, TimeInForce stopTimeInForce, TimeInForce limitTimeInForce) {
			if (position is null)
				throw new ArgumentNullException(nameof(position));

			decimal stopAmount = 0;
			decimal limitAmount = 0;
			decimal openPrice;
			if (stopLot != 0) {
				stopAmount = stopLot * position.ContractSize;
				openPrice = limitPrice;
			}
			else if (limitLot != 0) {
				limitAmount = limitLot * position.ContractSize;
				openPrice = stopPrice;
			}
			else {
				throw new ArgumentOutOfRangeException();
			}
			var instrument = GetInstrumentBySymbol(position.Symbol);
			string positionRef = position.ServerPositionRef;
			decimal amount = stopAmount + limitAmount;
			Broker.SubmitLiquidationOrders(positionRef, position.Symbol, position.ContractName, position.ContractSize, amount, position.BuySell, openPrice, stopPrice, stopTimeInForce, limitPrice, limitTimeInForce,
				position.Amount, position.OpenPrice, -1, string.Empty, "-1", "-1", limitAmount, stopAmount);
		}

		public static decimal CalculatePriceToOpenTrade(Quote quote, TradeSide side, decimal lot) {
			decimal askPrice = quote.AskPriceAdjusted;
			for (int i = 0; i < quote.OrderBook.Count; i++) {
				if (quote.OrderBook[i].Ask > 0m)
					askPrice = quote.OrderBook[i].Ask;
				if (quote.OrderBook[i].AskCumLot >= lot)
					break;
			}
			decimal bidPrice = quote.BidPriceAdjusted;
			for (int i = 0; i < quote.OrderBook.Count; i++) {
				if (quote.OrderBook[i].Bid > 0m)
					bidPrice = quote.OrderBook[i].Bid;
				if (quote.OrderBook[i].BidCumLot >= lot)
					break;
			}
			decimal price = TradingCalculator.GetPriceToOpenTrade(side, quote.BSD, askPrice, bidPrice);
			return price;
		}

		public static decimal CalculatePriceToCloseTrade(Quote quote, TradeSide side, decimal lot) {
			decimal askPrice = quote.AskPriceAdjusted;
			for (int i = 0; i < quote.OrderBook.Count; i++) {
				if (quote.OrderBook[i].Ask > 0m)
					askPrice = quote.OrderBook[i].Ask;
				if (quote.OrderBook[i].AskCumLot >= lot)
					break;
			}
			decimal bidPrice = quote.BidPriceAdjusted;
			for (int i = 0; i < quote.OrderBook.Count; i++) {
				if (quote.OrderBook[i].Bid > 0m)
					bidPrice = quote.OrderBook[i].Bid;
				if (quote.OrderBook[i].BidCumLot >= lot)
					break;
			}
			decimal price = TradingCalculator.GetPriceToCloseTrade(side, quote.BSD, askPrice, bidPrice);
			return price;
		}

		public Instrument GetInstrumentBySymbol(string symbol) {
			return DataProvider.GetInstrumentBySymbol(symbol);
		}

		/// <inheritdoc />
		public void Dispose() {
			if (_isDisposed)
				return;

			Core.Dispose();
			_timer.Stop();
			_timer.Dispose();
			_isDisposed = true;
		}
	}
}
