using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using uClient.Comm;
using V4;

namespace V4
{
    class V4Utils
    {
        static public XmlDocument xml = new XmlDocument();
        static public bool Login(string userName, string password, bool isLive, string ip, int port)
        {
            CppDll.init(ip, port);
            IntPtr ret = CppDll.Login(userName, password, isLive ? "YSG" : "DEM");
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            if (string.IsNullOrEmpty(res) || res.Contains("Error"))
            {
                return false;
            }
            string[] paras = GetInitData();
            SetInitParas(paras[0], paras[1], paras[2]);
            CppDll.GetSettingData();
            CppDll.GetWatchword();
            CppDll.initQuote(ip, 4529);
            CppDll.ConnectQuote();
            CppDll.LoginQuote();
            return true;
        }
        static public string[] GetInitData()
        {
            string[] paras = new string[10];
            IntPtr ret4 = CppDll.GetInitData();
            string res = Marshal.PtrToStringAnsi(ret4).ToString();
            xml.LoadXml(res);
            XmlNode accountNode = xml.SelectSingleNode("InitializeData/Accounts/Account");
            if (accountNode != null)
            {
                paras[0] = accountNode.Attributes["Id"].Value;
                XmlNodeList instrumentNodes = xml.SelectNodes("InitializeData/Instruments/Instrument");
                if (instrumentNodes != null && instrumentNodes.Count > 0)
                {
                    foreach (XmlNode node in instrumentNodes)
                    {
                        if (node.Attributes["OriginCode"].Value.Equals("XAUUSD"))
                        {
                            paras[1] = node.Attributes["Id"].Value;
                        }
                    }
                }
                XmlNodeList quotationNodes = xml.SelectNodes("InitializeData/Quotations/Quotation");
                if (quotationNodes != null && quotationNodes.Count > 0)
                {
                    foreach (XmlNode node in quotationNodes)
                    {
                        if (node.Attributes["InstrumentId"].Value.Equals(paras[1]))
                        {
                            paras[2] = node.Attributes["QuotePolicyId"].Value;
                        }
                    }
                }
            }
            return paras;
        }
        static public string[] GetSettingData()
        {
            string[] paras = new string[10];
            IntPtr ret5 = CppDll.GetSettingData();
            string res = Marshal.PtrToStringAnsi(ret5).ToString();
            xml.LoadXml(res);
            XmlNode node = xml.SelectSingleNode("SettingSource/Accounts/Account");
            paras[0] = node.Attributes["Code"].Value;
            paras[1] = node.Attributes["Name"].Value;
            paras[2] = node.Attributes["RateMarginD"].Value;
            paras[3] = node.Attributes["RateMarginLockD"].Value;
            paras[4] = node.Attributes["RateCommission"].Value;
            paras[5] = node.Attributes["QuotePolicyId"].Value;
            return paras;
        }

