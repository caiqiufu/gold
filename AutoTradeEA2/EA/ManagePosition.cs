using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uClient.Comm;

namespace uClient.Broker
{
    /// <summary>
    /// MANAGE_POSITION V4.2 PRICE-DISTANCE
    ///
    /// 核心逻辑：
    /// H4 Trend → M5 KDJ Entry → 价格距离 TP/SL
    /// → stopLoss价格距离定义1R → 分批平仓 TP1 → 分批平仓 TP2
    /// → 剩余仓位趋势/EMA退出
    ///
    /// 设计原则：
    /// 1. keepProfit / stopLoss 全部使用金价价格距离，不是美元金额
    /// 2. RiskR = Initial SL 的价格距离
    /// 3. TP1 / TP2 全部基于价格距离
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

        /// <summary>TP2相对于初始价格风险距离的R倍数</summary>
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
            // ========================================================
            // 价格距离参数
            // ========================================================
            // keepProfit / stopLoss 都是 XAUUSD 金价价格距离。
            // 例如：
            //   stopLoss = 20  -> 入场价反向 20.00 才触发止损
            //   keepProfit = 20 -> 入场价有利方向 20.00 触发 TP1
            //
            // Lot size / USD P&L 不参与这里的 TP/SL 判断。
            decimal keepProfit =
                Convert.ToDecimal(this.configParam["keepProfit"]);
            decimal stopLoss =
                Convert.ToDecimal(this.configParam["stopLoss"]);

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

                        decimal stopLossDistance = stopLoss;

                        if (stopLossDistance > 0)
                        {
                            decimal rebuiltStopLoss = rebuiltIsBuy
                                ? contextOpenPrice - stopLossDistance
                                : contextOpenPrice + stopLossDistance;

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
                                $"StopLossDistance={stopLoss} price");

