using System;
using System.Collections.Generic;
using System.Text;
using uClient.Comm;

namespace uClient.Broker
{
    public class EAExecute
    {
        public Log _Log;
        public EAConfig _EAConfig;
        //public string executeResultStr = "{symbol}:{price}:{optType}:{lotProportion}:{takeProfit}:{stopLoss}";
        /// <summary>
        /// {symbol}:{price}:{optType}:{lotProportion}:{takeProfit}:{stopLoss}
        /// 品类:价格:BUY/SELL/CLOSE:手数:止盈值:止损值
        /// </summary>
        public string executeResultStr = "{0}:{1}:{2}:{3}:{4}:{5}";
        /// <summary>
        /// 每次运行测试日志
        /// </summary>
        public Dictionary<string, ExecuteResult> _EAResult;
        public EAExecute(Log log)
        {
            this._Log = log;
        }
        /// <summary>
        /// EA执行结果,生成操作指令：{symbol}:{price}:{optType}:{lotProportion}
        /// </summary>
        /// <returns></returns>
        public string execute(EAChart eaChart, IDictionary<string, object> context)
        {
            bool flag = false;
            //Dictionary<string, ExecuteResult> result = new Dictionary<string, ExecuteResult>();
            string analysisDataType = context["analysisDataType"].ToString();
            _EAResult = new Dictionary<string, ExecuteResult>();
            if (_EAConfig != null && _EAConfig.EAList.Count > 0)
            {
                List<StrategyEA> executeEAList = null;

                if (string.Equals(analysisDataType, "D"))
                {
                    executeEAList = getDEAList();
                }
                if (string.Equals(analysisDataType, "K"))
                {
                    executeEAList = getKEAList();
                }
                if (string.Equals(analysisDataType, "H"))
                {
                    executeEAList = getHEAList();
                }
                if (executeEAList != null && executeEAList.Count > 0)
                {
                    _Log.LogInfo("执行EA策略[" + analysisDataType + "]开始:共计[" + executeEAList.Count + "]条EA");
                    foreach (StrategyEA ea in executeEAList)
                    {
                        ea._Log = _Log;
                        if (ea.active)
                        {
                            _Log.LogInfo(string.Format($"执行策略[{ea.name}]开始"));

                            // 1. 初始化计时器
                            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

                            try
                            {
                                // 2. 执行策略
                                flag = ea.Execute(eaChart, context);
                            }
                            finally
                            {
                                // 3. 停止计时
                                stopwatch.Stop();
                                ExecuteResult executeResult = new ExecuteResult();
                                _EAResult.Add(ea.code, executeResult);
                                executeResult.eaCode = ea.code;
                                executeResult.result = flag;
                                executeResult.desc = ea.ToDesc();

                                // 4. 在日志中输出执行时间（毫秒）
                                string timeConsumption = $"{stopwatch.ElapsedMilliseconds}ms";
                                // 获取当前时间字符串 (包含日期、时间及毫秒)
                                string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                //Console.WriteLine(string.Format($"[{currentTime}] 策略[{ea.name}]执行完毕，耗时: {timeConsumption}"));
                                // 5. 在日志中输出：当前时刻 + 耗时
                                _Log.LogInfo(string.Format($"策略[{ea.name}]执行完毕，耗时: {timeConsumption}"));
                                _Log.LogInfo($"[{ea.name}],执行结果[{flag}][{ea.ToDesc()}]");
                                _Log.LogInfo(string.Format($"执行策略[{ea.name}]结束"));
                            }
                        }
                        else
                        {
                            _Log.LogInfo(string.Format($"策略[{ea.name}]未启用"));
                        }
                    }
                    _Log.LogInfo("策略决策分析开始");
                    string res = eaResultAnalysis(_EAResult, context);
                    _Log.LogInfo("策略决策分析结束");
                    _Log.LogInfo($"策略决策分析结果[{res}]");
                    if (!string.IsNullOrEmpty(res))
                    {
                        if (context.ContainsKey("CurrentOrderDetail"))
                        {
                            context["CurrentOrderDetail"] = res;
                        }
                        else
                        {
                            context.Add("CurrentOrderDetail", res);
                        }
                    }
                    else
                    {
                        _Log.LogInfo("无策略生成");
                    }
                    _Log.LogInfo("执行EA策略完成");
                    return res;
                }
                else
                {
                    _Log.LogInfo($"未加载到策略[{analysisDataType}]数据");
                }
            }
            else
            {
                _Log.LogInfo("未配置策略数据");
            }
            return null;
        }

