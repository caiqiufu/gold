using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace uClient.Comm
{
    /// <summary>
    /// HTTP 操作类
    /// </summary>
    public class HTTPHelper
    {
        private static readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        private const string InsertSumPrefix =
            "insert into trade_daily_sum(symbol,pt,buy_valume,sell_valume,create_time,ds_name) values";
        private const string InsertHistoryPrefix =
            "insert into trade_daily_sum_history(symbol,pt,buy_valume,sell_valume,create_time,ds_name) values";

        // Dukascopy 页面中的 XAU 情绪数据对象
        private static readonly Regex DukascopyXauRegex =
            new Regex(@"\{""id"":""XAU"".*?\}", RegexOptions.Compiled);

        #region 公共辅助方法

        /// <summary>
        /// 转义拼接进 SQL 字符串字面量的值(外部数据不可信,防止注入/语法错误)。
        /// 注意:这只是权宜之计,最好让 DBUtils 支持参数化查询。
        /// </summary>
        private static string Esc(string s)
        {
            return (s ?? string.Empty).Replace("\\", "\\\\").Replace("'", "''");
        }

        /// <summary>生成单行 values 片段</summary>
        private static string BuildValues(string symbol, string pt, string buy, string sell, string dsName)
        {
            return string.Format("('{0}','{1}','{2}','{3}',sysdate(),'{4}')",
                Esc(symbol), Esc(pt), Esc(buy), Esc(sell), Esc(dsName));
        }

        /// <summary>同时写入汇总表和历史表</summary>
        private static void InsertBoth(string valuesSql)
        {
            DBUtils.executeSql(InsertSumPrefix + valuesSql);
            DBUtils.executeSql(InsertHistoryPrefix + valuesSql);
        }

        /// <summary>汇总表中是否已存在相同的 买/卖 数据</summary>
        private static bool Exists(string symbol, string buy, string sell, string dsName)
        {
            string sql = string.Format(
                "select count(1) as num from trade_daily_sum where symbol = '{0}' and buy_valume = '{1}' and sell_valume = '{2}' and ds_name = '{3}'",
                Esc(symbol), Esc(buy), Esc(sell), Esc(dsName));
            // Int16 最大 32767,count 超过会溢出,改用 Int64
            return Convert.ToInt64(DBUtils.query(sql)[0]["num"]) > 0;
        }

        /// <summary>数据不存在时才插入(两张表)</summary>
        private static void InsertIfNotExists(string symbol, string pt, string buy, string sell, string dsName, string tag)
        {
            if (Exists(symbol, buy, sell, dsName))
            {
                return;
            }
            InsertBoth(BuildValues(symbol, pt, buy, sell, dsName));
            _logger.Debug("{0} 插入{1}数据完成", tag, symbol);
        }

        /// <summary>安全读取 JSON 字段,缺失返回 null</summary>
        private static string Field(JToken token, string key)
        {
            return token?[key]?.ToString();
        }

        /// <summary>解析响应并返回 data 节点;任何异常/空值都返回 null 并记录日志</summary>
        private static JToken ParseData(string jsonString, string tag)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                _logger.Error("{0}请求返回结果为空", tag);
                return null;
            }
            try
            {
                var data = JObject.Parse(jsonString)["data"];
                if (data == null || data.Type == JTokenType.Null)
                {
                    _logger.Debug("{0} 返回结果无数据", tag);
                    return null;
                }
                return data;
            }
            catch (JsonException ex)
            {
                _logger.Error(ex, "{0} 解析JSON失败", tag);
                return null;
            }
        }

        /// <summary>金银两个品种的统一处理</summary>
        private static void SaveGoldSilver(string tag, string dsName, string time,
            string goldBuy, string goldSell, string silverBuy, string silverSell)
        {
            if (time == null || goldBuy == null || goldSell == null || silverBuy == null || silverSell == null)
            {
                _logger.Error("{0} 返回数据字段缺失", tag);
                return;
            }
            InsertIfNotExists("gold", time, goldBuy, goldSell, dsName, tag);
            InsertIfNotExists("silver", time, silverBuy, silverSell, dsName, tag);
        }

        #endregion

        /// <summary>
        /// 采集万州数据
        /// </summary>
        public static void WZSummaryData()
        {
            const string tag = "WZSummaryData";
            try
            {
                var data = ParseData(HTTPUtils.SendWZRequest("all", ""), tag);
                if (data == null) return;

                SaveGoldSilver(tag, "WZ",
                    Field(data, "time"),
                    Field(data, "gold_buy_num"), Field(data, "gold_sale_num"),
                    Field(data, "silver_buy_num"), Field(data, "silver_sale_num"));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "{0} 执行异常", tag);
            }
        }

        /// <summary>
        /// 采集金荣数据
        /// </summary>
        public static void JRSummaryData()
        {
            const string tag = "JRSummaryData";
            try
            {
                var data = ParseData(HTTPUtils.SendJRRequest("day", "gold"), tag);
                if (data == null) return;

                SaveGoldSilver(tag, "JR",
                    Field(data, "pt"),
                    Field(data, "jbp"), Field(data, "jsp"),
                    Field(data, "ybp"), Field(data, "ysp"));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "{0} 执行异常", tag);
            }
        }

        /// <summary>
        /// 采集数据中心 XAUUSD 各经纪商报价(多行合并为一次插入)
        /// </summary>
        public static void DataCenterData()
        {
            const string tag = "DataCenterData";
            try
            {
                var data = ParseData(HTTPUtils.SendDataCenterRequest("currency_pair", "XAUUSD"), tag);
                if (data == null) return;

                string time = DBUtils.getDateTime();
                var values = new List<string>();
                foreach (var item in data)
                {
                    string buy = Field(item, "buy_rate");
                    string sell = Field(item, "sell_rate");
                    string broker = Field(item, "broker");
                    if (buy == null || sell == null || string.IsNullOrEmpty(broker))
                    {
                        _logger.Warn("{0} 跳过字段不完整的记录: {1}", tag, item);
                        continue;
                    }
                    values.Add(BuildValues("gold", time, buy, sell, broker));
                }

                if (values.Count == 0) return;

                // 一条 SQL 批量插入,减少数据库往返次数
                InsertBoth(string.Join(",", values));
                _logger.Debug("{0} 插入数据完成,共 {1} 条", tag, values.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "{0} 执行异常", tag);
            }
        }

        /// <summary>
        /// Dukascopy情绪指数采集
        /// </summary>
        public static void DukascopySummaryData()
        {
            const string tag = "DukascopySummaryData";
            const string DS_NAME = "Dukascopy";
            try
            {
                string htmlString = HTTPUtils.SendDukascopyRequest();
                if (string.IsNullOrEmpty(htmlString))
                {
                    _logger.Error("{0}请求返回结果为空", tag);
                    return;
                }

                var values = new List<string>();
                foreach (Match match in DukascopyXauRegex.Matches(htmlString))
                {
                    try
                    {
                        var obj = JObject.Parse(match.Value);
                        long timestamp = obj.Value<long>("date");
                        string time = DateTimeOffset.FromUnixTimeMilliseconds(timestamp)
                            .UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                        string buy = Field(obj, "long");
                        string sell = Field(obj, "short");
                        if (buy == null || sell == null)
                        {
                            _logger.Warn("{0} 跳过字段缺失的记录: {1}", tag, match.Value);
                            continue;
                        }
                        values.Add(BuildValues("gold", time, buy, sell, DS_NAME));
                    }
                    catch (Exception ex)
                    {
                        // 单条解析失败不影响其他记录
                        _logger.Warn(ex, "{0} 解析记录失败: {1}", tag, match.Value);
                    }
                }

                if (values.Count == 0) return;

                InsertBoth(string.Join(",", values));
                _logger.Debug("{0} 插入数据完成,共 {1} 条", tag, values.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "{0} 执行异常", tag);
            }
        }

        /// <summary>
        /// 发送交易通知。notifytype: 1=短信 2=邮件 3=短信+邮件(默认2)
        /// </summary>
        public static string sendNotify(string notifytype, string[] phoneNumbers, string[] mails, string brokerName,
            string accountCode, string orderTime, string orderType, string orderSide, string price, string lots,
            string balance, string result)
        {
            if (string.IsNullOrEmpty(notifytype))
            {
                notifytype = "2";
            }

            bool sendSms = notifytype == "1" || notifytype == "3";
            bool sendMail = notifytype == "2" || notifytype == "3";

            if (sendSms)
            {
                // 注意:原代码中类型1传 balance、类型3传 price,此处保持原行为,请确认是否为笔误
                string extra = notifytype == "1" ? balance : price;
                SendSMS(phoneNumbers, accountCode, "", orderTime, orderSide, orderType, price, lots + " " + extra);
            }

            if (sendMail)
            {
                if (mails == null || mails.Length == 0)
                {
                    _logger.Warn("sendNotify 未配置收件邮箱,跳过邮件发送");
                }
                else
                {
                    string content = string.Format(
                        "[龙知易][订单提醒][{0}][{1}][{2}][{3}][{4}]手数[{5}]价格[{6}]余额[{7}]交易结果[{8}]",
                        brokerName, accountCode, orderTime, orderSide, orderType, lots, price, balance, result);
                    string subject = string.Format("交易提醒[{0}][{1}][{2}][{3}][{4}]",
                        brokerName, accountCode, orderSide, orderType, result);
                    EmailHelper.SendEmail(mails, subject, content);
                }
            }
            return "";
        }

        /// <summary>
        /// 发送短信(阿里云)。注意:第二个参数实际传入的是账号(accountCode)。
        /// </summary>
        public static string SendSMS(string[] numbers, string brokerName, string accountType, string orderTime,
            string orderSide, string orderType, string price, string lots)
        {
            if (numbers == null || numbers.Length == 0)
            {
                return "";
            }

            orderTime = (orderTime ?? string.Empty).Replace(':', '-');
            string contentParams = string.Format(
                "account_number:{0},order_time:{1},order_side:{2},order_type:{3},order_number:{4},order_count:{5}",
                brokerName, orderTime, orderSide, orderType, price, lots);

            return HTTPUtils.sendSMSByAliyun(numbers, contentParams);
        }
    }
}