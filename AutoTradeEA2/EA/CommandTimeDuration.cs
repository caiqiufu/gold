using System;
using System.Collections.Generic;
using System.Text;


namespace uClient.Broker
{
    /// <summary>
    /// COMMAND_TIME_DURATION
    /// 开仓时间和上次平仓时间间隔检查,时间间隔可配置,需要检查是否有下一次指数更新后才执行指令, 配置参数:commandTimeDuration
    /// 返回true,已大于时间间隔,满足开单要求
    /// </summary>
    public class CommandTimeDuration : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();


            //最新的数据
            double daily = Math.Round(eaChart.latestGoldDailyValue, 2);

            //前一个情绪指数,用于判断情绪指数是否变化
            double timeDurationGoldDailyValue = 0;
            if (context.ContainsKey("timeDurationGoldDailyValue"))
            {
                timeDurationGoldDailyValue = Convert.ToDouble(context["timeDurationGoldDailyValue"]);
            }

            string currentOrderType = context["CurrentOrderType"].ToString();
            string CurrentOrderDetail = context["CurrentOrderDetail"].ToString();
            string CommandCreateTime = context["CommandCreateTime"].ToString();
            string currentDateTimeStr = context["CurrentDateTime"].ToString();
            string analysisDataType = context["analysisDataType"].ToString();


            desc.AppendFormat("动态参数[CurrentOrderDetail{0}][CommandCreateTime{1}][CurrentDateTime{2}][currentOrderType{3}][commandTimeDuration{4}][timeDurationGoldDailyValue{5}];", CurrentOrderDetail, CommandCreateTime, currentDateTimeStr, currentOrderType, this.value, timeDurationGoldDailyValue);

            if (string.IsNullOrEmpty(currentOrderType))
            {
                desc.AppendFormat("策略名称[{0}]无开单,执行该策略;", this.name);
                return true;
            }
            if (string.IsNullOrEmpty(CommandCreateTime))
            {
                desc.AppendFormat("策略名称[{0}]未有指令发送时间,执行该策略;", this.name);
                return true;
            }
            //如果当前是平仓单,对新开仓单需要时间间隔校验
            if (string.Equals(currentOrderType, "CLOSE_BUY") || string.Equals(currentOrderType, "CLOSE_SELL"))
            {                           
                double commandTimeDuration = Convert.ToDouble(this.value);
                DateTime commandTime = DateTime.Parse(CommandCreateTime);
                DateTime currentTime = DateTime.Parse(currentDateTimeStr);

                //TimeSpan timeDifference = DateTime.Now - commandTime;
                TimeSpan timeDifference = currentTime - commandTime;
                double diffMin = Math.Round(timeDifference.TotalMinutes, 2);
                if (string.Equals(analysisDataType, "D"))
                {
                    if (diffMin >= commandTimeDuration)
                    {
                        if (timeDurationGoldDailyValue == 0)
                        {
                            timeDurationGoldDailyValue = daily;
                            context["timeDurationGoldDailyValue"] = daily.ToString();
                            desc.AppendFormat("策略名称[{0}]指令间隔时间为[{1}]Min,大于策略值[{2}]Min,指数[timeDurationGoldDailyValue{3}]为零,不执行该策略;", this.name, diffMin, commandTimeDuration, timeDurationGoldDailyValue);
                            return false;
                        }
                        else
                        {
                            if (timeDurationGoldDailyValue != daily)
                            {
                                //不相等表示指数已更新
                                desc.AppendFormat("策略名称[{0}]指令间隔时间为[{1}]Min,大于策略值[{2}]Min,指数[timeDurationGoldDailyValue{3}和latestGoldDailyValue{4}]不相等说明指数已更新,执行该策略;", this.name, diffMin, commandTimeDuration, timeDurationGoldDailyValue, daily);
                                return true;
                            }
                            else
                            {
                                //相等表示指数已更新
                                desc.AppendFormat("策略名称[{0}]指令间隔时间为[{1}]Min,大于策略值[{2}]Min,指数[timeDurationGoldDailyValue{3}和latestGoldDailyValue{4}]相等说明指数未更新,不执行该策略;", this.name, diffMin, commandTimeDuration, timeDurationGoldDailyValue, daily);
                                return false;
                            }
                        }
                    }
                }
                else
                {
                    if (diffMin > commandTimeDuration)
                    {
                        desc.AppendFormat("策略名称[{0}]指令间隔时间为[{1}]Min,大于策略值[{2}]Min,执行该策略;", this.name, diffMin, commandTimeDuration);
                        return true;
                    }
                    else
                    {
                        desc.AppendFormat("策略名称[{0}]指令间隔时间为[{1}]Min,小于策略值[{2}]Min,不执行该策略;", this.name, diffMin, commandTimeDuration);
                        return false;
                    }
                }
                desc.AppendFormat("策略名称[{0}]当前订单为[{1}],不执行该策略;", this.name, currentOrderType);
                return false;
            }
            else
            {
                desc.AppendFormat($"策略名称[{this.name}]已有订单[{currentOrderType}],不执行该策略;");
                return false;
            }
        }

        public override string ToDesc()
        {
            return desc.ToString();
        }
    }
}
