using System;
using System.Collections.Generic;

namespace uClient.Comm
{
    /// <summary>
    /// 平台
    /// </summary>
    public abstract class PlatformInf
    {
        /// <summary>
        /// 连接
        /// </summary>
       public abstract void Connect();
        /// <summary>
        /// 断开连接
        /// </summary>
       public abstract void DisConnect();
        /// <summary>
        /// 获取开仓订单
        /// </summary>
        /// <returns></returns>
        public abstract IList<Position> GetPosition();

        /// <summary>
        /// 更新账户资金信息
        /// </summary>
        public abstract void UpdateAccountCaptial();
        /// <summary>
        /// 更新订单信息
        /// </summary>
        public abstract void UpdatePositionGrid();
        /// <summary>
        /// 订阅行情
        /// </summary>
        /// <param name="symbol"></param>
        public abstract void SymbolSubscription(string symbol);
        /// <summary>
        /// 获取报价信息
        /// </summary>
        public abstract void NewQuote();
        /// <summary>
        /// 更新报价显示
        /// </summary>
        public abstract void UpdateQuote();
        /// <summary>
        /// 买多开仓
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        /// <param name="lots"></param>
        public abstract void OpenBuyOrder(bool _IsAutoOperationFlag, out DateTime _SendTime, decimal lots = 0.1m);
        /// <summary>
        /// 卖空开仓
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        /// <param name="lots"></param>
        public abstract void OpenSellOrder(bool _IsAutoOperationFlag, out DateTime _SendTime, decimal lots = 0.1m);
        /// <summary>
        /// 平仓
        /// </summary>
        /// <param name="position"></param>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        public abstract void CloseOrder(Position position, bool _IsAutoOperationFlag, out DateTime _SendTime);
        /// <summary>
        /// 订单执行后
        /// </summary>
        /// <param name="_IsAutoOperationFlag"></param>
        /// <param name="_SendTime"></param>
        public abstract void OrderUpdate(bool _IsAutoOperationFlag, DateTime _SendTime , string _LockOrderId);
        /// <summary>
        /// 是否连接
        /// </summary>
        /// <returns></returns>
        public abstract bool isConnect();

        /// <summary>
        /// 检查连接状态，如果是自动断链则发送通知短信
        /// </summary>
        public abstract void CheckPlatformConnectStatus();
        /// <summary>
        /// 获取当前价格
        /// </summary>
        /// <param name="AutoLockTradeSide"></param>
        /// <returns></returns>
        public abstract double GetCurrentPrice(string AutoLockTradeSide);
    }
}
