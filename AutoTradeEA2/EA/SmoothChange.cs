using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uClient.Broker
{
    /// <summary>
    /// SMOOTH_CHANGE
    /// 参数:indexDiffChangeCount
    /// 如果连续3个指数变动小于maxChangeDiff,就修改参数[maxDiffChangeDuration=1]
    /// 返回true,指数变化平滑
    /// </summary>
    public class SmoothChange : StrategyEA
    {
        private StringBuilder desc = new StringBuilder();
        public override bool Execute(EAChart eaChart, IDictionary<string, object> context)
        {
            desc.Clear();
            string analysisDataType = this.configParam["analysisDataType"];
            int indexDiffChangeCount = Convert.ToInt32(this.configParam["indexDiffChangeCount"]);
            double maxChangeDiff = Convert.ToDouble(this.configParam["maxChangeDiff"]);

            desc.AppendFormat("动态参数[analysisDataType{0}][indexDiffChangeCount{1}][maxChangeDiff{2}];", analysisDataType, indexDiffChangeCount, maxChangeDiff);
            
            if (analysisDataType.Contains("S"))
            {
                if (eaChart.latestGoldSumValueList.Count < indexDiffChangeCount)
                {
                    string indexStr = string.Join(", ", eaChart.latestGoldSumValueList);
                    desc.AppendFormat("策略名称[{0}][SUM指数{1}长度小于{2}],不执行该策略;", this.name, indexStr, indexDiffChangeCount);
                    return false;
                }
                else
                {
                    // 获取最后 n 个数据
                    double[] lastNElements = eaChart.latestGoldSumValueList.Skip(eaChart.latestGoldSumValueList.Count - indexDiffChangeCount).ToArray();
                    bool conditionMet = (lastNElements.All(x => x < 50)) ||(lastNElements.All(x => x > 50));
                    string indexStr = string.Join(", ", lastNElements);
                    if (conditionMet)
                    {
                        // 计算数组中的最大值和最小值
                        double max = lastNElements.Max();
                        double min = lastNElements.Min();
                        // 计算最大值和最小值之间的差值
                        double difference = max - min;
                        if (difference <= maxChangeDiff)
                        {
                            desc.AppendFormat("策略名称[{0}][SUM指数{1}]平均变化值{2}小于最大变化值{3}],执行该策略;", this.name, indexStr, difference, maxChangeDiff);
                            return true;
                        }
                        else
                        {
                            desc.AppendFormat("策略名称[{0}][SUM指数{1}]平均变化值{2}大于最大变化值{3}],不执行该策略;", this.name, indexStr, difference, maxChangeDiff);
                            return false;
                        }
                        
                    }
                    else
                    {
                        desc.AppendFormat("策略名称[{0}][SUM指数{1}]跨越50,不执行该策略;", this.name, indexStr);
                        return false;
                    }                   
                }
            }
            if (eaChart.latestGoldDailyValueList.Count < indexDiffChangeCount)
            {
                string indexStr = string.Join(", ", eaChart.latestGoldDailyValueList);
                desc.AppendFormat("策略名称[{0}][DAILY指数{1}长度小于{2}],不执行该策略;", this.name, indexStr, indexDiffChangeCount);
                return false;
            }
            else
            {
                // 获取最后 n 个数据
                double[] lastNElements = eaChart.latestGoldDailyValueList.Skip(eaChart.latestGoldDailyValueList.Count - indexDiffChangeCount).ToArray();
                bool conditionMet = (lastNElements.All(x => x < 50)) || (lastNElements.All(x => x > 50));
                string indexStr = string.Join(", ", lastNElements);
                if (conditionMet)
                {
                    // 计算数组中的最大值和最小值
                    double max = lastNElements.Max();
                    double min = lastNElements.Min();
                    // 计算最大值和最小值之间的差值
                    double difference = max - min;
                    if (difference <= maxChangeDiff)
                    {
                        desc.AppendFormat("策略名称[{0}][DAILY指数{1}]平均变化值{2}小于最大变化值{3}],执行该策略;", this.name, indexStr, difference, maxChangeDiff);
                        return true;
                    }
                    else
                    {
                        desc.AppendFormat("策略名称[{0}][DAILY指数{1}]平均变化值{2}大于最大变化值{3}],不执行该策略;", this.name, indexStr, difference, maxChangeDiff);
                        return false;
                    }

                }
                else
                {
                    desc.AppendFormat("策略名称[{0}][DAILY指数{1}]跨越50,不执行该策略;", this.name, indexStr);
                    return false;
                }
            }
        }

        public override string ToDesc()
        {
            return desc.ToString();
        }
    }
}
