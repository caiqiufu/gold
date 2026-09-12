using System;
using System.Collections.Generic;

namespace uClient.Comm
{
    /// <summary>
    /// 总体原则,同一时间只能有一张单子
    /// </summary>
    public class StrategyConfig
    {
        /// <summary>
        /// 当前指令生成的订单类型,BUY,CLOSE_BUY,P_CLOSE_BUY,SELL,CLOSE_SELL,P_CLOSE_SELL
        /// 后续新增加的类型,未放入到全局变量中,需要结合CurrentOrderTypeDetail参数进行判断,IN_SELL,IN_BUY,EVENT_BUY,EVENT_SELL,EVENT_CLOSE_BUY,EVENT_CLOSE_SELL
        /// </summary>
        public string CurrentOrderType { set; get; }

        /// <summary>
        /// 当前的订单类型明细
        /// BUY 正常策略开仓
        /// SELL 正常策略平仓
        /// TREND_BUY 趋势开仓
        /// TREND_SELL 趋势开仓
        /// IN_BUY 亏损加仓
        /// IN_SELL 亏损加仓
        /// CLOSE_BUY 正常策略平仓
        /// CLOSE_SELL 正常策略平仓
        /// CLOSE_BUY_STOP_LOSS  止损买平仓
        /// CLOSE_SELL_STOP_LOSS 止损卖平仓
        /// CLOSE_BUY_TAKE_PROFIT 止盈买平仓
        /// CLOSE_SELL_TAKE_PROFIT 止盈卖平仓
        /// CLOSE_KEEP_BUY_PROFIT 动态止盈买平仓
        /// CLOSE_KEEP_SELL_PROFIT 动态止盈卖平仓
        /// CLOSE_KEEP_BUY_LOSS 动态止损买平仓
        /// CLOSE_KEEP_SELL_LOSS 动态止损卖平仓
        /// CLOSE_BUY_TRENDORDER 趋势单平仓
        /// CLOSE_SELL_TRENDORDER 趋势单平仓
        /// EVENT_BUY 事件开仓
        /// EVENT_SELL 事件开仓
        /// CLOSE_EVENT_BUY 事件平仓
        /// CLOSE_EVENT_SELL  事件平仓
        /// EVENT_LOCK_BUY 事件锁仓
        /// EVENT_LOCK_SELL 事件锁仓
        /// EVENT_CLOSE 事件平仓
        /// BUY_GS 金银变化率平仓买入黄金
        /// SELL_GS 金银变化率开仓卖出黄金
        /// CLOSE_BUY_GS 金银变化率平仓
        /// CLOSE_SELL_GS 金银变化率平仓
        /// </summary>
        public string CurrentOrderTypeDetail { set; get; }

        /// <summary>
        /// 当前生成的订单明细
        /// </summary>
        public string CurrentOrderDetail { set; get; }

        /// <summary>
        /// 当前价订单价格
        /// </summary>
        public double CurrentPrice { set; get; }

        /// <summary>
        /// 最新黄金价格
        /// </summary>
        public double GOLD_PRICE { set; get; }

        /// <summary>
        /// 最新白银价格
        /// </summary>
        public double SILVER_PRICE { set; get; }

        /// <summary>
        /// 策略情绪指数类型JR/WZ/...
        /// </summary>
        public string DSType { set; get; }

        /// <summary>
        /// 策略端非交易时间
        /// </summary>
        public string NotradeDuration { set; get; }


        /// <summary>
        /// 发送通知
        /// </summary>
        public bool NotifyFlag { set; get; }

        /// <summary>
        /// 回测时间段
        /// </summary>
        public string TestDuration { set; get; }

        /// <summary>
        /// 选中策略
        /// </summary>
        public string SelectedStrategy { set; get; }

        public bool LoadDsData { set; get; }

        /// <summary>
        /// 全局变量,上下文数据
        /// </summary>
        public IDictionary<string, Object> Context = new Dictionary<String, Object>();

        /// <summary>
        /// 指令生成时间
        /// </summary>
        public string CommandCreateTime { set; get; }

        /// <summary>
        /// 分析数据时间区间
        /// </summary>
        public double timeDuration { set; get; }
        /// <summary>
        /// 斜率
        /// </summary>
        public double slope;
        /// <summary>
        /// 仓位比例
        /// </summary>
        public bool lotProportion;
        /// <summary>
        /// 止盈点数
        /// </summary>

        public double takeProfit;
        /// <summary>
        /// 止损点数
        /// </summary>

        public double stopLoss;

        /// <summary>
        /// 动态止盈设置点数,默认最小值为5
        /// </summary>

        public double keepProfit;
        /// <summary>
        /// 动态止损点数,超过动态止损后,就启动动态止损
        /// </summary>
        public double dynamicLost;

        /// <summary>
        /// 动态止盈点数，超过动态止盈点数后，就启动动态止盈
        /// </summary>
        public double dynamicProfit;

        /// <summary>
        /// 是否越过动态止盈点
        /// </summary>
        public bool overKeepProfit;

        /// <summary>
        /// 动态止盈回撤值,超过该回撤值后执行平仓指令
        /// </summary>
        public double keepProfitDiff;

        /// <summary>
        /// 第二是否越过动态止盈点
        /// </summary>
        public bool overKeepProfit2;
        /// <summary>
        /// 策略配置权重值，如买入策略 (sumOpen - sumClose)*0.4 + keepProfit*0.6 
        /// </summary>

        public double weightValue;



        /// <summary>
        /// 动态止盈执行比例
        /// </summary>

        public double ratioValue;

