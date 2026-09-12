using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TradingAPI.MT4Server;

namespace RealTimeQuotes
{
	class Program
	{
		static void Main(string[] args)
		{
			//Logger.OnMsg += Logger_OnMsg;
			new Program().Run();
			//Program2.Main2(args);
		}

		private void Run()
		{
			//Srv();
			//OrderUpdate();
			Ping();
			Connect();
			//ConnectSrv();
			//MarketMany();
			//Symbols();
			//Modify2();
			//AccountProfit();
			//ModifyPending();
			//ModifyPendingSafe();
			//aidan2(); 
			//ExecutionSpeed();
			//CancelPending();
			//ConnectMultiThread();
			//OpenedOrders();
			//OnConnectDisconnect();
			//ModifyMarket();
			//OrderHistory();
			//PendingFilled();
			//Updates();
			//Market();
			//AccountProfitCheck();
			//IsDemo();
			//Market();
			//search();
			//Modify3();
			//OHLC();
			//DownloadQuoteHistory();
			//GetDemo();
			//Quotes();
			//OpenOrderOnQuote();
			//CloseAllOrders();
		}

		private void CloseAllOrders()
		{
			var qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			Console.WriteLine("Connecting...");
			qc.Connect();
			var oc = new OrderClient(qc);
			Console.WriteLine("Connected");
			foreach (var order in qc.GetOpenedOrders())
			{
				while (qc.GetQuote(order.Symbol) == null)
					Thread.Sleep(1);
				if (order.Type == Op.Buy)
				{
					double bid = qc.GetQuote(order.Symbol).Bid;
					var closed = oc.OrderClose(order.Symbol, order.Ticket, order.Lots, bid, 100);
					Console.WriteLine("Closed " + closed.Ticket + " " + closed.ClosePrice);
				}
				else if (order.Type == Op.Sell)
				{
					double ask = qc.GetQuote(order.Symbol).Ask;
					var closed = oc.OrderClose(order.Symbol, order.Ticket, order.Lots, ask, 100);
					Console.WriteLine("Closed " + closed.Ticket + " " + closed.ClosePrice);
				}
				else
				{
					oc.OrderDelete(order.Ticket, order.Type, order.Symbol, order.Lots, order.OpenPrice);
					Console.WriteLine("Closed " + order.Ticket);
				}
			}
		}

		private void OpenOrderOnQuote()
		{
			var qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			Console.WriteLine("Connecting...");
			qc.Connect();
			qc.Subscribe("EURUSDi");
			var s = qc.GetSymbolInfo("EURUSDi");
			Console.WriteLine("Connected");
			var oc = new OrderClient(qc);
			qc.OnQuote += OnQuote3;
			Console.WriteLine("Press any key...");
			Console.ReadKey();

		}

