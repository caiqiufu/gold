using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using uClient.Comm;
using V4;

namespace V4
{
    public class V4ClientYSG
    {

        public DataGridView _GvPositions;
        public Button btn;
        public Form form;
        public V4ClientYSG(DataGridView gvPositions)
        {
            _GvPositions = gvPositions;
        }
        //Dll Function
        /// <summary>
        /// 初始化连接信息
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void init(string ip, int port);
        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool Connect();
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="accountType">YSG是真实 DEM是模拟</param>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr Login(string user, string pass, string accountType);
        /// <summary>
        /// 获取初始化数据,获取报价之前必须调用该方法获取初始化参数
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetInitData();
        /// <summary>
        /// 设置初始化数据
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="instrumentId"></param>
        /// <param name="quotePolicyId"></param>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetInitParas(string accountId, string instrumentId, string quotePolicyId);
        /// <summary>
        /// 获取设置数据
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetSettingData();

        /// <summary>
        /// 初始化报价参数
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void initQuote(string ip, int port);
        /// <summary>
        /// 连接行情
        /// </summary>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool ConnectQuote();
        /// <summary>
        /// 获取行情登录
        /// </summary>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoginQuote();

        /// <summary>
        /// 获取订单，账户更新信息
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetTradingData();
        /// <summary>
        /// 获取界面显示的报价，不是订单交易报价
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetDisplayQuote();

