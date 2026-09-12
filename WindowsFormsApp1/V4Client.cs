using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using V4;

namespace V4
{
    class V4Client
    {
        public double bid;
        public double ask;
        //Dll Function
        /// <summary>
        /// 初始化连接信息
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void init(string ip, int port);
        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool Connect();
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="accountType">YSG是真实 DEM是模拟</param>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr Login(string user, string pass, string accountType);
        /// <summary>
        /// 获取初始化数据,获取报价之前必须调用该方法获取初始化参数
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetInitData();
        /// <summary>
        /// 设置初始化数据
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="instrumentId"></param>
        /// <param name="quotePolicyId"></param>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetInitParas(string accountId, string instrumentId, string quotePolicyId);
        /// <summary>
        /// 获取设置数据
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetSettingData();

        /// <summary>
        /// 初始化报价参数
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void initQuote(string ip, int port);
        /// <summary>
        /// 连接行情
        /// </summary>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool ConnectQuote();
        /// <summary>
        /// 获取行情登录
        /// </summary>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoginQuote();

        /// <summary>
        /// 获取订单，账户更新信息
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetTradingData();
        /// <summary>
        /// 获取界面显示的报价，不是订单交易报价
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetDisplayQuote();

        /// <summary>
        /// 开仓买多
        /// </summary>
        /// <param name="price"></param>
        /// <param name="slippage"></param>
        /// <param name="lot"></param>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr OpenBuyOrder(double price, double slippage, double lot);
        /// <summary>
        /// 开仓卖空
        /// </summary>
        /// <param name="price"></param>
        /// <param name="slippage"></param>
        /// <param name="lot"></param>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
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
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr CloseOrder(double nowPrice, double slippage, double openPrice, double lot, string orderID, string openTime, string isBuy);
        /// <summary>
        /// 获取交易报价，订单交易中已经包含了价格获取
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetQuote();
        /// <summary>
        /// 心跳
        /// </summary>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void HeartPeer();
        /// <summary>
        /// 断开连接
        /// </summary>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool DisConnect();
        /// <summary>
        /// 获取初始化参数 SessionId|UserId|AccountId|InstrumentId|QuotePolicyId|ClientId|SessionId_QUOTE|Watchwords
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetInitParas();
        /// <summary>
        /// 报价心跳
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr HeartPeerQuote();
        /// <summary>
        /// 报价断开
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr DisConnectQuote();
        /// <summary>
        /// 获取watchword,并初始化值
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetWatchword();
        /// <summary>
        /// 获取Bid price
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern double GetBidPrice();
        /// <summary>
        /// 获取Ask price
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern double GetAskPrice();

        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr RecvData2();
        /// <summary>
        /// 用户登录和行情登录完成，只有这两个都完成后，才能刷新仓位和账户信息，并启动交易
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool isLoginCompleted();
        /// <summary>
        /// 用户连接
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool isConnect();
        /// <summary>
        /// 行情连接
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool isConnectQuote();
        /// <summary>
        /// 恢复
        /// </summary>
        /// <returns></returns>
        [DllImport("E:/OneDrive/07-svn/01-gold/Dll1/Debug/Dll1.dll", EntryPoint = "Recover", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr Recover();

        public XmlDocument xml = new XmlDocument();
        public bool Login1(string userName, string password, bool isLive, string ip, int port)
        {
            init(ip, 4523);
            IntPtr ret = Login(userName, password, isLive ? "YSG" : "DEM");
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            if (string.IsNullOrEmpty(res) || res.Contains("Error"))
            {
                return false;
            }
            string[] paras = GetInitData1();
            SetInitParas(paras[0], paras[1], paras[2]);
            GetSettingData();
            GetWatchword();
            initQuote(ip, 4529);
            ConnectQuote();
            LoginQuote();
            return true;
        }
        public string[] GetInitData1()
        {
            string[] paras = new string[10];
            IntPtr ret4 = GetInitData();
            string res = Marshal.PtrToStringAnsi(ret4).ToString();
            Console.WriteLine("GetInitData="+res);
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
            IntPtr ret5 = GetSettingData();
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

        public void SetInitParas1(string accountId, string instrumentId, string quotePolicyId)
        {
            SetInitParas(accountId, instrumentId, quotePolicyId);
        }
        public bool LoginQuota(string ip, int port)
        {
            initQuote(ip, port);
            ConnectQuote();
            LoginQuote();
            return true;
        }
        /// <summary>
        /// 获取SessionId
        /// </summary>
        /// <returns></returns>
        public string GetSession()
        {
            IntPtr ret = GetInitParas();
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            return res;
        }
        public bool Logout()
        {
            DisConnect();
            DisConnectQuote();
            return isLoginCompleted();
        }
        /// <summary>
        /// 获取开仓信息
        /// </summary>
        /// <returns></returns>
        public string CloseAllOrders()
        {
            string ress = "";
            IntPtr ret = GetTradingData();
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            if (res != null && res != "")
            {
                xml.LoadXml(res);
                XmlNode accountNode = xml.SelectSingleNode("Accounts/Account");
                if (accountNode != null)
                {
                    Console.WriteLine(res);
                    XmlNodeList transactionNodes = xml.SelectNodes("Accounts/Account/Transactions/Transaction");
                    foreach (XmlNode transactionNode in transactionNodes)
                    {
                        XmlNodeList orderNodes = transactionNode.SelectNodes("Orders/Order");
                        foreach (XmlNode orderNode in orderNodes)
                        {
                            string openTime = transactionNode.Attributes["ExecuteTime"].Value;
                            DateTime dt = DateTime.ParseExact(openTime, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
                            string orderId = orderNode.Attributes["ID"].Value;
                            string isBuy = orderNode.Attributes["IsBuy"].Value.Equals("True") ? "true" : "false";
                            double lot = Convert.ToDouble(orderNode.Attributes["Lot"].Value);
                            double openPrice = Convert.ToDouble(orderNode.Attributes["SetPrice"].Value);
                            double mprice = 0;
                            if (isBuy.Equals("true"))
                            {
                                mprice = ask;
                            }
                            else {
                                mprice = bid;
                            }
                            ress = ress + CloseOrder1(mprice, openPrice, lot, orderId, dt.ToString(), isBuy);
                        }
                    }
                }
            }
            return ress;
        }
        
        /// <summary>
        /// 开仓买多
        /// </summary>
        /// <param name="price"></param>
        /// <param name="slippage"></param>
        /// <param name="lot"></param>
        public string OpenBuyOrder1()
        {
            IntPtr ret = OpenBuyOrder(bid, 0, 0.5);
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            return res;
        }
        /// <summary>
        /// 开仓卖空
        /// </summary>
        /// <param name="price"></param>
        /// <param name="slippage"></param>
        /// <param name="lot"></param>
        /// <returns></returns>
        public string OpenSellOrder1()
        {
            IntPtr ret = OpenSellOrder(ask, 0, 0.5);
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            return res;
        }
        /// <summary>
        /// 平仓
        /// </summary>
        /// <param name="position"></param>
        /// <param name="tradeSide"></param>
        /// <param name="price"></param>
        /// <param name="slippage"></param>
        /// <returns></returns>
        public string CloseOrder1(double price, double openPrice,double lot,string orderId,string openTime,string isBuy)
        {
            IntPtr ret10 = CloseOrder(price, 0, openPrice, lot, orderId, openTime, isBuy);
            string res = Marshal.PtrToStringAnsi(ret10).ToString();
            return res;
        }
        /// <summary>
        /// 订阅显示的行情
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public void GetChartQuotaion(string symbol)
        {
            IntPtr ret8 = GetDisplayQuote();
            string res = Marshal.PtrToStringAnsi(ret8).ToString();
            xml.LoadXml(res);
            if (xml.SelectSingleNode("Result/error") == null)
            {
                Console.WriteLine(res);
            }
           
        }
        public void HeartPeerQuote1()
        {
            HeartPeerQuote();
        }
        public void DisConnectQuote1()
        {
            DisConnectQuote();
        }
        public void HeartPeer1()
        {
            HeartPeer();
        }
        public void GetQuote(string symbol)
        {
            IntPtr ret8 = GetDisplayQuote();
            string res = Marshal.PtrToStringAnsi(ret8).ToString();
            xml.LoadXml(res);
            string[] paras = res.Split(':');
            if (paras.Length > 1)
            {
                Console.WriteLine(res);
            }
        }
        public string getTrading2()
        {
            IntPtr ret = GetTradingData();
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            return res;
        }
        public string getPrice()
        {
            IntPtr ret = RecvData2();
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            return res;
        }

        public string Recover1()
        {
            IntPtr ret = Recover();
            string res = Marshal.PtrToStringAnsi(ret).ToString();
            return res;
        }
    }
}