		Order Order;
		[MethodImpl(MethodImplOptions.Synchronized)]
		private void OnQuote3(object sender, QuoteEventArgs args)
		{
			if (Order != null)
				return;
			try
			{
				if (args.Bid > 1)
				{
					var qc = (QuoteClient)sender;
					var oc = qc.OrderClient;
					Order = oc.OrderSend(args.Symbol, Op.Buy, 0.01, args.Ask, 0, 0, 0, null, 123, new DateTime());
					Console.WriteLine("Order " + Order.Ticket + " opened " + " " + Order.OpenTime);
					double bid = qc.GetQuote(args.Symbol).Bid;
					Order = oc.OrderClose(Order.Symbol, Order.Ticket, Order.Lots, bid, 0);
					Console.WriteLine("Closed " + Order.Ticket + " " + Order.CloseTime);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}


		void DownloadQuoteHistory()
		{
			var qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			Console.WriteLine("Connecting");
			qc.Connect();
			qc.Subscribe("GBPUSD");
			Timeframe tf = Timeframe.H1;
			while (qc.ServerTime == new DateTime())
				Thread.Sleep(10);
			qc.OnQuoteHistory += new QuoteHistoryEventHandler(delegate (object sender, QuoteHistoryEventArgs args)
			{
				if (args.Bars.Length == 1 && args.Bars[0].Time == new DateTime(1970, 1, 1))
					return; //null message means that it's the last message
				foreach (Bar bar in args.Bars)
					Console.WriteLine(bar);
			});
			/* Both tests freeze. */
			Console.WriteLine("-Test 1-");
			qc.DownloadQuoteHistory("GBPUSD", tf, qc.ServerTime, 0);
			Console.WriteLine("-Test 2-");
			qc.DownloadQuoteHistory("GBPUSD", tf, qc.ServerTime, 3);
			Console.WriteLine("Press any key...");
			Console.ReadKey();
			qc.Disconnect();
		}

		void OHLC()
		{
			var qc = new QuoteClient(8223204, "Ploy0511", "t02dsg.exness.cloud", 443);
			qc.OnQuoteHistory += new QuoteHistoryEventHandler(delegate (object sender, QuoteHistoryEventArgs args)
			{
				if (args.Bars.Length == 1 && args.Bars[0].Time == new DateTime(1970, 1, 1))
					return; //null message means that it's the last message
				foreach (Bar bar in args.Bars)
					Console.WriteLine(bar);
			});
			Console.WriteLine("Connecting...");
			qc.Connect();
			Console.WriteLine("Connected to server");
			qc.Subscribe("EURUSD");
			// ServerTime update goes with quotes, wait for first quote
			while (qc.ServerTime == new DateTime())
				Thread.Sleep(10);
			//request 10 previuos bars
			Timeframe tf = Timeframe.M5;
			qc.DownloadQuoteHistory("EURUSD", tf, qc.ServerTime.AddMinutes(-10 * (int)tf), 0);
			Console.WriteLine("Press any key...");
			Console.ReadKey();
			qc.Disconnect();
		}




		void Modify3()
		{
			var qc = new QuoteClient(8223204, "Ploy0511", "t02dsg.exness.cloud", 443);
			qc.Connect();
			var orderClient = new OrderClient(qc);
			orderClient.OrderModify(244749487, "eurgbp", Op.Buy, 0.1, 0.89855, 0.7, 0.9, new DateTime());
		}


		void IsDemo()
		{
			var qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			Console.WriteLine("Connecting...");
			qc.Connect();
			Console.WriteLine(qc.AccountType);
			Console.WriteLine(qc.IsInvestor);
		}

		void AccountProfitCheck()
		{
			var qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			Console.WriteLine("Connecting...");
			qc.Connect();
			Console.WriteLine("Connected");
			var oc = new OrderClient(qc);
			//var symbol  = "EURUSD";
			//while (qc.GetQuote(symbol) == null)
			//	Thread.Sleep(10);
			qc.Subscribe("GBPUSD");
			//double ask = qc.GetQuote(symbol).Ask;
			//double sl = ask - 0.0001;
			//Console.WriteLine(sl);
			//Order order = oc.OrderSend(symbol, Op.Buy, 0.3, ask, 0, sl, 0, null, 0, new DateTime());
			//Console.WriteLine("Order " + order.Ticket + " opened");
			//order = oc.OrderSend(symbol, Op.Buy, 0.3, ask, 0, sl, 0, null, 0, new DateTime());
			//Console.WriteLine("Order " + order.Ticket + " opened");
			//order = oc.OrderSend(symbol, Op.Buy, 0.4, ask, 0, sl, 0, null, 0, new DateTime());
			//Console.WriteLine("Order " + order.Ticket + " opened");
			//qc.Subscribe(qc.Symbols);
			qc.OnQuote += (QuoteEventHandler)delegate (object sender, QuoteEventArgs args)
			{
				var client = (QuoteClient)sender;
				//Console.WriteLine(args);
				//Console.WriteLine(qc.GetBidTickValue(args));
				//if (qc.AccountProfit < -15)
				//	qc.ToString();
				//Console.Write(Math.Round(qc.AccountProfit) + " ");
				//Console.WriteLine(args);
				Console.WriteLine(client.AccountProfit);
				//	 + " " + qc.AccountCredit
				//	 + " " + qc.AccountEquity
				//	 + " " + qc.AccountFreeMargin
				//	 + " " + qc.AccountLeverage
				//	 + " " + qc.AccountMargin
				//	 + " " + qc.AccountProfit);
			};

			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}

		//private void Qc_OnQuote(object sender, QuoteEventArgs args)
		//{
		//	var qc = (QuoteClient)sender;
		//	//Console.WriteLine(args);
		//	Console.WriteLine(qc.AccountBalance
		//		 + " " + qc.AccountCredit
		//		 + " " + qc.AccountEquity
		//		 + " " + qc.AccountFreeMargin
		//		 + " " + qc.AccountLeverage
		//		 + " " + qc.AccountMargin
		//		 + " " + qc.AccountProfit);
		//}

		void Market()
		{
			var qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			Console.WriteLine("Connecting...");
			qc.Connect();
			string symbol = "GBPUSD";
			OrderClient oc = new OrderClient(qc);
			oc.PlacedManually = false;
			Console.WriteLine("Connected to server");
			while (qc.GetQuote(symbol) == null)
				Thread.Sleep(10);
			double ask = qc.GetQuote(symbol).Ask;
			Order order = oc.OrderSend(symbol, Op.Buy, 0.01, ask, 0, 0, 0, null, 123, new DateTime());
			Console.WriteLine("Order " + order.Ticket + " opened");
			Console.WriteLine("Press any key...");
			Console.ReadKey();
			double bid = qc.GetQuote(symbol).Bid;
			order = oc.OrderClose(order.Symbol, order.Ticket, order.Lots, bid, 0);
			Console.WriteLine("Closed " + order.Ticket + " " + order.CloseTime);
			Console.WriteLine("Press any key...");
			Console.ReadKey();
			qc.Disconnect();
		}

		void Updates()
		{
			var qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			qc.OnOrderUpdate += Qc_OnOrderUpdate1;
			Console.WriteLine("Connecting...");
			qc.Connect();
			Console.WriteLine("Connected");
			Console.ReadKey();
		}

		private void Qc_OnOrderUpdate1(object sender, OrderUpdateEventArgs update)
		{
			Console.WriteLine(update.Order.Ticket + " " + update.Action + " " + update.Order.Type);
		}


		private void PendingFilled()
		{
			QuoteClient qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			qc.OnOrderUpdate += Qc_OnOrderUpdate1;
			Console.WriteLine("Connecting...");
			qc.Connect();
			var oc = new OrderClientSafe(new OrderClient(qc));
			Console.WriteLine("Connected to server");
			while (qc.GetQuote("EURUSD") == null)
				Thread.Sleep(10);
			double ask = qc.GetQuote("EURUSD").Ask;
			Order order = oc.OrderSend("EURUSD", Op.BuyStop, 0.01, ask, 100, 0, 0, null, new DateTime());
			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}



		private void OrderHistory()
		{
			QuoteClient qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			//qc.GetSymbolInfo("").Ex.
			//qc.Connect();
			//while (qc.GetQuote("EURUSD") == null) //wait first quote because server time updates with each quote
			//	Thread.Sleep(10);
			foreach (var item in qc.DownloadOrderHistory(DateTime.Now.AddMonths(-10), DateTime.Now.AddDays(1)))
				Console.WriteLine(item.Ticket);
			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}

		void OnConnectDisconnect()
		{
			//Logger.OnMsg += Logger_OnMsg;
			QuoteClient qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			qc.OnConnect += Qc_OnConnect;
			qc.OnDisconnect += Qc_OnDisconnect;
			Console.WriteLine("Connecting...");
			qc.Connect();
			Console.WriteLine("Press any key...");
			Console.ReadKey();
			qc.Disconnect();
		}

		void ModifyMarket()
		{
			QuoteClient qc = new QuoteClient(61013955, "h0200Da6A", "193.42.110.91", 443); //robo demo
			Console.WriteLine("Connecting...");
			qc.Connect();
			var oc = new OrderClient(qc);
			Console.WriteLine("Connected to server");
			while (qc.GetQuote("EURUSD") == null)
				Thread.Sleep(10);
			double ask = qc.GetQuote("EURUSD").Ask;
			Order order = oc.OrderSend("EURUSD", Op.Buy, 0.01, ask, 100, 0, 0, null, 0, new DateTime());
			Console.WriteLine("Order " + order.Ticket + " opened");
			order = oc.OrderModify(order.Ticket, order.Symbol, order.Type, order.Lots, order.OpenPrice, ask - 0.001, 0, new DateTime());
			Console.WriteLine("Modified");
			Console.WriteLine("Press any key...");
			Console.ReadKey();
			qc.Disconnect();
		}

		private void Qc_OnDisconnect(object sender, DisconnectEventArgs args)
		{
			Console.WriteLine("Disconnected");
		}

		private void Qc_OnConnect(object sender, ConnectEventArgs args)
		{
			Console.WriteLine("Connected");
		}

		private void OpenedOrders()
		{
			QuoteClient qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			Console.WriteLine("Connecting...");
			qc.Connect();
			Console.WriteLine("Connected to server");
			new Thread((ThreadStart)delegate
			{
				for (int i = 0; i < 1000; i++)
				{
					qc.ReconnectAsync();
					while (!qc.Connected)
						Thread.Sleep(1);
					Console.WriteLine("Connected");
				}
			}).Start();
			Console.WriteLine("Press any key...");
			while (!Console.KeyAvailable)
			{
				if (qc.GetOpenedOrders().Length != 19)
					throw new Exception();
			}
		}

		private void ConnectMultiThread()
		{
			int[] users = { 8110178, 8475055 };
			string[] passwords = { "qxTfjsi4", "ABcd1234" };
			string[] hosts = { "52.22.84.239", "dc-d01-06.mt4servers.net" };
			int[] ports = { 443, 443 };
			for (int i = 0; i < users.Length; i++)
			{
				var ind = i;
				new Thread((ThreadStart)delegate
				{
					try
					{
						QuoteClient qc = new QuoteClient(users[ind], passwords[ind], hosts[ind], ports[ind]);
						qc.Connect();
						Console.WriteLine(users[ind] + " connected to server");
					}
					catch (Exception ex)
					{
						Console.WriteLine(users[ind] + " " + ex.Message);
					}
				}).Start();
			}
			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}

		private void ExecutionSpeed()
		{
			QuoteClient qc = new QuoteClient(8110178, "qxTfjsi4", "52.22.84.239", 443);
			Console.WriteLine("Connecting...");
			qc.Connect();
			Console.WriteLine("Connected to server");
			OrderClient oc = new OrderClient(qc);
			oc.OnOrderProgress += Oc_OnOrderProgress;
			while (qc.GetQuote("EURUSD") == null)
				Thread.Sleep(10);
			double ask = qc.GetQuote("EURUSD").Ask;
			Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.ffffff"));
			oc.OrderSendAsync("EURUSD", Op.Buy, 0.01, ask, 100, 0, 0, null, 0, new DateTime());
			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}

		private void Oc_OnOrderProgress(object sender, OrderProgressEventArgs args)
		{
			Console.WriteLine(args.Type + " " + DateTime.Now.ToString("hh.mm.ss.ffffff"));
		}

		private void aidan()
		{
			QuoteClient qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);

			OrderClient oc = new OrderClient(qc);
			Console.WriteLine("Connecting...");
			qc.Connect();
			string symbol = "GBPJPY";
			Console.WriteLine("Connected to server");

			int nPosition = 1;
			//This is result file of testing.
			FileStream fs = new FileStream("Output.txt", FileMode.OpenOrCreate, FileAccess.Write);
			StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
			sw.WriteLine("This times is calculated between response of order with arriving updated position list.\n");
			while (nPosition <= 10)
			{
				Console.WriteLine("=====Test" + nPosition.ToString() + "=====");
				while (qc.GetQuote(symbol) == null)
					Thread.Sleep(10);
				double ask = qc.GetQuote(symbol).Ask;
				Console.WriteLine(ask);
				double size = (double)qc.GetSymbolGroupParams(symbol).lot_min / 100;
				//get previous position count to confirm that new order will be successful
				int nPrevCnt = qc.GetOpenedOrders().Length;
				//order send
				Order order = oc.OrderSend(symbol, Op.Buy, size, ask, 0, 0, 0, null, 0, new DateTime());
				long nTickTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
				Console.WriteLine("Order ticket = " + order.Ticket + " " + qc.AccountProfit);
				//wait for increasing the positions count
				while (true)
				{
					if (qc.GetOpenedOrders().Length == nPrevCnt + 1)
						break;
				}
				//calculate the time spent getting updated position list
				long nEndTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
				Console.WriteLine(nEndTime - nTickTime);
				long result = nEndTime - nTickTime;
				nPosition++;
				sw.WriteLine(result.ToString() + "ms");

				//order close 
				oc.OrderClose(order.Symbol, order.Ticket, order.Lots, qc.GetQuote(symbol).Bid, 0);

				Thread.Sleep(1000);
			}
			sw.WriteLine("\nThis is result time of 10 positions.");
			sw.Flush();
			sw.Close();/*
      while (qc.GetQuote(symbol).Ask == ask)
        Thread.Sleep(1000);*/
			Console.WriteLine(qc.GetQuote(symbol).Ask);
			Console.WriteLine("\n\nTest Finished!!!");
			Console.WriteLine("Press any key...");
			Console.ReadKey();
			qc.Disconnect();
		}

		private void aidan2()
		{
			QuoteClient qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			OrderClient oc = new OrderClient(qc);
			Console.WriteLine("Connecting...");
			qc.Connect();
			string symbol = "EURUSD";
			Console.WriteLine("Connected to server");
			while (qc.GetQuote(symbol) == null)
				Thread.Sleep(10);
			double ask = qc.GetQuote(symbol).Ask;
			Console.WriteLine(ask);
			var start = DateTime.Now;
			Order order = oc.OrderSend(symbol, Op.Buy, 0.01, ask, 0, 0, 0, null, 0, new DateTime());
			Console.WriteLine("Order ticket = " + order.Ticket + " " + qc.AccountProfit + " Ms = " + DateTime.Now.Subtract(start).Milliseconds);
			start = DateTime.Now;
			bool found = false;
			foreach (var item in qc.GetOpenedOrders())
				if (item.Ticket == order.Ticket)
				{
					found = true;
					break;
				}
			if (found)
				Console.WriteLine("Order appeared in opened orders in " + DateTime.Now.Subtract(start).Milliseconds + " ms");
			else
				throw new Exception();
			start = DateTime.Now;
			oc.OrderClose(order.Symbol, order.Ticket, order.Lots, qc.GetQuote(symbol).Bid, 0);
			Console.WriteLine("Order closed  Ms = " + DateTime.Now.Subtract(start).Milliseconds);
			Console.WriteLine("Press any key...");
			Console.ReadKey();
			qc.Disconnect();
		}

		void Ping()
		{
			QuoteClient qc = new QuoteClient(2088574390, "sdev8zv", "54.154.19.145", 443);
			qc.Connect();
			Server[] ar;
			QuoteClient.LoadSrv(qc.LatestSrv, out ar);
			foreach (var item in ar)
				Console.WriteLine(item.Host + " " + item.ping);
		}

		void Symbols()
		{
			QuoteClient qc = new QuoteClient(8475055, "ABcd1234", "dc-d01-06.mt4servers.net", 443); //XMGlobal
			qc.Connect();
		}

		void Follow()
		{
			//<follow followUser="1900109306" password="md4hhyn" ip="ATFXGM1-Demo.srv">
			//<bind user="2096604936" password="1wphjgr" ip="Tradize-Demo.srv" followType="-2" followValue="1"/>

			List<QuoteClient> list = new List<QuoteClient>();
			foreach (var ln in File.ReadAllLines("mass.txt"))
			{
				if (ln.StartsWith("<follow"))
				{
					var srv = QuoteClient.LoadSrv(ReadStr(ln, "ip=\"", "\""));
					var qc = new QuoteClient(Read(ln, "followUser=\"", "\""), ReadStr(ln, "password=\"", "\""), srv.Host, srv.Port);
					qc.Connect();
					list.Add(qc);
				}
			}
		}

		string ReadStr(string ln, string str, string str2)
		{
			int beg = ln.IndexOf(str) + str.Length;
			int end;
			if (str2 == Environment.NewLine)
				end = ln.Length;
			else
				end = ln.IndexOf(str2, beg);
			return ln.Substring(beg, end - beg);
		}

		int Read(string ln, string str, string str2)
		{
			int beg = ln.IndexOf(str) + str.Length;
			int end = ln.IndexOf(str2, beg);
			var s = ln.Substring(beg, end - beg);
			if (s.ToLower().Contains("x"))
				return Convert.ToInt32(s, 16);
			else
				return Convert.ToInt32(s);
		}

		void OrderUpdate()
		{
			var qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			qc.OnOrderUpdate += Qc_OnOrderUpdate;
			qc.OnQuote += Qc_OnQuote;
			Console.WriteLine("Connecting...");
			qc.Connect();
			Server[] srv;
			var s = QuoteClient.LoadSrv(qc.LatestSrv, out srv);
			Console.WriteLine((double)qc.GetSymbolGroupParams("EURUSD").lot_step / 100);
			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}

		private void Qc_OnOrderUpdate(object sender, OrderUpdateEventArgs update)
		{
			var qc = (QuoteClient)sender;
			Console.WriteLine(update.Order.Ticket + " " + update.Action + " " + update.Order.Profit
				+ " " + qc.AccountBalance + " " + qc.AccountMargin + " " + qc.AccountEquity);
		}

		void MarketMany()
		{
			try
			{
				QuoteClient qc = new QuoteClient(6582578, "Abcd123456", "t01d00.exness.cloud", 443);
				Console.WriteLine("Connecting...");
				qc.Connect();
				qc.Disconnect();
				qc.Connect();
				string symbol = "EURUSDm";
				OrderClient oc = new OrderClient(qc);
				Console.WriteLine("Connected to server");
				while (qc.GetQuote(symbol) == null)
					Thread.Sleep(10);
				for (int i = 0; i < 10; i++)
				{
					new Thread((ThreadStart)delegate
					{
						double ask = qc.GetQuote(symbol).Ask;
						Order order = oc.OrderSend(symbol, Op.Buy, 0.1, ask, 0, 0, 0, null, 0, new DateTime());
						Console.WriteLine("Order " + order.Ticket + " opened");
						double bid = qc.GetQuote(symbol).Bid;
						oc.OrderClose(order.Symbol, order.Ticket, order.Lots, bid, 0);
						Console.WriteLine("Closed " + order.Ticket);
					}).Start();

				}
				Console.WriteLine("Press any key...");
				Console.ReadKey();
				qc.Disconnect();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine("Press any key...");
				Console.ReadKey();
			}

		}

		void AccountProfit()
		{
			var qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			Console.WriteLine("Connecting...");
			qc.Connect();
			Console.WriteLine("Connected");
			var g = qc.GetSymbolGroupParams("EURUSD");
			qc.OnQuote += Qc_OnQuote;
			qc.Subscribe("EURUSD");
			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}

		private void Qc_OnQuote(object sender, QuoteEventArgs args)
		{
			var qc = (QuoteClient)sender;
			//Console.WriteLine(args);
			Console.WriteLine(qc.AccountMargin + " " + qc.AccountProfit + " " + qc.AccountEquity);
		}


		void Quotes()
		{
			QC1 = new QuoteClient(61013955, "h0200Da6A", "185.10.45.25", 443);
			QC2 = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			Console.WriteLine("Connecting...");
			QC1.Connect();
			QC2.Connect();
			var oc1 = new OrderClient(QC1);
			var oc2 = new OrderClient(QC2);
			Console.WriteLine("Connected");
			QC1.OnQuote += Qc_OnQuote12;
			QC2.OnQuote += Qc_OnQuote12;
			QC1.Subscribe(Symbol);
			QC2.Subscribe(Symbol);
			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}

		QuoteClient QC1;
		QuoteClient QC2;
		QuoteEventArgs Q1;
		QuoteEventArgs Q2;
		Order Order1;
		string Symbol = "EURUSD";

		[MethodImpl(MethodImplOptions.Synchronized)]
		private void Qc_OnQuote12(object sender, QuoteEventArgs args)
		{
			if (args.Symbol != Symbol)
				return;
			var qc = (QuoteClient)sender;
			if (qc.User == 61013955)
				Q1 = args;
			if (qc.User == 8681572)
				Q2 = args;
			if (Q1 == null || Q2 == null)
				return;
			if (Q1.Ask < Q2.Ask)
			{
				if (Order1 != null)
					if (Order1.Type == Op.Sell)
					{
						Order1 = QC1.OrderClient.OrderClose(Order1.Symbol, Order1.Ticket, Order1.Lots, Q1.Ask, 100);
						Console.WriteLine("Order " + Order1.Ticket + " closed at " + Order1.ClosePrice);
						Order1 = null;
						return;
					}
				if (Order1 == null)
				{
					Order1 = QC1.OrderClient.OrderSend(Symbol, Op.Buy, 0.1, Q1.Ask, 100);
					Console.WriteLine("Order " + Order1.Ticket + " opened at " + Order1.OpenPrice);
				}
			}
		}

		void Modify2()
		{
			try
			{
				QuoteClient qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
				Console.WriteLine("Connecting...");
				qc.Connect();
				OrderClient oc = new OrderClient(qc);
				Console.WriteLine("Connected to server");
				while (qc.GetQuote("EURUSD") == null)
					Thread.Sleep(10);
				double ask = qc.GetQuote("EURUSD").Ask;
				Order order = oc.OrderSend("EURUSD", Op.Buy, 0.1, ask, 0, 0, 0, null, 0, new DateTime());
				Console.WriteLine("Order " + order.Ticket + " opened");

				oc.OrderModify(order.Ticket, order.Symbol, order.Type, order.Lots, order.OpenPrice, 1, 1.2, new DateTime());
				Console.WriteLine("Modified. Press any key...");
				Console.ReadKey();
				qc.Disconnect();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine("Press any key...");
				Console.ReadKey();
			}
		}

		void ModifyPendingSafe()
		{
			QuoteClient qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);


			Console.WriteLine("Connecting...");
			qc.Connect();
			var oc = new OrderClientSafe(new OrderClient(qc));
			oc.TradeTimeoutSafe = 300000;
			Console.WriteLine("Connected to server");
			while (qc.GetQuote("EURUSD") == null)
				Thread.Sleep(10);
			for (int i = 0; i < 1; i++)
			{
				double ask = qc.GetQuote("EURUSD").Ask;
				Order order = oc.OrderSend("EURUSD", Op.BuyStop, 0.01, ask + 0.01, 100, 0, 0, null, new DateTime());
				Console.WriteLine("Order " + order.Ticket + " opened");
				Thread.Sleep(300);
				try
				{
					ask = qc.GetQuote("EURUSD").Ask;
					order = oc.OrderModify(order.Ticket, order.Symbol, order.Type, order.Lots, ask + 0.02, 0, 0, new DateTime());
					Console.WriteLine("Modified");
					Thread.Sleep(300);
				}
				catch (ServerException ex)
				{
					Console.WriteLine(ex.Message);
				}
				catch (Exception ex)
				{
					throw ex;
				}
				try
				{
					oc.OrderDelete(order.Ticket, order.Type, order.Symbol, order.Lots, order.OpenPrice);
					Console.WriteLine("Deleted");
					Thread.Sleep(300);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Delete " + ex.Message);
					try
					{
						double bid = qc.GetQuote("EURUSD").Ask;
						oc.OrderClose("EURUSD", order.Ticket, order.Lots, bid, 100);
						Console.WriteLine("Closed");
					}
					catch (Exception e)
					{
						Console.WriteLine("Close " + e.Message);
					}
				}
			}
			Console.WriteLine("Press any key...");
			Console.ReadKey();
			qc.Disconnect();
		}