        /// <summary>
        /// 开仓买多
        /// </summary>
        /// <param name="price"></param>
        /// <param name="slippage"></param>
        /// <param name="lot"></param>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr OpenBuyOrder(double price, double slippage, double lot);
        /// <summary>
        /// 开仓卖空
        /// </summary>
        /// <param name="price"></param>
        /// <param name="slippage"></param>
        /// <param name="lot"></param>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr OpenSellOrder(double price, double slippage, double lot);
        /// <summary>
        /// 平仓
        /// </summary>
        /// <param name="nowPrice">平仓价格</param>
        /// <param name="slippage">偏差</param>
        /// <param name="openPrice">Order/SetPrice</param>
        /// <param name="lot">Order/Lot</param>
        /// <param name="orderID">Order/ID</param>
        /// <param name="openTime">Order/InterestValueDate</param>
        /// <param name="isBuy">Order/IsBuy  如果订单是IsBuy=true，则值为false，反之亦然</param>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CloseOrder(double nowPrice, double slippage, double openPrice, double lot, string orderID, string openTime, string isBuy);
        /// <summary>
        /// 获取交易报价，订单交易中已经包含了价格获取
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetQuote();
        /// <summary>
        /// 心跳
        /// </summary>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void HeartPeer();
        /// <summary>
        /// 断开连接
        /// </summary>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool DisConnect();
        /// <summary>
        /// 获取初始化参数 SessionId|UserId|AccountId|InstrumentId|QuotePolicyId|ClientId|SessionId_QUOTE|Watchwords
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetInitParas();
        /// <summary>
        /// 报价心跳
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr HeartPeerQuote();
        /// <summary>
        /// 报价断开
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool DisConnectQuote();
        /// <summary>
        /// 获取watchword,并初始化值
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetWatchword();
        /// <summary>
        /// 获取Bid price
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern double GetBidPrice();
        /// <summary>
        /// 获取Ask price
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern double GetAskPrice();

        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr RecvData2();
        /// <summary>
        /// 用户登录和行情登录完成，只有这两个都完成后，才能刷新仓位和账户信息，并启动交易
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool isLoginCompleted();
        /// <summary>
        /// 用户连接
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool isConnect();
        /// <summary>
        /// 行情连接
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool isConnectQuote();
        /// <summary>
        /// 恢复
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", EntryPoint = "Recover", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr Recover();
        /// <summary>
        /// GetWatchWorlds
        /// </summary>
        /// <returns></returns>
        [DllImport("Dll1YGS0503.dll", EntryPoint = "GetWatchWorlds", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetWatchWorlds(string clientId);



        public XmlDocument xml = new XmlDocument();
        public bool Login1(string userName, string password, string pathName, string ip, int port)
        {
            init(ip, 4523);
            IntPtr ret = Login(userName, password,pathName);
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            if (string.IsNullOrEmpty(res) || res.Contains("Error"))
            {
                throw new Exception("Login error," + res);
            }
            string[] initparas = GetInitData1();
            SetInitParas(initparas[0], initparas[1], initparas[2]);
            GetSettingData();
            string[] paras = GetInitParas1();
            GetWatchWorlds(paras[5]);
            GetWatchword();
            initQuote(ip, 4529);
            ConnectQuote();
            LoginQuote();
            return true;
        }
        public string[] GetInitData1()
        {
            string[] paras = new string[10];
            IntPtr ret = default(IntPtr);
            RenderUI(() => {
                ret = GetInitData();
            });
            string res = Marshal.PtrToStringAnsi(ret).ToString();
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
        public string[] GetSettingData1()
        {
            string[] paras = new string[10];
            IntPtr ret = default(IntPtr);
            RenderUI(() => {
                ret = GetSettingData();
            });
            string res = Marshal.PtrToStringAnsi(ret).ToString();
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

        public void SetInitParas1(string accountId, string instrumentId, string quotePolicyId)
        {
            RenderUI(() => {
                SetInitParas(accountId, instrumentId, quotePolicyId);
            });
        }
        public void LoginQuota(string ip, int port)
        {
            RenderUI(() => {
                initQuote(ip, port);
                ConnectQuote();
                LoginQuote();
            });
        }
        /// <summary>
        /// 获取SessionId 需要切换回主线程
        /// </summary>
        /// <returns></returns>
        public string[] GetInitParas1()
        {
            IntPtr ret = default(IntPtr);
            RenderUI(() => {
                ret = GetInitParas();
            });
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            return res.Split('|');
        }
        /// <summary>
        /// 需要切换回主线程
        /// </summary>
        public void HeartPeer1()
        {
            RenderUI(() => {
                HeartPeer();
            });
        }
        /// <summary>
        /// 需要切换回主线程
        /// </summary>
        public void HeartPeerQuote1()
        {
            RenderUI(() => {
                HeartPeerQuote();
            });
        }
        /// <summary>
        /// 需要切换回主线程
        /// </summary>
        /// <returns></returns>
        public bool Logout()
        {
            RenderUI(() => {
                DisConnect();
                DisConnectQuote();
            });
            return isLoginCompleted();
        }
        /// <summary>
        /// 开仓买多，需要切换回主线程
        /// </summary>
        /// <param name="price"></param>
        /// <param name="slippage"></param>
        /// <param name="lot"></param>
        public OrderProgressEvent OpenBuyOrder1(double price, double slippage, double lot)
        {
            DateTime sendTime = DateTime.Now;
            IntPtr ret = default(IntPtr);
            RenderUI(() => {
                ret = OpenBuyOrder(price, 0, lot);
            });
            string res = Marshal.PtrToStringAnsi(ret);
            //Console.WriteLine("res="+ res);
            OrderProgressEvent orderEvent = GenerateOrderProgressEvent(uClient.Comm.Enum.Open, res);
            orderEvent.SendTime = sendTime;
            orderEvent.ReceiveTime = DateTime.Now;
            orderEvent.postion.BuySell = "Buy";
            orderEvent.postion.Lot = (decimal)lot;
            orderEvent.postion.Symbol = "LLG";
            orderEvent.postion.OpenPrice = (decimal)price;
            return orderEvent;
        }
        /// <summary>
        /// 开仓卖空，需要切换回主线程
        /// </summary>
        /// <param name="price"></param>
        /// <param name="slippage"></param>
        /// <param name="lot"></param>
        /// <returns></returns>
        public OrderProgressEvent OpenSellOrder1(double price, double slippage, double lot)
        {
            DateTime sendTime = DateTime.Now;
            IntPtr ret = default(IntPtr);
            RenderUI(() => {
                ret = OpenSellOrder(price, 0, lot);
            });
            string res = Marshal.PtrToStringAnsi(ret);
            OrderProgressEvent orderEvent = GenerateOrderProgressEvent(uClient.Comm.Enum.Open, res);
            orderEvent.SendTime = sendTime;
            orderEvent.ReceiveTime = DateTime.Now;
            orderEvent.postion.BuySell = "Sell";
            orderEvent.postion.Lot = (decimal)lot;
            orderEvent.postion.Symbol = "LLG";
            orderEvent.postion.OpenPrice = (decimal)price;
            return orderEvent;
        }
        /// <summary>
        /// 平仓，需要切换回主线程
        /// </summary>
        /// <param name="position"></param>
        /// <param name="tradeSide"></param>
        /// <param name="price"></param>
        /// <param name="slippage"></param>
        /// <returns></returns>
        public OrderProgressEvent CloseOrder1(Position position, double price, double slippage)
        {
            DateTime sendTime = DateTime.Now;
            string isBuy = uClient.Comm.Enum.Buy.Contains(position.BuySell) ? "true" : "false";
            IntPtr ret = default(IntPtr);
            RenderUI(() => {
                ret = CloseOrder(price, 0, (double)position.OpenPrice, (double)position.Lot, position.Ticket, position.CreationTime.ToString(), isBuy);
            });
            //Console.WriteLine("Close price= "+ price);
            string res = Marshal.PtrToStringAnsi(ret);
            OrderProgressEvent orderEvent = GenerateOrderProgressEvent(uClient.Comm.Enum.Close, res);
            orderEvent.SendTime = sendTime;
            orderEvent.ReceiveTime = DateTime.Now;
            orderEvent.postion.BuySell = position.BuySell;
            orderEvent.postion.Lot = position.Lot;
            orderEvent.postion.OpenPrice = position.OpenPrice;
            orderEvent.postion.Symbol = "LLG";
            orderEvent.postion.ClosePrice = (decimal)price;
            return orderEvent;
        }
        /// <summary>
        /// 获取开仓信息，需要切换回主线程
        /// </summary>
        /// <returns></returns>
        public string TradingDataString()
        {
            IntPtr ret = default(IntPtr);
            string res = "";
            RenderUI(() => {
                try {
                    ret = GetTradingData();
                    res = Marshal.PtrToStringAnsi(ret);
                } catch (Exception e) 
                {
                    res = "";
                    Console.WriteLine(e.Message);
                }              
            });
            //IntPtr ret = GetTradingData();          
            return res;
        }
        public TradingData TradingData(string res)
        {
            TradingData data = new TradingData();
            if (res != "" && res.Contains("Accounts"))
            {
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
                        string openTime = transactionNode.Attributes["ExecuteTime"].Value;
                        if (!string.IsNullOrEmpty(openTime))
                        {
                            DateTime dt = DateTime.ParseExact(openTime, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                            XmlNodeList orderNodes = transactionNode.SelectNodes("Orders/Order");
                            foreach (XmlNode orderNode in orderNodes)
                            {
                                uClient.Comm.Position order = new uClient.Comm.Position();
                                data.positions.Add(order);
                                order.Ticket = orderNode.Attributes["ID"].Value;
                                order.CreationTime = dt;
                                order.BuySell = orderNode.Attributes["IsBuy"].Value.Equals("True") ? "Buy" : "Sell";
                                order.Lot = Convert.ToDecimal(orderNode.Attributes["Lot"].Value);
                                //order.OpenPrice = Convert.ToDecimal(orderNode.Attributes["SetPrice"].Value);
                                order.OpenPrice = Convert.ToDecimal(orderNode.Attributes["ExecutePrice"].Value);
                                order.CurrentPrice = Convert.ToDecimal(orderNode.Attributes["LivePrice"].Value);
                                order.Profit = Convert.ToDecimal(orderNode.Attributes["TradePLFloat"].Value);
                                order.Symbol = "LLG";
                            }
                        }
                    }
                }
            }
            return data;
        }

        private OrderProgressEvent GenerateOrderProgressEvent(string orderType, string result)
        {
            //Console.WriteLine("result="+ result);
            OrderProgressEvent orderEvent = new OrderProgressEvent();
            orderEvent.result = result;
            orderEvent.Exception = new Exception();
            Position position = new Position();
            orderEvent.postion = position;
            /**
if (result != null && result != "" && result.Contains("|") && result.Split('|').Length == 2)
{
    try
    {
        string[] ress = result.Split('|');
        string price = ress[0];
        string res = ress[1];
        if (res != "" && res.Contains("<transactionError>") && res.Contains("</transactionError>"))
        {
            int start = res.IndexOf("<transactionError>") + "<transactionError>".Length;
            int end = res.IndexOf("</transactionError>");
            string resText = res.Substring(start, end - start);
            if (resText == "OK")
            {
                if (orderType == uClient.Comm.Enum.Open)
                {
                    position.OpenPrice = Convert.ToDecimal(price);
                    orderEvent.Type = ProgressType.Opened;
                }
                else
                {
                    orderEvent.Type = ProgressType.Closed;
                    position.ClosePrice = Convert.ToDecimal(price);
                }
            }
            else
            {
                orderEvent.Type = ProgressType.Rejected;
                orderEvent.Exception = new Exception(resText);
            }
        }
        else
        {
            orderEvent.Type = ProgressType.Rejected;
            orderEvent.Exception = new Exception("成交价格[" + price + "],异常信息" + res);
        }
    }
    catch (Exception e)
    {
        orderEvent.Type = ProgressType.Rejected;
        orderEvent.Exception = new Exception("服务器端异常:" + e.Message);
    }
}
else
{
    orderEvent.Type = ProgressType.Rejected;
    orderEvent.Exception = new Exception("服务器端异常:" + result);
}             
 */
            if (!string.IsNullOrEmpty(result))
            {
                if (result.Contains("<transactionError>") && result.Contains("</transactionError>"))
                {
                    int start = result.IndexOf("<transactionError>") + "<transactionError>".Length;
                    int end = result.IndexOf("</transactionError>");
                    string resText = result.Substring(start, end - start);
                    if (resText == "OK")
                    {
                        if (orderType == uClient.Comm.Enum.Open)
                        {
                            orderEvent.Type = ProgressType.Opened;
                        }
                        else if (orderType == uClient.Comm.Enum.Close)
                        {
                            orderEvent.Type = ProgressType.Closed;
                        }
                    }
                    else
                    {
                        orderEvent.Type = ProgressType.Rejected;
                        Exception ex = new Exception(resText);
                        orderEvent.Exception = ex;
                    }
                }
                else
                {
                    orderEvent.Type = ProgressType.Rejected;
                    Exception ex = new Exception("异常信息" + result);
                    orderEvent.Exception = ex;
                }
            }
            else
            {
                orderEvent.Type = ProgressType.Rejected;
                Exception ex = new Exception("异常信息" + result);
                orderEvent.Exception = ex;
            }
            return orderEvent;
        }
        public Quote GetQuote1()
        {
            IntPtr ret = RecvData2();
            string res = Marshal.PtrToStringAnsi(ret);
            //Console.WriteLine("res="+ res);
            //19,18 是YSG， 24是WFB
            if (res != "" && res.Contains(':'))
            {
                string[] paras = res.Split(':');
                if (paras.Length==12)
                {
                    Quote q = new Quote("LLG", Convert.ToDecimal(paras[2]), Convert.ToDecimal(paras[1]), DateTime.Now);
                    return q;
                }
            }
            return null;
        }
        public Quote GetQuote2()
        {
            IntPtr ret = GetQuote();
            string res = Marshal.PtrToStringAnsi(ret);
            //Console.WriteLine("res="+ res);
            //19,18 是YSG， 24是WFB
            if (res != "" && res.Contains(':'))
            {
                string[] paras = res.Split(':');
                if (paras.Length == 12)
                {
                    Quote q = new Quote("LLG", Convert.ToDecimal(paras[2]), Convert.ToDecimal(paras[1]), DateTime.Now);
                    return q;
                }
            }
            return null;
        }
        public Quote GetDisplayQuote1()
        {
            IntPtr ret = default(IntPtr);
            RenderUI(() => {
                ret = GetDisplayQuote();
            });
            string res = Marshal.PtrToStringAnsi(ret);
            //Console.WriteLine("res="+ res);
            if (res != "" && res.Contains("Open="))
            {
                xml.LoadXml(res);
                XmlNode ChartQuotaionNode = xml.SelectSingleNode("ChartQuotaions/ChartQuotaion");
                if (ChartQuotaionNode!=null)
                {
                    decimal OpenPrice = Convert.ToDecimal(ChartQuotaionNode.Attributes["Open"].Value);
                    decimal ClosePrice = Convert.ToDecimal(ChartQuotaionNode.Attributes["Close"].Value);
                    Quote q = new Quote("LLG", ClosePrice, OpenPrice, DateTime.Now);
                    return q;
                }              
            }
            return null;
        }
        /// <summary>
        /// 不用在主线程中执行
        /// </summary>
        /// <returns></returns>
        public bool ConnectQuote1()
        {
            bool res = false;
            RenderUI(() => {
                res = ConnectQuote();
            });
            //bool res = ConnectQuote();
            return res;
        }
        public bool DisConnectQuote1()
        {
            bool res = false;
            RenderUI(() => {
                res = DisConnectQuote();
            });
            //bool res = DisConnectQuote();
            return res;
        }
        /// <summary>
        /// 需要在主线程中执行
        /// </summary>
        /// <returns></returns>
        public string LoginQuote1()
        {
            IntPtr ret = default(IntPtr);
            RenderUI(() => {
                ret = LoginQuote();
            });
            //IntPtr ret = LoginQuote();
            string res = Marshal.PtrToStringAnsi(ret);
            return res;
        }
        public double GetAskPrice1()
        {
            double price = 0;
            RenderUI(() => {
                price = GetAskPrice();
            });
            return price;
        }
        public double GetBidPrice1()
        {
            double price = 0;
            RenderUI(() => {
                price = GetBidPrice();
            });
            return price;
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
        //如果你的后台线程在更新一个UI控件的状态后不需要等待，而是要继续往下处理，那么你就应该使用Invoke来进行异步处理。
        protected void AsynRenderUI(Control control, Action action)
        {
            control.Invoke(new Action(delegate ()
            {
                action();
            }));
        }
    }
}
