using M4.IBroker;
using System;
using uClient.INF;

namespace uClient
{
    /// <summary>
    /// 交易平台策略接口。
    /// 通过此接口统一不同交易平台（如 MT4, MT5, MF4 等）的操作行为，实现多态切换。
    /// </summary>
    public interface IPlatformStrategy
    {
        /// <summary>
        /// 获取当前平台是否已成功连接并登录。
        /// </summary>
        /// <value>如果已连接返回 <c>true</c>；否则返回 <c>false</c>。</value>
        bool IsConnected { get; }

        /// <summary>
        /// 异步或同步执行平台连接与登录操作。
        /// </summary>
        /// <param name="server">服务器地址（IP 或 域名）。</param>
        /// <param name="login">交易账户账号。</param>
        /// <param name="password">交易账户密码。</param>
        /// <exception cref="System.Exception">当网络不可达或认证失败时可能抛出异常。</exception>
        void Connect(string server, int login, string password);

        /// <summary>
        /// 断开与交易平台的连接，并释放相关网络资源。
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 执行下单（开仓/平仓）操作。
        /// </summary>
        /// <param name="paras">包含品种、手数、方向、止损止盈等信息的下单参数对象。</param>
        /// <returns>订单请求发送成功返回 <c>true</c>（不代表一定成交）；发送失败返回 <c>false</c>。</returns>
        bool PlaceOrder(IOrderParameters paras);

        /// <summary>
        /// 获取指定品种的最新市场报价。
        /// </summary>
        /// <param name="symbol">交易品种名称（如 "XAUUSD"）。</param>
        /// <returns>返回包含卖出价和买入价的报价对象。</returns>
        ISymbolQuote GetQuote(string symbol);

        /// <summary>
        /// 获取当前登录账户的详细信息（余额、净值、杠杆等）。
        /// </summary>
        /// <returns>账户信息对象。</returns>
        AccountInfo GetAccountInfo();

        /// <summary>
        /// 当收到平台推送的报价更新时触发的事件。
        /// </summary>
        event EventHandler<ISymbolQuote> OnQuoteUpdated;

        /// <summary>
        /// 当订单状态发生变化（如成交、被拒绝、人工平仓）时触发的事件。
        /// </summary>
        event EventHandler<IOrderUpdateEvent> OnOrderChanged;
    }
}