		void ModifyPending()
		{

			QuoteClient qc = new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
			Console.WriteLine("Connecting...");
			qc.Connect();
			var oc = new OrderClientSafe(new OrderClient(qc));
			Console.WriteLine("Connected to server");
			while (qc.GetQuote("EURUSD") == null)
				Thread.Sleep(10);
			for (int i = 0; i < 20; i++)
			{
				double ask = qc.GetQuote("EURUSD").Ask;
				Order order = oc.OrderSend("EURUSD", Op.BuyStop, 0.01, ask + 0.01, 100, 0, 0, null, new DateTime());
				Console.WriteLine("Order " + order.Ticket + " opened");
				Thread.Sleep(300);
				try
				{
					ask = qc.GetQuote("EURUSD").Ask;
					order = oc.OrderModify(order.Ticket, order.Symbol, order.Type, order.Lots, ask, 0, 0, new DateTime());
					Console.WriteLine("Modified");
					Thread.Sleep(300);
				}
				catch (ServerException ex)
				{
					Console.WriteLine(ex.Message);
				}
				catch (Exception ex)
				{
					throw ex;
				}
				try
				{
					oc.OrderDelete(order.Ticket, order.Type, order.Symbol, order.Lots, order.OpenPrice);
					Console.WriteLine("Deleted");
					Thread.Sleep(300);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Delete " + ex.Message);
					try
					{
						double bid = qc.GetQuote("EURUSD").Ask;
						oc.OrderClose("EURUSD", order.Ticket, order.Lots, bid, 100);
						Console.WriteLine("Closed");
					}
					catch (Exception e)
					{
						Console.WriteLine("Close " + e.Message);
					}
				}
			}
			Console.WriteLine("Press any key...");
			Console.ReadKey();
			qc.Disconnect();
		}

