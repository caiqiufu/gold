using System;
using System.Collections.Generic;

namespace uClient.Comm
{
    public class TradingData
    {

        public string AccountBalance = "0";
        public string AccountEquity = "0";
        public string AccountMargin = "0";
        public string AccountFreeMargin = "0";
        public string Profit = "0";
        public IList<uClient.Comm.Position> positions = new List<uClient.Comm.Position>();
    }
    /// <summary>
    /// 价格数据
    /// </summary>
    public class PricePoint
    {
        public DateTime Time { get; set; }
        public decimal Price { get; set; }
    }
    /// <summary>
    /// 蜡烛图数据
    /// </summary>
    public class Candle
    {
        public DateTime Time { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }

        public bool IsFilled { get; set; } = false;
    }

    /// <summary>
    /// 统一交易信号枚举（Unified Trade Signal）
    ////// 这个枚举包含：
    /// 1. 市场趋势状态（Trend）
    /// 2. 开仓信号（Entry）
    /// 3. 持仓阶段（Trade Stage）
    /// 4. 仓位管理（SL / TP / Trailing）
    /// 5. 平仓信号（Exit）
    /// 
    /// 设计目标：
    /// 整个交易系统任何时候只输出一个 TradeSignal
    /// 执行引擎只需要根据 TradeSignal 做对应动作
    /// 
    /// ===============================
    /// 信号大类：
    /// ===============================
    /// A. 市场状态（Market State）
    /// B. 开仓信号（Entry）
    /// C. 持仓阶段（Position Stage）
    /// D. 仓位管理（Position Management）
    /// E. 平仓信号（Exit）
    ////// ===============================
    /// 信号优先级（从高到低）：
    /// ===============================
    /// StopLossHit
    /// ExitByEMA
    /// TakeProfit2
    /// TakeProfit1
    /// MoveStopToBreakEven
    /// TrailingStopUpdate
    /// OpenBuy / OpenSell
    /// TrendBullish / TrendBearish / TrendNeutral
    /// None
    /// </summary>
    public enum TradeSignal
    {
        // =====================================================
        // 0. 无信号
        // =====================================================
        None,


        // =====================================================
        // A. 市场状态（Market Trend State）
        // =====================================================
        /// <summary>
        /// 多头趋势
        /// EMA50 > EMA200
        /// 只允许做多
        /// </summary>
        TrendBullish,

        /// <summary>
        /// 空头趋势
        /// EMA50 < EMA200
        /// 只允许做空
        /// </summary>
        TrendBearish,

        /// <summary>
        /// 震荡 / 无趋势
        /// 不开仓
        /// </summary>
        TrendNeutral,


        // =====================================================
        // B. 开仓信号（Entry Signal）
        // =====================================================
        /// <summary>
        /// 开多仓
        /// 条件：
        /// - TrendBullish
        /// - 价格在 EMA50 上方
        /// - ATR 波动足够
        /// </summary>
        OpenBuy,

        /// <summary>
        /// 开空仓
        /// 条件：
        /// - TrendBearish
        /// - 价格在 EMA50 下方
        /// </summary>
        OpenSell,


        // =====================================================
        // C. 持仓阶段（Trade Stage）
        // =====================================================
        /// <summary>
        /// 风险阶段（刚开仓）
        /// 止损 = 初始止损
        /// </summary>
        PositionRiskPhase,

        /// <summary>
        /// 保本阶段（盈利 >= 1R）
        /// 止损 = 开仓价
        /// </summary>
        PositionBreakEvenPhase,

        /// <summary>
        /// 盈利阶段（盈利 >= 2R）
        /// 已部分止盈
        /// </summary>
        PositionProfitPhase,

        /// <summary>
        /// 趋势跟踪阶段（Trailing）
        /// 使用 ATR Trailing Stop
        /// </summary>
        PositionTrailingPhase,


        // =====================================================
        // D. 仓位管理（Position Management）
        // =====================================================
        /// <summary>
        /// 移动止损到保本（盈利 >= 1R）
        /// </summary>
        MoveStopToBreakEven,

        /// <summary>
        /// 止盈1（盈利 >= 2R）
        /// 平部分仓位
        /// </summary>
        TakeProfit1,

        /// <summary>
        /// 止盈2（盈利 >= 3R）
        /// 再平部分仓位
        /// </summary>
        TakeProfit2,

        /// <summary>
        /// ATR 跟踪止损更新
        /// </summary>
        TrailingStopUpdate,


        // =====================================================
        // E. 平仓信号（Exit）
        // =====================================================
        /// <summary>
        /// EMA 趋势退出
        /// </summary>
        ExitByEMA,

        /// <summary>
        /// 止损触发
        /// </summary>
        StopLossHit
    }

    public class TradePosition
    {
        public string eaTradeId;

        /// <summary>唯一持仓ID（支持多单）</summary>
        public string PositionId;

        public bool IsBuy;

        public decimal EntryPrice;

        /// <summary>当前止损（动态）</summary>
        public decimal StopLoss;

        /// <summary>初始止损（固定）</summary>
        public decimal InitialStopLoss;

        /// <summary>1R 风险距离</summary>
        public decimal RiskR;

        public bool BreakEvenActivated;

        public bool TP1Hit;

        public bool TP2Hit;

        /// <summary>最高价（多单用）</summary>
        public decimal HighestPrice;

        /// <summary>最低价（空单用）</summary>
        public decimal LowestPrice;

        /// <summary>反向信号计数（EMA退出用）</summary>
        public int OppositeSignalCount;

        /// <summary>开仓时间</summary>
        public DateTime OpenTime;

        /// <summary>仓位大小（手数）</summary>
        public decimal Volume;

        /// <summary>当前浮动盈亏</summary>
        public decimal FloatingProfit;

        /// <summary>最大回撤</summary>
        public decimal MaxDrawdown;
    }

}
