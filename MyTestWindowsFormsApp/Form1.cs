using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using V4;

namespace MyTestWindowsFormsApp
{
    public partial class Form1 : Form
    {
        bool isLogin = false;
        V4Client _Client = new V4Client();

        void getPrice()
        {
            while (true)
            {
                IntPtr ret = RecvData2();
                string res = Marshal.PtrToStringAnsi(ret).ToString();
                Console.WriteLine(res);
                if (res != "" && res.Contains("19:"))
                {
                    _Client.bid = Convert.ToDouble(res.Substring(3, 7));
                    _Client.ask = Convert.ToDouble(res.Substring(11, 7));
                }
                //System.Threading.Thread.Sleep(100);
            }
        }
        void getTrading()
        {
            while (true)
            {
                IntPtr ret = GetTradingData();
                string res = Marshal.PtrToStringAnsi(ret).ToString();
                Console.WriteLine(res);
                System.Threading.Thread.Sleep(1000);
            }
        }
        void getTrading2()
        {
            if (isLogin)
            {
                textBox1.Text = DateTime.Now.ToString();
                IntPtr ret = GetTradingData();
                string res = Marshal.PtrToStringAnsi(ret).ToString();
                Console.WriteLine(res);              
            }
        }
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
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!isLogin)
            {
                //string strIP = "203.160.75.182";
                //string strUser = "40725";
                //string strPass = "qazwsx123";

                bool res0 = _Client.Login1("55365", "qazwsx123", false, "203.160.75.182", 4523);
                Console.WriteLine("Login=" + res0);
                //IntPtr ret = Login(strUser, strPass, "DEM");
                //string tem = Marshal.PtrToStringAnsi(ret);
                //Console.WriteLine("Login=" + tem);

                //IntPtr ret1 = GetInitParas();
                //string tem1 = Marshal.PtrToStringAnsi(ret1);
                //Console.WriteLine("GetInitParas=" + tem1);
                //string[] paras = tem1.Split('|');
                //string clientId = paras[5];

                //IntPtr ret2 = GetInitData();
                //string tem2 = Marshal.PtrToStringAnsi(ret2);
                //Console.WriteLine("GetInitData=" + tem2);

                //IntPtr ret3 = GetSettingData();
                //string tem3 = Marshal.PtrToStringAnsi(ret3);
                //Console.WriteLine("GetSettingData=" + tem3);

                //IntPtr ret4 = GetWatchword();
                //string tem4 = Marshal.PtrToStringAnsi(ret4);
                //Console.WriteLine("GetWatchword=" + tem4);


                //string[] paras2 = myGetInitData(tem2);
                //SetInitParas(paras2[0], paras2[1], paras2[2]);

                //IntPtr ret5 = GetInitParas();
                //string tem5 = Marshal.PtrToStringAnsi(ret5);
                //Console.WriteLine("GetInitParas2=" + tem5);


                //initQuote(strIP, 4529);

                //bool quotec = ConnectQuote();
                //Console.WriteLine("ConnectQuote=" + quotec);

                //IntPtr ret6 = LoginQuote();
                //string tem6 = Marshal.PtrToStringAnsi(ret6);
                //Console.WriteLine("LoginQuote=" + tem6);

                isLogin = true;


            }

            Action a = getPrice;
            a.BeginInvoke(null, null);

            //Action b = getTrading;
            //b.BeginInvoke(null, null);
            /*a.BeginInvoke(null, null);
                        int i = 0;
                        while (true) 
                        {
                            //_Client.OpenBuyOrder1();
                            if (i>0)
                            {
                                break;
                            }
                            i++;
                            System.Threading.Thread.Sleep(500);
                        }
                        int j = 0;
                        while (true)
                        {
                            //_Client.OpenSellOrder1();
                            if (j > 0)
                            {
                                break;
                            }
                            j++;
                            System.Threading.Thread.Sleep(500);
                        }
                        int k = 0;
                        while (true)
                        {
                            //_Client.CloseAllOrders();
                            if (k > 0)
                            {
                                break;
                            }
                            k++;
                            System.Threading.Thread.Sleep(500);
                        }*/
            //IntPtr ret11 = GetTradingData();
            //string tem11 = Marshal.PtrToStringAnsi(ret11);
            //Console.WriteLine("GetTradingData=" + tem11);

            //必须要有该心跳后才获取报价
            //HeartPeerQuote();
            //IntPtr ret7 = RecvData2();//接收行情数据并解密
            //string tem7 = Marshal.PtrToStringAnsi(ret7);
            //Console.WriteLine("RecvData2=" + tem7);

            //HeartPeerQuote();
            //IntPtr ret8 = GetQuote();
            //string tem8 = Marshal.PtrToStringAnsi(ret8);
            //Console.WriteLine("GetQuote=" + tem8);

            //HeartPeerQuote();
            //double ask = GetAskPrice();
            //Console.WriteLine("GetAskPrice=" + ask);
            //IntPtr ret8 = GetDisplayQuote();
            //string res = Marshal.PtrToStringAnsi(ret8).ToString();
            //Console.WriteLine("GetDisplayQuote=" + res);

            //HeartPeerQuote();
            //double bid = GetBidPrice();
            //Console.WriteLine("GetBidPrice=" + bid);

            //IntPtr ret19 = HeartPeer();
            //string tem19 = Marshal.PtrToStringAnsi(ret19);
            //Console.WriteLine("HeartPeer=" + tem19);

            //IntPtr ret9 = OpenBuyOrder(Convert.ToString(bid),"0","0.5");
            //IntPtr ret9 = OpenBuyOrder(bid+0.01, 0,0.5);
            //string tem9 = Marshal.PtrToStringAnsi(ret9);
            //Console.WriteLine("OpenBuyOrder=" + tem9);

            //IntPtr ret10 = CloseOrder("0", "0", "1725.90", "0.5", "e8915729-68c7-4ebb-958b-48e642fcebba", "2021-03-25 00:00:00","true");
            //string tem10 = Marshal.PtrToStringAnsi(ret10);
            //Console.WriteLine("CloseOrder=" + tem10);
        }
        public static XmlDocument xml = new XmlDocument();
        public static string[] myGetInitData(string res)
        {
            string[] paras = new string[10];
            //IntPtr ret4 = GetInitData();
            //string res = Marshal.PtrToStringAnsi(ret4).ToString();
            //Console.WriteLine("GetInitData=" + res);
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

        private void button2_Click(object sender, EventArgs e)
        {
            _Client.OpenBuyOrder1();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _Client.OpenSellOrder1();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _Client.CloseAllOrders();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            getTrading2();
        }
    }
}
