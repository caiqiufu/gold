using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;
using uClient.Broker;

namespace uClient.Broker
{
    /// <summary>
    /// SICOLLECT_TIME_DURATION
    /// 情绪指标采集时间判断,由于金荣的情绪指数采集时间会不断变化,导致采集到的数据有延迟,影响策略执行,针对延迟采集到的数据,不执行策略
    /// 配置参数SICollectTimeDuration,单位是分钟
    /// 返回true,情绪指数采集时间符合开仓要求
    /// </summary>
    public class SICollectTimeDuration : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();
            string currentOrderType = context["CurrentOrderType"].ToString();
            string CurrentOrderDetail = context["CurrentOrderDetail"].ToString();
            string CommandCreateTime = context["CommandCreateTime"].ToString();
            string currentDateTimeStr = context["CurrentDateTime"].ToString();
            string analysisDataType = this.configParam["analysisDataType"];
            int SICollectTimeDuration = Convert.ToInt16(this.configParam["SICollectTimeDuration"]);

            string indexTimeStr = "";
            string yvaluesStr = "";
            if (analysisDataType.Contains("D"))
            {
                DataPointCollection points = eaChart.chart_daily.Series[0].Points;
                DataPoint dataPoint = points.LastOrDefault();
                if (dataPoint!=null) 
                {
                    indexTimeStr = dataPoint.AxisLabel;
                    double[] yvalues = dataPoint.YValues;
                    yvaluesStr = string.Join(", ", yvalues.Select(d => d.ToString()));
                }
                
            }

            desc.AppendFormat("动态参数[CurrentOrderDetail{0}][CommandCreateTime{1}][CurrentDateTime{2}][currentOrderType{3}][SICollectTimeDuration{4}][indexTime{5}][analysisDataType{6}];", CurrentOrderDetail, CommandCreateTime, currentDateTimeStr, currentOrderType, SICollectTimeDuration, indexTimeStr, analysisDataType);


            //如果当前是平仓单,对新开仓单时的情绪指数采集时间校验
            if (string.Equals(currentOrderType, "CLOSE_BUY") || string.Equals(currentOrderType, "CLOSE_SELL") || string.IsNullOrEmpty(currentOrderType))
            {
                // 获取当前时间
                DateTime currentTime = DateTime.Parse(currentDateTimeStr);
                if (SICollectTimeDuration > 10)
                {
                    // 获取当前时间的分钟部分
                    int currentMinute = currentTime.Minute;
                    if (currentMinute > SICollectTimeDuration)
                    {
                        desc.AppendFormat("策略名称[{0}]指数采集间隔时间为[{1}]Min,大于策略值[{2}]Min,不执行该策略;", this.name, currentMinute, SICollectTimeDuration);
                        return false;
                    }
                    else
                    {
                        desc.AppendFormat("策略名称[{0}]指数采集间隔时间为[{1}]Min,小于策略值[{2}]Min,执行该策略;", this.name, currentMinute, SICollectTimeDuration);
                    }
                }
                else
                {
                    desc.AppendFormat("策略名称[{0}]小于策略值[{1}]Min,不执行策略值检查,执行该策略;", this.name, SICollectTimeDuration);
                }
                if (!string.IsNullOrEmpty(indexTimeStr))
                {
                    DateTime indexTime = DateTime.Parse(indexTimeStr);
                    TimeSpan timeDifference = currentTime - indexTime;
                    double diffMin = Math.Round(timeDifference.TotalMinutes, 2);
                    if (diffMin > 10)
                    {
                        desc.AppendFormat("策略名称[{0}]指数采集时间和当前执行时间间隔[{1}]Min,大于策略值[{2}]Min,不执行该策略;", this.name, diffMin, "10");
                        return false;
                    }
                    else
                    {
                        desc.AppendFormat("策略名称[{0}]指数采集时间和当前执行时间间隔[{1}]Min,小于策略值[{2}]Min,执行该策略;", this.name, diffMin, "10");
                    }
                }
                else
                {
                    desc.AppendFormat("策略名称[{0}]未获取到情绪指数采集时间,不执行该策略;", this.name);
                    return false;
                }
                return true;
            }
            else
            {
                desc.AppendFormat("策略名称[{0}]当前订单为[{1}],执行该策略;", this.name, currentOrderType);
                return true;
            }           
        }

        public override string ToDesc()
        {
            return desc.ToString();
        }
    }
}
