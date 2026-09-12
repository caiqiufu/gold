using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace uClient.Comm
{
    /// <summary>
    /// HTTP 操作类
    /// </summary>
    public class HTTPHelper
    {
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 采集万州数据
        /// </summary>
        public static void WZSummaryData()
        {
            string DS_NAME = "WZ";
            string jsonString = HTTPUtils.SendWZRequest("all", "");
            if (string.IsNullOrEmpty(jsonString))
            {
                _logger.Error("WZSummaryData请求返回结果为空");
                return;
            }
            var jsonObject = JObject.Parse(jsonString);
            var data = jsonObject["data"];
            if (data != null)
            {
                string time = data["time"].ToString();
                string gold_buy_num = data["gold_buy_num"].ToString();
                string gold_sale_num = data["gold_sale_num"].ToString();
                string silver_buy_num = data["silver_buy_num"].ToString();
                string silver_sale_num = data["silver_sale_num"].ToString();

                var sqlCheckExistGold = new StringBuilder();
                sqlCheckExistGold.AppendFormat("select count(1) as num from trade_daily_sum where symbol = '{0}' and buy_valume ='{1}' and sell_valume = '{2}' and ds_name = '{3}' ", "gold", gold_buy_num, gold_sale_num, DS_NAME);

                int goldnum = Convert.ToInt16(DBUtils.query(sqlCheckExistGold.ToString())[0]["num"]);
                if (goldnum == 0)
                {
                    var insertSql = new StringBuilder();
                    insertSql.Append("insert into trade_daily_sum(symbol,pt,buy_valume,sell_valume,create_time,ds_name) values");

                    var insertSqlHistory = new StringBuilder();
                    insertSqlHistory.Append("insert into trade_daily_sum_history(symbol,pt,buy_valume,sell_valume,create_time,ds_name) values");

                    insertSql.AppendFormat("('{0}','{1}','{2}','{3}',{4},'{5}')", "gold", time, gold_buy_num, gold_sale_num, "sysdate()", DS_NAME);
                    insertSqlHistory.AppendFormat("('{0}','{1}','{2}','{3}',{4},'{5}')", "gold", time, gold_buy_num, gold_sale_num, "sysdate()", DS_NAME);
                    DBUtils.executeSql(insertSql.ToString());
                    DBUtils.executeSql(insertSqlHistory.ToString());
                    _logger.Debug("WZSummaryData 插入数据完成");
                }
                var sqlCheckExistSilver = new StringBuilder();
                sqlCheckExistSilver.AppendFormat("select count(1) as num from trade_daily_sum where symbol = '{0}' and buy_valume ='{1}' and sell_valume = '{2}' and ds_name = '{3}' ", "silver", silver_buy_num, silver_sale_num, DS_NAME);
                int silvernum = Convert.ToInt16(DBUtils.query(sqlCheckExistSilver.ToString())[0]["num"]);
                if (silvernum == 0)
                {
                    var insertSql = new StringBuilder();
                    insertSql.Append("insert into trade_daily_sum(symbol,pt,buy_valume,sell_valume,create_time,ds_name) values");

                    var insertSqlHistory = new StringBuilder();
                    insertSqlHistory.Append("insert into trade_daily_sum_history(symbol,pt,buy_valume,sell_valume,create_time,ds_name) values");


                    insertSql.AppendFormat("('{0}','{1}','{2}','{3}',{4},'{5}')", "silver", time, silver_buy_num, silver_sale_num, "sysdate()", DS_NAME);
                    insertSqlHistory.AppendFormat("('{0}','{1}','{2}','{3}',{4},'{5}')", "silver", time, silver_buy_num, silver_sale_num, "sysdate()", DS_NAME);
                    DBUtils.executeSql(insertSql.ToString());
                    DBUtils.executeSql(insertSqlHistory.ToString());
                    _logger.Debug("WZSummaryData 插入数据完成");
                }
            }
            else
            {
                _logger.Debug("WZSummaryData 返回结果无数据");
            }
        }
        /// <summary>
        /// 采集金荣数据
        /// </summary>
        public static void JRSummaryData()
        {
            string DS_NAME = "JR";
            string jsonString = HTTPUtils.SendJRRequest("day", "gold");
            if (string.IsNullOrEmpty(jsonString))
            {
                _logger.Error("JRSummaryData请求返回结果为空");
                return;
            }
            var jsonObject = JObject.Parse(jsonString);
            var data = jsonObject["data"];
            if (data != null)
            {
                string time = data["pt"].ToString();
                string gold_buy_num = data["jbp"].ToString();
                string gold_sale_num = data["jsp"].ToString();
                string silver_buy_num = data["ybp"].ToString();
                string silver_sale_num = data["ysp"].ToString();
                
                var sqlCheckExistGold = new StringBuilder();
                sqlCheckExistGold.AppendFormat("select count(1) as num from trade_daily_sum where symbol = '{0}' and buy_valume ='{1}' and sell_valume = '{2}' and ds_name = '{3}' ", "gold", gold_buy_num, gold_sale_num, DS_NAME);

                int goldnum = Convert.ToInt16(DBUtils.query(sqlCheckExistGold.ToString())[0]["num"]);
                if (goldnum == 0)
                {
                    var insertSql = new StringBuilder();
                    insertSql.Append("insert into trade_daily_sum(symbol,pt,buy_valume,sell_valume,create_time,ds_name) values");
                    var insertSqlHistory = new StringBuilder();
                    insertSqlHistory.Append("insert into trade_daily_sum_history(symbol,pt,buy_valume,sell_valume,create_time,ds_name) values");

                    insertSql.AppendFormat("('{0}','{1}','{2}','{3}',{4},'{5}')", "gold", time, gold_buy_num, gold_sale_num, "sysdate()", DS_NAME);
                    insertSqlHistory.AppendFormat("('{0}','{1}','{2}','{3}',{4},'{5}')", "gold", time, gold_buy_num, gold_sale_num, "sysdate()", DS_NAME);
                    DBUtils.executeSql(insertSql.ToString());
                    DBUtils.executeSql(insertSqlHistory.ToString());
                    _logger.Debug("JRSummaryData 插入数据完成");
                }
                var sqlCheckExistSilver = new StringBuilder();
                sqlCheckExistSilver.AppendFormat("select count(1) as num from trade_daily_sum where symbol = '{0}' and buy_valume ='{1}' and sell_valume = '{2}' and ds_name = '{3}' ", "silver", silver_buy_num, silver_sale_num, DS_NAME);
                int silvernum = Convert.ToInt16(DBUtils.query(sqlCheckExistSilver.ToString())[0]["num"]);
                if (silvernum == 0)
                {
                    var insertSql = new StringBuilder();
                    insertSql.Append("insert into trade_daily_sum(symbol,pt,buy_valume,sell_valume,create_time,ds_name) values");
                    var insertSqlHistory = new StringBuilder();
                    insertSqlHistory.Append("insert into trade_daily_sum_history(symbol,pt,buy_valume,sell_valume,create_time,ds_name) values");

                    insertSql.AppendFormat("('{0}','{1}','{2}','{3}',{4},'{5}')", "silver", time, silver_buy_num, silver_sale_num, "sysdate()", DS_NAME);
                    insertSqlHistory.AppendFormat("('{0}','{1}','{2}','{3}',{4},'{5}')", "silver", time, silver_buy_num, silver_sale_num, "sysdate()", DS_NAME);
                    DBUtils.executeSql(insertSql.ToString());
                    DBUtils.executeSql(insertSqlHistory.ToString());
                    _logger.Debug("JRSummaryData 插入数据完成");
                }
            }
            else
            {
                _logger.Debug("JRSummaryData 返回结果无数据");
            }
        }

        public static void DataCenterData()
        {
            string jsonString = HTTPUtils.SendDataCenterRequest("currency_pair", "XAUUSD");
            if (string.IsNullOrEmpty(jsonString))
            {
                _logger.Error("DataCenterData请求返回结果为空");
                return;
            }
            var jsonObject = JObject.Parse(jsonString);
            var data = jsonObject["data"];
            if (data != null)
            {
                
                string time = DBUtils.getDateTime();
                foreach (var item in data)
                {
                    var insertSql = new StringBuilder();
                    insertSql.Append("insert into trade_daily_sum(symbol,pt,buy_valume,sell_valume,create_time,ds_name) values");

                    var insertSqlHistory = new StringBuilder();
                    insertSqlHistory.Append("insert into trade_daily_sum_history(symbol,pt,buy_valume,sell_valume,create_time,ds_name) values");

                    string gold_buy_num = item["buy_rate"].ToString();
                    string gold_sale_num = item["sell_rate"].ToString();
                    string ds_name = item["broker"].ToString();
                    insertSql.AppendFormat("('{0}','{1}','{2}','{3}',{4},'{5}')", "gold", time, gold_buy_num, gold_sale_num, "sysdate()", ds_name);
                    insertSqlHistory.AppendFormat("('{0}','{1}','{2}','{3}',{4},'{5}')", "gold", time, gold_buy_num, gold_sale_num, "sysdate()", ds_name);
                    DBUtils.executeSql(insertSql.ToString());
                    DBUtils.executeSql(insertSqlHistory.ToString());
                    _logger.Debug("DataCenterData 插入数据完成");
                }
            }
            else
            {
                _logger.Debug("DataCenterData 返回结果无数据");
            }
        }
        /// <summary>
        /// Dukascopy情绪指数采集
        /// </summary>
        public static void DukascopySummaryData()
        {
            string DS_NAME = "Dukascopy";
            string htmlString = HTTPUtils.SendDukascopyRequest();
            if (string.IsNullOrEmpty(htmlString))
            {
                _logger.Error("DukascopySummaryData请求返回结果为空");
                return;
            }

            // 正则匹配 {"id":"XAU",...} 这种 JSON 对象
            Regex regex = new Regex(@"\{""id"":""XAU"".*?\}");
            MatchCollection matches = regex.Matches(htmlString);

            foreach (Match match in matches)
            {
                string jsonStr = match.Value;
                // 解析成字典
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr);

                var insertSql = new StringBuilder();
                insertSql.Append("insert into trade_daily_sum(symbol,pt,buy_valume,sell_valume,create_time,ds_name) values");

                var insertSqlHistory = new StringBuilder();
                insertSqlHistory.Append("insert into trade_daily_sum_history(symbol,pt,buy_valume,sell_valume,create_time,ds_name) values");

                // 获取时间戳（毫秒）
                long timestamp = Convert.ToInt64(dict["date"].ToString());
                DateTime dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;

                string time = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                string gold_buy_num = dict["long"].ToString();
                string gold_sale_num = dict["short"].ToString();
                string ds_name = DS_NAME;
                insertSql.AppendFormat("('{0}','{1}','{2}','{3}',{4},'{5}')", "gold", time, gold_buy_num, gold_sale_num, "sysdate()", ds_name);
                insertSqlHistory.AppendFormat("('{0}','{1}','{2}','{3}',{4},'{5}')", "gold", time, gold_buy_num, gold_sale_num, "sysdate()", ds_name);
                DBUtils.executeSql(insertSql.ToString());
                DBUtils.executeSql(insertSqlHistory.ToString());
                _logger.Debug("DukascopySummaryData 插入数据完成");
            }
        }

        public static string sendNotify(string notifytype, string[] phoneNumbers, string[] mails, string brokerName, string accountCode, string orderTime, string orderType, string orderSide, string price, string lots, string balance, string result)
        {
            if (string.IsNullOrEmpty(notifytype)) 
            {
                notifytype = "2";
            }
            if (string.Equals(notifytype, "1"))
            {
                SendSMS(phoneNumbers, accountCode, "", orderTime, orderSide, orderType, price, lots + " " + balance);
            }
            if (string.Equals(notifytype, "2"))
            {
                StringBuilder content = new StringBuilder();
                //[龙知易][订单提醒][%{broker_name}][%{order_time}][%{order_side}][%{order_type}][成交][%{order_number}]手数:%{order_count}
                content.AppendFormat("[龙知易][订单提醒][{0}][{1}][{2}][{3}][{4}]手数[{5}]价格[{6}]余额[{7}]交易结果[{8}]", brokerName, accountCode, orderTime, orderSide, orderType, lots, price, balance, result);
                EmailHelper.SendEmail(mails, "交易提醒[" + brokerName + "][" + accountCode + "][" + orderSide + "][" + orderType + "][" + result + "]", content.ToString());
            }
            if (string.Equals(notifytype, "3"))
            {
                SendSMS(phoneNumbers, accountCode, "", orderTime, orderSide, orderType, price, lots + " " + price);
                StringBuilder content = new StringBuilder();
                //[龙知易][订单提醒][%{broker_name}][%{order_time}][%{order_side}][%{order_type}][成交][%{order_number}]手数:%{order_count}
                content.AppendFormat("[龙知易][订单提醒][{0}][{1}][{2}][{3}][{4}]手数[{5}]价格[{6}]余额[{7}]交易结果[{8}]", brokerName, accountCode, orderTime, orderSide, orderType, lots, price, balance, result);
                EmailHelper.SendEmail(mails, "交易提醒[" + brokerName + "][" + accountCode + "][" + orderSide + "][" + orderType + "][" + result + "]", content.ToString());
            }
            return "";
        }

        public static string SendSMS(string[] numbers, string brokerName, string accountType, string orderTime, string orderSide, string orderType, string price, string lots)
        {
            orderTime = orderTime.Replace(':', '-');
            //清晰明了短信内容
            string contentParams = string.Format("account_number:{0},order_time:{1},order_side:{2},order_type:{3},order_number:{4},order_count:{5}", brokerName, orderTime, orderSide, orderType, price, lots);
            //用验证码模版测试
            //string contentParams = string.Format("{code:{0}}", brokerName.Substring(4));
            //string contentParams = "{\"code\":\""+ brokerName.Substring(4) + "\"}";
            //翻译短信内容
            //string contentParams = GetSMSContent(brokerName, accountType, orderTime, orderSide, orderType, price, lots);
            //string[] numbers =new string[2] {"15994725242","18682087500"};
            //string[] numbers = new string[1] { "15994725242"};
            //string[] numbers = new string[1] { "18205149743" };
            if (numbers != null && numbers.Length > 0)
            {
                //return SendSMS(numbers, contentParams);
                return HTTPUtils.sendSMSByAliyun(numbers, contentParams);
            }
            else
            {
                return "";
            }
        }    
    }
}