        /// <summary>
        /// 开仓情绪差值，在止盈、止损、动态止盈后，需要调整情绪差值后才能重新开仓
        /// </summary>

        public double openDiff;

        /// <summary>
        /// 情绪指数变化最小值
        /// </summary>

        public double minChangeDiff;

        /// <summary>
        /// 情绪指数变化最大值
        /// </summary>
        public double maxChangeDiff;

        /// <summary>
        /// 情绪指数变化最大值的时间区间,有多少个指数需要进行分析
        /// </summary>
        public double maxDiffChangeDuration;
        /// <summary>
        /// 分析数据类型 D,H,K
        /// </summary>
        public string analysisDataType = "D";

        /// <summary>
        /// 分笔时间段
        /// </summary>
        public int onetradeDuration;

        /// <summary>
        /// 趋势单开仓值
        /// </summary>

        public double trendOrder;
        /// <summary>
        /// 趋势单平仓值
        /// </summary>

        public double trendOrder1;

        /// <summary>
        /// 是否连续开单，指在上一个指令完成后，继续判断下一个指令，不要有情绪指数变化的判断
        /// </summary>
        public bool continuousOrder;
        /// <summary>
        /// 反向单，根据目前指数反向开单，趋势单不反向开单
        /// </summary>
        public bool reverseProportion;
        /// <summary>
        /// 采用指数平仓,否则只能其他方式平仓
        /// </summary>
        public bool indexClose;

        /// <summary>
        /// 是否限制单量
        /// 1) 如果一天单量超过指定单量,就修改参数[maxDiffChangeDuration=2]
        /// 2) 如果连续3个指数变动小于maxChangeDiff,就修改参数[maxDiffChangeDuration=1]
        /// </summary>
        public bool limitOrder;

        /// <summary>
        /// 交易日最大订单数
        /// </summary>
        public int maxOrderCount = 20;
        /// <summary>
        /// 周最大亏损值
        /// </summary>
        public double maxLossPoint = 35;

        /// <summary>
        /// 连续亏损单数
        /// </summary>
        public int lossOrderContinuousCount = 5;

        /// <summary>
        /// 周总亏损单数
        /// </summary>
        public int lossOrderTotalCount = 10;

        /// <summary>
        /// 判断指数是否平滑时共计需要判断的指数点数
        /// </summary>
        public int indexDiffChangeCount = 3;

        /// <summary>
        /// 开平仓最新的SUM值
        /// </summary>
        public double latestGoldSumValue;
        /// <summary>
        /// 开平仓最新的Hourly值
        /// </summary>
        public double latestGoldHourlyValue;
        /// <summary>
        /// 开平仓最新Daily值
        /// </summary>
        public double latestGoldDailyValue;

        /// <summary>
        /// 最新黄金白银变化率最新值
        /// </summary>
        public double latestGSValue;



        /// <summary>
        /// 最新黄金白银变化率斜率
        /// 1: 黄金斜率比白银大, 黄金卖出,白银买入
        /// 0: 黄金斜率比白银小,黄金买入,白银卖出
        /// </summary>
        public string lastGSSlopeValue;

        /// <summary>
        /// 最新黄金白银开仓比例,以1手黄金为基准单位
        /// </summary>
        public string lastGSLotRatioValue;

        /// <summary>
        /// GS策略最大盈利
        /// </summary>
        public double gsMaxProfitValue;
        /// <summary>
        /// GS策略最大亏损
        /// </summary>
        public double gsMaxLossValue;

        /// <summary>
        /// 时间间隔后的Sum最新指数值
        /// </summary>
        public double timeDurationGoldSumValue;
        /// <summary>
        /// 时间间隔后的Hourly最新指数值
        /// </summary>
        public double timeDurationGoldHourlyValue;
        /// <summary>
        /// 时间间隔后的Daily最新指数值
        /// </summary>
        public double timeDurationGoldDailyValue;
        /// <summary>
        /// 时间间隔后的GS最新指数值
        /// </summary>
        public double timeDurationGSValue;

        /// <summary>
        /// 加仓点数
        /// </summary>
        public double increasePositionPoint;

        /// <summary>
        /// 两个指令之间的间隔时间,避免在数据更新前后段时间内开平仓
        /// </summary>
        public double commandTimeDuration;


        //回测参数
        public string retestNumDuration;
        public int openMax;
        public int openDuration;
        public int closeDuration;
        public int offset;
        public int hourlyOffset;


        //非农,CPI等数据行情
        /// <summary>
        /// 事件策略标识,启动该表示后,只分析事件策略
        /// </summary>
        public bool eventTradeFlag;

        /// <summary>
        /// 分析数据的时间区间(s),即在该时间段内如果跳次数大于总计跳次数,就立即执行策略
        /// </summary>
        public int dataTimeDuration = 5;

        /// <summary>
        /// 跳次大小
        /// </summary>
        public int dataPriceDiffSize = 3;
        /// <summary>
        /// 总计跳次个数
        /// </summary>
        public int dataPriceDiffTotalCount = 15;
        /// <summary>
        /// 正数跳次个数
        /// </summary>
        public int dataPricePositiveCount = 10;

        /// <summary>
        /// 仓位信息
        /// </summary>
        public TradePosition TradePositionInfo;

        /// <summary>
        /// 马丁策略加仓位1
        /// </summary>
        public bool MartingaleAddPosition1 = false;
        /// <summary>
        /// 马丁策略加仓位2
        /// </summary>
        public bool MartingaleAddPosition2 = false;
        /// <summary>
        /// 马丁策略加仓位3
        /// </summary>
        public bool MartingaleAddPosition3 = false;
    }
}
