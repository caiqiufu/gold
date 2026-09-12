using System;
using System.Linq;
using System.Threading;
using M4.Common.Enums;

namespace M4.Demo {
	internal static class Program {
		private static void Main() {
			using (var client = new Client()) {
				if (client.Login("90002057", "ab168168", false, "120.79.16.234", 20408, out string message)) {
					Console.WriteLine("登录成功");
					Console.WriteLine();
				}
				else {
					Console.WriteLine($"登录失败：{message}");
					Console.ReadKey(true);
					return;
				}
				// 登录

				client.SystemMessageReceived += Client_SystemMessageReceived;
				client.QuoteUpdated += Client_QuoteUpdated;
				client.OpenPositionUpdated += Client_OpenPositionUpdated;
				// 订阅事件

				//Console.WriteLine(Formatter.Format(client.AccountInfo));
				//Console.WriteLine();
				// 获取账户信息

				//Console.WriteLine(Formatter.Format(client.Quotes.First(t => t.DisplayName == "本地伦敦金")));
				//Console.WriteLine();
				// 获取报价

				var position = client.OpenPositions.FirstOrDefault();
				if (!(position is null)) {
					Console.WriteLine(Formatter.Format(position));
					Console.WriteLine();
				}
				// 获取持仓

				//client.PlaceOrder("LLGUSDUSD", TradeSide.Buy, client.Quotes.First(t => t.DisplayName == "本地伦敦金").Price - 1, 1m, TradeType.Limit, TimeInForce.Week);
				// 下单

				if (!(position is null))
					client.LiquidateOrder(position, 1, position.Lot);
				// 平仓

				Console.ReadKey(true);
				client.Logout();
			}
			Console.ReadKey(true);
		}

		/// <summary>
		/// 保持登录状态
		/// </summary>
		/// <param name="client"></param>
		/// <param name="login"></param>
		private static void KeepConnectedStatus(Client client, Func<bool> login) {
			object syncRoot = new object();
			client.ConnectionStatusChanged += (status, message) => {
				if (client.IsConnected)
					return;
				lock (syncRoot) {
					if (client.IsConnected)
						return;
					Console.WriteLine("失去连接");
					for (int i = 1; i <= 10; i++) {
						Console.WriteLine($"正在第 {i} 次重试");
						if (login()) {
							SpinWait.SpinUntil(() => client.IsConnected);
							Console.WriteLine("重新登录成功");
							break;
						}
					}
				}
			};
		}

		private static void Client_SystemMessageReceived(Client sender, SystemMessageReceivedEventArgs e) {
			Console.WriteLine($"{nameof(Client_SystemMessageReceived)}:");
			Console.WriteLine($"[{e.Message.MessageTime}] [{e.Message.MessageType}] {e.Message.Title} {(string.IsNullOrEmpty(e.Message.Message) ? string.Join(" ", e.Message.MessageItems) : e.Message.Message)}");
		}

		private static void Client_QuoteUpdated(Client sender, QuoteUpdatedEventArgs e) {
		}

		private static void Client_OpenPositionUpdated(Client sender, PositionUpdatedEventArgs e) {
		}
	}
}