		void CancelPending()
		{
			QuoteClient qc = new QuoteClient(8475055, "ABcd1234", "dc-d01-06.mt4servers.net", 443);
			Console.WriteLine("Connecting...");
			qc.Connect();
			OrderClient oc = new OrderClient(qc);
			Console.WriteLine("Connected to server");
			Order order = oc.OrderSend("USDJPY", Op.SellLimit, 0.05, 105, 0, 0, 0, null, 0, new DateTime());
			Console.WriteLine("Order " + order.Ticket + " opened");
			oc.OrderDelete(order.Ticket, order.Type, order.Symbol, order.Lots, order.OpenPrice);
			Console.WriteLine("Deleted. Press any key...");
			Console.ReadKey();
		}

		void Srv()
		{
			//var s = QuoteClient.LoadSrv(@"C:\Users\Administrator\Downloads\Alpari-Pro.ECN-Demo.srv");
			Server[] ar;
			var s = QuoteClient.LoadSrv(@"ATFXGM1-Demo.srv", out ar);
			Console.WriteLine(s.Host);
		}

		void Connect()
		{
			Logger.OnMsg += Logger_OnMsg;
			QuoteClient qc = new QuoteClient(3011599, "led369369", "main.trade.best-leader.com", 443);
			Console.WriteLine("Connecting...");
			qc.Connect();
			Console.WriteLine("Connected to server. Balance = " + qc.AccountBalance);
			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}

