using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uClient.Broker
{
    public class EAEntity
    {
        /// <summary>
        /// 策略配置数据,BUY,SELL,CLOSE_BUY,CLOSE_SELL
        /// </summary>
        public string code { set; get; }
        /// <summary>
        /// 名称
        /// </summary>
        public string name { set; get; }
        /// <summary>
        /// 汇总
        /// </summary>
        public double sum { set; get; }
        /// <summary>
        /// 汇总
        /// </summary>
        public double sum1 { set; get; }
        /// <summary>
        /// 近1天
        /// </summary>
        public double daily { set; get; }
        /// <summary>
        /// 近1天
        /// </summary>
        public double daily1 { set; get; }
        /// <summary>
        /// 分时
        /// </summary>
        public double hourly { set; get; }
        /// <summary>
        /// 分时
        /// </summary>
        public double hourly1 { set; get; }
        /// <summary>
        /// 分价
        /// </summary>
        public double price { set; get; }
        /// <summary>
        /// 分价
        /// </summary>
        public double price1 { set; get; }
        /// <summary>
        /// 黄金白银变化率平仓
        /// </summary>
        public double gs { set; get; }
        /// <summary>
        /// 黄金白银变化率开仓
        /// </summary>
        public double gs1 { set; get; }
        /// <summary>
        /// 分笔
        /// </summary>
        public double onetrade { set; get; }
        /// <summary>
        /// 分笔
        /// </summary>
        public double onetrade1 { set; get; }

        /// <summary>
        /// 不执行比例
        /// </summary>
        //public double noTrade { set; get; }

        /// <summary>
        /// 启用状态
        /// </summary>
        public bool active { set; get; }
        public override string ToString()
        {
            return "code="+ code+","+ "name=" + name + "," + "sum=" + sum + "," + "sum1=" + sum1 + "," + "daily=" + daily + "," + "daily1=" + daily1 + "," + "hourly=" + hourly + "," + "hourly1=" + hourly1 + "," + "price=" + price + "," + "price1=" + price1 + "," + "gs=" + gs + "," + "gs1=" + gs1 + ",";
        }
    }
}
