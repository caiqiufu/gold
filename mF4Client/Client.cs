using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using M4.Common.Classes;
using M4.Common.Enums;
using M4.IBroker;
using M4.Interfaces;
using M4.Interfaces.UserCode;
using mFinance.Settings;

namespace M4.Demo {
	/// <summary>
	/// M4客户端
	/// </summary>
	public sealed class Client : IDisposable {
		private readonly ICore _core;
		private readonly Timer _timer;
		private ConnectionStatus _connectionStatus;
		private bool _isDisposed;

		/// <summary>
		/// 核心接口（所有接口集合）
		/// </summary>
		public ICore Core => _core;

		/// <summary>
		/// 代理商接口
		/// </summary>
		public IBroker.IBroker Broker => _core.Broker;

		/// <summary>
		/// 数据接口
		/// </summary>
		public IDataProvider DataProvider => _core.DataProvider;

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
		public ConnectionStatus ConnectionStatus => _connectionStatus;

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
		public IList<Quote> LatestQuotesList => DataProvider.LatestQuotesList;

		/// <summary>
		/// 当前持有仓位
		/// </summary>
		public IList<Position> OpenPositionsNew => UpdatePositions(Broker.OpenPositionsNew);

		/// <summary>
		/// 账户信息更新事件
		/// </summary>
		public event EventHandler AccountUpdate {
			add => Broker.AccountUpdate += value;
			remove => Broker.AccountUpdate -= value;
		}

		/// <summary>
		/// 连接状态改变事件
		/// </summary>
		public event Action<ConnectionStatus, string> ConnectionStatusChanged {
			add => DataProvider.ConnectionStatusChanged += value;
			remove => DataProvider.ConnectionStatusChanged -= value;
		}

		/// <summary>
		/// 报价更新事件
		/// </summary>
		public event Action<Quote> NewQuote {
			add => DataProvider.NewQuote += value;
			remove => DataProvider.NewQuote -= value;
		}

		/// <summary>
		/// 当前持有仓位列表更新事件
		/// </summary>
		public event EventHandler<EventArgs> OpenPositionsUpdated {
			add => Broker.OpenPositionsUpdated += value;
			remove => Broker.OpenPositionsUpdated -= value;
		}

		/// <summary />
		public Client() {
			_core = CreateCore();
			RegisterEvents();
			_timer = CreateTimer();
			_connectionStatus = ConnectionStatus.Disconnected;
		}

		private static ICore CreateCore() {
			var core = (ICore)new Core.Core();
			core.Settings.CheckAndInitialiseDefaults();
			var cultureInfo = new CultureInfo(core.Settings.CurrentCulture);
			core.CurrentCluture = cultureInfo;
			var versionInfo = FileVersionInfo.GetVersionInfo("SUI_mF4.exe");
			core.Settings.DisclaimerVersion = new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart).ToString();
			core.Settings.FxServerClientVersion = AssemblyName.GetAssemblyName("FxServerClient.dll").Version;
			return core;
		}

		private Timer CreateTimer() {
			var timer = new Timer {
				AutoReset = true,
				Interval = 1000.0
			};
			timer.Elapsed += (_1, _2) => ServerTime = ServerTime.AddSeconds(1.0);
			timer.Start();
			return timer;
		}

		private void RegisterEvents() {
			DataProvider.ConnectionStatusChanged += (status, _) => _connectionStatus = status;
			Broker.ServerTimeReceived += (_, e) => ServerTime = e.Value.ServerTime;
		}

		/// <summary>
		/// 登录
		/// </summary>
		/// <param name="userName">用户名</param>
		/// <param name="password">密码</param>
		/// <param name="isLive">是否真实账户，true表示真实账户，false表示模拟账户</param>
		/// <param name="pathIndex">服务器路径（0~?）</param>
		/// <param name="timeout">登录超时多少毫秒后返回失败</param>
		/// <returns></returns>
		public (bool IsSuccessful, string Message) Login(string userName, string password, bool isLive, int pathIndex = 0, int timeout = 10 * 1000) {
			if (timeout < 0)
				return LoginCore(userName, password, isLive, pathIndex);
			var task = Task.Run(() => LoginCore(userName, password, isLive, pathIndex));
			return Task.WaitAny(task, Task.Delay(timeout)) == 0 ? task.Result : (false, "Login timeout");
		}

		private (bool IsSuccessful, string Message) LoginCore(string userName, string password, bool isLive, int pathIndex = 0) {
			var userLogin = new FxServerAdapter.UserLogin {
				userName = userName,
				password = password,
				ic_ok = isLive ? "0" : "1",
				isHexDecode = FxConstants.HexDecode
			};
			var environment = isLive ? EnvironmentSettings.Live : EnvironmentSettings.Demo;
			bool? isSuccessful = null;
			string message = null;
			DataProvider.Login(userLogin, pathIndex, environment, s => {
				message = s;
				isSuccessful = string.IsNullOrEmpty(s);
			});
			return (isSuccessful == true, message);
		}

		/// <summary>
		/// 登出
		/// </summary>
		public void Logout() {
			Broker.SendLogoutMessages(false, Broker.Account.Login);
			DataProvider.Logout();
		}

		private IList<Position> UpdatePositions(IList<Position> positions) {
			foreach (var position in positions)
				UpdatePosition(position);
			return positions;
		}

		private void UpdatePosition(Position position) {
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
		}

		/// <inheritdoc />
		public void Dispose() {
			if (_isDisposed)
				return;
			_core.Dispose();
			_timer.Stop();
			_timer.Dispose();
			_isDisposed = true;
		}
	}
}