                            context["TradePositionInfo"] = tradePositionInfo;
                            positionRebuiltThisTick = true;
                        }
                        else
                        {
                            _log.AppendLine(
                                "⚠️ POSITION REBUILD FAILED: " +
                                "price distance <= 0; management skipped.");

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
            // 正常执行 Price-Distance 管理。
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
                     stopLoss));
                return true;
            }

            // ========================================================
            // 8. 仓位基础状态检查
            // ========================================================
            if (tradePositionInfo == null || tradePositionInfo.EntryPrice <= 0)
            {
                _log.AppendLine(
                    $"仓位EntryPrice无效[{(tradePositionInfo == null ? 0 : tradePositionInfo.EntryPrice)}]，停止仓位管理");
                RemoveTradeSignals(context);
                return false;
            }

            // ========================================================
            // 9. 将仓位内部RiskR统一到当前价格距离stopLoss
            // ========================================================
            // 旧版本可能遗留 RiskR=90 / SL=Entry±90 等状态。
            // 当前版本的 stopLoss 是价格距离，因此必须统一：
            //   RiskR = stopLoss
            //   InitialSL = Entry ± stopLoss
            //   CurrentSL = Entry ± stopLoss
            //
            // 这只是内部状态同步，不发送任何Broker SL修改命令。
            SynchronizePositionRisk(tradePositionInfo, stopLoss);

            // ========================================================
            // 10. 执行 Tick 级别仓位管理
            // ========================================================
            var signals = IndicatorHelper.ManagePositionSignal(
                tradePositionInfo,
                currentPrice,
                (decimal)atr,
                (decimal)ema,
                strategyResult,
                keepProfit,
                 stopLoss);
            if (signals == null)
                signals = new List<TradeSignal>();

            // ========================================================
            // 11. 每个Tick输出完整仓位状态
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
                 stopLoss));
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
        /// 4. 计算当前盈利价格距离
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
            decimal keepProfitDistance,
            decimal stopLossDistance,
            // 保留第8参数仅为兼容旧调用方；当前版本完全忽略它。
            decimal unusedCompatibilityParameter = 0m)
        {
            var signals = new List<TradeSignal>();

            if (pos == null)
                return signals;

            if (price <= 0)
                return signals;

            bool atrReady = atr > 0;
            bool emaReady = ema > 0;

            // ========================================================
            // 参数全部为“价格距离”
            // ========================================================
            if (keepProfitDistance <= 0)
                keepProfitDistance = 20m;

            if (stopLossDistance <= 0)
                stopLossDistance = 30m;

            if (stopLossDistance <= 0)
                return signals;

            // ========================================================
            // 1. 统一仓位内部SL/RiskR
            // ========================================================
            // 当前版本不把USD金额转换成价格。
            SynchronizePositionRisk(pos, stopLossDistance);

            // ========================================================
            // 2. 更新MFE轨迹
            // ========================================================
            UpdatePriceExtremes(pos, price);

            // ========================================================
            // 3. 当前盈利价格距离 / R
            // ========================================================
            decimal profitDistance = CalculateProfit(pos, price);

            decimal currentR =
                profitDistance / stopLossDistance;

            // ========================================================
            // 4. 初始SL触发
            // ========================================================
            // 直接比较价格距离：
            //
            // BUY : Entry - Current >= stopLossDistance
            // SELL: Current - Entry >= stopLossDistance
            //
            // 不计算USD，不计算Lot。
            if (IsStrategyStopLossHit(
                    pos,
                    price,
                    stopLossDistance))
            {
                signals.Add(TradeSignal.StopLossHit);
                return signals;
            }

            // ========================================================
            // 5. TP1：第一批平仓
            // ========================================================
            // TP1直接使用keepProfit价格距离。
            if (!pos.TP1Hit &&
                profitDistance >= keepProfitDistance)
            {
                pos.TP1Hit = true;
                signals.Add(TradeSignal.TakeProfit1);
                return signals;
            }

            // ========================================================
            // 6. TP2：第二批平仓
            // ========================================================
            // 正常：
            //   profitDistance >= RiskR * 1.80
            //
            // 动态提前：
            //   TP1完成
            //   + profitDistance >= RiskR * 1.20
            //   + 回撤 >= 0.80 ATR
            if (pos.TP1Hit && !pos.TP2Hit)
            {
                decimal pullback = pos.IsBuy
                    ? Math.Max(0m, pos.HighestPrice - price)
                    : Math.Max(0m, price - pos.LowestPrice);

                decimal pullbackAtr =
                    atrReady ? pullback / atr : 0m;

                decimal tp2Distance =
                    stopLossDistance * DEFAULT_TP2_R;

                decimal pullbackTp2MinDistance =
                    stopLossDistance * DEFAULT_PULLBACK_TP2_MIN_R;

                bool normalTP2 =
                    profitDistance >= tp2Distance;

                bool pullbackTP2 =
                    atrReady &&
                    profitDistance >= pullbackTp2MinDistance &&
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
            decimal stopLossDistance)
        {
            if (pos == null || stopLossDistance <= 0)
                return false;

            if (pos.IsBuy)
                return price <= pos.EntryPrice - stopLossDistance;

            return price >= pos.EntryPrice + stopLossDistance;
        }

        // ============================================================
        // Synchronize Position Risk
        // ============================================================

        /// <summary>
        /// 将TradePosition内部的RiskR/InitialSL/CurrentSL统一到
        /// 当前策略配置的“价格距离stopLoss”。
        ///
        /// 注意：
        /// 1. 这里只修改内存中的TradePosition状态；
        /// 2. 不发送任何Broker SL修改命令；
        /// 3. 不使用Lot、不使用USD金额；
        /// 4. TP1/TP2/SL均以价格距离计算。
        /// </summary>
        private static void SynchronizePositionRisk(
            TradePosition pos,
            decimal stopLossDistance)
        {
            if (pos == null || pos.EntryPrice <= 0 || stopLossDistance <= 0)
                return;

            decimal expectedStopLoss = pos.IsBuy
                ? pos.EntryPrice - stopLossDistance
                : pos.EntryPrice + stopLossDistance;

            pos.RiskR = stopLossDistance;
            pos.InitialStopLoss = expectedStopLoss;

            // 当前版本没有BreakEven/Trailing，因此Current SL始终保持
            // 初始策略止损价格。
            pos.StopLoss = expectedStopLoss;
            pos.BreakEvenActivated = false;
        }

        // ============================================================
        // Legacy-compatible Normalize SL
        // ============================================================

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
            decimal keepProfitDistance,
            decimal stopLossDistance)
        {
            if (pos == null)
                return "仓位状态[NULL]";

            // 保证状态显示与实际价格距离判断完全一致。
            SynchronizePositionRisk(pos, stopLossDistance);

            decimal profitDistance =
                CalculateProfit(pos, price);

            decimal currentR =
                stopLossDistance > 0
                    ? profitDistance / stopLossDistance
                    : 0m;

            decimal pullback = pos.IsBuy
                ? Math.Max(0m, pos.HighestPrice - price)
                : Math.Max(0m, price - pos.LowestPrice);

            decimal pullbackAtr =
                atr > 0 ? pullback / atr : 0m;

            bool trendAligned = pos.IsBuy
                ? string.Equals(
                    strategyResult,
                    "BUY TREND",
                    StringComparison.OrdinalIgnoreCase)
                : string.Equals(
                    strategyResult,
                    "SELL TREND",
                    StringComparison.OrdinalIgnoreCase);

            decimal tp1Distance = keepProfitDistance;
            decimal tp2Distance =
                stopLossDistance * DEFAULT_TP2_R;

            decimal tp1Price = pos.IsBuy
                ? pos.EntryPrice + tp1Distance
                : pos.EntryPrice - tp1Distance;

            decimal tp2Price = pos.IsBuy
                ? pos.EntryPrice + tp2Distance
                : pos.EntryPrice - tp2Distance;

            string state;

            if (IsStrategyStopLossHit(
                    pos,
                    price,
                    stopLossDistance))
            {
                state = "STOP_LOSS";
            }
            else if (!trendAligned)
            {
                state = "TREND_MISALIGNED";
            }
            else if (pullbackAtr >= DEFAULT_PULLBACK_TP2_ATR)
            {
                state = "DEEP_PULLBACK";
            }
            else if (pullbackAtr >=
                     DEFAULT_PULLBACK_TP2_ATR * 0.625m)
            {
                state = "PULLBACK_WARNING";
            }
            else if (pos.TP2Hit)
            {
                state = "TP2_DONE_REMAINING";
            }
            else if (pos.TP1Hit &&
                     profitDistance >=
                     stopLossDistance *
                     DEFAULT_PULLBACK_TP2_MIN_R)
            {
                state = "TP1_DONE_MANAGING";
            }
            else if (pos.TP1Hit)
            {
                state = "TP1_DONE_HOLD";
            }
            else if (profitDistance >= keepProfitDistance)
            {
                state = "TP1_READY";
            }
            else if (currentR > 0)
            {
                state = "PROFIT_HOLD";
            }
            else
            {
                state = "LOSS_HOLD";
            }

            string action =
                signals != null && signals.Count > 0
                    ? string.Join(
                        ",",
                        signals.Select(s => s.ToString()))
                    : "HOLD";

            decimal slDistanceFromEntry =
                Math.Abs(pos.EntryPrice - pos.StopLoss);

            return
                "============================================================" +
                Environment.NewLine +
                "MANAGE_POSITION V4.2 PRICE-DISTANCE STATE" +
                Environment.NewLine +
                "============================================================" +
                Environment.NewLine +
                $"Direction          : {(pos.IsBuy ? "BUY" : "SELL")}" +
                Environment.NewLine +
                $"Entry              : {pos.EntryPrice}" +
                Environment.NewLine +
                $"Current Price      : {price}" +
                Environment.NewLine +
                $"Initial SL         : {pos.InitialStopLoss}" +
                Environment.NewLine +
                $"Current SL         : {pos.StopLoss}" +
                Environment.NewLine +
                $"SL Distance        : {slDistanceFromEntry:F4}" +
                Environment.NewLine +
                $"Configured SL Dist : {stopLossDistance:F4}" +
                Environment.NewLine +
                $"Risk R             : {stopLossDistance:F4}" +
                Environment.NewLine +
                $"Current R          : {currentR:F4}" +
                Environment.NewLine +
                $"Profit Distance    : {profitDistance:F4}" +
                Environment.NewLine +
                $"TP1 Distance       : {tp1Distance:F4}" +
                Environment.NewLine +
                $"TP1 Price          : {tp1Price:F4}" +
                Environment.NewLine +
                $"TP2 Distance       : {tp2Distance:F4}" +
                Environment.NewLine +
                $"TP2 Price          : {tp2Price:F4}" +
                Environment.NewLine +
                $"Highest Price      : {pos.HighestPrice}" +
                Environment.NewLine +
                $"Lowest Price       : {pos.LowestPrice}" +
                Environment.NewLine +
                $"Pullback           : {pullback:F4}" +
                Environment.NewLine +
                $"Pullback / ATR     : {pullbackAtr:F4}" +
                Environment.NewLine +
                $"H4 Trend           : {strategyResult}" +
                Environment.NewLine +
                $"EMA                : {ema}" +
                Environment.NewLine +
                $"ATR                : {atr}" +
                Environment.NewLine +
                $"Trend Aligned      : {trendAligned}" +
                Environment.NewLine +
                $"SL Modification    : DISABLED" +
                Environment.NewLine +
                $"BreakEven          : DISABLED" +
                Environment.NewLine +
                $"TP1 Partial Close  : {pos.TP1Hit}" +
                Environment.NewLine +
                $"TP2 Partial Close  : {pos.TP2Hit}" +
                Environment.NewLine +
                $"TP1 Target R       : {DEFAULT_PARTIAL_TP1_R:F2}" +
                Environment.NewLine +
                $"TP2 Target R       : {DEFAULT_PARTIAL_TP2_R:F2}" +
                Environment.NewLine +
                $"TP2 Pullback ATR   : {DEFAULT_PULLBACK_TP2_ATR:F2}" +
                Environment.NewLine +
                $"Position State     : {state}" +
                Environment.NewLine +
                $"Action             : {action}" +
                Environment.NewLine +
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
