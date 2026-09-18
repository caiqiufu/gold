using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uClient.Comm;

namespace uClient.Broker
{
    /// <summary>
    /// MANAGE_POSITION V4.1
    ///
    /// 核心逻辑：
    /// H4 Trend → M5 KDJ Entry → 策略金额 TP/SL
    /// → stopLoss金额定义1R → 分批平仓 TP1 → 分批平仓 TP2
    /// → 剩余仓位趋势/EMA退出
    ///
    /// 设计原则：
    /// 1. Initial SL 建立后，RiskR 固定
    /// 2. TP1 / TP2 全部基于 Initial RiskR
    /// 3. 本文件不发送任何 SL 修改信号
    /// 4. 本文件不执行 BreakEven / Trailing Stop
    /// 5. 仓位管理通过 TakeProfit1 / TakeProfit2 分批平仓完成
    /// 6. EAExecute.cs 不修改，继续使用现有 P_CLOSE_BUY / P_CLOSE_SELL
    /// 7. 一个 Tick 最多只产生一个可执行的平仓信号，避免 TP1/TP2 同Tick重复执行
    /// 8. StopLossHit / ExitByEMA 保持为整仓退出信号
    /// </summary>
    public class ManagePosition : StrategyEA
    {
        private readonly StringBuilder _log = new StringBuilder();

        // ============================================================
        // 默认风控参数
        // ============================================================

        /// <summary>XAUUSD: 1 lot × 金价移动1.00 = 100 USD</summary>
        private const decimal DEFAULT_GOLD_USD_PER_LOT_PER_PRICE = 100.00m;
        /// <summary>TP2相对于策略止损金额的R倍数</summary>
        private const decimal DEFAULT_TP2_R = 1.80m;
        /// <summary>EMA Buffer，仅用于趋势退出确认</summary>
        private const decimal DEFAULT_EMA_BUFFER_ATR = 0.20m;

        /// <summary>EMA 反向信号连续确认次数</summary>
        private const int DEFAULT_OPPOSITE_CONFIRM_COUNT = 2;

        // ============================================================
        // V4：分批平仓参数
        // ============================================================

        /// <summary>第一批平仓：1.00R</summary>
        private const decimal DEFAULT_PARTIAL_TP1_R = 1.00m;

        /// <summary>第二批平仓：1.80R</summary>
        private const decimal DEFAULT_PARTIAL_TP2_R = 1.80m;

        /// <summary>
        /// TP2 动态提前平仓的最低盈利门槛。
        /// 只有已经完成 TP1，并且当前盈利 >= 1.20R 时，
        /// 才允许因趋势明显回撤而提前执行第二批平仓。
        /// </summary>
        private const decimal DEFAULT_PULLBACK_TP2_MIN_R = 1.20m;

        /// <summary>
        /// TP2 动态提前平仓需要的回撤：>= 0.80 ATR。
        /// </summary>
        private const decimal DEFAULT_PULLBACK_TP2_ATR = 0.80m;

        // ============================================================
        // Execute
        // ============================================================

