using System;

namespace uClient.INF
{
    /// <summary>
    /// 实时报价事件参数类。
    /// 用于承载交易品种的最新市场深度信息和成交数据。
    /// </summary>
    public class ISymbolQuote
    {
        /// <summary>
        /// 交易品种标识（例如："EURUSD"、"XAUUSD" 或 "BTCUSD"）。
        /// </summary>
        public string Symbol;

        /// <summary>
        /// 买入价 (Bid)。
        /// 交易者卖出持仓时参考的价格，通常是盘口买一价。
        /// </summary>
        public double Bid;

        /// <summary>
        /// 卖出价 (Ask)。
        /// 交易者买入开仓时参考的价格，通常是盘口卖一价。
        /// </summary>
        public double Ask;

        /// <summary>
        /// 服务器时间。
        /// 报价产生时交易平台服务器的精确时间，用于计算 K 线及判断报价延迟。
        /// </summary>
        public DateTime Time;

        /// <summary>
        /// 最后成交价 (Last)。
        /// 市场上最后一笔订单的实际成交价格。在某些品种（如期货/股票）中非常关键。
        /// </summary>
        public double Last;

        /// <summary>
        /// 交易量 (Volume)。
        /// 当前报价周期内的成交总量或滴答量（Tick Volume）。
        /// </summary>
        public ulong Volume;

        /// <summary>
        /// 内部使用的扩展字段 0。
        /// </summary>
        internal ulong _E000;

        /// <summary>
        /// 内部使用的状态标识 1。
        /// </summary>
        internal short _E001;

        /// <summary>
        /// 本地接收时间。
        /// 记录报价到达本地计算机的准确时间，常用于计算网络延迟（LocalTime - ServerTime）。
        /// </summary>
        internal readonly DateTime _E002 = DateTime.Now;

        /// <summary>
        /// 将报价对象转换为格式化字符串。
        /// </summary>
        /// <returns>返回格式如 "品种 买入价 卖出价" 的字符串。</returns>
        public override string ToString()
        {
            // 使用 $ 符号进行字符串插值，用制表符 \t 分隔以便于对齐
            // :F5 表示强制保留 5 位小数（金融交易常用）
            return $"{Symbol}\tBid: {Bid:F5}\tAsk: {Ask:F5}\tTime: {Time:HH:mm:ss.fff}";
        }
    }
}
