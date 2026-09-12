using System;
using System.Collections.Generic;
using System.Text;

namespace uClient.Broker
{

    /// <summary>
    /// M_EA
    /// 马丁策略（Martingale Strategy）
    /// 趋势+KDJ入场(H/K 策略入场)
    /// 加仓层数3层
    /// 加仓点位:止损点60%,80%，90%
    /// </summary>
    public class MStrategy : StrategyEA
    {
        // ------------------- 策略常量参数 -------------------

        private StringBuilder _log = new StringBuilder();
        /// <summary>
        /// 返回策略的实时状态描述，供前端或 UI 显示
        /// </summary>
        public override string ToDesc() => _log.ToString();

        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {

            _log.Clear();

            string _IsSimulateTest = context["IsSimulateTest"].ToString();

            // 从上下文中提取必要数据
            //当前订单类型
            string currentOrderType = context.ContainsKey("CurrentOrderType") ? context["CurrentOrderType"].ToString() : "NONE";
            if (string.IsNullOrEmpty(currentOrderType) || string.Equals(currentOrderType, "CLOSE_BUY") || string.Equals(currentOrderType, "CLOSE_SELL"))
            {
                _log.AppendFormat($"策略名称[{this.name}]无开单,不执行该策略");
                return false;
            }
            //当前时间
            string currentDateTimeStr = context.ContainsKey("CurrentDateTime") ? context["CurrentDateTime"].ToString() : DateTime.Now.ToString();
            string notradeDuration = this.configParam["NotradeDuration"].ToString();
            //如果是周六就把结束时间提前15分钟
            // 1. 将字符串解析为 DateTime 对象
            if (DateTime.TryParse(currentDateTimeStr, out DateTime dt))
            {
                // 2. 判断是否为周六 (DayOfWeek.Saturday)
                if (dt.DayOfWeek == DayOfWeek.Saturday)
                {
                    // 3. 调用工具类修改结束时间
                    // 注意：提前 15 分钟要传 -15，传正 15 会变成延后
                    notradeDuration = Utils.UpdateStartTime(notradeDuration, -15);
                    _log.AppendFormat($"策略名称[{this.name}],周六时把设置时间范围提前15分钟[{notradeDuration}];");
                }
            }
            if (string.IsNullOrEmpty(notradeDuration))
            {
                _log.AppendFormat($"策略名称[{this.name}]未配置非执行时间,采用数据库默认配置时间;");
            }
            bool flag = Utils.checkIsTradeTime(notradeDuration, currentDateTimeStr);
            if (!flag)
            {
                _log.AppendFormat($"策略名称[{this.name}]不在设置时间范围[{notradeDuration}],不允许交易,不执行该策略;");
                return false;
            }
            //当前价格
            double quotePrice = Convert.ToDouble(context["price"]);
            //当前开仓价格
            double currentPrice = Convert.ToDouble(context["CurrentPrice"]);

            //配置的止损点数
            double stopLoss = Convert.ToDouble(this.configParam["stopLoss"]);
            //第一加仓状态true:已加仓
            bool MartingaleAddPosition1 = Convert.ToBoolean(context["MartingaleAddPosition1"]);
            //第二加仓状态true:已加仓
            bool MartingaleAddPosition2 = Convert.ToBoolean(context["MartingaleAddPosition2"]);
            //第三加仓状态true:已加仓
            bool MartingaleAddPosition3 = Convert.ToBoolean(context["MartingaleAddPosition3"]);
            //第一加仓点数
            double MartingaleAddPositionPrice1 = Math.Round(stopLoss * Convert.ToDouble(context["MartingaleAddPositionPrice1"]), 2);
            //第二加仓点数
            double MartingaleAddPositionPrice2 = Math.Round(stopLoss * Convert.ToDouble(context["MartingaleAddPositionPrice2"]), 2);
            //第三加仓点数
            double MartingaleAddPositionPrice3 = Math.Round(stopLoss * Convert.ToDouble(context["MartingaleAddPositionPrice3"]), 2);
            //当前亏损点数
            double lostPoint = 0;
            if (string.Equals("BUY", currentOrderType))
            {
                lostPoint = Math.Round(quotePrice - currentPrice, 2);
            }
            if (string.Equals("SELL", currentOrderType))
            {
                lostPoint = Math.Round(currentPrice - quotePrice, 2);
            }

            _log.AppendFormat($"动态参数[currentOrderType{currentOrderType}][quotePrice{quotePrice}][currentPrice{currentPrice}][lostPoint{lostPoint}][currentDateTimeStr{currentDateTimeStr}][stopLoss{stopLoss}][MartingaleAddPosition1{MartingaleAddPosition1}][MartingaleAddPosition2{MartingaleAddPosition2}][MartingaleAddPosition3{MartingaleAddPosition3}][MartingaleAddPositionPrice1{MartingaleAddPositionPrice1}][MartingaleAddPositionPrice2{MartingaleAddPositionPrice2}][MartingaleAddPositionPrice3{MartingaleAddPositionPrice3}];");

            if (lostPoint > 0)
            {
                _log.AppendFormat($"策略名称[{this.name}]当前盈利[{lostPoint}],不执行该策略");
                return false;
            }
            else if (-lostPoint <= MartingaleAddPositionPrice1)
            {
                _log.AppendFormat($"策略名称[{this.name}]当前亏损[{-lostPoint}]小于[{MartingaleAddPositionPrice1}],不执行该策略");
                return false;
            }
            else
            {
                if (!MartingaleAddPosition1)
                {
                    if (-lostPoint > MartingaleAddPositionPrice1 && -lostPoint < MartingaleAddPositionPrice2)
                    {
                        _log.AppendFormat($"策略名称[{this.name}]当前亏损[{-lostPoint}]大于第一加仓点数[{MartingaleAddPositionPrice1}],执行该策略");
                        context["MartingaleAddPosition1"] = true;
                        context["AddPositionLevel"] = 1;
                        context["AddPositionLot"] = context["MartingaleAddPositionLot1"];
                        return true;
                    }
                }
                if (!MartingaleAddPosition2)
                {
                    if (-lostPoint > MartingaleAddPositionPrice2 && -lostPoint < MartingaleAddPositionPrice3)
                    {
                        _log.AppendFormat($"策略名称[{this.name}]当前亏损[{-lostPoint}]大于第二加仓点数[{MartingaleAddPositionPrice2}],执行该策略");
                        context["MartingaleAddPosition2"] = true;
                        context["AddPositionLevel"] = 2;
                        context["AddPositionLot"] = context["MartingaleAddPositionLot2"];
                        return true;
                    }
                }
                if (!MartingaleAddPosition3)
                {
                    if (-lostPoint > MartingaleAddPositionPrice3 && -lostPoint < stopLoss)
                    {
                        _log.AppendFormat($"策略名称[{this.name}]当前亏损[{-lostPoint}]大于第三加仓点数[{MartingaleAddPositionPrice3}],执行该策略");
                        context["MartingaleAddPosition3"] = true;
                        context["AddPositionLevel"] = 3;
                        context["AddPositionLot"] = context["MartingaleAddPositionLot3"];
                        return true;
                    }
                }
                if (MartingaleAddPosition1 || MartingaleAddPosition2 || MartingaleAddPosition1)
                {
                    _log.AppendFormat($"策略名称[{this.name}]当前亏损[{-lostPoint}]已完成加仓,不执行该策略");
                    return false;
                }
                else
                {
                    _log.AppendFormat($"策略名称[{this.name}]当前亏损[{-lostPoint}]满足加仓,不执行该策略");
                    return false;
                }

            }
        }
    }
}