        static public void SetInitParas(string accountId, string instrumentId, string quotePolicyId)
        {
            CppDll.SetInitParas(accountId, instrumentId, quotePolicyId);
        }
        static public void LoginQuota(string ip, int port)
        {
            CppDll.initQuote(ip, port);
            CppDll.ConnectQuote();
            CppDll.LoginQuote();
        }
        /// <summary>
        /// 获取SessionId
        /// </summary>
        /// <returns></returns>
        static public string GetSession()
        {
            IntPtr ret = CppDll.GetInitParas();
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            return res;
        }
        static public bool Logout()
        {
            CppDll.DisConnect();
            CppDll.DisConnectQuote();
            return CppDll.isLoginCompleted();
        }
        /// <summary>
        /// 获取开仓信息
        /// </summary>
        /// <returns></returns>
        static public TradingData TradingData()
        {
            TradingData data = new TradingData();
            IntPtr ret = CppDll.GetTradingData();
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            xml.LoadXml(res);
            XmlNode accountNode = xml.SelectSingleNode("Accounts/Account");
            if (accountNode != null)
            {
                data.AccountBalance = Convert.ToDecimal(accountNode.Attributes["Balance"].Value).ToString("f2");
                data.AccountEquity = Convert.ToDecimal(accountNode.Attributes["Equity"].Value).ToString("f2");
                data.AccountMargin = Convert.ToDecimal(accountNode.Attributes["Necessary"].Value).ToString("f2");
                data.AccountFreeMargin = (Convert.ToDecimal(data.AccountEquity) - Convert.ToDecimal(data.AccountMargin)).ToString("f2");
                XmlNodeList transactionNodes = xml.SelectNodes("Accounts/Account/Transactions/Transaction");
                foreach (XmlNode transactionNode in transactionNodes)
                {
                    XmlNodeList orderNodes = transactionNode.SelectNodes("Orders/Order");
                    foreach (XmlNode orderNode in orderNodes)
                    {
                        uClient.Comm.Position order = new uClient.Comm.Position();
                        data.positions.Add(order);
                        order.Ticket = orderNode.Attributes["ID"].Value;
                        string openTime = orderNode.Attributes["InterestValueDate"].Value;
                        DateTime dt = DateTime.ParseExact(openTime, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                        order.CreationTime = dt;
                        order.BuySell = orderNode.Attributes["IsBuy"].Value.Equals("True") ? "Buy" : "Sell";
                        order.Lot = Convert.ToDecimal(orderNode.Attributes["Lot"].Value);
                        order.OpenPrice = Convert.ToDecimal(orderNode.Attributes["SetPrice"].Value);
                        order.CurrentPrice = Convert.ToDecimal(orderNode.Attributes["LivePrice"].Value);
                        order.Profit = Convert.ToDecimal(orderNode.Attributes["TradePLFloat"].Value);
                        order.Symbol = "LLG";
                    }
                }
            }
            return data;
        }
        static private OrderProgressEvent GenerateOrderProgressEvent(string orderType, string result)
        {
            OrderProgressEvent orderEvent = new OrderProgressEvent();
            Position position = new Position();
            orderEvent.postion = position;
            string[] res = result.Split('|');
            string price = res[0];
            xml.LoadXml(res[1]);
            XmlNode resultNode = xml.SelectSingleNode("Result/transactionError");
            string resText = resultNode.InnerText.ToString();
            if (resText == "OK")
            {
                if (orderType == uClient.Comm.Enum.Open)
                {
                    position.OpenPrice = Convert.ToDecimal(price);
                    orderEvent.Type = ProgressType.Opened;
                }
                else if (orderType == uClient.Comm.Enum.Close)
                {
                    orderEvent.Type = ProgressType.Closed;
                    position.ClosePrice = Convert.ToDecimal(price);
                }
            }
            else
            {
                orderEvent.Type = ProgressType.Rejected;
                Exception ex = new Exception(resText);
                orderEvent.Exception = ex;
            }
            return orderEvent;
        }
        /// <summary>
        /// 开仓买多
        /// </summary>
        /// <param name="price"></param>
        /// <param name="slippage"></param>
        /// <param name="lot"></param>
        static public OrderProgressEvent OpenBuyOrder(string price, string slippage, string lot)
        {
            //IntPtr ret = CppDll.OpenBuyOrder(price, slippage, lot);
            //string res = Marshal.PtrToStringAnsi(ret).ToString();
            //OrderProgressEvent orderEvent = GenerateOrderProgressEvent(uClient.Comm.Enum.Open, res);
            //orderEvent.postion.BuySell = "Buy";
            //orderEvent.postion.Lot = Convert.ToDecimal(lot);
            //orderEvent.postion.Symbol = "LLG";
            //return orderEvent;
            return null;
        }
        /// <summary>
        /// 开仓卖空
        /// </summary>
        /// <param name="price"></param>
        /// <param name="slippage"></param>
        /// <param name="lot"></param>
        /// <returns></returns>
        static public OrderProgressEvent OpenSellOrder(string price, string slippage, string lot)
        {
            IntPtr ret = CppDll.OpenSellOrder(price, slippage, lot);
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            OrderProgressEvent orderEvent = GenerateOrderProgressEvent(uClient.Comm.Enum.Open, res);
            orderEvent.postion.BuySell = "Sell";
            orderEvent.postion.Lot = Convert.ToDecimal(lot);
            orderEvent.postion.Symbol = "LLG";
            return orderEvent;
        }
        /// <summary>
        /// 平仓
        /// </summary>
        /// <param name="position"></param>
        /// <param name="tradeSide"></param>
        /// <param name="price"></param>
        /// <param name="slippage"></param>
        /// <returns></returns>
        static public OrderProgressEvent CloseOrder(uClient.Comm.Position position, string price, string slippage)
        {
            string isBuy = uClient.Comm.Enum.Buy.Contains(position.BuySell) ? "true" : "false";
            IntPtr ret10 = CppDll.CloseOrder(price, slippage, position.OpenPrice.ToString(), position.Lot.ToString(), position.Ticket, position.CreationTime.ToString(), isBuy);
            string res = Marshal.PtrToStringAnsi(ret10).ToString();
            OrderProgressEvent orderEvent = GenerateOrderProgressEvent(uClient.Comm.Enum.Close, res);
            orderEvent.postion.BuySell = position.BuySell;
            orderEvent.postion.Lot = position.Lot;
            orderEvent.postion.OpenPrice = position.OpenPrice;
            orderEvent.postion.Symbol = "LLG";
            return orderEvent;
        }
        /// <summary>
        /// 订阅显示的行情
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        static public Quote GetChartQuotaion(string symbol)
        {
            IntPtr ret8 = CppDll.GetDisplayQuote();
            string res = Marshal.PtrToStringAnsi(ret8).ToString();
            xml.LoadXml(res);
            if (xml.SelectSingleNode("Result/error") == null)
            {
                string Open = xml.SelectSingleNode("ChartQuotaions/ChartQuotaion").Attributes["Open"].Value;
                string Close = xml.SelectSingleNode("ChartQuotaions/ChartQuotaion").Attributes["Close"].Value;
                string Date = xml.SelectSingleNode("ChartQuotaions/ChartQuotaion").Attributes["Date"].Value;
                DateTime dt = DateTime.ParseExact(Date, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
                uClient.Comm.Quote q = new uClient.Comm.Quote(symbol, Convert.ToDecimal(Open), Convert.ToDecimal(Close), dt);
                return q;
            }
            else
            {
                return null;
            }
        }
        static public Quote GetQuote(string symbol)
        {
            IntPtr ret8 = CppDll.GetQuote();
            string res = Marshal.PtrToStringAnsi(ret8).ToString();
            string[] paras = res.Split(':');
            if (paras.Length > 1)
            {
                uClient.Comm.Quote q = new uClient.Comm.Quote(symbol, Convert.ToDecimal(paras[1]), Convert.ToDecimal(paras[2]), DateTime.Now);
                return q;
            }
            return null;
        }
        static public IList<Position> GetOpenedOrders()
        {
            return null;
        }
        static public bool isInitCompleted()
        {
            return CppDll.isLoginCompleted();
        }
    }
}