        /// <summary>
        /// 获取KD策略列表
        /// </summary>
        /// <returns></returns>
        public List<StrategyEA> getHEAList()
        {
            if (_EAConfig != null && _EAConfig.EAList.Count > 0)
            {
                List<StrategyEA> eaList = new List<StrategyEA>();
                foreach (StrategyEA ea in _EAConfig.EAList)
                {
                    if (isHEA(ea))
                    {
                        eaList.Add(ea);
                    }
                }
                return eaList;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 获取D策略列表
        /// </summary>
        /// <returns></returns>
        public List<StrategyEA> getDEAList()
        {
            if (_EAConfig != null && _EAConfig.EAList.Count > 0)
            {
                List<StrategyEA> eaList = new List<StrategyEA>();
                foreach (StrategyEA ea in _EAConfig.EAList)
                {
                    if (isDEA(ea))
                    {
                        eaList.Add(ea);
                    }
                }
                return eaList;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// D策略,情绪指数采用不同的平台
        /// </summary>
        /// <returns></returns>
        public List<StrategyEA> getDXEAList()
        {
            if (_EAConfig != null && _EAConfig.EAList.Count > 0)
            {
                List<StrategyEA> eaList = new List<StrategyEA>();
                foreach (StrategyEA ea in _EAConfig.EAList)
                {
                    if (isDEA(ea))
                    {
                        eaList.Add(ea);
                    }
                }
                return eaList;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 获取K策略列表
        /// </summary>
        /// <returns></returns>
        public List<StrategyEA> getKEAList()
        {
            if (_EAConfig != null && _EAConfig.EAList.Count > 0)
            {
                List<StrategyEA> eaList = new List<StrategyEA>();
                foreach (StrategyEA ea in _EAConfig.EAList)
                {
                    if (isKEA(ea))
                    {
                        eaList.Add(ea);
                    }
                }
                return eaList;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 判断是否是D策略
        /// </summary>
        /// <param name="ea"></param>
        /// <returns></returns>
        public bool isDEA(StrategyEA ea)
        {
            List<string> DEAList = new List<string>
            {
                "BUY",
                "BUY_TRENDORDER",
                "CLOSE_BUY",
                "CLOSE_SELL",
                "CLOSE_BUY_TRENDORDER",
                "CLOSE_ORDER_WEEKEND",
                "CLOSE_ORDER_DAILY",
                "CLOSE_SELL_TRENDORDER",
                "COMMAND_TIME_DURATION",
                "INCREASE_BUY_TRENDORDER",
                "INCREASE_SELL_TRENDORDER",
                "KEEP_LOSS",
                "KEEP_PROFIT",
                "MAX_DIFFCHANGE",
                "MAX_ORDER_COUNT",
                "NOTRADE_DURATION",
                "SELL",
                "SELL_TRENDORDER",
                "STOP_LOSS",
                "TAKE_PROFIT",
                "TREND_EMA",
                "OPEN_MA_PRICE",
                "MANAGE_POSITION",
                "M_EA",
                "CLOSE_EMA"
            };

            if (ea == null)
            {
                return false;
            }
            if (DEAList.Contains(ea.code))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 判断是否是H策略
        /// </summary>
        /// <param name="ea"></param>
        /// <returns></returns>
        public bool isHEA(StrategyEA ea)
        {
            List<string> EAList = new List<string>
            {
                "CLOSE_ORDER_WEEKEND",
                "CLOSE_ORDER_DAILY",
                "COMMAND_TIME_DURATION",
                "NOTRADE_DURATION",
                "MAX_ORDER_COUNT",
                "TREND_EMA",
                "KEEP_LOSS",
                "KEEP_PROFIT",
                "STOP_LOSS",
                "TAKE_PROFIT",
                "CLOSE_EMA",
                "MANAGE_POSITION",
                "H_EA"
            };
            if (ea == null)
            {
                return false;
            }
            if (EAList.Contains(ea.code))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断是否是K策略
        /// </summary>
        /// <param name="ea"></param>
        /// <returns></returns>
        public bool isKEA(StrategyEA ea)
        {
            List<string> EAList = new List<string>
            {
                "CLOSE_ORDER_WEEKEND",
                "CLOSE_ORDER_DAILY",
                "COMMAND_TIME_DURATION",
                "KEEP_LOSS",
                "KEEP_PROFIT",
                "MAX_ORDER_COUNT",
                "TREND_EMA",
                "NOTRADE_DURATION",
                "STOP_LOSS",
                "TAKE_PROFIT",
                "CLOSE_KDJ",
                "CLOSE_EMA",
                "MANAGE_POSITION",
                "M_EA",
                "K_EA"
            };

            if (ea == null)
            {
                return false;
            }
            if (EAList.Contains(ea.code))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 执行Para EA,执行后生成BIZ_CODE
        /// BIZ_CODE=10001 执行策略LIMIT_ORDER_COUNT
        /// BIZ_CODE=10002 执行策略SMOOTH_CHANGE
        /// </summary>
        /// <param name="eaChart"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string executePara(EAChart eaChart, IDictionary<string, Object> context)
        {
            bool flag;
            _EAResult = new Dictionary<string, ExecuteResult>();
            if (_EAConfig != null && _EAConfig.ParaEAList.Count > 0)
            {
                foreach (StrategyEA ea in _EAConfig.ParaEAList)
                {
                    ea._Log = _Log;
                    if (ea.active)
                    {
                        _Log.LogInfo(string.Format($"执行策略[{ea.name}]开始"));
                        flag = ea.Execute(eaChart, context);
                        ExecuteResult executeResult = new ExecuteResult();
                        _EAResult.Add(ea.code, executeResult);
                        executeResult.eaCode = ea.code;
                        executeResult.result = flag;
                        executeResult.desc = ea.ToDesc();
                        _Log.LogInfo(ea.ToDesc());
                        _Log.LogInfo(string.Format($"执行策略[{ea.name}]结束"));
                    }
                    else
                    {
                        _Log.LogInfo(string.Format($"策略[{ea.name}]未启用"));
                    }
                }
                _Log.LogInfo("策略决策分析开始");
                if (_EAResult.ContainsKey("LIMIT_ORDER_COUNT"))
                {
                    ExecuteResult limitOrderCount = _EAResult["LIMIT_ORDER_COUNT"];
                    _Log.LogInfo(limitOrderCount.desc);
                    if (limitOrderCount.result)
                    {
                        return "10001";
                    }
                }
                if (_EAResult.ContainsKey("SMOOTH_CHANGE"))
                {
                    ExecuteResult smoothChange = _EAResult["SMOOTH_CHANGE"];
                    _Log.LogInfo(smoothChange.desc);
                    if (smoothChange.result)
                    {
                        return "10002";
                    }
                }
                _Log.LogInfo("策略决策分析结束");
                return null;
            }
            else
            {
                _Log.LogInfo("无策略配置");
            }
            _Log.LogInfo("无可执行策略");
            return null;
        }

        /// <summary>
        /// 执行结果分析
        /// </summary>
        /// <returns></returns>
        public string eaResultAnalysis(Dictionary<string, ExecuteResult> result, IDictionary<string, Object> context)
        {
            string CurrentOrderType = context["CurrentOrderType"].ToString();
            if (string.IsNullOrEmpty(CurrentOrderType) || string.Equals(CurrentOrderType, "CLOSE_BUY") || string.Equals(CurrentOrderType, "CLOSE_SELL"))
            {
                _Log.LogInfo("BUY 决策分析开始");
                string fr = buyAnalysis(result, context);
                _Log.LogInfo("BUY 决策分析结束");
                if (!string.IsNullOrEmpty(fr))
                {
                    _Log.LogInfo("[BUY]指令分析结果:" + fr);
                    context["CurrentOrderType"] = "BUY";
                    return fr;
                }
                else
                {
                    _Log.LogInfo("[BUY]无指令执行");
                }
                _Log.LogInfo("SELL 决策分析开始");
                fr = sellAnalysis(result, context);
                _Log.LogInfo("SELL 决策分析结束");
                if (!string.IsNullOrEmpty(fr))
                {
                    _Log.LogInfo("[SELL]指令分析结果:" + fr);
                    context["CurrentOrderType"] = "SELL";
                    return fr;
                }
                else
                {
                    _Log.LogInfo("[SELL]无指令执行");
                }
                _Log.LogInfo("[BUY]和[SELL]无指令执行");
                return null;
            }
            if (string.Equals(CurrentOrderType, "BUY"))
            {
                _Log.LogInfo("INCREASE BUY 决策分析开始");
                string fr = IncreaseBuyAnalysis(result, context);
                _Log.LogInfo("INCREASE BUY 决策分析结束");
                if (!string.IsNullOrEmpty(fr))
                {
                    _Log.LogInfo("[INCREASE BUY]指令分析结果:" + fr);
                    context["CurrentOrderType"] = "BUY";
                    return fr;
                }
                else
                {
                    _Log.LogInfo("[INCREASE BUY]无指令执行");
                }
                _Log.LogInfo("[INCREASE BUY]无指令执行");
            }

            if (string.Equals(CurrentOrderType, "BUY"))
            {
                _Log.LogInfo("CLOSE BUY 决策分析开始");
                string fr = closeBuyAnalysis(result, context);
                _Log.LogInfo("CLOSE BUY 决策分析结束");
                if (!string.IsNullOrEmpty(fr))
                {
                    _Log.LogInfo("[CLOSE BUY]指令分析结果:" + fr);

                    if (context.ContainsKey("CurrentOrderTypeDetail") && (
                        string.Equals(context["CurrentOrderTypeDetail"], "MANAGE_POSITION_CLOSE_BUY_TP1") ||
                        string.Equals(context["CurrentOrderTypeDetail"], "MANAGE_POSITION_CLOSE_BUY_TP2")))
                    {
                        //部分平仓,设置订单状态为"BUY"
                        context["CurrentOrderType"] = "BUY";
                    }
                    else
                    {
                        context["CurrentOrderType"] = "CLOSE_BUY";
                    }
                    return fr;
                }
                else
                {
                    _Log.LogInfo("[CLOSE BUY]无指令执行");
                }
                _Log.LogInfo("[CLOSE BUY]无指令执行");
            }

            if (string.Equals(CurrentOrderType, "SELL"))
            {
                _Log.LogInfo("INCREASE SELL 决策分析开始");
                string fr = IncreaseSellAnalysis(result, context);
                _Log.LogInfo("INCREASE SELL 决策分析结束");
                if (!string.IsNullOrEmpty(fr))
                {
                    _Log.LogInfo("[INCREASE SELL]指令分析结果:" + fr);
                    context["CurrentOrderType"] = "SELL";
                    return fr;
                }
                else
                {
                    _Log.LogInfo("[INCREASE SELL]无指令执行");
                }
                _Log.LogInfo("[INCREASE SELL]无指令执行");
            }

            if (string.Equals(CurrentOrderType, "SELL"))
            {
                _Log.LogInfo("CLOSE SELL 决策分析开始");
                string fr = closeSellAnalysis(result, context);
                _Log.LogInfo("CLOSE SELL 决策分析结束");
                if (!string.IsNullOrEmpty(fr))
                {
                    _Log.LogInfo("[CLOSE SELL]指令分析结果:" + fr);
                    if (context.ContainsKey("CurrentOrderTypeDetail") && (
                        string.Equals(context["CurrentOrderTypeDetail"], "MANAGE_POSITION_CLOSE_SELL_TP1") ||
                        string.Equals(context["CurrentOrderTypeDetail"], "MANAGE_POSITION_CLOSE_SELL_TP2")))
                    {
                        //部分平仓,设置订单状态为"SELL"
                        context["CurrentOrderType"] = "SELL";
                    }
                    else
                    {
                        context["CurrentOrderType"] = "CLOSE_SELL";
                    }
                    return fr;
                }
                else
                {
                    _Log.LogInfo("[CLOSE SELL]无指令执行");
                }
                _Log.LogInfo("[CLOSE SELL]无指令执行");
            }
            return null;
        }

        public string buyAnalysis(Dictionary<string, ExecuteResult> result, IDictionary<string, Object> context)
        {
            string CurrentOrderTypeDetail = context["CurrentOrderTypeDetail"].ToString();
            StringBuilder executeDesc = new StringBuilder();
            if (result != null && result.Count > 0)
            {
                if (result.ContainsKey("MAX_ORDER_COUNT"))
                {
                    ExecuteResult maxOrderCount = result["MAX_ORDER_COUNT"];
                    _Log.LogInfo(maxOrderCount.desc);
                    if (maxOrderCount.result)
                    {
                        return null;
                    }
                }
                if (result.ContainsKey("SICOLLECT_TIME_DURATION"))
                {
                    ExecuteResult siCollectResult = result["SICOLLECT_TIME_DURATION"];
                    _Log.LogInfo(siCollectResult.desc);
                    if (!siCollectResult.result)
                    {
                        return null;
                    }
                }
                if (result.ContainsKey("NOTRADE_DURATION"))
                {
                    ExecuteResult noTradeDurationResult = result["NOTRADE_DURATION"];
                    _Log.LogInfo(noTradeDurationResult.desc);
                    if (!noTradeDurationResult.result)
                    {
                        return null;
                    }
                }
                if (result.ContainsKey("COMMAND_TIME_DURATION"))
                {
                    ExecuteResult commandTimeDurationResult = result["COMMAND_TIME_DURATION"];
                    _Log.LogInfo(commandTimeDurationResult.desc);
                    if (!commandTimeDurationResult.result)
                    {
                        return null;
                    }
                }
                if (result.ContainsKey("TREND_EMA"))
                {
                    ExecuteResult trendEMAResult = result["TREND_EMA"];
                    _Log.LogInfo(trendEMAResult.desc);
                    if (trendEMAResult.result)
                    {
                        string tResult = context["StrategyResultTrendEMA"].ToString();
                        _Log.LogInfo("[StrategyResultTrendEMA=" + tResult + "]");
                        if (!string.Equals(tResult, "BUY TREND"))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                if (result.ContainsKey("TREND_KDJ"))
                {
                    ExecuteResult trendKDJResult = result["TREND_KDJ"];
                    _Log.LogInfo(trendKDJResult.desc);
                    if (trendKDJResult.result)
                    {
                        string tResult = context["StrategyResultTrendKDJ"].ToString();
                        _Log.LogInfo("[StrategyResultTrendKDJ=" + tResult + "]");
                        if (!string.Equals(tResult, "BUY TREND"))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                if (result.ContainsKey("OPEN_KDJ"))
                {
                    ExecuteResult KDJResult = result["OPEN_KDJ"];
                    _Log.LogInfo(KDJResult.desc);
                    if (KDJResult.result)
                    {
                        string kResult = context["StrategyResultOPENKDJ"].ToString();
                        _Log.LogInfo("[StrategyResultOPENKDJ=" + kResult + "]");
                        if (!string.Equals(kResult, "BUY"))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                if (result.ContainsKey("OPEN_MA_PRICE"))
                {
                    ExecuteResult MAPriceResult = result["OPEN_MA_PRICE"];
                    _Log.LogInfo(MAPriceResult.desc);
                    if (MAPriceResult.result)
                    {
                        string mResult = context["StrategyResultEMAPrice"].ToString();
                        _Log.LogInfo("[StrategyResultEMAPrice=" + mResult + "]");
                        if (!string.Equals(mResult, "EMA_PRICE_ABOVE"))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                //普通买入单
                if (result.ContainsKey("BUY") && !string.Equals(CurrentOrderTypeDetail, "TREND_BUY") && !string.Equals(CurrentOrderTypeDetail, "TREND_SELL") && !string.Equals(CurrentOrderTypeDetail, "BUY") && !string.Equals(CurrentOrderTypeDetail, "SELL"))
                {
                    _Log.LogInfo("普通买入单策略分析");
                    ExecuteResult buyResult = result["BUY"];
                    _Log.LogInfo(buyResult.desc);
                    if (buyResult.result)
                    {
                        bool flag = true;
                        if (result.ContainsKey("MAX_DIFFCHANGE"))
                        {
                            ExecuteResult maxDiffChangeResult = result["MAX_DIFFCHANGE"];
                            flag = maxDiffChangeResult.result;
                            _Log.LogInfo(maxDiffChangeResult.desc);
                        }
                        if (flag)
                        {
                            context["CurrentOrderTypeDetail"] = "BUY";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            context["openPrice"] = price;

                            string sl = (Convert.ToDouble(price) - Convert.ToDouble(context["stopLoss"].ToString())).ToString();
                            if (context.ContainsKey("StrategyStopLoss"))
                            {
                                sl = context["StrategyStopLoss"].ToString();

                            }
                            TradePosition tradePositionInfo = IndicatorHelper.OpenPosition(true, Convert.ToDecimal(price), Convert.ToDecimal(sl));
                            context["TradePositionInfo"] = tradePositionInfo;
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "BUY", "1", tradePositionInfo.RiskR, tradePositionInfo.StopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }
                //趋势买入单
                if (result.ContainsKey("BUY_TRENDORDER") && !string.Equals(CurrentOrderTypeDetail, "TREND_BUY") && !string.Equals(CurrentOrderTypeDetail, "TREND_SELL") && !string.Equals(CurrentOrderTypeDetail, "BUY") && !string.Equals(CurrentOrderTypeDetail, "SELL"))
                {
                    _Log.LogInfo("趋势买入单策略分析");
                    ExecuteResult buyTrendResult = result["BUY_TRENDORDER"];
                    _Log.LogInfo(buyTrendResult.desc);
                    if (buyTrendResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "TREND_BUY";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["openPrice"] = price;

                        string sl = (Convert.ToDouble(price) - Convert.ToDouble(context["stopLoss"].ToString())).ToString();
                        if (context.ContainsKey("StrategyStopLoss"))
                        {
                            sl = context["StrategyStopLoss"].ToString();

                        }
                        TradePosition tradePositionInfo = IndicatorHelper.OpenPosition(true, Convert.ToDecimal(price), Convert.ToDecimal(sl));
                        context["TradePositionInfo"] = tradePositionInfo;
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "BUY", "1", tradePositionInfo.RiskR, tradePositionInfo.StopLoss);
                        return executeDesc.ToString();
                    }
                }
                //K EA
                if (result.ContainsKey("K_EA"))
                {
                    ExecuteResult kResult = result["K_EA"];
                    if (kResult.result)
                    {
                        string strategyResultK = context["StrategyResultK"].ToString();
                        _Log.LogInfo(kResult.desc);
                        if (string.Equals(strategyResultK, "BUY"))
                        {
                            _Log.LogInfo("K线开仓策略分析");
                            context["CurrentOrderTypeDetail"] = "BUY_K";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            context["openPrice"] = price;

                            string sl = (Convert.ToDouble(price) - Convert.ToDouble(context["stopLoss"].ToString())).ToString();
                            if (context.ContainsKey("StrategyStopLoss"))
                            {
                                sl = context["StrategyStopLoss"].ToString();

                            }
                            TradePosition tradePositionInfo = IndicatorHelper.OpenPosition(true, Convert.ToDecimal(price), Convert.ToDecimal(sl));
                            context["TradePositionInfo"] = tradePositionInfo;
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "BUY", "1", tradePositionInfo.RiskR, tradePositionInfo.StopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }
                //EMA趋势判断开仓
                if (result.ContainsKey("OPEN_EMA"))
                {
                    ExecuteResult emaResult = result["OPEN_EMA"];
                    _Log.LogInfo(emaResult.desc);
                    string tResult = context["StrategyResultO"].ToString();
                    _Log.LogInfo("[StrategyResultO=" + tResult + "]");
                    if (emaResult.result && string.Equals(tResult, "BUY TREND"))
                    {
                        _Log.LogInfo("EMA开仓策略分析");
                        context["CurrentOrderTypeDetail"] = "BUY_E";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["openPrice"] = price;

                        string sl = (Convert.ToDouble(price) - Convert.ToDouble(context["stopLoss"].ToString())).ToString();
                        if (context.ContainsKey("StrategyStopLoss"))
                        {
                            sl = context["StrategyStopLoss"].ToString();

                        }
                        TradePosition tradePositionInfo = IndicatorHelper.OpenPosition(true, Convert.ToDecimal(price), Convert.ToDecimal(sl));
                        context["TradePositionInfo"] = tradePositionInfo;
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "BUY", "1", tradePositionInfo.RiskR, tradePositionInfo.StopLoss);
                        return executeDesc.ToString();
                    }
                }
                //H EA
                if (result.ContainsKey("H_EA"))
                {
                    ExecuteResult hResult = result["H_EA"];
                    _Log.LogInfo(hResult.desc);
                    if (hResult.result)
                    {
                        string strategyResultResult = context["StrategyResultH"].ToString();
                        _Log.LogInfo("[StrategyResultH=" + strategyResultResult + "]");
                        if (string.Equals(strategyResultResult, "BUY"))
                        {
                            _Log.LogInfo("H开仓策略分析");
                            context["CurrentOrderTypeDetail"] = "BUY_H";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            context["openPrice"] = price;

                            string sl = (Convert.ToDouble(price) - Convert.ToDouble(context["stopLoss"].ToString())).ToString();
                            if (context.ContainsKey("StrategyStopLoss"))
                            {
                                sl = context["StrategyStopLoss"].ToString();

                            }
                            TradePosition tradePositionInfo = IndicatorHelper.OpenPosition(true, Convert.ToDecimal(price), Convert.ToDecimal(sl));
                            context["TradePositionInfo"] = tradePositionInfo;
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "BUY", "1", tradePositionInfo.RiskR, tradePositionInfo.StopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }
            }
            return null;
        }
        public string sellAnalysis(Dictionary<string, ExecuteResult> result, IDictionary<string, Object> context)
        {
            string CurrentOrderTypeDetail = context["CurrentOrderTypeDetail"].ToString();

            StringBuilder executeDesc = new StringBuilder();
            if (result != null && result.Count > 0)
            {
                if (result.ContainsKey("MAX_ORDER_COUNT"))
                {
                    ExecuteResult maxOrderCount = result["MAX_ORDER_COUNT"];
                    _Log.LogInfo(maxOrderCount.desc);
                    if (maxOrderCount.result)
                    {
                        return null;
                    }
                }
                if (result.ContainsKey("SICOLLECT_TIME_DURATION"))
                {
                    ExecuteResult siCollectResult = result["SICOLLECT_TIME_DURATION"];
                    _Log.LogInfo(siCollectResult.desc);
                    if (!siCollectResult.result)
                    {
                        return null;
                    }
                }
                if (result.ContainsKey("NOTRADE_DURATION"))
                {
                    ExecuteResult noTradeDurationResult = result["NOTRADE_DURATION"];
                    _Log.LogInfo(noTradeDurationResult.desc);
                    if (!noTradeDurationResult.result)
                    {
                        return null;
                    }
                }
                if (result.ContainsKey("COMMAND_TIME_DURATION"))
                {
                    ExecuteResult commandTimeDurationResult = result["COMMAND_TIME_DURATION"];
                    _Log.LogInfo(commandTimeDurationResult.desc);
                    if (!commandTimeDurationResult.result)
                    {
                        return null;
                    }
                }
                if (result.ContainsKey("TREND_EMA"))
                {
                    ExecuteResult trendEMAResult = result["TREND_EMA"];
                    _Log.LogInfo(trendEMAResult.desc);
                    if (trendEMAResult.result)
                    {
                        string tResult = context["StrategyResultTrendEMA"].ToString();
                        _Log.LogInfo("[StrategyResultTrendEMA=" + tResult + "]");
                        if (!string.Equals(tResult, "SELL TREND"))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                if (result.ContainsKey("TREND_KDJ"))
                {
                    ExecuteResult trendKDJResult = result["TREND_KDJ"];
                    _Log.LogInfo(trendKDJResult.desc);
                    if (trendKDJResult.result)
                    {
                        string tResult = context["StrategyResultTrendKDJ"].ToString();
                        _Log.LogInfo("[StrategyResultTrendKDJ=" + tResult + "]");
                        if (!string.Equals(tResult, "SELL TREND"))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                if (result.ContainsKey("OPEN_KDJ"))
                {
                    ExecuteResult KDJResult = result["OPEN_KDJ"];
                    _Log.LogInfo(KDJResult.desc);
                    if (KDJResult.result)
                    {
                        string kResult = context["StrategyResultOPENKDJ"].ToString();
                        _Log.LogInfo("[StrategyResult=" + kResult + "]");
                        if (!string.Equals(kResult, "SELL"))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                if (result.ContainsKey("OPEN_MA_PRICE"))
                {
                    ExecuteResult MAPriceResult = result["OPEN_MA_PRICE"];
                    _Log.LogInfo(MAPriceResult.desc);
                    if (MAPriceResult.result)
                    {
                        string mResult = context["StrategyResultEMAPrice"].ToString();
                        _Log.LogInfo("[StrategyResultEMAPrice=" + mResult + "]");
                        if (!string.Equals(mResult, "EMA_PRICE_BELOW"))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }


                //普通卖出单
                if (result.ContainsKey("SELL") && !string.Equals(CurrentOrderTypeDetail, "TREND_BUY") && !string.Equals(CurrentOrderTypeDetail, "TREND_SELL") && !string.Equals(CurrentOrderTypeDetail, "BUY") && !string.Equals(CurrentOrderTypeDetail, "SELL"))
                {
                    _Log.LogInfo("普通卖出单策略分析");
                    ExecuteResult sellResult = result["SELL"];
                    _Log.LogInfo(sellResult.desc);
                    if (sellResult.result)
                    {
                        bool flag = true;
                        if (result.ContainsKey("MAX_DIFFCHANGE"))
                        {
                            ExecuteResult maxDiffChangeResult = result["MAX_DIFFCHANGE"];
                            _Log.LogInfo(maxDiffChangeResult.desc);
                            flag = maxDiffChangeResult.result;
                        }
                        if (flag)
                        {
                            context["CurrentOrderTypeDetail"] = "SELL";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            context["openPrice"] = price;

                            string sl = (Convert.ToDouble(price) + Convert.ToDouble(context["stopLoss"].ToString())).ToString();
                            if (context.ContainsKey("StrategyStopLoss"))
                            {
                                sl = context["StrategyStopLoss"].ToString();

                            }
                            TradePosition tradePositionInfo = IndicatorHelper.OpenPosition(false, Convert.ToDecimal(price), Convert.ToDecimal(sl));
                            context["TradePositionInfo"] = tradePositionInfo;
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "SELL", "1", tradePositionInfo.RiskR, tradePositionInfo.StopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }

                //趋势卖出单
                if (result.ContainsKey("SELL_TRENDORDER") && !string.Equals(CurrentOrderTypeDetail, "TREND_BUY") && !string.Equals(CurrentOrderTypeDetail, "TREND_SELL") && !string.Equals(CurrentOrderTypeDetail, "BUY") && !string.Equals(CurrentOrderTypeDetail, "SELL"))
                {
                    _Log.LogInfo("趋势卖出单策略分析");
                    ExecuteResult sellTrendResult = result["SELL_TRENDORDER"];
                    _Log.LogInfo(sellTrendResult.desc);
                    if (sellTrendResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "TREND_SELL";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["openPrice"] = price;

                        string sl = (Convert.ToDouble(price) + Convert.ToDouble(context["stopLoss"].ToString())).ToString();
                        if (context.ContainsKey("StrategyStopLoss"))
                        {
                            sl = context["StrategyStopLoss"].ToString();

                        }
                        TradePosition tradePositionInfo = IndicatorHelper.OpenPosition(false, Convert.ToDecimal(price), Convert.ToDecimal(sl));
                        context["TradePositionInfo"] = tradePositionInfo;
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "SELL", "1", tradePositionInfo.RiskR, tradePositionInfo.StopLoss);
                        return executeDesc.ToString();
                    }
                }
                //K EA
                if (result.ContainsKey("K_EA"))
                {
                    ExecuteResult kResult = result["K_EA"];
                    _Log.LogInfo(kResult.desc);
                    if (kResult.result)
                    {
                        string strategyResultK = context["StrategyResultK"].ToString();
                        _Log.LogInfo("[StrategyResultK=" + strategyResultK + "]");
                        if (string.Equals(strategyResultK, "SELL"))
                        {
                            context["CurrentOrderTypeDetail"] = "SELL_K";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            context["openPrice"] = price;

                            string sl = (Convert.ToDouble(price) + Convert.ToDouble(context["stopLoss"].ToString())).ToString();
                            if (context.ContainsKey("StrategyStopLoss"))
                            {
                                sl = context["StrategyStopLoss"].ToString();

                            }
                            TradePosition tradePositionInfo = IndicatorHelper.OpenPosition(false, Convert.ToDecimal(price), Convert.ToDecimal(sl));
                            context["TradePositionInfo"] = tradePositionInfo;
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "SELL", "1", tradePositionInfo.RiskR, tradePositionInfo.StopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }
                //EMA趋势判断开仓
                if (result.ContainsKey("OPEN_EMA"))
                {
                    ExecuteResult emaResult = result["OPEN_EMA"];
                    _Log.LogInfo(emaResult.desc);
                    string tResult = context["StrategyResultO"].ToString();
                    _Log.LogInfo("[StrategyResultO=" + tResult + "]");
                    if (emaResult.result && string.Equals(tResult, "SELL TREND"))
                    {
                        _Log.LogInfo("EMA开仓策略分析");
                        context["CurrentOrderTypeDetail"] = "SELL_E";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["openPrice"] = price;

                        string sl = (Convert.ToDouble(price) + Convert.ToDouble(context["stopLoss"].ToString())).ToString();
                        if (context.ContainsKey("StrategyStopLoss"))
                        {
                            sl = context["StrategyStopLoss"].ToString();

                        }
                        TradePosition tradePositionInfo = IndicatorHelper.OpenPosition(false, Convert.ToDecimal(price), Convert.ToDecimal(sl));
                        context["TradePositionInfo"] = tradePositionInfo;
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "SELL", "1", tradePositionInfo.RiskR, tradePositionInfo.StopLoss);
                        return executeDesc.ToString();
                    }
                }
                //KD判断开仓
                if (result.ContainsKey("H_EA"))
                {
                    ExecuteResult hResult = result["H_EA"];
                    _Log.LogInfo(hResult.desc);
                    if (hResult.result)
                    {
                        string strategyResultH = context["StrategyResultH"].ToString();
                        _Log.LogInfo("[StrategyResultH=" + strategyResultH + "]");
                        if (string.Equals(strategyResultH, "SELL"))
                        {
                            _Log.LogInfo("KD开仓策略分析");
                            context["CurrentOrderTypeDetail"] = "SELL_H";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            context["openPrice"] = price;

                            string sl = (Convert.ToDouble(price) + Convert.ToDouble(context["stopLoss"].ToString())).ToString();
                            if (context.ContainsKey("StrategyStopLoss"))
                            {
                                sl = context["StrategyStopLoss"].ToString();

                            }
                            TradePosition tradePositionInfo = IndicatorHelper.OpenPosition(false, Convert.ToDecimal(price), Convert.ToDecimal(sl));
                            context["TradePositionInfo"] = tradePositionInfo;
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "SELL", "1", tradePositionInfo.RiskR, tradePositionInfo.StopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }

            }
            return null;
        }
        /// <summary>
        /// 加仓买入
        /// </summary>
        /// <param name="result"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string IncreaseBuyAnalysis(Dictionary<string, ExecuteResult> result, IDictionary<string, Object> context)
        {
            StringBuilder executeDesc = new StringBuilder();
            if (result != null && result.Count > 0)
            {
                if (result.ContainsKey("NOTRADE_DURATION"))
                {
                    ExecuteResult noTradeDurationResult = result["NOTRADE_DURATION"];
                    _Log.LogInfo(noTradeDurationResult.desc);
                    if (!noTradeDurationResult.result)
                    {
                        return null;
                    }
                }
                //加仓买入单
                if (result.ContainsKey("INCREASE_BUY_TRENDORDER"))
                {
                    _Log.LogInfo("加仓买入单策略分析");
                    ExecuteResult buyTrendResult = result["INCREASE_BUY_TRENDORDER"];
                    _Log.LogInfo(buyTrendResult.desc);
                    if (buyTrendResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "IN_BUY";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["inOpenPrice"] = price;

                        TradePosition tradePositionInfo = (TradePosition)context["TradePositionInfo"];
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "BUY", "1", tradePositionInfo.RiskR, tradePositionInfo.StopLoss);
                        return executeDesc.ToString();
                    }
                }
                //M_EA
                if (result.ContainsKey("M_EA"))
                {
                    ExecuteResult mResult = result["M_EA"];
                    _Log.LogInfo(mResult.desc);
                    if (mResult.result)
                    {
                        int AddPositionLevel = Convert.ToInt32(context["AddPositionLevel"]);
                        string AddPosition = "M" + AddPositionLevel.ToString();
                        double AddPositionLot = Convert.ToDouble(context["AddPositionLot"]);
                        if (string.Equals(context["CurrentOrderType"], "BUY"))
                        {
                            _Log.LogInfo("M开仓策略分析");
                            context["CurrentOrderTypeDetail"] = AddPosition + "_BUY";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            //第一加仓价格
                            context["openPrice" + AddPosition] = price;

                            TradePosition tradePositionInfo = (TradePosition)context["TradePositionInfo"];
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "BUY", AddPositionLot, tradePositionInfo.RiskR, tradePositionInfo.StopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 加仓卖出
        /// </summary>
        /// <param name="result"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string IncreaseSellAnalysis(Dictionary<string, ExecuteResult> result, IDictionary<string, Object> context)
        {
            StringBuilder executeDesc = new StringBuilder();
            if (result != null && result.Count > 0)
            {
                if (result.ContainsKey("NOTRADE_DURATION"))
                {
                    ExecuteResult noTradeDurationResult = result["NOTRADE_DURATION"];
                    _Log.LogInfo(noTradeDurationResult.desc);
                    if (!noTradeDurationResult.result)
                    {
                        return null;
                    }
                }
                //加仓卖出单
                if (result.ContainsKey("INCREASE_SELL_TRENDORDER"))
                {
                    _Log.LogInfo("加仓卖出单策略分析");
                    ExecuteResult sellTrendResult = result["INCREASE_SELL_TRENDORDER"];
                    _Log.LogInfo(sellTrendResult.desc);
                    if (sellTrendResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "IN_SELL";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["inOpenPrice"] = price;

                        TradePosition tradePositionInfo = (TradePosition)context["TradePositionInfo"];
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "SELL", "1", tradePositionInfo.RiskR, tradePositionInfo.StopLoss);
                        return executeDesc.ToString();
                    }
                }
                //M_EA 马丁加仓
                if (result.ContainsKey("M_EA"))
                {
                    ExecuteResult mResult = result["M_EA"];
                    _Log.LogInfo(mResult.desc);
                    if (mResult.result)
                    {
                        int AddPositionLevel = Convert.ToInt32(context["AddPositionLevel"]);
                        string AddPosition = "M" + AddPositionLevel.ToString();
                        double AddPositionLot = Convert.ToDouble(context["AddPositionLot"]);
                        if (string.Equals(context["CurrentOrderType"], "SELL"))
                        {
                            _Log.LogInfo("M开仓策略分析");
                            context["CurrentOrderTypeDetail"] = AddPosition + "_SELL";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            //第一加仓价格
                            context["openPrice" + AddPosition] = price;

                            TradePosition tradePositionInfo = (TradePosition)context["TradePositionInfo"];
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "SELL", AddPositionLot, tradePositionInfo.RiskR, tradePositionInfo.StopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }
            }
            return null;
        }
        public string closeBuyAnalysis(Dictionary<string, ExecuteResult> result, IDictionary<string, Object> context)
        {
            string currentOrderTypeDetail = "";
            if (context.ContainsKey("CurrentOrderTypeDetail"))
            {
                currentOrderTypeDetail = context["CurrentOrderTypeDetail"].ToString();
            }
            if (string.IsNullOrEmpty(currentOrderTypeDetail))
            {
                StringBuilder desc = new StringBuilder();
                desc.AppendFormat("CurrentOrderTypeDetail 值为空,不执行任何CLOSE_BUY策略");
                _Log.LogInfo(desc.ToString());
                return null;
            }

            StringBuilder executeDesc = new StringBuilder();
            if (result != null && result.Count > 0)
            {
                //趋势单开仓，趋势单平仓
                if (result.ContainsKey("CLOSE_BUY_TRENDORDER") && string.Equals(currentOrderTypeDetail, "TREND_BUY"))
                {
                    ExecuteResult buyTrendResult = result["CLOSE_BUY_TRENDORDER"];
                    _Log.LogInfo(buyTrendResult.desc);
                    if (buyTrendResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "CLOSE_BUY_TRENDORDER";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["closePrice"] = price;
                        string takeProfit = "50";
                        string stopLoss = "50";
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_BUY", "1", takeProfit, stopLoss);
                        return executeDesc.ToString();
                    }
                }
                //止盈平仓
                if (result.ContainsKey("TAKE_PROFIT"))
                {
                    ExecuteResult takeProfitResult = result["TAKE_PROFIT"];
                    _Log.LogInfo(takeProfitResult.desc);
                    if (takeProfitResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "CLOSE_TAKE_PROFIT";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["closePrice"] = price;
                        string takeProfit = "50";
                        string stopLoss = "50";
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_BUY", "1", takeProfit, stopLoss);
                        return executeDesc.ToString();
                    }
                }
                //止损平仓
                if (result.ContainsKey("STOP_LOSS"))
                {
                    ExecuteResult stopLossResult = result["STOP_LOSS"];
                    _Log.LogInfo(stopLossResult.desc);
                    if (stopLossResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "CLOSE_STOP_LOSS";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["closePrice"] = price;
                        string takeProfit = "50";
                        string stopLoss = "50";
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_BUY", "1", takeProfit, stopLoss);
                        return executeDesc.ToString();
                    }
                }
                //动态止盈
                if (result.ContainsKey("KEEP_PROFIT"))
                {
                    ExecuteResult keepProfitResult = result["KEEP_PROFIT"];
                    _Log.LogInfo(keepProfitResult.desc);
                    if (keepProfitResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "CLOSE_KEEP_BUY_PROFIT";
                        context["overKeepProfit"] = "F";
                        context["overKeepProfit2"] = "F";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["closePrice"] = price;
                        string takeProfit = "50";
                        string stopLoss = "50";
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_BUY", "1", takeProfit, stopLoss);
                        return executeDesc.ToString();
                    }
                }

                //动态止损
                if (result.ContainsKey("KEEP_LOSS"))
                {
                    ExecuteResult keepLossResult = result["KEEP_LOSS"];
                    _Log.LogInfo(keepLossResult.desc);
                    if (keepLossResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "CLOSE_KEEP_BUY_LOSS";
                        context["overKeepProfit"] = "F";
                        context["overKeepProfit2"] = "F";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["closePrice"] = price;
                        string takeProfit = "50";
                        string stopLoss = "50";
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_BUY", "1", takeProfit, stopLoss);
                        return executeDesc.ToString();
                    }
                }

                //KDJ 指数平仓
                if (result.ContainsKey("CLOSE_KDJ"))
                {
                    ExecuteResult KDJResult = result["CLOSE_KDJ"];
                    _Log.LogInfo(KDJResult.desc);
                    if (KDJResult.result)
                    {
                        string kResult = context["StrategyResultCLOSEKDJ"].ToString();
                        _Log.LogInfo("[StrategyResult=" + kResult + "]");
                        if (string.Equals(kResult, "CLOSE_BUY"))
                        {
                            context["CurrentOrderTypeDetail"] = "CLOSE_BUY_KDJ";
                            context["overKeepProfit"] = "F";
                            context["overKeepProfit2"] = "F";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            context["closePrice"] = price;
                            string takeProfit = "50";
                            string stopLoss = "50";
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_BUY", "1", takeProfit, stopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }

                //周六不持仓过周末
                if (result.ContainsKey("CLOSE_ORDER_WEEKEND"))
                {
                    ExecuteResult weekendResult = result["CLOSE_ORDER_WEEKEND"];
                    _Log.LogInfo(weekendResult.desc);
                    if (weekendResult.result)
                    {
                        string currentOrderType = context["CurrentOrderType"].ToString();
                        if (string.Equals(currentOrderType, "BUY"))
                        {
                            context["CurrentOrderTypeDetail"] = "CLOSE_BUY_WEEKEND";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            context["closePrice"] = price;
                            string takeProfit = "50";
                            string stopLoss = "50";
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_BUY", "1", takeProfit, stopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }
                //不持仓过夜
                if (result.ContainsKey("CLOSE_ORDER_DAILY"))
                {
                    ExecuteResult dailyendResult = result["CLOSE_ORDER_DAILY"];
                    _Log.LogInfo(dailyendResult.desc);
                    if (dailyendResult.result)
                    {
                        string currentOrderType = context["CurrentOrderType"].ToString();
                        if (string.Equals(currentOrderType, "BUY"))
                        {
                            context["CurrentOrderTypeDetail"] = "CLOSE_BUY_DAILYEND";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            context["closePrice"] = price;
                            string takeProfit = "50";
                            string stopLoss = "50";
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_BUY", "1", takeProfit, stopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }

                //月末周六不持仓过月末
                if (result.ContainsKey("CLOSE_ORDER_MONTHEND"))
                {
                    ExecuteResult monthEndResult = result["CLOSE_ORDER_MONTHEND"];
                    _Log.LogInfo(monthEndResult.desc);
                    if (monthEndResult.result)
                    {
                        //暂无不持仓过周末单子
                    }
                }

                //情绪指数平仓
                if (result.ContainsKey("CLOSE_BUY") && (string.Equals(currentOrderTypeDetail, "BUY") || string.Equals(currentOrderTypeDetail, "IN_BUY")))
                {
                    ExecuteResult closeBuyResult = result["CLOSE_BUY"];
                    _Log.LogInfo(closeBuyResult.desc);
                    if (closeBuyResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "CLOSE_BUY";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["closePrice"] = price;
                        string takeProfit = "50";
                        string stopLoss = "50";
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_BUY", "1", takeProfit, stopLoss);
                        return executeDesc.ToString();
                    }
                }

                //EMA趋势反转平仓
                if (result.ContainsKey("CLOSE_EMA"))
                {
                    if (string.Equals(context["CurrentOrderType"], "BUY"))
                    {
                        ExecuteResult emaCloseResult = result["CLOSE_EMA"];
                        _Log.LogInfo(emaCloseResult.desc);
                        string tResult = context["StrategyResultC"].ToString();
                        _Log.LogInfo("[StrategyResultC=" + tResult + "]");
                        if (emaCloseResult.result && string.Equals(tResult, "SELL TREND"))
                        {
                            context["CurrentOrderTypeDetail"] = "CLOSE_BUY_EMA";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            context["closePrice"] = price;
                            string takeProfit = "50";
                            string stopLoss = "50";
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_BUY", "1", takeProfit, stopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }
                //K平仓
                if (result.ContainsKey("K_EA"))
                {
                    if (string.Equals(context["CurrentOrderType"], "BUY"))
                    {
                        ExecuteResult kCloseResult = result["K_EA"];
                        _Log.LogInfo(kCloseResult.desc);
                        if (kCloseResult.result)
                        {
                            string tResult = context["StrategyResultK"].ToString();
                            _Log.LogInfo("[StrategyResultK=" + tResult + "]");
                            if (string.Equals(tResult, "CLOSE_BUY"))
                            {
                                context["CurrentOrderTypeDetail"] = "CLOSE_BUY_K";
                                //品种
                                string symbol = context["symbol"].ToString();
                                //最新价格
                                string price = context["price"].ToString();
                                context["closePrice"] = price;
                                string takeProfit = "50";
                                string stopLoss = "50";
                                executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_BUY", "1", takeProfit, stopLoss);
                                return executeDesc.ToString();
                            }
                        }
                    }
                }
                //H 策略平仓
                if (result.ContainsKey("H_EA"))
                {
                    if (string.Equals(context["CurrentOrderType"], "BUY"))
                    {
                        ExecuteResult hCloseResult = result["H_EA"];
                        _Log.LogInfo(hCloseResult.desc);
                        if (hCloseResult.result)
                        {
                            string tResult = context["StrategyResultH"].ToString();
                            _Log.LogInfo("[StrategyResultH=" + tResult + "]");
                            if (string.Equals(tResult, "CLOSE_BUY"))
                            {
                                context["CurrentOrderTypeDetail"] = "CLOSE_BUY_H";
                                //品种
                                string symbol = context["symbol"].ToString();
                                //最新价格
                                string price = context["price"].ToString();
                                context["closePrice"] = price;
                                string takeProfit = "50";
                                string stopLoss = "50";
                                executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_BUY", "1", takeProfit, stopLoss);
                                return executeDesc.ToString();
                            }
                        }
                    }
                }
                //仓位管理平仓
                if (result.ContainsKey("MANAGE_POSITION"))
                {
                    ExecuteResult mangePositionResult = result["MANAGE_POSITION"];
                    _Log.LogInfo(mangePositionResult.desc);
                    if (mangePositionResult.result)
                    {
                        if (context.ContainsKey("TradePositionInfo") && context.ContainsKey("TradeSignals"))
                        {
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            string takeProfit = "50";
                            string stopLoss = "50";

                            TradePosition tradePositionInfo = (TradePosition)context["TradePositionInfo"];
                            List<TradeSignal> signals = (List<TradeSignal>)context["TradeSignals"];

                            // 全部平仓
                            if (signals.Contains(TradeSignal.StopLossHit))
                            {
                                context["CurrentOrderTypeDetail"] = "MANAGE_POSITION_SL_CLOSE_BUY_SL";
                                context["closePrice"] = price;
                                executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_BUY", "1", takeProfit, stopLoss);

                            }
                            // 全部平仓
                            if (signals.Contains(TradeSignal.ExitByEMA))
                            {
                                context["CurrentOrderTypeDetail"] = "MANAGE_POSITION_EMA_CLOSE_BUY_EMA";
                                context["closePrice"] = price;
                                executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_BUY", "1", takeProfit, stopLoss);
                            }
                            // 平 50% 仓位
                            if (signals.Contains(TradeSignal.TakeProfit1))
                            {
                                context["CurrentOrderTypeDetail"] = "MANAGE_POSITION_CLOSE_BUY_TP1";
                                context["closePriceP1"] = price;
                                executeDesc.AppendFormat(executeResultStr, symbol, price, "P_CLOSE_BUY", context["ManagePositionTakeProfitLot1"], takeProfit, stopLoss);
                            }

                            // 再平 30% 仓位
                            if (signals.Contains(TradeSignal.TakeProfit2))
                            {
                                context["CurrentOrderTypeDetail"] = "MANAGE_POSITION_CLOSE_BUY_TP2";
                                context["closePriceP2"] = price;
                                executeDesc.AppendFormat(executeResultStr, symbol, price, "P_CLOSE_BUY", context["ManagePositionTakeProfitLot2"], takeProfit, stopLoss);
                            }
                            return executeDesc.ToString();
                        }
                    }
                }
            }
            else
            {
                _Log.LogInfo("策略结果中无任何数据,不执行策略");
            }
            return null;
        }
        public string closeSellAnalysis(Dictionary<string, ExecuteResult> result, IDictionary<string, Object> context)
        {
            string currentOrderTypeDetail = "";
            if (context.ContainsKey("CurrentOrderTypeDetail"))
            {
                currentOrderTypeDetail = context["CurrentOrderTypeDetail"].ToString();
            }
            if (string.IsNullOrEmpty(currentOrderTypeDetail))
            {
                StringBuilder desc = new StringBuilder();
                desc.AppendFormat("CurrentOrderTypeDetail 值为空,不执行任何CLOSE_SELL策略");
                _Log.LogInfo(desc.ToString());
                return null;
            }

            StringBuilder executeDesc = new StringBuilder();
            if (result != null && result.Count > 0)
            {
                //趋势单
                if (result.ContainsKey("CLOSE_SELL_TRENDORDER") && string.Equals(currentOrderTypeDetail, "TREND_SELL"))
                {
                    ExecuteResult sellTrendResult = result["CLOSE_SELL_TRENDORDER"];
                    _Log.LogInfo(sellTrendResult.desc);
                    if (sellTrendResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "CLOSE_SELL_TRENDORDER";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["closePrice"] = price;
                        string takeProfit = "50";
                        string stopLoss = "50";
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_SELL", "1", takeProfit, stopLoss);
                        return executeDesc.ToString();
                    }
                }
                //止盈
                if (result.ContainsKey("TAKE_PROFIT"))
                {
                    ExecuteResult takeProfitResult = result["TAKE_PROFIT"];
                    _Log.LogInfo(takeProfitResult.desc);
                    if (takeProfitResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "CLOSE_TAKE_PROFIT";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["closePrice"] = price;
                        string takeProfit = "50";
                        string stopLoss = "50";
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_SELL", "1", takeProfit, stopLoss);
                        return executeDesc.ToString();
                    }
                }
                //止损
                if (result.ContainsKey("STOP_LOSS"))
                {
                    ExecuteResult stopLossResult = result["STOP_LOSS"];
                    _Log.LogInfo(stopLossResult.desc);
                    if (stopLossResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "CLOSE_STOP_LOSS";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["closePrice"] = price;
                        string takeProfit = "50";
                        string stopLoss = "50";
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_SELL", "1", takeProfit, stopLoss);
                        return executeDesc.ToString();
                    }
                }
                //动态止盈
                if (result.ContainsKey("KEEP_PROFIT"))
                {
                    ExecuteResult keepProfitResult = result["KEEP_PROFIT"];
                    _Log.LogInfo(keepProfitResult.desc);
                    if (keepProfitResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "CLOSE_KEEP_SELL_PROFIT";
                        context["overKeepProfit"] = "F";
                        context["overKeepProfit2"] = "F";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["closePrice"] = price;
                        string takeProfit = "50";
                        string stopLoss = "50";
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_SELL", "1", takeProfit, stopLoss);
                        return executeDesc.ToString();
                    }
                }

                //动态止损
                if (result.ContainsKey("KEEP_LOSS"))
                {
                    ExecuteResult keepLossResult = result["KEEP_LOSS"];
                    _Log.LogInfo(keepLossResult.desc);
                    if (keepLossResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "CLOSE_KEEP_SELL_LOSS";
                        context["overKeepProfit"] = "F";
                        context["overKeepProfit2"] = "F";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["closePrice"] = price;
                        string takeProfit = "50";
                        string stopLoss = "50";
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_SELL", "1", takeProfit, stopLoss);
                        return executeDesc.ToString();
                    }
                }
                //KDJ 指数平仓
                if (result.ContainsKey("CLOSE_KDJ"))
                {
                    ExecuteResult KDJResult = result["CLOSE_KDJ"];
                    _Log.LogInfo(KDJResult.desc);
                    if (KDJResult.result)
                    {
                        string kResult = context["StrategyResultCLOSEKDJ"].ToString();
                        _Log.LogInfo("[StrategyResult=" + kResult + "]");
                        if (string.Equals(kResult, "CLOSE_SELL"))
                        {
                            context["CurrentOrderTypeDetail"] = "CLOSE_SELL_KDJ";
                            context["overKeepProfit"] = "F";
                            context["overKeepProfit2"] = "F";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            context["closePrice"] = price;
                            string takeProfit = "50";
                            string stopLoss = "50";
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_SELL", "1", takeProfit, stopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }

                //周六不持仓过周末
                if (result.ContainsKey("CLOSE_ORDER_WEEKEND"))
                {
                    ExecuteResult weekendResult = result["CLOSE_ORDER_WEEKEND"];
                    _Log.LogInfo(weekendResult.desc);
                    if (weekendResult.result)
                    {
                        string currentOrderType = context["CurrentOrderType"].ToString();
                        if (string.Equals(currentOrderType, "SELL"))
                        {
                            context["CurrentOrderTypeDetail"] = "CLOSE_SELL_WEEKEND";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            context["closePrice"] = price;
                            string takeProfit = "50";
                            string stopLoss = "50";
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_SELL", "1", takeProfit, stopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }

                //不持仓过夜
                if (result.ContainsKey("CLOSE_ORDER_DAILY"))
                {
                    ExecuteResult weekendResult = result["CLOSE_ORDER_DAILY"];
                    _Log.LogInfo(weekendResult.desc);
                    if (weekendResult.result)
                    {
                        string currentOrderType = context["CurrentOrderType"].ToString();
                        if (string.Equals(currentOrderType, "SELL"))
                        {
                            context["CurrentOrderTypeDetail"] = "CLOSE_SELL_DAILYEND";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            context["closePrice"] = price;
                            string takeProfit = "50";
                            string stopLoss = "50";
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_SELL", "1", takeProfit, stopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }

                //月末周六不持仓过月末
                if (result.ContainsKey("CLOSE_ORDER_MONTHEND"))
                {
                    ExecuteResult monthEndResult = result["CLOSE_ORDER_MONTHEND"];
                    _Log.LogInfo(monthEndResult.desc);
                    if (monthEndResult.result)
                    {
                        //暂无不持仓过周末单子
                    }
                }

                if (result.ContainsKey("CLOSE_SELL") && (string.Equals(currentOrderTypeDetail, "SELL") || string.Equals(currentOrderTypeDetail, "IN_SELL")))
                {
                    ExecuteResult closeSellResult = result["CLOSE_SELL"];
                    _Log.LogInfo(closeSellResult.desc);
                    if (closeSellResult.result)
                    {
                        context["CurrentOrderTypeDetail"] = "CLOSE_SELL";
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        context["closePrice"] = price;
                        string takeProfit = "50";
                        string stopLoss = "50";
                        executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_SELL", "1", takeProfit, stopLoss);
                        return executeDesc.ToString();
                    }
                }

                //EMA趋势反转平仓
                if (result.ContainsKey("CLOSE_EMA"))
                {
                    if (string.Equals(context["CurrentOrderType"], "SELL"))
                    {
                        ExecuteResult emaCloseResult = result["CLOSE_EMA"];
                        _Log.LogInfo(emaCloseResult.desc);
                        string tResult = context["StrategyResultC"].ToString();
                        _Log.LogInfo("[StrategyResulCt=" + tResult + "]");
                        if (emaCloseResult.result && string.Equals(tResult, "BUY TREND"))
                        {
                            context["CurrentOrderTypeDetail"] = "CLOSE_SELL_EMA";
                            //品种
                            string symbol = context["symbol"].ToString();
                            //最新价格
                            string price = context["price"].ToString();
                            context["closePrice"] = price;
                            string takeProfit = "50";
                            string stopLoss = "50";
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_SELL", "1", takeProfit, stopLoss);
                            return executeDesc.ToString();
                        }
                    }
                }
                //K 平仓
                if (result.ContainsKey("K_EA"))
                {
                    if (string.Equals(context["CurrentOrderType"], "SELL"))
                    {
                        ExecuteResult kCloseResult = result["K_EA"];
                        _Log.LogInfo(kCloseResult.desc);
                        if (kCloseResult.result)
                        {
                            string tResult = context["StrategyResultK"].ToString();
                            _Log.LogInfo("[StrategyResultK=" + tResult + "]");
                            if (string.Equals(tResult, "CLOSE_SELL"))
                            {
                                context["CurrentOrderTypeDetail"] = "CLOSE_SELL_K";
                                //品种
                                string symbol = context["symbol"].ToString();
                                //最新价格
                                string price = context["price"].ToString();
                                context["closePrice"] = price;
                                string takeProfit = "50";
                                string stopLoss = "50";
                                executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_SELL", "1", takeProfit, stopLoss);
                                return executeDesc.ToString();
                            }
                        }

                    }
                }
                //H 平仓
                if (result.ContainsKey("H_EA"))
                {
                    if (string.Equals(context["CurrentOrderType"], "SELL"))
                    {
                        ExecuteResult hCloseResult = result["H_EA"];
                        _Log.LogInfo(hCloseResult.desc);
                        if (hCloseResult.result)
                        {
                            string tResult = context["StrategyResultH"].ToString();
                            _Log.LogInfo("[StrategyResultH=" + tResult + "]");
                            if (string.Equals(tResult, "CLOSE_SELL"))
                            {
                                context["CurrentOrderTypeDetail"] = "CLOSE_SELL_H";
                                //品种
                                string symbol = context["symbol"].ToString();
                                //最新价格
                                string price = context["price"].ToString();
                                context["closePrice"] = price;
                                string takeProfit = "50";
                                string stopLoss = "50";
                                executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_SELL", "1", takeProfit, stopLoss);
                                return executeDesc.ToString();
                            }
                        }
                    }
                }
                //仓位管理平仓
                if (result.ContainsKey("MANAGE_POSITION"))
                {
                    if (context.ContainsKey("TradePositionInfo") && context.ContainsKey("TradeSignals"))
                    {
                        //品种
                        string symbol = context["symbol"].ToString();
                        //最新价格
                        string price = context["price"].ToString();
                        string takeProfit = "50";
                        string stopLoss = "50";

                        List<TradeSignal> signals = (List<TradeSignal>)context["TradeSignals"];

                        // 全部平仓
                        if (signals.Contains(TradeSignal.StopLossHit))
                        {
                            context["CurrentOrderTypeDetail"] = "MANAGE_POSITION_SL_CLOSE_SELL_SL";
                            context["closePrice"] = price;
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_SELL", "1", takeProfit, stopLoss);
                        }
                        // 全部平仓
                        if (signals.Contains(TradeSignal.ExitByEMA))
                        {
                            context["CurrentOrderTypeDetail"] = "MANAGE_POSITION_EMA_CLOSE_SELL_EMA";
                            context["closePrice"] = price;
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "CLOSE_SELL", "1", takeProfit, stopLoss);
                        }
                        // 平 50% 仓位
                        if (signals.Contains(TradeSignal.TakeProfit1))
                        {
                            context["CurrentOrderTypeDetail"] = "MANAGE_POSITION_CLOSE_SELL_TP1";
                            context["closePriceP1"] = price;
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "P_CLOSE_SELL", context["ManagePositionTakeProfitLot1"], takeProfit, stopLoss);
                        }

                        // 再平 30% 仓位
                        if (signals.Contains(TradeSignal.TakeProfit2))
                        {
                            context["CurrentOrderTypeDetail"] = "MANAGE_POSITION_CLOSE_SELL_TP2";
                            context["closePriceP2"] = price;
                            executeDesc.AppendFormat(executeResultStr, symbol, price, "P_CLOSE_SELL", context["ManagePositionTakeProfitLot2"], takeProfit, stopLoss);
                        }
                        return executeDesc.ToString();
                    }
                }
            }
            else
            {
                _Log.LogInfo("策略结果中无任何数据,不执行策略");
            }
            return null;
        }
    }
}