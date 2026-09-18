using uClient.Comm;

namespace uClient.Broker
{
    /// <summary>
    /// 策略列表
    /// </summary>
    public class StrategyUtils
    {
        /// <summary>
        /// 初始化策略列表
        /// </summary>
        /// <param name="_StrategyConfig"></param>
        /// <param name="_EAConfig"></param>
        public static void initStrategyList(EAStrategyConfig _StrategyConfig, EAConfig _EAConfig, Config _Config)
        {
            if (_StrategyConfig.EAEntityList != null && _StrategyConfig.EAEntityList.Count > 0)
            {
                //公共参数，该参数不可通过界面设置
                //string changeDiff = "3";

                foreach (EAEntity eaEntity in _StrategyConfig.EAEntityList)
                {
                    _StrategyConfig.EAEntity.Add(eaEntity.code, eaEntity);
                    //增加的所有策略数据在此初始化
                    switch (eaEntity.code)
                    {
                        case "BUY":
                            StrategyEA buyStrategyEA = new BuyTradeProportion();
                            buyStrategyEA.code = eaEntity.code;
                            buyStrategyEA.name = eaEntity.name;
                            buyStrategyEA.type = eaEntity.code;
                            buyStrategyEA.symbol = "";
                            buyStrategyEA.active = eaEntity.active;
                            buyStrategyEA.sumConfigValue = eaEntity.sum;
                            buyStrategyEA.sumConfigValue1 = eaEntity.sum1;
                            buyStrategyEA.gsConfigValue = eaEntity.gs;
                            buyStrategyEA.gsConfigValue1 = eaEntity.gs1;
                            buyStrategyEA.dailyConfigValue = eaEntity.daily;
                            buyStrategyEA.dailyConfigValue1 = eaEntity.daily1;
                            buyStrategyEA.hourlyConfigValue = eaEntity.hourly;
                            buyStrategyEA.hourlyConfigValue1 = eaEntity.hourly1;
                            buyStrategyEA.configParam.Add("minChangeDiff", _StrategyConfig.minChangeDiff.ToString());
                            buyStrategyEA.configParam.Add("maxChangeDiff", _StrategyConfig.maxChangeDiff.ToString());
                            buyStrategyEA.configParam.Add("maxDiffChangeDuration", _StrategyConfig.maxDiffChangeDuration.ToString());
                            buyStrategyEA.configParam.Add("openDiff", _StrategyConfig.openDiff.ToString());
                            buyStrategyEA.configParam.Add("continuousOrder", _StrategyConfig.continuousOrder ? "T" : "F");
                            buyStrategyEA.configParam.Add("reverseProportion", _StrategyConfig.reverseProportion ? "T" : "F");

                            _EAConfig.EAList.Add(buyStrategyEA);
                            _EAConfig.EA.Add(buyStrategyEA.code, buyStrategyEA);
                            break;
                        case "SELL":
                            StrategyEA sellStrategyEA = new SellTradeProportion();
                            sellStrategyEA.code = eaEntity.code;
                            sellStrategyEA.name = eaEntity.name;
                            sellStrategyEA.type = eaEntity.code;
                            sellStrategyEA.symbol = "";
                            sellStrategyEA.active = eaEntity.active;
                            sellStrategyEA.sumConfigValue = eaEntity.sum;
                            sellStrategyEA.sumConfigValue1 = eaEntity.sum1;
                            sellStrategyEA.gsConfigValue = eaEntity.gs;
                            sellStrategyEA.gsConfigValue1 = eaEntity.gs1;
                            sellStrategyEA.dailyConfigValue = eaEntity.daily;
                            sellStrategyEA.dailyConfigValue1 = eaEntity.daily1;
                            sellStrategyEA.hourlyConfigValue = eaEntity.hourly;
                            sellStrategyEA.hourlyConfigValue1 = eaEntity.hourly1;

                            sellStrategyEA.configParam.Add("minChangeDiff", _StrategyConfig.minChangeDiff.ToString());
                            sellStrategyEA.configParam.Add("maxChangeDiff", _StrategyConfig.maxChangeDiff.ToString());
                            sellStrategyEA.configParam.Add("maxDiffChangeDuration", _StrategyConfig.maxDiffChangeDuration.ToString());
                            sellStrategyEA.configParam.Add("openDiff", _StrategyConfig.openDiff.ToString());
                            sellStrategyEA.configParam.Add("continuousOrder", _StrategyConfig.continuousOrder ? "T" : "F");
                            sellStrategyEA.configParam.Add("reverseProportion", _StrategyConfig.reverseProportion ? "T" : "F");

                            _EAConfig.EAList.Add(sellStrategyEA);
                            _EAConfig.EA.Add(sellStrategyEA.code, sellStrategyEA);
                            break;
                        case "CLOSE_BUY":
                            StrategyEA closeBuyStrategyEA = new CloseBuyTradeProportion();
                            closeBuyStrategyEA.code = eaEntity.code;
                            closeBuyStrategyEA.name = eaEntity.name;
                            closeBuyStrategyEA.type = eaEntity.code;
                            closeBuyStrategyEA.symbol = "";
                            //closeBuyStrategyEA.active = eaEntity.active;
                            closeBuyStrategyEA.active = _StrategyConfig.indexClose;
                            closeBuyStrategyEA.sumConfigValue = eaEntity.sum;
                            closeBuyStrategyEA.sumConfigValue1 = eaEntity.sum1;
                            closeBuyStrategyEA.gsConfigValue = eaEntity.gs;
                            closeBuyStrategyEA.gsConfigValue1 = eaEntity.gs1;
                            closeBuyStrategyEA.dailyConfigValue = eaEntity.daily;
                            closeBuyStrategyEA.dailyConfigValue1 = eaEntity.daily1;
                            closeBuyStrategyEA.hourlyConfigValue = eaEntity.hourly;
                            closeBuyStrategyEA.hourlyConfigValue1 = eaEntity.hourly1;
                            closeBuyStrategyEA.configParam.Add("minChangeDiff", _StrategyConfig.minChangeDiff.ToString());
                            closeBuyStrategyEA.configParam.Add("maxChangeDiff", _StrategyConfig.maxChangeDiff.ToString());
                            closeBuyStrategyEA.configParam.Add("maxDiffChangeDuration", _StrategyConfig.maxDiffChangeDuration.ToString());
                            closeBuyStrategyEA.configParam.Add("openDiff", _StrategyConfig.openDiff.ToString());
                            closeBuyStrategyEA.configParam.Add("continuousOrder", _StrategyConfig.continuousOrder ? "T" : "F");
                            closeBuyStrategyEA.configParam.Add("reverseProportion", _StrategyConfig.reverseProportion ? "T" : "F");

                            _EAConfig.EAList.Add(closeBuyStrategyEA);
                            _EAConfig.EA.Add(closeBuyStrategyEA.code, closeBuyStrategyEA);
                            break;
                        case "CLOSE_SELL":
                            StrategyEA closeSellStrategyEA = new CloseSellTradeProportion();
                            closeSellStrategyEA.code = eaEntity.code;
                            closeSellStrategyEA.name = eaEntity.name;
                            closeSellStrategyEA.symbol = "";
                            closeSellStrategyEA.type = eaEntity.code;
                            //closeSellStrategyEA.active = eaEntity.active;
                            closeSellStrategyEA.active = _StrategyConfig.indexClose;
                            closeSellStrategyEA.sumConfigValue = eaEntity.sum;
                            closeSellStrategyEA.sumConfigValue1 = eaEntity.sum1;
                            closeSellStrategyEA.gsConfigValue = eaEntity.gs;
                            closeSellStrategyEA.gsConfigValue1 = eaEntity.gs1;
                            closeSellStrategyEA.dailyConfigValue = eaEntity.daily;
                            closeSellStrategyEA.dailyConfigValue1 = eaEntity.daily1;
                            closeSellStrategyEA.hourlyConfigValue = eaEntity.hourly;
                            closeSellStrategyEA.hourlyConfigValue1 = eaEntity.hourly1;
                            closeSellStrategyEA.configParam.Add("minChangeDiff", _StrategyConfig.minChangeDiff.ToString());
                            closeSellStrategyEA.configParam.Add("maxChangeDiff", _StrategyConfig.maxChangeDiff.ToString());
                            closeSellStrategyEA.configParam.Add("maxDiffChangeDuration", _StrategyConfig.maxDiffChangeDuration.ToString());
                            closeSellStrategyEA.configParam.Add("openDiff", _StrategyConfig.openDiff.ToString());
                            closeSellStrategyEA.configParam.Add("continuousOrder", _StrategyConfig.continuousOrder ? "T" : "F");
                            closeSellStrategyEA.configParam.Add("reverseProportion", _StrategyConfig.reverseProportion ? "T" : "F");

                            _EAConfig.EAList.Add(closeSellStrategyEA);
                            _EAConfig.EA.Add(closeSellStrategyEA.code, closeSellStrategyEA);
                            break;
                    }
                }
                //时间区间斜率分析策略
                StrategyEA timeDurationSlope = new TimeDurationSlope();
                timeDurationSlope.code = "TIME_DURATION_SLOPE";
                timeDurationSlope.name = "时间区间斜率";
                timeDurationSlope.symbol = "";
                timeDurationSlope.value = _StrategyConfig.timeDuration.ToString();
                timeDurationSlope.type = "";
                timeDurationSlope.active = _StrategyConfig.timeDuration != 0;
                timeDurationSlope.configParam.Add("slope", _StrategyConfig.slope.ToString());
                _EAConfig.EAList.Add(timeDurationSlope);
                _EAConfig.EA.Add(timeDurationSlope.code, timeDurationSlope);

                //非交易时间
                StrategyEA noTradeDuration = new NoTradeDuration();
                noTradeDuration.code = "NOTRADE_DURATION";
                noTradeDuration.name = "非执行时间";
                noTradeDuration.symbol = "";
                //默认采用EANotradeDuration
                noTradeDuration.value = _StrategyConfig.NotradeDuration;
                noTradeDuration.type = "";
                noTradeDuration.active = true;
                noTradeDuration.configParam.Add("NotradeDuration", _StrategyConfig.NotradeDuration);
                _EAConfig.EAList.Add(noTradeDuration);
                _EAConfig.EA.Add(noTradeDuration.code, noTradeDuration);

                //趋势单
                StrategyEA buyTrendOrder = new BuyTrendOrder();
                buyTrendOrder.code = "BUY_TRENDORDER";
                buyTrendOrder.name = "买多趋势单";
                buyTrendOrder.symbol = "";
                buyTrendOrder.value = _StrategyConfig.trendOrder.ToString();
                buyTrendOrder.type = "";
                buyTrendOrder.active = _StrategyConfig.trendOrder != 0 && _StrategyConfig.trendOrder1 != 0;
                buyTrendOrder.configParam.Add("trendOrder", _StrategyConfig.trendOrder.ToString());
                buyTrendOrder.configParam.Add("trendOrder1", _StrategyConfig.trendOrder1.ToString());

                buyTrendOrder.configParam.Add("analysisDataType", _StrategyConfig.analysisDataType);
                buyTrendOrder.configParam.Add("minChangeDiff", _StrategyConfig.minChangeDiff.ToString());
                buyTrendOrder.configParam.Add("maxChangeDiff", _StrategyConfig.maxChangeDiff.ToString());
                buyTrendOrder.configParam.Add("maxDiffChangeDuration", _StrategyConfig.maxDiffChangeDuration.ToString());
                buyTrendOrder.configParam.Add("openDiff", _StrategyConfig.openDiff.ToString());
                buyTrendOrder.configParam.Add("continuousOrder", _StrategyConfig.continuousOrder ? "T" : "F");
                buyTrendOrder.configParam.Add("reverseProportion", _StrategyConfig.reverseProportion ? "T" : "F");

                _EAConfig.EAList.Add(buyTrendOrder);
                _EAConfig.EA.Add(buyTrendOrder.code, buyTrendOrder);

                //趋势单
                StrategyEA closeBuyTrendOrder = new CloseBuyTrendOrder();
                closeBuyTrendOrder.code = "CLOSE_BUY_TRENDORDER";
                closeBuyTrendOrder.name = "买平趋势单";
                closeBuyTrendOrder.symbol = "";
                closeBuyTrendOrder.value = _StrategyConfig.trendOrder.ToString();
                closeBuyTrendOrder.type = "";

                closeBuyTrendOrder.active = _StrategyConfig.trendOrder != 0 && _StrategyConfig.trendOrder1 != 0;
                closeBuyTrendOrder.configParam.Add("trendOrder", _StrategyConfig.trendOrder.ToString());
                closeBuyTrendOrder.configParam.Add("trendOrder1", _StrategyConfig.trendOrder1.ToString());

                closeBuyTrendOrder.configParam.Add("analysisDataType", _StrategyConfig.analysisDataType);
                closeBuyTrendOrder.configParam.Add("minChangeDiff", _StrategyConfig.minChangeDiff.ToString());
                closeBuyTrendOrder.configParam.Add("maxChangeDiff", _StrategyConfig.maxChangeDiff.ToString());
                closeBuyTrendOrder.configParam.Add("maxDiffChangeDuration", _StrategyConfig.maxDiffChangeDuration.ToString());
                closeBuyTrendOrder.configParam.Add("openDiff", _StrategyConfig.openDiff.ToString());
                closeBuyTrendOrder.configParam.Add("continuousOrder", _StrategyConfig.continuousOrder ? "T" : "F");
                closeBuyTrendOrder.configParam.Add("reverseProportion", _StrategyConfig.reverseProportion ? "T" : "F");

                _EAConfig.EAList.Add(closeBuyTrendOrder);
                _EAConfig.EA.Add(closeBuyTrendOrder.code, closeBuyTrendOrder);

                //趋势单
                StrategyEA sellTrendOrder = new SellTrendOrder();
                sellTrendOrder.code = "SELL_TRENDORDER";
                sellTrendOrder.name = "卖空趋势单";
                sellTrendOrder.symbol = "";
                sellTrendOrder.value = _StrategyConfig.trendOrder.ToString();
                sellTrendOrder.type = "";

                sellTrendOrder.active = _StrategyConfig.trendOrder != 0 && _StrategyConfig.trendOrder1 != 0;
                sellTrendOrder.configParam.Add("trendOrder", _StrategyConfig.trendOrder.ToString());
                sellTrendOrder.configParam.Add("trendOrder1", _StrategyConfig.trendOrder1.ToString());

                sellTrendOrder.configParam.Add("analysisDataType", _StrategyConfig.analysisDataType);
                sellTrendOrder.configParam.Add("minChangeDiff", _StrategyConfig.minChangeDiff.ToString());
                sellTrendOrder.configParam.Add("maxChangeDiff", _StrategyConfig.maxChangeDiff.ToString());
                sellTrendOrder.configParam.Add("maxDiffChangeDuration", _StrategyConfig.maxDiffChangeDuration.ToString());
                sellTrendOrder.configParam.Add("openDiff", _StrategyConfig.openDiff.ToString());
                sellTrendOrder.configParam.Add("continuousOrder", _StrategyConfig.continuousOrder ? "T" : "F");
                sellTrendOrder.configParam.Add("reverseProportion", _StrategyConfig.reverseProportion ? "T" : "F");

                _EAConfig.EAList.Add(sellTrendOrder);
                _EAConfig.EA.Add(sellTrendOrder.code, sellTrendOrder);

                //趋势单
                StrategyEA closeSellTrendOrder = new CloseSellTrendOrder();
                closeSellTrendOrder.code = "CLOSE_SELL_TRENDORDER";
                closeSellTrendOrder.name = "卖平趋势单";
                closeSellTrendOrder.symbol = "";
                closeSellTrendOrder.value = _StrategyConfig.trendOrder1.ToString();
                closeSellTrendOrder.type = "";
                closeSellTrendOrder.active = _StrategyConfig.trendOrder != 0 && _StrategyConfig.trendOrder1 != 0;
                closeSellTrendOrder.configParam.Add("trendOrder", _StrategyConfig.trendOrder.ToString());
                closeSellTrendOrder.configParam.Add("trendOrder1", _StrategyConfig.trendOrder1.ToString());

                closeSellTrendOrder.configParam.Add("analysisDataType", _StrategyConfig.analysisDataType);
                closeSellTrendOrder.configParam.Add("minChangeDiff", _StrategyConfig.minChangeDiff.ToString());
                closeSellTrendOrder.configParam.Add("maxChangeDiff", _StrategyConfig.maxChangeDiff.ToString());
                closeSellTrendOrder.configParam.Add("maxDiffChangeDuration", _StrategyConfig.maxDiffChangeDuration.ToString());
                closeSellTrendOrder.configParam.Add("openDiff", _StrategyConfig.openDiff.ToString());
                closeSellTrendOrder.configParam.Add("continuousOrder", _StrategyConfig.continuousOrder ? "T" : "F");
                closeSellTrendOrder.configParam.Add("reverseProportion", _StrategyConfig.reverseProportion ? "T" : "F");

                _EAConfig.EAList.Add(closeSellTrendOrder);
                _EAConfig.EA.Add(closeSellTrendOrder.code, closeSellTrendOrder);

                //止盈
                StrategyEA takeProfit = new TakeProfit();
                takeProfit.code = "TAKE_PROFIT";
                takeProfit.name = "止盈";
                takeProfit.symbol = "";
                takeProfit.value = _StrategyConfig.takeProfit.ToString();
                takeProfit.type = "";
                takeProfit.active = _StrategyConfig.takeProfit != 0;
                _EAConfig.EAList.Add(takeProfit);
                _EAConfig.EA.Add(takeProfit.code, takeProfit);

                //止损
                StrategyEA stopLoss = new StopLoss();
                stopLoss.code = "STOP_LOSS";
                stopLoss.name = "止损";
                stopLoss.symbol = "";
                stopLoss.value = _StrategyConfig.stopLoss.ToString();
                stopLoss.type = "";
                stopLoss.active = _StrategyConfig.stopLoss != 0;
                _EAConfig.EAList.Add(stopLoss);
                _EAConfig.EA.Add(stopLoss.code, stopLoss);

                //动态止盈
                StrategyEA keepProfit = new KeepProfit();
                keepProfit.code = "KEEP_PROFIT";
                keepProfit.name = "动态止盈";
                keepProfit.symbol = "";
                keepProfit.value = _StrategyConfig.keepProfit.ToString();
                keepProfit.type = "";
                keepProfit.active = _StrategyConfig.EAKP;
                keepProfit.configParam.Add("keepProfitDiff", _StrategyConfig.keepProfitDiff.ToString());

                keepProfit.configParam.Add("slope", _StrategyConfig.slope.ToString());
                keepProfit.configParam.Add("stopLoss", _StrategyConfig.stopLoss.ToString());
                keepProfit.configParam.Add("takeProfit", _StrategyConfig.takeProfit.ToString());
                if (_StrategyConfig.weightValue == 0)
                {
                    _StrategyConfig.weightValue = 0.4;
                }
                if (_StrategyConfig.ratioValue == 0)
                {
                    _StrategyConfig.ratioValue = 0.6;
                }
                keepProfit.configParam.Add("weightValue", _StrategyConfig.weightValue.ToString());
                keepProfit.configParam.Add("ratioValue", _StrategyConfig.ratioValue.ToString());
                _EAConfig.EAList.Add(keepProfit);
                _EAConfig.EA.Add(keepProfit.code, keepProfit);



                //动态止损
                StrategyEA keepLoss = new KeepLoss();
                keepLoss.code = "KEEP_LOSS";
                keepLoss.name = "动态止损";
                keepLoss.symbol = "";
                keepLoss.value = "";
                keepLoss.type = "";
                keepLoss.active = _StrategyConfig.EAKL;
                _EAConfig.EAList.Add(keepLoss);
                _EAConfig.EA.Add(keepLoss.code, keepLoss);


                //情绪指数最大偏差判断
                StrategyEA maxDiffChange = new MaxDiffChange();
                maxDiffChange.code = "MAX_DIFFCHANGE";
                maxDiffChange.name = "最大偏差";
                maxDiffChange.symbol = "";
                maxDiffChange.value = _StrategyConfig.maxChangeDiff.ToString();
                maxDiffChange.type = "";
                maxDiffChange.active = _StrategyConfig.maxChangeDiff != 0;
                maxDiffChange.configParam.Add("maxDiffChangeDuration", _StrategyConfig.maxDiffChangeDuration.ToString());
                maxDiffChange.configParam.Add("analysisDataType", _StrategyConfig.analysisDataType);
                maxDiffChange.configParam.Add("minChangeDiff", _StrategyConfig.minChangeDiff.ToString());
                maxDiffChange.configParam.Add("maxChangeDiff", _StrategyConfig.maxChangeDiff.ToString());
                _EAConfig.EAList.Add(maxDiffChange);
                _EAConfig.EA.Add(maxDiffChange.code, maxDiffChange);


                //趋势单加仓
                StrategyEA increaseBuyTrendOrder = new IncreaseBuyPosition();
                increaseBuyTrendOrder.code = "INCREASE_BUY_TRENDORDER";
                increaseBuyTrendOrder.name = "买多趋势单加仓";
                increaseBuyTrendOrder.symbol = "";
                increaseBuyTrendOrder.value = _StrategyConfig.trendOrder.ToString();
                increaseBuyTrendOrder.type = "";
                increaseBuyTrendOrder.active = _StrategyConfig.increasePositionPoint > 5;
                increaseBuyTrendOrder.configParam.Add("reverseProportion", _StrategyConfig.reverseProportion ? "T" : "F");
                increaseBuyTrendOrder.configParam.Add("increasePositionPoint", _StrategyConfig.increasePositionPoint.ToString());
                _EAConfig.EAList.Add(increaseBuyTrendOrder);
                _EAConfig.EA.Add(increaseBuyTrendOrder.code, increaseBuyTrendOrder);

                //趋势单加仓
                StrategyEA increaseSellTrendOrder = new IncreaseSellPosition();
                increaseSellTrendOrder.code = "INCREASE_SELL_TRENDORDER";
                increaseSellTrendOrder.name = "卖空趋势单加仓";
                increaseSellTrendOrder.symbol = "";
                increaseSellTrendOrder.value = _StrategyConfig.trendOrder.ToString();
                increaseSellTrendOrder.type = "";

                increaseSellTrendOrder.active = _StrategyConfig.increasePositionPoint > 5;
                increaseSellTrendOrder.configParam.Add("reverseProportion", _StrategyConfig.reverseProportion ? "T" : "F");
                increaseSellTrendOrder.configParam.Add("increasePositionPoint", _StrategyConfig.increasePositionPoint.ToString());

                _EAConfig.EAList.Add(increaseSellTrendOrder);
                _EAConfig.EA.Add(increaseSellTrendOrder.code, increaseSellTrendOrder);

                //不持单过周末判断
                StrategyEA closeOrderForWeekend = new CloseOrderForWeekEnd();
                closeOrderForWeekend.code = "CLOSE_ORDER_WEEKEND";
                closeOrderForWeekend.name = "不持单过周末";
                closeOrderForWeekend.symbol = "";
                closeOrderForWeekend.value = _StrategyConfig.NotradeDuration;
                closeOrderForWeekend.type = "";
                closeOrderForWeekend.active = true;
                closeOrderForWeekend.configParam.Add("NotradeDuration", _StrategyConfig.NotradeDuration);
                _EAConfig.EAList.Add(closeOrderForWeekend);
                _EAConfig.EA.Add(closeOrderForWeekend.code, closeOrderForWeekend);

                //不持单过月末周末判断
                StrategyEA closeOrderForDailyEnd = new CloseOrderForDailyEnd();
                closeOrderForDailyEnd.code = "CLOSE_ORDER_DAILY";
                closeOrderForDailyEnd.name = "不持单过夜";
                closeOrderForDailyEnd.symbol = "";
                closeOrderForDailyEnd.value = _StrategyConfig.NotradeDuration;
                closeOrderForDailyEnd.type = "";
                closeOrderForDailyEnd.active = true;
                closeOrderForDailyEnd.configParam.Add("NotradeDuration", _StrategyConfig.NotradeDuration);
                _EAConfig.EAList.Add(closeOrderForDailyEnd);
                _EAConfig.EA.Add(closeOrderForDailyEnd.code, closeOrderForDailyEnd);

                //开仓时间和上次平仓时间间隔检查
                StrategyEA commandTimeDuration = new CommandTimeDuration();
                commandTimeDuration.code = "COMMAND_TIME_DURATION";
                commandTimeDuration.name = "开仓时间和上次平仓时间间隔检查";
                commandTimeDuration.symbol = "";
                commandTimeDuration.value = _StrategyConfig.commandTimeDuration.ToString();
                commandTimeDuration.type = "";
                commandTimeDuration.active = _StrategyConfig.commandTimeDuration > 0;
                _EAConfig.EAList.Add(commandTimeDuration);
                _EAConfig.EA.Add(commandTimeDuration.code, commandTimeDuration);

                //情绪指数采集时间是否有延迟判断,比如正常是准点采集,如果延迟30分钟采集到的数据会导致开仓和预期不一致,不开单
                StrategyEA siCollectTimeDuration = new SICollectTimeDuration();
                siCollectTimeDuration.code = "SICOLLECT_TIME_DURATION";
                siCollectTimeDuration.name = "情绪指数采集时间延迟判断";
                siCollectTimeDuration.symbol = "";
                siCollectTimeDuration.value = "";
                siCollectTimeDuration.type = "";
                siCollectTimeDuration.active = false;
                siCollectTimeDuration.configParam.Add("analysisDataType", _StrategyConfig.analysisDataType);
                //默认超过35分钟后采集的数据不再执行策略
                siCollectTimeDuration.configParam.Add("SICollectTimeDuration", "0");
                _EAConfig.EAList.Add(siCollectTimeDuration);
                _EAConfig.EA.Add(siCollectTimeDuration.code, siCollectTimeDuration);


                //每一个交易日总计开仓数量,超过开仓数量后不再开单
                StrategyEA maxOrderCount = new MaxOrderCount();
                maxOrderCount.code = "MAX_ORDER_COUNT";
                maxOrderCount.name = "单个交易日订单总量检查";
                maxOrderCount.symbol = "";
                maxOrderCount.value = "";
                maxOrderCount.type = "";
                maxOrderCount.active = true;
                maxOrderCount.configParam.Add("analysisDataType", _StrategyConfig.analysisDataType);
                maxOrderCount.configParam.Add("maxOrderCount", _StrategyConfig.maxOrderCount.ToString());
                maxOrderCount.configParam.Add("maxLossPoint", _StrategyConfig.maxLossPoint.ToString());
                maxOrderCount.configParam.Add("lossOrderContinuousCount", _StrategyConfig.lossOrderContinuousCount.ToString());
                maxOrderCount.configParam.Add("lossOrderTotalCount", _StrategyConfig.lossOrderTotalCount.ToString());
                maxOrderCount.configParam.Add("noTradeDuration", _StrategyConfig.NotradeDuration);
                _EAConfig.EAList.Add(maxOrderCount);
                _EAConfig.EA.Add(maxOrderCount.code, maxOrderCount);

                /////////////////////////////////////////////////////
                ///2)定时执行策略,该策略执行后判断是否需要修改策略参数
                /////////////////////////////////////////////////////

                //检查单个交易日订单总量,超过后修改参数[maxDiffChangeDuration=2]
                StrategyEA limitOrderCount = new LimitOrderCount();
                limitOrderCount.code = "LIMIT_ORDER_COUNT";
                limitOrderCount.name = "检查单个交易日订单总量";
                limitOrderCount.symbol = "";
                limitOrderCount.value = "";
                limitOrderCount.type = "";
                limitOrderCount.active = _StrategyConfig.limitOrder;
                limitOrderCount.configParam.Add("analysisDataType", _StrategyConfig.analysisDataType);
                limitOrderCount.configParam.Add("minChangeDiff", _StrategyConfig.minChangeDiff.ToString());
                limitOrderCount.configParam.Add("maxChangeDiff", _StrategyConfig.maxChangeDiff.ToString());
                limitOrderCount.configParam.Add("maxOrderCount", _StrategyConfig.maxOrderCount.ToString());
                limitOrderCount.configParam.Add("noTradeDuration", _StrategyConfig.NotradeDuration);
                _EAConfig.ParaEAList.Add(limitOrderCount);
                _EAConfig.EA.Add(limitOrderCount.code, limitOrderCount);

                //检查单个交易日订单总量,超过后修改参数[maxDiffChangeDuration=2]
                StrategyEA smoothChange = new SmoothChange();
                smoothChange.code = "SMOOTH_CHANGE";
                smoothChange.name = "指数平滑变化";
                smoothChange.symbol = "";
                smoothChange.value = "";
                smoothChange.type = "";
                smoothChange.active = _StrategyConfig.limitOrder;
                smoothChange.configParam.Add("analysisDataType", _StrategyConfig.analysisDataType);
                smoothChange.configParam.Add("maxChangeDiff", _StrategyConfig.maxChangeDiff.ToString());
                smoothChange.configParam.Add("indexDiffChangeCount", _StrategyConfig.indexDiffChangeCount.ToString());
                _EAConfig.ParaEAList.Add(smoothChange);
                _EAConfig.EA.Add(smoothChange.code, smoothChange);


                //判断趋势EMA
                StrategyEA trendEMA = new TrendEMA();
                trendEMA.code = "TREND_EMA";
                trendEMA.name = "判断趋势EMA";
                trendEMA.symbol = "";
                trendEMA.value = "";
                trendEMA.type = "";
                trendEMA.active = true;
                trendEMA.configParam.Add("EMAATRPeriod", _Config.SysConfig["dukascopy.ema.period"]);
                trendEMA.configParam.Add("EMAPeriod", _Config.SysConfig["dukascopy.emaPeriod"]);
                trendEMA.configParam.Add("ATRPeriod", _Config.SysConfig["dukascopy.atrPeriod"]);
                trendEMA.configParam.Add("TrendCount", _Config.SysConfig["dukascopy.trendCount"]);
                _EAConfig.EAList.Add(trendEMA);
                _EAConfig.EA.Add(trendEMA.code, trendEMA);

                //判断趋势KDJ
                StrategyEA trendKDJ = new TrendKDJ();
                trendKDJ.code = "TREND_KDJ";
                trendKDJ.name = "判断趋势KDJ";
                trendKDJ.symbol = "";
                trendKDJ.value = "";
                trendKDJ.type = "";
                trendKDJ.active = true;
                trendKDJ.configParam.Add("EMAATRPeriod", _Config.SysConfig["dukascopy.ema.period"]);
                trendKDJ.configParam.Add("EMAPeriod", _Config.SysConfig["dukascopy.emaPeriod"]);
                trendKDJ.configParam.Add("ATRPeriod", _Config.SysConfig["dukascopy.atrPeriod"]);
                trendKDJ.configParam.Add("TrendCount", _Config.SysConfig["dukascopy.trendCount"]);
                _EAConfig.EAList.Add(trendKDJ);
                _EAConfig.EA.Add(trendKDJ.code, trendKDJ);

                // Inside Bar + 突破逻辑（支持返回买/卖信号）
                StrategyEA KStrategy = new KStrategy();
                KStrategy.code = "K_EA";
                KStrategy.name = "K EA";
                KStrategy.symbol = "";
                KStrategy.value = "";
                KStrategy.type = "";
                KStrategy.active = true;
                KStrategy.configParam.Add("EMAATRPeriod", _Config.SysConfig["dukascopy.ema.period"]);
                KStrategy.configParam.Add("KPeriod", _Config.SysConfig["dukascopy.k.period"]);
                KStrategy.configParam.Add("EMAPeriod", _Config.SysConfig["dukascopy.emaPeriod"]);
                KStrategy.configParam.Add("ATRPeriod", _Config.SysConfig["dukascopy.atrPeriod"]);
                KStrategy.configParam.Add("Oversold_Level", _Config.SysConfig["KDJ.Oversold_Level"]);
                KStrategy.configParam.Add("Overbought_Level", _Config.SysConfig["KDJ.Overbought_Level"]);
                KStrategy.configParam.Add("TrendCount", "1");
                _EAConfig.EAList.Add(KStrategy);
                _EAConfig.EA.Add(KStrategy.code, KStrategy);

                // EMA趋势开仓
                StrategyEA emaOpen = new EMAOpen();
                emaOpen.code = "OPEN_EMA";
                emaOpen.name = "EMA 趋势开仓";
                emaOpen.symbol = "";
                emaOpen.value = "";
                emaOpen.type = "";
                emaOpen.active = true;
                emaOpen.configParam.Add("EMAATRPeriod", _Config.SysConfig["dukascopy.ema.period"]);
                emaOpen.configParam.Add("KPeriod", _Config.SysConfig["dukascopy.k.period"]);
                emaOpen.configParam.Add("EMAPeriod", _Config.SysConfig["dukascopy.emaPeriod"]);
                emaOpen.configParam.Add("ATRPeriod", _Config.SysConfig["dukascopy.atrPeriod"]);
                emaOpen.configParam.Add("TrendCount", "1");
                _EAConfig.EAList.Add(emaOpen);
                _EAConfig.EA.Add(emaOpen.code, emaOpen);

                //EMA趋势平仓
                StrategyEA emaClose = new EMAClose();
                emaClose.code = "CLOSE_EMA";
                emaClose.name = "EMA 趋势反转平仓";
                emaClose.symbol = "";
                emaClose.value = "";
                emaClose.type = "";
                emaClose.active = true;
                emaClose.configParam.Add("EMAATRPeriod", _Config.SysConfig["dukascopy.ema.period"]);
                emaClose.configParam.Add("EMAPeriod", _Config.SysConfig["dukascopy.emaPeriod"]);
                emaClose.configParam.Add("ATRPeriod", _Config.SysConfig["dukascopy.atrPeriod"]);
                emaClose.configParam.Add("TrendCount", "1");
                _EAConfig.EAList.Add(emaClose);
                _EAConfig.EA.Add(emaClose.code, emaClose);

                //AI 推理开仓
                StrategyEA aiKLineDetector = new AIKLineDetector();
                aiKLineDetector.code = "OPEN_AI_KLINE";
                aiKLineDetector.name = "AI推理判断趋势";
                aiKLineDetector.symbol = "";
                aiKLineDetector.value = "";
                aiKLineDetector.type = "";
                aiKLineDetector.active = true;
                aiKLineDetector.configParam.Add("KPeriod", _Config.SysConfig["dukascopy.k.period"]);
                _EAConfig.EAList.Add(aiKLineDetector);
                _EAConfig.EA.Add(aiKLineDetector.code, aiKLineDetector);

                //指定时间内随机时间平仓
                StrategyEA closeInTimeDuration = new CloseInTimeDuration();
                closeInTimeDuration.code = "CLOSE_IN_TIMEDURATION";
                closeInTimeDuration.name = "指定时间内随机时间平仓";
                closeInTimeDuration.symbol = "";
                closeInTimeDuration.value = "";
                closeInTimeDuration.type = "";
                closeInTimeDuration.active = true;
                _EAConfig.EAList.Add(closeInTimeDuration);
                _EAConfig.EA.Add(closeInTimeDuration.code, closeInTimeDuration);

                //H 策略
                StrategyEA hStrategy = new HStrategy();
                hStrategy.code = "H_EA";
                hStrategy.name = "H策略";
                hStrategy.symbol = "";
                hStrategy.value = "";
                hStrategy.type = "";
                hStrategy.active = true;
                hStrategy.configParam.Add("TrendCount", _Config.SysConfig["dukascopy.trendCount"]);
                hStrategy.configParam.Add("EMAPeriod", _Config.SysConfig["dukascopy.emaPeriod"]);
                hStrategy.configParam.Add("Oversold_Level", _Config.SysConfig["KDJ.Oversold_Level"]);
                hStrategy.configParam.Add("Overbought_Level", _Config.SysConfig["KDJ.Overbought_Level"]);
                _EAConfig.EAList.Add(hStrategy);
                _EAConfig.EA.Add(hStrategy.code, hStrategy);


                //KDJ 指标判断
                StrategyEA KDJOpen = new KDJOpen();
                KDJOpen.code = "OPEN_KDJ";
                KDJOpen.name = "KDJ 开仓";
                KDJOpen.symbol = "";
                KDJOpen.value = "";
                KDJOpen.type = "";
                KDJOpen.active = true;
                KDJOpen.configParam.Add("TrendCount", _Config.SysConfig["dukascopy.trendCount"]);
                KDJOpen.configParam.Add("EMAPeriod", _Config.SysConfig["dukascopy.emaPeriod"]);
                KDJOpen.configParam.Add("Oversold_Level", _Config.SysConfig["KDJ.Oversold_Level"]);
                KDJOpen.configParam.Add("Overbought_Level", _Config.SysConfig["KDJ.Overbought_Level"]);
                _EAConfig.EAList.Add(KDJOpen);
                _EAConfig.EA.Add(KDJOpen.code, KDJOpen);

                //KDJ 指标判断
                StrategyEA kDJClose = new KDJClose();
                kDJClose.code = "CLOSE_KDJ";
                kDJClose.name = "KDJ 平仓";
                kDJClose.symbol = "";
                kDJClose.value = "";
                kDJClose.type = "";
                kDJClose.active = true;
                kDJClose.configParam.Add("EMAATRPeriod", _Config.SysConfig["dukascopy.ema.period"]);
                kDJClose.configParam.Add("KPeriod", _Config.SysConfig["dukascopy.k.period"]);
                kDJClose.configParam.Add("EMAPeriod", _Config.SysConfig["dukascopy.emaPeriod"]);
                kDJClose.configParam.Add("ATRPeriod", _Config.SysConfig["dukascopy.atrPeriod"]);
                kDJClose.configParam.Add("Oversold_Level", _Config.SysConfig["KDJ.Oversold_Level"]);
                kDJClose.configParam.Add("Overbought_Level", _Config.SysConfig["KDJ.Overbought_Level"]);
                kDJClose.configParam.Add("TrendCount", "1");
                _EAConfig.EAList.Add(kDJClose);
                _EAConfig.EA.Add(kDJClose.code, kDJClose);

                // EMA趋势开仓
                StrategyEA maPriceOpen = new MAPriceOpen();
                maPriceOpen.code = "OPEN_MA_PRICE";
                maPriceOpen.name = "EMA均线价格偏差";
                maPriceOpen.symbol = "";
                maPriceOpen.value = "";
                maPriceOpen.type = "";
                maPriceOpen.active = true;
                maPriceOpen.configParam.Add("EMAATRPeriod", _Config.SysConfig["dukascopy.ema.period"]);
                maPriceOpen.configParam.Add("KPeriod", _Config.SysConfig["dukascopy.k.period"]);
                maPriceOpen.configParam.Add("EMAPeriod", _Config.SysConfig["dukascopy.emaPeriod"]);
                maPriceOpen.configParam.Add("ATRPeriod", _Config.SysConfig["dukascopy.atrPeriod"]);
                maPriceOpen.configParam.Add("EMAPriceDiff", "5");
                _EAConfig.EAList.Add(maPriceOpen);
                _EAConfig.EA.Add(maPriceOpen.code, maPriceOpen);

                // 仓位管理
                StrategyEA managePosition = new ManagePosition();
                managePosition.code = "MANAGE_POSITION";
                managePosition.name = "仓位管理，包含开仓，平仓管理";
                managePosition.symbol = "";
                managePosition.value = "";
                managePosition.type = "";
                managePosition.active = true;
                managePosition.configParam.Add("EMAATRPeriod", _Config.SysConfig["dukascopy.ema.period"]);
                managePosition.configParam.Add("KPeriod", _Config.SysConfig["dukascopy.k.period"]);
                managePosition.configParam.Add("EMAPeriod", _Config.SysConfig["dukascopy.emaPeriod"]);
                managePosition.configParam.Add("ATRPeriod", _Config.SysConfig["dukascopy.atrPeriod"]);
                managePosition.configParam.Add("TrendCount", _Config.SysConfig["dukascopy.trendCount"]);
                managePosition.configParam.Add("EMAPriceDiff", "5");
                managePosition.configParam.Add("keepProfit", _StrategyConfig.keepProfit.ToString());
                managePosition.configParam.Add("stopLoss", _StrategyConfig.stopLoss.ToString());
                _EAConfig.EAList.Add(managePosition);
                _EAConfig.EA.Add(managePosition.code, managePosition);

                // 马丁策略
                StrategyEA mStrategy = new MStrategy();
                mStrategy.code = "M_EA";
                mStrategy.name = "马丁策略(Martingale Strategy)";
                mStrategy.symbol = "";
                mStrategy.value = "";
                mStrategy.type = "";
                mStrategy.active = true;
                mStrategy.configParam.Add("stopLoss", _StrategyConfig.stopLoss.ToString());
                mStrategy.configParam.Add("NotradeDuration", _StrategyConfig.NotradeDuration);
                _EAConfig.EAList.Add(mStrategy);
                _EAConfig.EA.Add(mStrategy.code, mStrategy);
            }
        }
    }
}