		void qc_OnQuote(object sender, QuoteEventArgs args)
		{
			Console.WriteLine(args.Symbol + " " + args.Bid + " " + args.Ask);
		}

		void ConnectSrv()
		{
			try
			{
				Server[] slaves;
				MainServer primary = QuoteClient.LoadSrv(@"C:\Users\User\Downloads\exness.srv", out slaves);
				QuoteClient qc = Connect(primary, slaves, 8223204, "Ploy0511");
				Console.WriteLine("Connected to server");
				Console.WriteLine(qc.AccountBalance);
				qc.OnQuote += qc_OnQuote;
				qc.Subscribe("EURUSD");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			Console.WriteLine("Press any key...");
			Console.ReadKey();
		}

		QuoteClient Connect(MainServer primary, Server[] slaves, int user, string password)
		{
			Console.WriteLine("Connecting...");
			QuoteClient qc = new QuoteClient(user, password, primary.Host, primary.Port);
			try
			{
				qc.Connect();
				return qc;
			}
			catch (Exception)
			{
				Console.WriteLine("Cannot connect to main server");
				return ConnectSlaves(slaves, user, password);
			}
		}

		QuoteClient ConnectSlaves(Server[] slaves, int user, string password)
		{
			Console.WriteLine("Connecting to slaves...");
			foreach (var server in slaves)
			{
				QuoteClient qc = new QuoteClient(user, password, server.Host, server.Port);
				try
				{
					qc.Connect();
					return qc;
				}
				catch (Exception)
				{
				}
			}
			throw new Exception("Cannot connect to slaves");
		}


		static object Lock = new object();
		static void Logger_OnMsg(object sender, string msg, Logger.MsgType type)
		{
			lock (Lock)
			{
				string txt = type.ToString().PadLeft(5) + ": " + msg;
				if ((int)type > (int)Logger.MsgType.Info)
					Console.WriteLine(txt);
			}
		}

		static QuoteClient BuildQuoteClient()
		{
			return new QuoteClient(8681572, "zy3ojco", "mt4-demopro-dc1.roboforex.com", 443);
		}

		QuoteClient _currentQuoteClient;

		private void Run2()
		{
			_currentQuoteClient = BuildQuoteClient();
			_currentQuoteClient.OnConnect += OnConnected;
			_currentQuoteClient.OnDisconnect += OnDisconnected;
			Console.WriteLine("Connecting...");
			_currentQuoteClient.Connect();
			Thread.Sleep(10000);
			_currentQuoteClient.ReconnectAsync();
			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
		}

		private void OnDisconnected(object sender, DisconnectEventArgs args)
		{
			Console.WriteLine("Disconnected");
		}

		private void OnConnected(object sender, ConnectEventArgs args)
		{
			Console.WriteLine("Connected");
			for (int i = 0; i < 1; i++)
			{
				Console.WriteLine($"----- ExecuteFlow {i + 1} -------");
				ExecuteFlow();
				//Thread.Sleep(10 * 1000);
				//if (!((QuoteClient)sender).Connected)
				//	break;
			}
		}

		private void ExecuteFlow()
		{
			Console.WriteLine("----- Step 1 -------");
			Console.WriteLine($"AccountBalance: {_currentQuoteClient.AccountBalance}");
			Console.WriteLine($"AccountCredit: {_currentQuoteClient.AccountCredit}");
			Console.WriteLine($"AccountEquity: {_currentQuoteClient.AccountEquity}");
			Console.WriteLine($"AccountFreeMargin: {_currentQuoteClient.AccountFreeMargin}");
			Console.WriteLine($"AccountMargin: {_currentQuoteClient.AccountMargin}");
			Console.WriteLine($"AccountProfit: {_currentQuoteClient.AccountProfit}");

			Thread.Sleep(1000);
			Console.WriteLine("----- Step 2 -------");

			var orders = _currentQuoteClient.GetOpenedOrders();
			foreach (var order in orders)
			{
				Console.WriteLine($"Ticket: {order.Ticket}");
			}

			Thread.Sleep(1000);
			Console.WriteLine("----- Step 3 -------");

			Console.WriteLine($"Account currency: {_currentQuoteClient.Account.currency}");

			Thread.Sleep(1000);
			Console.WriteLine("----- Step 4 -------");

			foreach (var symbolName in _currentQuoteClient.Symbols)
			{
				Console.WriteLine($"symbolName: {symbolName}");
			}

			var historyOrders = _currentQuoteClient.DownloadOrderHistory(new DateTime(2020, 3, 24), new DateTime(2020, 3, 26)).ToList();
			foreach (var order in orders)
			{
				Console.WriteLine($"Ticket: {order.Ticket}");
			}
		}
	}
}
