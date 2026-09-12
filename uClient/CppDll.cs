using System;
using System.Runtime.InteropServices;
using System.Text;

namespace V4
{
    class CppDll
    {
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
        public static extern IntPtr OpenSellOrder(string price, string slippage, string lot);
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
        public static extern IntPtr CloseOrder(string nowPrice, string slippage, string openPrice, string lot, string orderID, string openTime, string isBuy);
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
    }
}