        /// <summary>
        /// 策略核心执行函数
        /// </summary>
        public override bool Execute(EAChart eaChart, IDictionary<string, Object> context)
        {
            _log.Clear();
            if (context == null)
            {
                _log.AppendLine("Context为空，不执行仓位管理");
                return false;
            }

            // ========================================================
            // 1. 基础参数
            // ========================================================
            string isSimulateTest = GetContextString(context, "IsSimulateTest", "F");
            string currentOrderType = GetContextString(context, "CurrentOrderType", "");
            string emaAtrPeriod = GetConfigString("EMAATRPeriod", "H4");
            int emaPeriod = GetConfigInt("EMAPeriod", 20);
            int atrPeriod = GetConfigInt("ATRPeriod", 14);
            string analysisDataType = GetContextString(context, "analysisDataType", "");
            decimal currentPrice = GetContextDecimal(context, "price", 0m);
            int trendCount = GetConfigInt("TrendCount", 2);
            decimal keepProfit =
                Convert.ToDecimal(this.configParam["keepProfit"]);
            decimal stopLoss =
                Convert.ToDecimal(this.configParam["stopLoss"]);

            // 策略金额：USD / 1 lot。
            // stopLoss 定义 1R；keepProfit 直接定义 TP1。
            decimal goldUsdPerLotPerPrice =
                GetConfigDecimal("GoldUsdPerLotPerPrice",
                    DEFAULT_GOLD_USD_PER_LOT_PER_PRICE);

            if (goldUsdPerLotPerPrice <= 0)
                goldUsdPerLotPerPrice = DEFAULT_GOLD_USD_PER_LOT_PER_PRICE;
            if (keepProfit <= 0) keepProfit = 20m;
            if (stopLoss <= 0) stopLoss = 30m;

            string currentDateTimeStr;
            if (context.ContainsKey("CurrentDateTime") && context["CurrentDateTime"] != null)
            {
                currentDateTimeStr = context["CurrentDateTime"].ToString();
            }
            else
            {
                currentDateTimeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            // ========================================================
            // 2. 当前持仓
            // ========================================================
            TradePosition tradePositionInfo = null;
            if (context.ContainsKey("TradePositionInfo") && context["TradePositionInfo"] is TradePosition)
            {
                tradePositionInfo = (TradePosition)context["TradePositionInfo"];
            }

            // 没有价格，直接退出
            if (currentPrice <= 0)
            {
                _log.AppendLine($"无效当前价格[{currentPrice}]，不执行仓位管理");
                RemoveTradeSignals(context);
                return false;
            }

            // ========================================================
            // 3. 获取 H4 EMA + ATR Trend
            // ========================================================
            IDictionary<string, string> datas = null;
            string strategyResult = "NO TREND";
            double strategyBarPrice = 0;
            double strategyCurrentPrice = 0;
            double ema = 0;
            double atr = 0;
            double swingHigh = 0;
            double swingLow = 0;

            if (string.Equals(isSimulateTest, "T", StringComparison.OrdinalIgnoreCase))
            {
                datas = DSHelper.getStrategyResultTest("TREND_EMA_ATR", "XAUUSD", emaAtrPeriod, currentDateTimeStr, trendCount);
            }
            else
            {
                datas = DSHelper.getStrategyResult("TREND_EMA_ATR", "XAUUSD", emaAtrPeriod, currentDateTimeStr, trendCount);
            }

            if (datas != null)
            {
                strategyResult = GetDictionaryString(datas, "result_status", "NO TREND");
                strategyBarPrice = GetDictionaryDouble(datas, "bar_price");
                strategyCurrentPrice = GetDictionaryDouble(datas, "current_price");
                ema = GetDictionaryDouble(datas, "ema");
                atr = GetDictionaryDouble(datas, "atr");
                swingHigh = GetDictionaryDouble(datas, "swing_high");
                swingLow = GetDictionaryDouble(datas, "swing_low");
            }

            // ========================================================
            // 4. 日志
            // ========================================================
            _log.AppendLine($"动态参数[EMAATRPeriod={emaAtrPeriod}][EMAperiod={emaPeriod}][ATRperiod={atrPeriod}][CurrentDateTime={currentDateTimeStr}][StrategyResult={strategyResult}][BarPrice={strategyBarPrice}][StrategyPrice={strategyCurrentPrice}][QuotePrice={currentPrice}][OrderType={currentOrderType}]");
            _log.AppendLine($"趋势数据[EMA={ema}][ATR={atr}][SwingHigh={swingHigh}][SwingLow={swingLow}]");

            // ========================================================
            // 5. 没有持仓
            // ========================================================
            if (tradePositionInfo == null)
            {
                _log.AppendLine("没有订单，不执行仓位管理");
                RemoveTradeSignals(context);
                return false;
            }

            // ========================================================
            // 6. 仓位方向一致性检查
            // ========================================================
            // 关键保护：
            // CurrentOrderType 是当前实际订单方向；
            // TradePositionInfo.IsBuy 是仓位状态方向。
            //
            // 如果二者不一致，绝对不能直接使用旧仓位执行
            // StopLossHit / TakeProfit / EMA Exit。
            //
            // 典型危险场景：
            //   CurrentOrderType = SELL
            //   TradePositionInfo.IsBuy = true
            //
            // 旧 BUY 的价格状态可能立即触发 SL，然后错误生成
            // CLOSE_SELL。
            // ========================================================
            bool currentOrderIsBuy = string.Equals(
                currentOrderType, "BUY", StringComparison.OrdinalIgnoreCase);

            bool currentOrderIsSell = string.Equals(
                currentOrderType, "SELL", StringComparison.OrdinalIgnoreCase);

            bool positionRebuiltThisTick = false;

            if ((currentOrderIsBuy || currentOrderIsSell) &&
                tradePositionInfo != null)
            {
                bool directionMatches =
                    (currentOrderIsBuy && tradePositionInfo.IsBuy) ||
                    (currentOrderIsSell && !tradePositionInfo.IsBuy);

                if (!directionMatches)
                {
                    _log.AppendLine(
                        $"⚠️ POSITION DIRECTION MISMATCH: " +
                        $"CurrentOrderType={currentOrderType}, " +
                        $"TradePositionInfo.IsBuy={tradePositionInfo.IsBuy}, " +
                        $"Entry={tradePositionInfo.EntryPrice}, " +
                        $"CurrentPrice={currentPrice}");

                    // ------------------------------------------------
                    // 如果上下文提供了当前真实开仓价，则重建仓位状态。
                    // 这样可以修复旧 BUY 状态残留后继续管理真实 SELL。
                    // 如果没有可靠 openPrice，则宁可 HOLD，也绝不
                    // 使用错误方向的旧仓位触发平仓。
                    // ------------------------------------------------
                    decimal contextOpenPrice =
                        GetContextDecimal(context, "openPrice", 0m);

                    if (contextOpenPrice > 0)
                    {
                        bool rebuiltIsBuy = currentOrderIsBuy;

                        decimal moneyRiskDistance =
                            stopLoss / goldUsdPerLotPerPrice;

                        if (moneyRiskDistance > 0)
                        {
                            decimal rebuiltStopLoss = rebuiltIsBuy
                                ? contextOpenPrice - moneyRiskDistance
                                : contextOpenPrice + moneyRiskDistance;

                            tradePositionInfo = IndicatorHelper.OpenPosition(
                                rebuiltIsBuy,
                                contextOpenPrice,
                                rebuiltStopLoss);

                            _log.AppendLine(
                                $"✅ POSITION STATE REBUILT: " +
                                $"Direction={(rebuiltIsBuy ? "BUY" : "SELL")}, " +
                                $"Entry={contextOpenPrice}, " +
                                $"StopLoss={rebuiltStopLoss}, " +
                                $"RiskR={tradePositionInfo.RiskR}, " +
                                $"MoneyRisk={stopLoss} USD/lot");

                            context["TradePositionInfo"] = tradePositionInfo;
                            positionRebuiltThisTick = true;
                        }
                        else
                        {
                            _log.AppendLine(
                                "⚠️ POSITION REBUILD FAILED: " +
                                "money risk distance <= 0; management skipped.");

                            RemoveTradeSignals(context);
                            return false;
                        }
                    }
                    else
                    {
                        _log.AppendLine(
                            "⚠️ POSITION REBUILD SKIPPED: " +
                            "context[openPrice] unavailable. " +
                            "NO CLOSE SIGNAL will be generated.");

                        RemoveTradeSignals(context);
                        return false;
                    }
                }
            }

            // ========================================================
            // 7. 如果本Tick刚刚重建仓位状态
            // ========================================================
            // 不允许同一个Tick继续执行SL/TP/EMA判断。
            // 目的：防止“旧仓位状态 + 新订单”在同一Tick中
            // 被错误地立即平仓。
            //
            // 下一Tick开始，再用新的 Entry / Direction / RiskR
            // 正常执行 Money-Risk 管理。
            // ========================================================
            if (positionRebuiltThisTick)
            {
                RemoveTradeSignals(context);

                _log.AppendLine(
                    "POSITION STATE REBUILT THIS TICK -> " +
                    "SKIP SL/TP/EMA FOR THIS TICK");

                _log.AppendLine(BuildPositionManagementStatus(
                    tradePositionInfo,
                    currentPrice,
                    (decimal)atr,
                    (decimal)ema,
                    strategyResult,
                    new List<TradeSignal>(),
                    keepProfit,
                    stopLoss,
                    goldUsdPerLotPerPrice));

                return true;
            }

            // ========================================================
            // 8. 仓位基础状态检查
            // ========================================================
            if (tradePositionInfo == null || tradePositionInfo.RiskR <= 0)
            {
                _log.AppendLine(
                    $"仓位RiskR无效[{(tradePositionInfo == null ? 0 : tradePositionInfo.RiskR)}]，停止仓位管理");
                RemoveTradeSignals(context);
                return false;
            }

            // ========================================================
            // 9. 执行 Tick 级别仓位管理
            // ========================================================
            var signals = IndicatorHelper.ManagePositionSignal(
                tradePositionInfo,
                currentPrice,
                (decimal)atr,
                (decimal)ema,
                strategyResult,
                keepProfit,
                stopLoss,
                goldUsdPerLotPerPrice);

            if (signals == null)
                signals = new List<TradeSignal>();

            // ========================================================
            // 10. 每个Tick输出完整仓位状态
            // HOLD不是管理失败。
            // ========================================================
            _log.AppendLine(BuildPositionManagementStatus(
                tradePositionInfo,
                currentPrice,
                (decimal)atr,
                (decimal)ema,
                strategyResult,
                signals,
                keepProfit,
                stopLoss,
                goldUsdPerLotPerPrice));

            if (signals.Count > 0)
            {
                string signalText = string.Join(",", signals.Select(s => s.ToString()));
                _log.AppendLine($"仓位更新信号[{signalText}]");
                _log.AppendLine($"仓位信息[{JsonConvert.SerializeObject(tradePositionInfo)}]");
                context["TradeSignals"] = signals;
            }
            else
            {
                RemoveTradeSignals(context);
            }

            context["TradePositionInfo"] = tradePositionInfo;

            // 有持仓且管理器正常运行，即使本Tick只是HOLD，也返回true。
            return true;
        }

        // ============================================================
        // ManagePositionSignal
        // ============================================================

        /// <summary>
        /// Tick级别仓位管理核心。
        ///
        /// 风控顺序：
        /// 1. 基础校验
        /// 2. 修复非法SL
        /// 3. 更新最高/最低价
        /// 4. 计算浮盈
        /// 5. 初始SL检查
        /// 6. TP1分批平仓
        /// 7. TP2分批平仓
        /// 8. EMA整仓退出
        /// </summary>
        public static List<TradeSignal> ManagePositionSignal(
            TradePosition pos,
            decimal price,
            decimal atr,
            decimal ema,
            string strategyResult,
            decimal keepProfitMoneyPerLot,
            decimal stopLossMoneyPerLot,
            decimal goldUsdPerLotPerPrice = DEFAULT_GOLD_USD_PER_LOT_PER_PRICE)
        {
            var signals = new List<TradeSignal>();

            if (pos == null)
                return signals;

            if (price <= 0)
                return signals;

            bool atrReady = atr > 0;
            bool emaReady = ema > 0;

            if (goldUsdPerLotPerPrice <= 0)
                goldUsdPerLotPerPrice = DEFAULT_GOLD_USD_PER_LOT_PER_PRICE;
            if (keepProfitMoneyPerLot <= 0)
                keepProfitMoneyPerLot = 20m;
            if (stopLossMoneyPerLot <= 0)
                stopLossMoneyPerLot = 30m;

            bool riskValid = stopLossMoneyPerLot > 0;
            if (!riskValid)
                return signals;

            // ========================================================
            // 1. 初始SL方向校验
            // ========================================================
            // 注意：
            // 这里只负责保证初始SL数据合法。
            // 不进行任何动态SL移动。
            NormalizeStopLoss(pos);

            // ========================================================
            // 2. 更新MFE轨迹
            // ========================================================
            UpdatePriceExtremes(pos, price);

            // ========================================================
            // 3. 当前盈利 / R
            // ========================================================
            decimal profitPrice = CalculateProfit(pos, price);
            decimal profitMoneyPerLot =
                profitPrice * goldUsdPerLotPerPrice;
            decimal currentR =
                profitMoneyPerLot / stopLossMoneyPerLot;

            // ========================================================
            // 4. 真实SL触发
            // ========================================================
            // SL仍然是整仓退出的最高优先级。
            if (IsStrategyStopLossHit(
                    pos,
                    price,
                    stopLossMoneyPerLot,
                    goldUsdPerLotPerPrice))
            {
                signals.Add(TradeSignal.StopLossHit);
                return signals;
            }

            // ========================================================
            // 5. TP1：第一批平仓
            // ========================================================
            // 重要：
            // 只产生 TakeProfit1。
            // 不再同时产生 MoveStopToBreakEven。
            //
            // EAExecute.cs 会继续按照现有逻辑：
            // TakeProfit1 -> P_CLOSE_BUY / P_CLOSE_SELL
            // -> ManagePositionTakeProfitLot1
            // ========================================================
            // TP1直接由策略 keepProfit 金额控制。
            if (!pos.TP1Hit && profitMoneyPerLot >= keepProfitMoneyPerLot)
            {
                pos.TP1Hit = true;
                signals.Add(TradeSignal.TakeProfit1);
                return signals;
            }

            // ========================================================
            // 6. TP2：第二批平仓
            // ========================================================
            // 正常条件：
            // currentR >= 1.80R
            //
            // 动态条件：
            // TP1已经完成
            // + 当前至少达到1.20R
            // + 从MFE回撤 >= 0.80 ATR
            //
            // 这样仓位管理是“分批平仓”，而不是修改SL。
            if (pos.TP1Hit && !pos.TP2Hit)
            {
                decimal pullback = pos.IsBuy
                    ? Math.Max(0m, pos.HighestPrice - price)
                    : Math.Max(0m, price - pos.LowestPrice);

                decimal pullbackAtr = atrReady ? pullback / atr : 0m;

                decimal tp2MoneyPerLot =
                    stopLossMoneyPerLot * DEFAULT_TP2_R;

                decimal pullbackTp2MinMoneyPerLot =
                    stopLossMoneyPerLot * DEFAULT_PULLBACK_TP2_MIN_R;

                bool normalTP2 = profitMoneyPerLot >= tp2MoneyPerLot;

                bool pullbackTP2 =
                    atrReady &&
                    profitMoneyPerLot >= pullbackTp2MinMoneyPerLot &&
                    pullbackAtr >= DEFAULT_PULLBACK_TP2_ATR;

                if (normalTP2 || pullbackTP2)
                {
                    pos.TP2Hit = true;
                    signals.Add(TradeSignal.TakeProfit2);
                    return signals;
                }
            }

            // ========================================================
            // 7. EMA趋势退出
            // ========================================================
            // TP1 / TP2 已经在上面优先处理。
            // 只有没有触发分批平仓时，才检查整仓EMA退出。
            if (emaReady && atrReady)
            {
                UpdateEmaExit(
                    pos,
                    price,
                    atr,
                    ema,
                    strategyResult,
                    signals);
            }

            return signals;
        }

        // ============================================================
        // StopLoss
        // ============================================================
        private static bool IsStrategyStopLossHit(
            TradePosition pos,
            decimal price,
            decimal stopLossMoneyPerLot,
            decimal goldUsdPerLotPerPrice)
        {
            if (pos == null || stopLossMoneyPerLot <= 0 || goldUsdPerLotPerPrice <= 0)
                return false;

            decimal stopDistance =
                stopLossMoneyPerLot / goldUsdPerLotPerPrice;

            if (pos.IsBuy)
                return price <= pos.EntryPrice - stopDistance;

            return price >= pos.EntryPrice + stopDistance;
        }

        // ============================================================
        // Normalize SL
        // ============================================================
        private static void NormalizeStopLoss(TradePosition pos)
        {
            if (pos.RiskR <= 0)
                return;

            if (pos.IsBuy)
            {
                if (pos.StopLoss >= pos.EntryPrice)
                {
                    // 如果已经处于BE以上，不允许因为异常数据重新把SL放回去。
                    if (pos.BreakEvenActivated)
                        return;
                    pos.StopLoss = pos.EntryPrice - pos.RiskR;
                }
            }
            else
            {
                if (pos.StopLoss <= pos.EntryPrice)
                {
                    if (pos.BreakEvenActivated)
                        return;
                    pos.StopLoss = pos.EntryPrice + pos.RiskR;
                }
            }
        }

        // ============================================================
        // Price Extremes
        // ============================================================
        private static void UpdatePriceExtremes(TradePosition pos, decimal price)
        {
            if (pos.IsBuy)
            {
                if (price > pos.HighestPrice)
                    pos.HighestPrice = price;
            }
            else
            {
                if (price < pos.LowestPrice)
                    pos.LowestPrice = price;
            }
        }

        // ============================================================
        // Profit
        // ============================================================
        private static decimal CalculateProfit(TradePosition pos, decimal price)
        {
            if (pos.IsBuy)
                return price - pos.EntryPrice;
            return pos.EntryPrice - price;
        }


        // ============================================================
        // EMA Exit
        // ============================================================
        private static void UpdateEmaExit(TradePosition pos, decimal price, decimal atr, decimal ema, string strategyResult, List<TradeSignal> signals)
        {
            if (atr <= 0 || ema <= 0)
                return;

            decimal emaBuffer = atr * DEFAULT_EMA_BUFFER_ATR;
            bool oppositeTrend = false;

            if (pos.IsBuy)
            {
                oppositeTrend = string.Equals(strategyResult, "SELL TREND", StringComparison.OrdinalIgnoreCase)
                    && price < ema - emaBuffer;
            }
            else
            {
                oppositeTrend = string.Equals(strategyResult, "BUY TREND", StringComparison.OrdinalIgnoreCase)
                    && price > ema + emaBuffer;
            }

            if (oppositeTrend)
            {
                pos.OppositeSignalCount++;
            }
            else
            {
                pos.OppositeSignalCount = 0;
            }

            // 必须有真实回撤
            decimal pullback;
            if (pos.IsBuy)
            {
                pullback = pos.HighestPrice - price;
            }
            else
            {
                pullback = price - pos.LowestPrice;
            }

            if (pos.OppositeSignalCount >= DEFAULT_OPPOSITE_CONFIRM_COUNT && pullback > atr)
            {
                signals.Add(TradeSignal.ExitByEMA);
            }
        }

        // ============================================================
        // Calculate Initial Stop Loss
        // ============================================================

        /// <summary>
        /// 计算初始SL：
        /// BUY: Entry - ATR * multiplier，与 SwingLow 比较后取更低位置
        /// SELL: Entry + ATR * multiplier，与 SwingHigh 比较后取更高位置
        ///
        /// 注意：RiskR 在开仓时固定，后续Trailing不会重新计算RiskR。
        /// </summary>
        public static decimal CalculateInitialStopLoss(bool isBuy, decimal entryPrice, decimal atr, decimal swingHigh, decimal swingLow, decimal atrMultiplier = 1.20m)
        {
            if (entryPrice <= 0)
                throw new ArgumentException("Entry price must be > 0");

            // ATR异常保护
            if (atr <= 0)
            {
                atr = entryPrice * 0.005m;
            }
            if (atrMultiplier <= 0)
                atrMultiplier = 1.20m;

            // BUY
            if (isBuy)
            {
                decimal atrSL = entryPrice - atr * atrMultiplier;
                decimal structureSL = (swingLow > 0 && swingLow < entryPrice) ? swingLow - atr * 0.25m : atrSL;
                decimal finalSL = Math.Min(atrSL, structureSL);
                if (finalSL >= entryPrice)
                    finalSL = atrSL;
                return finalSL;
            }

            // SELL
            decimal sellAtrSL = entryPrice + atr * atrMultiplier;
            decimal sellStructureSL = (swingHigh > 0 && swingHigh > entryPrice) ? swingHigh + atr * 0.25m : sellAtrSL;
            decimal finalSellSL = Math.Max(sellAtrSL, sellStructureSL);
            if (finalSellSL <= entryPrice)
                finalSellSL = sellAtrSL;
            return finalSellSL;
        }

        // ============================================================
        // Open Position
        // ============================================================

        /// <summary>
        /// 开仓时创建TradePosition。
        /// RiskR = Entry - InitialSL，一旦建立后续固定。
        /// </summary>
        public static TradePosition OpenPosition(bool isBuy, decimal entry, decimal stopLoss)
        {
            if (entry <= 0)
                throw new ArgumentException("Entry price must be > 0");
            if (stopLoss <= 0)
                throw new ArgumentException("StopLoss must be > 0");
            if (isBuy && stopLoss >= entry)
                throw new ArgumentException("Buy单止损必须小于入场价");
            if (!isBuy && stopLoss <= entry)
                throw new ArgumentException("Sell单止损必须大于入场价");

            decimal risk = Math.Abs(entry - stopLoss);
            if (risk <= 0)
                throw new ArgumentException("RiskR cannot be 0");

            return new TradePosition
            {
                eaTradeId = DBUtils.GetUniqueSerialNumber(),
                IsBuy = isBuy,
                EntryPrice = entry,
                StopLoss = stopLoss,
                InitialStopLoss = stopLoss,
                // 关键：RiskR固定为Initial SL距离
                RiskR = risk,
                HighestPrice = entry,
                LowestPrice = entry,
                BreakEvenActivated = false,
                TP1Hit = false,
                TP2Hit = false,
                OppositeSignalCount = 0,
                OpenTime = DateTime.Now
            };
        }

        // ============================================================
        // Position Management Status
        // ============================================================

        /// <summary>
        /// 每个Tick输出完整仓位状态。
        /// 仓位管理只通过分批平仓信号执行，不修改SL。
        /// </summary>
        private static string BuildPositionManagementStatus(
            TradePosition pos,
            decimal price,
            decimal atr,
            decimal ema,
            string strategyResult,
            List<TradeSignal> signals,
            decimal keepProfitMoneyPerLot,
            decimal stopLossMoneyPerLot,
            decimal goldUsdPerLotPerPrice)
        {
            if (pos == null)
                return "仓位状态[NULL]";

            decimal profitPrice = CalculateProfit(pos, price);
            decimal profitMoneyPerLot =
                profitPrice * goldUsdPerLotPerPrice;
            decimal currentR =
                stopLossMoneyPerLot > 0
                    ? profitMoneyPerLot / stopLossMoneyPerLot
                    : 0m;

            decimal pullback = pos.IsBuy
                ? Math.Max(0m, pos.HighestPrice - price)
                : Math.Max(0m, price - pos.LowestPrice);

            decimal pullbackAtr = atr > 0 ? pullback / atr : 0m;

            bool trendAligned = pos.IsBuy
                ? string.Equals(strategyResult, "BUY TREND", StringComparison.OrdinalIgnoreCase)
                : string.Equals(strategyResult, "SELL TREND", StringComparison.OrdinalIgnoreCase);

            string state;

            if (IsStrategyStopLossHit(
                    pos,
                    price,
                    stopLossMoneyPerLot,
                    goldUsdPerLotPerPrice))
                state = "STOP_LOSS";
            else if (!trendAligned)
                state = "TREND_MISALIGNED";
            else if (pullbackAtr >= DEFAULT_PULLBACK_TP2_ATR)
                state = "DEEP_PULLBACK";
            else if (pullbackAtr >= DEFAULT_PULLBACK_TP2_ATR * 0.625m)
                state = "PULLBACK_WARNING";
            else if (pos.TP2Hit)
                state = "TP2_DONE_REMAINING";
            else if (pos.TP1Hit &&
                     profitMoneyPerLot >=
                     stopLossMoneyPerLot * DEFAULT_PULLBACK_TP2_MIN_R)
                state = "TP1_DONE_MANAGING";
            else if (pos.TP1Hit)
                state = "TP1_DONE_HOLD";
            else if (profitMoneyPerLot >= keepProfitMoneyPerLot)
                state = "TP1_READY";
            else if (currentR > 0)
                state = "PROFIT_HOLD";
            else
                state = "LOSS_HOLD";

            string action = signals != null && signals.Count > 0
                ? string.Join(",", signals.Select(s => s.ToString()))
                : "HOLD";

            decimal slDistanceFromPrice = Math.Abs(price - pos.StopLoss);

            return
                "============================================================" + Environment.NewLine +
                "MANAGE_POSITION V4.1 MONEY-RISK STATE" + Environment.NewLine +
                "============================================================" + Environment.NewLine +
                $"Direction          : {(pos.IsBuy ? "BUY" : "SELL")}" + Environment.NewLine +
                $"Entry              : {pos.EntryPrice}" + Environment.NewLine +
                $"Current Price      : {price}" + Environment.NewLine +
                $"Initial SL         : {pos.InitialStopLoss}" + Environment.NewLine +
                $"Current SL         : {pos.StopLoss}" + Environment.NewLine +
                $"SL Distance        : {slDistanceFromPrice}" + Environment.NewLine +
                $"Risk R             : {pos.RiskR}" + Environment.NewLine +
                $"Current R          : {currentR:F4}" + Environment.NewLine +
                $"Profit Price      : {profitPrice:F4}" + Environment.NewLine + $"Profit USD/lot     : {profitMoneyPerLot:F2}" + Environment.NewLine +
                $"Highest Price      : {pos.HighestPrice}" + Environment.NewLine +
                $"Lowest Price       : {pos.LowestPrice}" + Environment.NewLine +
                $"Pullback           : {pullback:F4}" + Environment.NewLine +
                $"Pullback / ATR     : {pullbackAtr:F4}" + Environment.NewLine +
                $"H4 Trend           : {strategyResult}" + Environment.NewLine +
                $"EMA                : {ema}" + Environment.NewLine +
                $"ATR                : {atr}" + Environment.NewLine +
                $"Trend Aligned      : {trendAligned}" + Environment.NewLine +
                $"SL Modification    : DISABLED" + Environment.NewLine +
                $"BreakEven          : DISABLED" + Environment.NewLine +
                $"TP1 Partial Close  : {pos.TP1Hit}" + Environment.NewLine +
                $"TP2 Partial Close  : {pos.TP2Hit}" + Environment.NewLine +
                $"TP1 Target R       : {DEFAULT_PARTIAL_TP1_R}" + Environment.NewLine +
                $"TP2 Target R       : {DEFAULT_PARTIAL_TP2_R}" + Environment.NewLine +
                $"TP2 Pullback ATR    : {DEFAULT_PULLBACK_TP2_ATR}" + Environment.NewLine +
                $"Position State     : {state}" + Environment.NewLine +
                $"Action             : {action}" + Environment.NewLine +
                "============================================================";
        }

        // ============================================================
        // Context Helpers
        // ============================================================
        private string GetConfigString(string key, string defaultValue)
        {
            try
            {
                if (configParam == null)
                    return defaultValue;
                if (!configParam.ContainsKey(key))
                    return defaultValue;
                string value = configParam[key];
                return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
            }
            catch
            {
                return defaultValue;
            }
        }

        private decimal GetConfigDecimal(string key, decimal defaultValue)
        {
            string value = GetConfigString(
                key,
                defaultValue.ToString(System.Globalization.CultureInfo.InvariantCulture));

            decimal result;
            return decimal.TryParse(
                value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out result)
                ? result
                : defaultValue;
        }

        private int GetConfigInt(string key, int defaultValue)
        {
            string value = GetConfigString(key, defaultValue.ToString());
            int result;
            return int.TryParse(value, out result) ? result : defaultValue;
        }

        private static string GetContextString(IDictionary<string, Object> context, string key, string defaultValue)
        {
            if (context == null || !context.ContainsKey(key) || context[key] == null)
                return defaultValue;
            return context[key].ToString();
        }

        private static decimal GetContextDecimal(IDictionary<string, Object> context, string key, decimal defaultValue)
        {
            if (context == null || !context.ContainsKey(key) || context[key] == null)
                return defaultValue;
            decimal result;
            return decimal.TryParse(context[key].ToString(), out result) ? result : defaultValue;
        }

        private static string GetDictionaryString(IDictionary<string, string> data, string key, string defaultValue)
        {
            if (data == null || !data.ContainsKey(key) || data[key] == null)
                return defaultValue;
            string value = data[key].ToString();
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private static double GetDictionaryDouble(IDictionary<string, string> data, string key)
        {
            if (data == null || !data.ContainsKey(key) || data[key] == null)
                return 0;
            double result;
            return double.TryParse(data[key].ToString(), out result) ? result : 0;
        }

        private static void RemoveTradeSignals(IDictionary<string, Object> context)
        {
            if (context != null && context.ContainsKey("TradeSignals"))
            {
                context.Remove("TradeSignals");
            }
        }

        // ============================================================
        // Description
        // ============================================================
        public override string ToDesc()
        {
            return _log.ToString();
        }

        public double ToDoubleSafe(object value, double defaultValue = 0)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;
            double result;
            return double.TryParse(value.ToString(), out result) ? result : defaultValue;
        }
    }
}