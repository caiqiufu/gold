using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uClient.Broker;
using uClient.Comm;

namespace uClient.Broker
{
    /// <summary>
    /// MAX_DIFFCHANGE
    /// 最小和最大偏差值检查,情绪指数变化需要大于设置的最小变化值,或者未跨越50%后小于最大变化值,判断两个或者多个情绪指数变化,目的是避免情绪指数剧烈波动
    /// 参数:maxDiffChangeDuration(需要检查的情绪指数点数),maxChangeDiff(最大变化值)
    /// 任意一个偏差值不满足条件,都不执行策略,比如分时数据满足开仓条件,但是不满足偏差值,近一天满足偏差值条件,输出策略不满足条件
    /// </summary>
    public class MaxDiffChange : StrategyEA
    {
        
        //生成操作指令：{symbol}:{price}:{optType}:{lotProportion}
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();

            //double maxDiffChangeDuration = Convert.ToDouble(this.configParam["maxDiffChangeDuration"]);
            //该参数会被动态修改,需要从内存中读取
            double maxDiffChangeDuration = Convert.ToDouble(context["maxDiffChangeDuration"].ToString());
            double minChangeDiff = Convert.ToDouble(this.configParam["minChangeDiff"]);
            double maxChangeDiff = Convert.ToDouble(this.configParam["maxChangeDiff"]);  

            List<double> GoldSumValueList = eaChart.latestGoldSumValueList.ToList();
            List<double> GoldDailyValueList = eaChart.latestGoldDailyValueList.ToList();
            List<double> GoldHourlyValueList = eaChart.latestGoldHourlyValueList.ToList();


            desc.AppendFormat("动态参数[GoldSumValueList{0}][GoldDailyValueList{1}][GoldHourlyValueList{2}][maxDiffChangeDuration{3}][minChangeDiff{4}][maxChangeDiff{5}];", JsonConvert.SerializeObject(GoldSumValueList), JsonConvert.SerializeObject(GoldDailyValueList) , JsonConvert.SerializeObject(GoldHourlyValueList), maxDiffChangeDuration, minChangeDiff, maxChangeDiff);

            //是否满足策略标识
            bool flag = true;
            string diffs = "";

            if ( GoldDailyValueList.Count > maxDiffChangeDuration && maxDiffChangeDuration >= 1)
            {
                for (int i = 0; i < maxDiffChangeDuration; i++)
                {
                    double currentDaily = GoldDailyValueList[GoldDailyValueList.Count - 1];
                    double preDaily = GoldDailyValueList[GoldDailyValueList.Count - 2 - i];
                    double diff = Math.Round(Math.Abs(currentDaily - preDaily),2);
                    diffs = diffs + diff + ",";
                    //如果两个情绪指数跨越50%,则判断是否满足
                    if ((GoldDailyValueList[GoldDailyValueList.Count - 1] > 50.00 && GoldDailyValueList[GoldDailyValueList.Count - 2 - i] < 50.00) || (GoldDailyValueList[GoldDailyValueList.Count - 1] < 50.00 && GoldDailyValueList[GoldDailyValueList.Count - 2 - i] > 50.00))
                    {              
                        if (diff > maxChangeDiff || diff < minChangeDiff)
                        {
                            desc.AppendFormat("策略名称[DAILY{0}]策略值变化[{1}]大于[{2}]或者小于[{3}],当前参数值[{4}],待比较前值[{5}],不执行该策略;", this.name, diff, maxChangeDiff, minChangeDiff, currentDaily, preDaily);
                            //只要有一个条件不满足就返回false
                            flag = false;
                            break;
                        }
                    }
                    else
                    {
                        //如果都在同侧,只判断变化值是否大于最小值
                        if (diff < minChangeDiff)
                        {
                            desc.AppendFormat("策略名称[DAILY{0}]策略值变化[{1}]小于[{2}],当前参数值[{3}],待比较前值[{4}],不执行该策略;", this.name, diff, minChangeDiff, currentDaily, preDaily);
                            //只要有一个条件不满足就返回false
                            flag = false;
                            break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(diffs))
                {
                    diffs = diffs.Substring(0, diffs.Length - 1);
                }
                if (!flag)
                {
                    desc.AppendFormat("策略名称[DAILY{0}]策略值[{1}]变化大于[{2}]或者小于[{3}],不执行该策略;", this.name, diffs, maxChangeDiff, minChangeDiff);
                    return false;
                }
                else
                {
                    desc.AppendFormat("策略名称[DAILY{0}]策略值[{1}]变化在区间[{2}]-[{3}],执行该策略;", this.name, diffs, minChangeDiff, maxChangeDiff);
                }
            }
            return true;
        }

        public override string ToDesc()
        {
            return desc.ToString();
        }
    }
}
