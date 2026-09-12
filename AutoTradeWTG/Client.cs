using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using M4.Common.Classes;
using M4.Common.Enums;
using M4.Interfaces.UserCode;
using mFinance.Settings;

namespace WTG.Broker
{
    /// <summary>
    /// M4客户端 老版本
    /// </summary>
    public class Client : M4.Client.Client
    {
        public Client() : base()
        {
            Init();
        }
        public override void Init()
        {
            Core = CreateCore();
            RegisterEvents();
            ConnectionStatus = ConnectionStatus.Disconnected;
        }
        public static new ICore CreateCore()
        {
            var core = (ICore)new M4.Core.Core();
            core.Settings.CheckAndInitialiseDefaults();
            core.IsSaveSettingRequired = false;
            core.CurrentCluture = new CultureInfo(core.Settings.CurrentCulture);
            var versionInfo = FileVersionInfo.GetVersionInfo(Application.StartupPath + "\\WTG_mF4.exe");
            core.Settings.DisclaimerVersion = new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart).ToString();
            core.Settings.FxServerClientVersion = AssemblyName.GetAssemblyName("FxServerClient.dll").Version;
            return core;
        }
        private static Order CreateOrder(Instrument instrument, TradeSide tradeSide, TradeType tradeType, bool bsd, int slippage)
        {
            var order = new Order
            {
                Symbol = instrument.Symbol,
                ContractName = instrument.ContractName,
                ContractSize = instrument.ContractSize,
                BuySell = tradeSide,
                Side = tradeSide,
                Type = tradeType,
                Status = OrderStatus.New,
                RequestedAmount = FxConstants.InitLotSize * instrument.ContractSize,
                RequestedLot = FxConstants.InitLotSize,
                //BSD = bsd, //代码异常,先注释
                Slippage = slippage,
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
        public override void RegisterEvents()
        {
            Broker.ServerTimeReceived += (sender, e) => ServerTime = e.Value.ServerTime;
            Broker.AccountUpdate += (sender, e) => OnAccountUpdated(AccountInfo);
            DataProvider.ConnectionStatusChanged += (status, message) => OnConnectionStatusChanged(status, message);
            DataProvider.SystemMessageReceived += (sender, e) => OnSystemMessageReceived(e.Value);
            DataProvider.NewQuote += quote => OnQuoteUpdated(quote);
            Broker.OpenPositionsUpdated += (sender, e) => {
                foreach (var openPosition in OpenPositions)
                    OnOpenPositionUpdated(openPosition);
            };
            _timer.AutoReset = true;
            _timer.Interval = 1000;
            _timer.Elapsed += (sender, e) => ServerTime = ServerTime.AddSeconds(1.0);
            _timer.Start();
        }
        public override void UpdatePosition(Position position)
        {
            string liqRef = position.ServerPositionRef + "|";
            var order = Broker.Orders.Where(o => o.LiquidationRef.StartsWith(liqRef) && o.OrderType == TradeType.Limit && o.Status == OrderStatus.Received).FirstOrDefault();
            var order2 = Broker.Orders.Where(o => o.LiquidationRef.StartsWith(liqRef) && o.OrderType == TradeType.Stop && o.Status == OrderStatus.Received).FirstOrDefault();
            if (!(order is null))
            {
                position.LimitOrderRef = order.ServerOrderRef;
                position.LimitOrderPrice = order.RequestedPrice;
                position.LimitOrderGoodTill = order.GoodTill;
                position.LimitOrderAmount = order.RequestedAmount;
            }
            else
            {
                position.LimitOrderPrice = 0M;
                position.LimitOrderRef = string.Empty;
                position.LimitOrderGoodTill = TimeInForce.Unknown;
                position.LimitOrderAmount = 0M;
            }
            if (order2 != null)
            {
                position.StopOrderRef = order2.ServerOrderRef;
                position.StopOrderPrice = order2.RequestedPrice;
                position.StopOrderGoodTill = order2.GoodTill;
                position.StopOrderAmount = order2.RequestedAmount;
                position.TrailingStopPips = (order2.TrailingStopPips > 0) ? order2.TrailingStopPips : 0;
            }
            else
            {
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
        public override void PlaceOrder(Quote quote, TradeSide side, int slippage, decimal lot)
        {
            if (quote is null)
                throw new ArgumentNullException(nameof(quote));

            var instrument = GetInstrumentBySymbol(quote.Symbol);
            //var order = CreateOrder(instrument, side, TradeType.Market, false, slippage, 0, 0);  //新版本
            var order = CreateOrder(instrument, side, TradeType.Market, false, slippage); //老版本
            order.CurrentPrice = CalculatePriceToOpenTrade(quote, side, lot);
            order.PriceTag = quote.PriceTag;
            order.QuoteID = quote.QuoteID;
            //order.isUnrestrictedMarket = FxConstants.DefaultTradingMode == 2; //代码异常,先注释
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
        public override void PlaceOrder(string symbol, TradeSide side, decimal price, decimal lot, TradeType orderType, TimeInForce timeInForce)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentException($"'{nameof(symbol)}' cannot be null or empty", nameof(symbol));
            if (orderType != TradeType.Stop && orderType != TradeType.Limit)
                throw new ArgumentOutOfRangeException(nameof(orderType));

            var instrument = GetInstrumentBySymbol(symbol);
            var order = CreateOrder(instrument, side, TradeType.Market, false, 0);
            order.OrderType = orderType;
            order.Slippage = 0;
            order.GoodTill = timeInForce;
            order.Price = price;
            order.CurrentPrice = price;
            order.Quantity = lot * instrument.ContractSize;
            Broker.PlaceOrder(order);
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
        public override void LiquidateOrder(Position position, decimal stopPrice, decimal limitPrice, decimal stopLot, decimal limitLot, TimeInForce stopTimeInForce, TimeInForce limitTimeInForce)
        {
            if (position is null)
                throw new ArgumentNullException(nameof(position));

            decimal stopAmount = 0;
            decimal limitAmount = 0;
            decimal openPrice;
            if (stopLot != 0)
            {
                stopAmount = stopLot * position.ContractSize;
                openPrice = limitPrice;
            }
            else if (limitLot != 0)
            {
                limitAmount = limitLot * position.ContractSize;
                openPrice = stopPrice;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
            var instrument = GetInstrumentBySymbol(position.Symbol);
            string positionRef = position.ServerPositionRef;
            decimal amount = stopAmount + limitAmount;
            //Broker.SubmitLiquidationOrders(positionRef, position.Symbol, position.ContractName, position.ContractSize, amount, position.BuySell, openPrice, stopPrice, stopTimeInForce, limitPrice, limitTimeInForce,
            //	position.Amount, position.OpenPrice, -1, string.Empty, "-1", "-1", limitAmount, stopAmount); //新代码
            Broker.SubmitLiquidationOrders(positionRef, position.Symbol, position.ContractName, position.ContractSize, amount, position.BuySell, openPrice, stopPrice, stopTimeInForce, limitPrice, limitTimeInForce,
                position.Amount, position.OpenPrice, -1); // 老代码
        }
    }
}
