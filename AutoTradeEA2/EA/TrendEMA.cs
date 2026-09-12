using System;
using System.Collections.Generic;
using System.Text;

namespace uClient.Broker
{
    /// <summary>
    /// TREND_EMA
    /// EMA 判断趋势,判断两条趋势记录
    /// 参数:BUY TREND:买入趋势,SELL TREND卖出趋势,NO TREND:无交易趋势
    /// 返回true,未超过开仓数量,可以开仓,否则不能开仓
    /// </summary>
    public class TrendEMA : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();

            string _IsSimulateTest = context["IsSimulateTest"].ToString();
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
            //currentDateTimeStr = "2026-03-24 14:12:28";
            string strategyResult = "NO TREND";
            double strategyBarPrice = 0;
            double strategyCurrentPrice = 0;
            double ema = 0;
            double atr = 0;
            double swingHigh = 0;
            double swingLow = 0;
            IDictionary<string, string> datas = null;
            if (string.Equals(_IsSimulateTest, "T"))
            {
                datas = DSHelper.getStrategyResultTest("TREND_EMA_ATR", "XAUUSD", EMAATRPeriod, currentDateTimeStr, trendCount);
                if (datas != null)
                {
                    strategyResult = datas["result_status"];
                    strategyBarPrice = Convert.ToDouble(datas["bar_price"]);
                    strategyCurrentPrice = Convert.ToDouble(datas["current_price"]);
                    ema = Convert.ToDouble(datas["ema"]);
                    atr = Convert.ToDouble(datas["atr"]);
                    swingHigh = Convert.ToDouble(datas["swing_high"]);
                    swingLow = Convert.ToDouble(datas["swing_low"]);
                }
            }
            else
            {
                datas = DSHelper.getStrategyResult("TREND_EMA_ATR", "XAUUSD", EMAATRPeriod, currentDateTimeStr, trendCount);
                if (datas != null)
                {
                    strategyResult = datas["result_status"];
                    strategyBarPrice = Convert.ToDouble(datas["bar_price"]);
                    strategyCurrentPrice = Convert.ToDouble(datas["current_price"]);
                    ema = Convert.ToDouble(datas["ema"]);
                    atr = Convert.ToDouble(datas["atr"]);
                    swingHigh = Convert.ToDouble(datas["swing_high"]);
                    swingLow = Convert.ToDouble(datas["swing_low"]);
                }
            }
            desc.AppendLine($"趋势相关数据[ema{ema}][atr{atr}][swingHigh{swingHigh}][swingLow{swingLow}]");
            //Console.WriteLine($"当前趋势[{strategyResult}]");
            desc.AppendLine($"当前趋势[{strategyResult}]");
            desc.AppendLine($"动态参数[EMAATRPeriod{EMAATRPeriod}][EMAperiod{EMAperiod}][ATRperiod{ATRperiod}][currentDateTimeStr{currentDateTimeStr}][strategyResult{strategyResult}][strategyBarPrice{strategyBarPrice}][strategyCurrentPrice{strategyCurrentPrice}][quotePrice{currentPrice}];");

            context["StrategyResultTrendEMA"] = strategyResult;

            if (string.IsNullOrEmpty(strategyResult) || string.Equals(strategyResult, "NO TREND"))
            {
                desc.AppendLine($"策略名称[{this.name}]不是趋势行情[strategyResult{strategyResult}],不执行该策略");
                if (context.ContainsKey("StrategyStopLoss"))
                {
                    context.Remove("StrategyStopLoss");
                }
                return false;
            }

            if (string.Equals(strategyResult, "BUY TREND"))
            {
                //quotePrice > strategyPrice 表示BUY趋势已经反转
                //desc.AppendFormat("策略名称[{0}]趋势行情{1},[当前价格{2}]<[趋势价格{3}],行情反转,不执行该策略", this.name, strategyResult, quotePrice, strategyBarPrice);
                desc.AppendLine($"策略名称[{this.name}]趋势行情{strategyResult},执行该策略");
                decimal sl = IndicatorHelper.CalculateInitialStopLoss(true, currentPrice, (decimal)atr, (decimal)swingHigh, (decimal)swingLow);
                context["StrategyStopLoss"] = sl.ToString();
                desc.AppendLine($"趋势策略止损值[{sl}]");
                return true;
            }

            if (string.Equals(strategyResult, "SELL TREND"))
            {
                // quotePrice < strategyPrice 表示SELL趋势已经反转
                //desc.AppendFormat("策略名称[{0}]趋势行情{1},[当前价格{2}]>[趋势价格{3}],行情反转,不执行该策略", this.name, strategyResult, quotePrice, strategyBarPrice);
                desc.AppendLine($"策略名称[{this.name}]趋势行情{strategyResult},执行该策略");
                decimal sl = IndicatorHelper.CalculateInitialStopLoss(false, currentPrice, (decimal)atr, (decimal)swingHigh, (decimal)swingLow);
                context["StrategyStopLoss"] = sl.ToString();
                desc.AppendLine($"趋势策略止损值[{sl}]");
                return true;
            }
            desc.AppendLine($"策略名称[{this.name}]不是趋势行情[{strategyResult}],不执行该策略");
            return false;
        }
        public override string ToDesc()
        {
            return desc.ToString();
        }
    }
}
