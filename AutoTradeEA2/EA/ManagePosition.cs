using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uClient.Comm;

namespace uClient.Broker
{
    /// <summary>
    /// MANAGE_POSITION
    /// 仓位管理，包含开仓，平仓管理
    /// </summary>
    public class ManagePosition : StrategyEA
    {
        private StringBuilder _log = new StringBuilder();

        /// <summary>
        /// 策略核心执行函数，由系统定时或按Tick触发
        /// </summary>
        /// <param name="eaChart">图表对象</param>
        /// <param name="context">上下文信息，包含当前价格、持仓状态等</param>
        /// <returns>是否有操作执行</returns>
        public override bool Execute(EAChart eaChart, IDictionary<string, Object> context)
        {
            _log.Clear();

            string _IsSimulateTest = context["IsSimulateTest"].ToString();
            string currentOrderType = context["CurrentOrderType"].ToString();
            string EMAATRPeriod = this.configParam["EMAATRPeriod"];
            int EMAperiod = Convert.ToInt32(this.configParam["EMAPeriod"]);
            int ATRperiod = Convert.ToInt32(this.configParam["ATRPeriod"]);
            string analysisDataType = context["analysisDataType"].ToString();

            decimal currentPrice = decimal.Parse(context["price"].ToString());
            int trendCount = Convert.ToInt32(this.configParam["TrendCount"]);

            string currentDateTimeStr;
            if (context.ContainsKey("CurrentDateTime"))
            {
                currentDateTimeStr = context["CurrentDateTime"].ToString();
            }
            else
            {
                currentDateTimeStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            TradePosition tradePositionInfo = null;
            if (context.ContainsKey("TradePositionInfo"))
            {
                tradePositionInfo = (TradePosition)context["TradePositionInfo"];
            }
            IDictionary<string, string> datas = null;
            string strategyResult = "NO TREND";
            double strategyBarPrice = 0;
            double strategyCurrentPrice = 0;
            double ema = 0;
            double atr = 0;
            double swingHigh = 0;
            double swingLow = 0;
            if (string.Equals(_IsSimulateTest, "T"))
            {
                datas = DSHelper.getStrategyResultTest("TREND_EMA_ATR", "XAUUSD", EMAATRPeriod, currentDateTimeStr, trendCount);
                if (datas != null)
                {
                    strategyResult = datas["result_status"]?.ToString();
                    strategyBarPrice = ToDoubleSafe(datas["bar_price"]);
                    strategyCurrentPrice = ToDoubleSafe(datas["current_price"]);
                    ema = ToDoubleSafe(datas["ema"]);
                    atr = ToDoubleSafe(datas["atr"]);
                    swingHigh = ToDoubleSafe(datas["swing_high"]);
                    swingLow = ToDoubleSafe(datas["swing_low"]);
                }
            }
            else
            {
                datas = DSHelper.getStrategyResult("TREND_EMA_ATR", "XAUUSD", EMAATRPeriod, currentDateTimeStr, trendCount);
                if (datas != null)
                {
                    strategyResult = datas["result_status"]?.ToString();
                    strategyBarPrice = ToDoubleSafe(datas["bar_price"]);
                    strategyCurrentPrice = ToDoubleSafe(datas["current_price"]);
                    ema = ToDoubleSafe(datas["ema"]);
                    atr = ToDoubleSafe(datas["atr"]);
                    swingHigh = ToDoubleSafe(datas["swing_high"]);
                    swingLow = ToDoubleSafe(datas["swing_low"]);
                }
            }

            _log.AppendLine($"动态参数[EMAATRPeriod{EMAATRPeriod}][EMAperiod{EMAperiod}][ATRperiod{ATRperiod}][currentDateTimeStr{currentDateTimeStr}][strategyResult{strategyResult}][strategyBarPrice{strategyBarPrice}][strategyCurrentPrice{strategyCurrentPrice}][quotePrice{currentPrice}][currentOrderType{currentOrderType}];");
            _log.AppendLine($"趋势相关数据[ema{ema}][atr{atr}][swingHigh{swingHigh}][swingLow{swingLow}]");
            // === Tick级别仓位管理 ===
            if (tradePositionInfo != null)
            {
                var signals = IndicatorHelper.ManagePositionSignal(tradePositionInfo, currentPrice, (decimal)atr, (decimal)ema, strategyResult);

                if (signals == null || signals.Count == 0)
                {
                    _log.AppendLine("没有仓位更新信息");
                    if (context.ContainsKey("TradeSignals"))
                    {
                        context.Remove("TradeSignals");
                    }
                }
                else
                {
                    var allStr = string.Join(",", signals.Select(s => s.ToString()));
                    _log.AppendLine($"仓位更新信号[{allStr}]");
                    _log.AppendLine($"仓位信息[{JsonConvert.SerializeObject(tradePositionInfo)}]");
                    context["TradePositionInfo"] = tradePositionInfo;
                    context["TradeSignals"] = signals;
                    return true;
                }
            }
            else
            {
                _log.AppendLine("没有订单不执行仓位管理");
            }
            return false;
        }

        /// <summary>
        /// 返回策略的实时状态描述，供前端或 UI 显示
        /// </summary>
        public override string ToDesc() => _log.ToString();

        public double ToDoubleSafe(object value, double defaultValue = 0)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            double result;
            if (double.TryParse(value.ToString(), out result))
                return result;

            return defaultValue;
        }
    }
}
