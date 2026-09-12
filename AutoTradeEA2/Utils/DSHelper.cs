
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using uClient.Comm;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace uClient.Broker
{
    //金荣数据源
    public class DSHelper
    {

        /// <summary>
        /// GS策略时获取回测数据,查询数据效率低
        /// </summary>
        /// <param name="testDuration"></param>
        /// <returns></returns>
        public static List<IDictionary<string, string>> getGSQuoteList(string testDuration)
        {
            if (!string.IsNullOrEmpty(testDuration))
            {
                DateTime startDateTime, endDateTime;
                string format = "yyyyMMdd";
                if (testDuration.Contains("-"))
                {
                    startDateTime = DateTime.ParseExact(testDuration.Split('-')[0], format, CultureInfo.InvariantCulture);
                    endDateTime = DateTime.ParseExact(testDuration.Split('-')[1], format, CultureInfo.InvariantCulture);
                }
                else
                {
                    startDateTime = DateTime.ParseExact(testDuration, format, CultureInfo.InvariantCulture);
                    endDateTime = DateTime.Now;
                }

                return getGSQuoteList(startDateTime, endDateTime);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 查询指定时间段内的报价数据
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        public static List<IDictionary<string, string>> getQuoteList(string testDuration)
        {
            if (!string.IsNullOrEmpty(testDuration))
            {
                DateTime startDateTime, endDateTime;
                string format = "yyyyMMdd";
                if (testDuration.Contains("-"))
                {
                    startDateTime = DateTime.ParseExact(testDuration.Split('-')[0], format, CultureInfo.InvariantCulture);
                    endDateTime = DateTime.ParseExact(testDuration.Split('-')[1], format, CultureInfo.InvariantCulture);
                }
                else
                {
                    startDateTime = DateTime.ParseExact(testDuration, format, CultureInfo.InvariantCulture);
                    endDateTime = DateTime.Now;
                }

                //return getQuoteList(startDateTime, endDateTime, "test");
                return getQuoteList(startDateTime, endDateTime, "dukascopy");
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取近一天数据
        /// </summary>
        /// <param name="dsType"></param>
        /// <returns></returns>
        public static List<IDictionary<string, string>> getDailyData(string dsType)
        {
            string sql = "SELECT * FROM (SELECT * FROM trade_daily_sum where symbol = 'gold' and ds_name='"+ dsType + "' AND create_time >= NOW() - INTERVAL 10 HOUR order by create_time desc  LIMIT 24) tab order by tab.create_time asc";
            List<IDictionary<string, string>> datas = DBUtils.query(sql);
            return datas;
        }
        /// <summary>
        /// 进一天数据,查询回测数据
        /// </summary>
        /// <param name="dsType"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static List<IDictionary<string, string>> getTestDailyData(string dsType, DateTime time)
        {
            string sql = "SELECT * FROM (SELECT * FROM trade_daily_sum_history where symbol = 'gold' and ds_name='" + dsType + "' AND create_time BETWEEN DATE_SUB('" + time2string(time) + "', INTERVAL 10 HOUR) AND '" + time2string(time) + "' order by create_time desc  LIMIT 24) tab order by tab.create_time asc";
            List<IDictionary<string, string>> datas = DBUtils.query(sql);
            return datas;
        }
        /// <summary>
        /// 汇总数据
        /// </summary>
        /// <param name="dsType"></param>
        /// <returns></returns>
        public static List<IDictionary<string, string>> getSumData(string dsType)
        {
            string sql = "SELECT * FROM (SELECT * FROM trade_sum where symbol = 'gold'  and ds_name='"+ dsType + "' AND create_time >= NOW() - INTERVAL 10 HOUR  order by create_time desc  LIMIT 24) tab order by tab.create_time asc";
            List<IDictionary<string, string>> datas = DBUtils.query(sql);
            return datas;
        }
        /// <summary>
        /// 获取黄金和白银的价格列表
        /// </summary>
        /// <param name="dsType"></param>
        /// <param name="symbol"></param>
        /// <param name="count">判断区间</param>
        /// <returns></returns>
        public static List<IDictionary<string, string>> getPrice(string recordCount,string dsType, string symbol)
        {
            string sql = "SELECT * FROM (SELECT * FROM trade_quote where symbol = '" + symbol + "' AND gold_price > 0 AND silver_price >0 order by create_time desc  LIMIT "+ recordCount + ") tab order by tab.create_time asc";
            List<IDictionary<string, string>> datas = DBUtils.query(sql);
            return datas;
        }
        /// <summary>
        /// 获取黄金和白银的价格列表回测
        /// 获取最新的600条数据再按照时间升序,第一条数据为最早数据
        /// </summary>
        /// <param name="dsType"></param>
        /// <param name="symbol"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static List<IDictionary<string, string>> getTestPrice(string recordCount, string dsType, string symbol, DateTime time)
        {
            string sql = "SELECT * FROM (SELECT * FROM trade_quote where symbol = '" + symbol + "' AND gold_price > 0 AND silver_price >0 AND create_time<= '" + time2string(time) + "'  order by create_time desc  LIMIT "+ recordCount + ") tab order by tab.create_time asc";
            List<IDictionary<string, string>> datas = DBUtils.query(sql);
            return datas;
        }
        /// <summary>
        /// 按照时间升序,第一条数据为最早数据
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        public static List<IDictionary<string, string>> getQuoteList(DateTime startDateTime, DateTime endDateTime, string dsName)
        {
            string sql = "SELECT id,symbol,price,DATE_FORMAT(time,'%Y-%m-%d %H:%i:%s') as time,DATE_FORMAT(create_time,'%Y-%m-%d %H:%i:%s') as create_time, ds_name FROM trade_quote where symbol = 'gold' and create_time between '" + time2string(startDateTime) + "' and  '"+ time2string(endDateTime) + "' and ds_name = '"+ dsName + "' order by create_time asc ";
            List<IDictionary<string, string>> datas = DBUtils.query(sql);
            return datas;
        }
        /// <summary>
        /// GS策略时获取回测数据,查询数据效率低
        /// 按照时间升序,第一条数据为最早数据
        /// </summary>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        public static List<IDictionary<string, string>> getGSQuoteList(DateTime startDateTime, DateTime endDateTime)
        {
            string sql = "SELECT gold_price,silver_price,DATE_FORMAT(time,'%Y-%m-%d %H:%i:%s') as time FROM trade_quote where create_time between '" + time2string(startDateTime) + "' and  '" + time2string(endDateTime) + "' order by time asc ";
            List<IDictionary<string, string>> datas = DBUtils.query(sql);
            return datas;
        }
        /// <summary>
        /// 获取指定时间段内的指数
        /// </summary>
        /// <param name="indexTable"></param>
        /// <param name="startDateTime"></param>
        /// <param name="endDateTime"></param>
        /// <returns></returns>
        public static List<IDictionary<string, string>> getIndexList(string indexTable,DateTime startDateTime, DateTime endDateTime)
        {
            string sql = "SELECT id,symbol,DATE_FORMAT(create_time,'%Y-%m-%d %H:%i:%s') as time,DATE_FORMAT(create_time,'%Y-%m-%d %H:%i:%s') as create_time FROM " + indexTable + " where symbol = 'gold' and create_time between '" + time2string(startDateTime) + "' and  '" + time2string(endDateTime) + "' order by create_time asc ";
            List<IDictionary<string, string>> datas = DBUtils.query(sql);
            return datas;
        }
        /// <summary>
        /// 获取指定时间的报价列表
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static List<IDictionary<string, string>> getPriceByTimeList(string dateTime)
        {
            string sql = "SELECT id,symbol,price,DATE_FORMAT(time,'%Y-%m-%d %H:%i:%s') as time,DATE_FORMAT(create_time,'%Y-%m-%d %H:%i:%s') as create_time FROM trade_quote where symbol = 'gold' ORDER BY ABS(TIMESTAMPDIFF(SECOND, create_time, '"+ dateTime + "')) ASC LIMIT 1 ";
            List<IDictionary<string, string>> datas = DBUtils.query(sql);
            return datas;
        }
        /// <summary>
        /// 获取指定时间的价格
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static IDictionary<string, string> getPriceByTime(string dateTime)
        {
            List<IDictionary<string, string>> datas = getPriceByTimeList(dateTime);
            if (datas != null && datas.Count > 0)
            {
                return datas[0];
            }
            else
            {
                return null;
            }
        }

        public static string time2string(DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }


        /// <summary>
        /// 获取最新的K线数据
        /// </summary>
        /// <param name="period"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static List<Candle> getKline(string period, string symbol)
        {
            string sql = "SELECT * FROM (SELECT * FROM trade_kline_data kdata WHERE kdata.symbol = '" + symbol + "' AND kdata.period = '" + period + "' ORDER BY kdata.open_time DESC LIMIT 15 ) tab ORDER BY tab.open_time ASC";
            List<IDictionary<string, string>> datas = DBUtils.query(sql);
            if (datas != null && datas.Count > 0)
            {
                var result = new List<Candle>();
                foreach (var dataItem in datas)
                {
                    var time = Convert.ToDateTime(dataItem["open_time"]);
                    var open = Convert.ToDecimal(dataItem["open_price"]);
                    var close = Convert.ToDecimal(dataItem["close_price"]);
                    var high = Convert.ToDecimal(dataItem["high_price"]);
                    var low = Convert.ToDecimal(dataItem["low_price"]);
                    result.Add(new Candle { Time = time, Open = open, High = high, Low = low, Close = close });
                }
                return result;
            }
            return null;
        }

        public static List<Candle> getTestKline(string period, string symbol, DateTime time)
        {
            string sql = "SELECT * FROM (SELECT * FROM trade_kline_data kdata WHERE kdata.symbol = '" + symbol + "' AND kdata.period = '" + period + "' AND kdata.open_time<='"+ time2string(time) + "' ORDER BY kdata.open_time DESC LIMIT 15 ) tab ORDER BY tab.open_time ASC";
            List<IDictionary<string, string>> datas = DBUtils.query(sql);
            if (datas != null && datas.Count > 0)
            {
                var result = new List<Candle>();
                foreach (var dataItem in datas)
                {
                    var otime = Convert.ToDateTime(dataItem["open_time"]);
                    var open = Convert.ToDecimal(dataItem["open_price"]);
                    var close = Convert.ToDecimal(dataItem["close_price"]);
                    var high = Convert.ToDecimal(dataItem["high_price"]);
                    var low = Convert.ToDecimal(dataItem["low_price"]);
                    result.Add(new Candle { Time = otime, Open = open, High = high, Low = low, Close = close });
                }
                return result;
            }
            return null;
        }

        public static IDictionary<string, string> getStrategyLatest(string strategyCode, string symbol, string period, string currentDateTimeStr)
        {
            var sqlQueryStr30Min = new StringBuilder();
            sqlQueryStr30Min.AppendFormat("SELECT * FROM trade_strategy_result sr where sr.symbol = '{0}' and sr.strategy_code = '{1}' AND sr.period = '{2}' AND sr.strategy_date BETWEEN DATE_SUB('{3}', INTERVAL 65 MINUTE) AND '{3}' AND sr.result_type ='Test' ORDER BY sr.strategy_date,sr.modify_date DESC LIMIT 1", symbol, strategyCode, period, currentDateTimeStr);

            var sqlQueryStr1H = new StringBuilder();
            sqlQueryStr1H.AppendFormat("SELECT * FROM trade_strategy_result sr where sr.symbol = '{0}' and sr.strategy_code = '{1}' AND sr.period = '{2}' AND sr.strategy_date BETWEEN DATE_SUB('{3}', INTERVAL 130 MINUTE) AND '{3}' and sr.result_type ='Live' ORDER BY sr.strategy_date,sr.modify_date DESC LIMIT 1", symbol, strategyCode, period, currentDateTimeStr);

            var sqlQueryStr4H = new StringBuilder();
            sqlQueryStr4H.AppendFormat("SELECT * FROM trade_strategy_result sr where sr.symbol = '{0}' and sr.strategy_code = '{1}' AND sr.period = '{2}' AND sr.strategy_date BETWEEN DATE_SUB('{3}', INTERVAL 500 MINUTE) AND '{3}' and sr.result_type ='Live' ORDER BY sr.strategy_date,sr.modify_date DESC LIMIT 1", symbol, strategyCode, period, currentDateTimeStr);

            string sqlQueryStr = sqlQueryStr1H.ToString();

            if (string.Equals(period, "THIRTY_MINS"))
            {
                sqlQueryStr = sqlQueryStr30Min.ToString();
            }
            if (string.Equals(period, "ONE_HOUR"))
            {
                sqlQueryStr = sqlQueryStr1H.ToString();
            }
            if (string.Equals(period, "FOUR_HOURS"))
            {
                sqlQueryStr = sqlQueryStr4H.ToString();
            }
            List<IDictionary<string, string>> datas = DBUtils.query(sqlQueryStr);
            if (datas != null && datas.Count == 1)
            {
                return datas[0];
            }
            return null;
        }

        /// <summary>
        /// 获取最新的一条策略结果数据
        /// </summary>
        /// <param name="strategyCode"></param>
        /// <param name="symbol"></param>
        /// <param name="period"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <returns></returns>
        public static IDictionary<string, string> getStrategyResult(string strategyCode, string symbol, string period, string currentDateTimeStr, int trendCount)
        {
            var sqlQueryStr30Min = new StringBuilder();
            sqlQueryStr30Min.AppendFormat("SELECT * FROM trade_strategy_result sr where sr.symbol = '{0}' and sr.strategy_code = '{1}' AND sr.period = '{2}' AND sr.strategy_date BETWEEN DATE_SUB('{3}', INTERVAL 65 MINUTE) AND '{3}' AND sr.result_type ='Live' ORDER BY sr.modify_date DESC LIMIT 2", symbol, strategyCode, period, currentDateTimeStr);

            var sqlQueryStr1H = new StringBuilder();
            sqlQueryStr1H.AppendFormat("SELECT * FROM trade_strategy_result sr where sr.symbol = '{0}' and sr.strategy_code = '{1}' AND sr.period = '{2}' AND sr.strategy_date BETWEEN DATE_SUB('{3}', INTERVAL 130 MINUTE) AND '{3}' and sr.result_type ='Live' ORDER BY sr.modify_date DESC LIMIT 2", symbol, strategyCode, period, currentDateTimeStr);

            var sqlQueryStr4H = new StringBuilder();
            sqlQueryStr4H.AppendFormat("SELECT * FROM trade_strategy_result sr where sr.symbol = '{0}' and sr.strategy_code = '{1}' AND sr.period = '{2}' AND sr.strategy_date BETWEEN DATE_SUB('{3}', INTERVAL 500 MINUTE) AND '{3}' and sr.result_type ='Live' ORDER BY sr.modify_date DESC LIMIT 2", symbol, strategyCode, period, currentDateTimeStr);

            string sqlQueryStr = sqlQueryStr1H.ToString();

            if (string.Equals(period, "THIRTY_MINS"))
            {
                sqlQueryStr = sqlQueryStr30Min.ToString();
            }
            if (string.Equals(period, "ONE_HOUR"))
            {
                sqlQueryStr = sqlQueryStr1H.ToString();
            }
            if (string.Equals(period, "FOUR_HOURS"))
            {
                sqlQueryStr = sqlQueryStr4H.ToString();
            }
            List<IDictionary<string, string>> datas = DBUtils.query(sqlQueryStr);
            
            if (trendCount == 1 && datas != null && datas.Count > 0)
            {
                return datas[0];
            }
            if (trendCount == 2 && datas != null && datas.Count > 1)
            {
                if (string.Equals(datas[0]["result_status"], datas[1]["result_status"]))
                {
                    return datas[0];
                }
            }
            return null;
        }

        /// <summary>
        /// 获取最新的一条策略结果数据
        /// </summary>
        /// <param name="strategyCode"></param>
        /// <param name="symbol"></param>
        /// <param name="period"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <returns></returns>
        public static IDictionary<string, string> getStrategyResultTest(string strategyCode, string symbol,string period, string currentDateTimeStr, int trendCount)
        {
            var sqlQueryStr30Min = new StringBuilder();
            sqlQueryStr30Min.AppendFormat("SELECT * FROM trade_strategy_result sr where sr.symbol = '{0}' and sr.strategy_code = '{1}' AND sr.period = '{2}' AND sr.strategy_date BETWEEN DATE_SUB('{3}', INTERVAL 65 MINUTE) AND '{3}' AND sr.create_date BETWEEN DATE_SUB('{3}', INTERVAL 65 MINUTE) AND '{3}' AND sr.result_type ='Live' ORDER BY sr.modify_date DESC LIMIT 2", symbol, strategyCode, period, currentDateTimeStr);

            var sqlQueryStr1H = new StringBuilder();
            sqlQueryStr1H.AppendFormat("SELECT * FROM trade_strategy_result sr where sr.symbol = '{0}' and sr.strategy_code = '{1}' AND sr.period = '{2}' AND sr.strategy_date BETWEEN DATE_SUB('{3}', INTERVAL 130 MINUTE) AND '{3}' AND sr.create_date BETWEEN DATE_SUB('{3}', INTERVAL 130 MINUTE) AND '{3}' and sr.result_type ='Live' ORDER BY sr.modify_date DESC LIMIT 2", symbol, strategyCode, period, currentDateTimeStr);

            var sqlQueryStr4H = new StringBuilder();
            sqlQueryStr4H.AppendFormat("SELECT * FROM trade_strategy_result sr where sr.symbol = '{0}' and sr.strategy_code = '{1}' AND sr.period = '{2}' AND sr.strategy_date BETWEEN DATE_SUB('{3}', INTERVAL 500 MINUTE) AND '{3}' AND sr.create_date BETWEEN DATE_SUB('{3}', INTERVAL 500 MINUTE) AND '{3}' and sr.result_type ='Live' ORDER BY sr.modify_date DESC LIMIT 2", symbol, strategyCode, period, currentDateTimeStr);

            string sqlQueryStr = sqlQueryStr1H.ToString();

            if (string.Equals(period, "THIRTY_MINS"))
            {
                sqlQueryStr = sqlQueryStr30Min.ToString();
            }
            if (string.Equals(period, "ONE_HOUR"))
            {
                sqlQueryStr = sqlQueryStr1H.ToString();
            }
            if (string.Equals(period, "FOUR_HOURS"))
            {
                sqlQueryStr = sqlQueryStr4H.ToString();
            }
            List<IDictionary<string, string>> datas = DBUtils.query(sqlQueryStr);

            if (trendCount == 1 && datas != null && datas.Count > 0)
            {
                return datas[0];
            }
            if (trendCount == 2 && datas != null && datas.Count > 1)
            {
                if (string.Equals(datas[0]["result_status"], datas[1]["result_status"]))
                {
                    return datas[0];
                }
            }
            return null;
        }
        public static IDictionary<string, string> getStrategyLatestTest(string strategyCode, string symbol, string period, string currentDateTimeStr)
        {
            var sqlQueryStr30Min = new StringBuilder();
            sqlQueryStr30Min.AppendFormat("SELECT * FROM trade_strategy_result sr where sr.symbol = '{0}' and sr.strategy_code = '{1}' AND sr.period = '{2}' AND sr.strategy_date BETWEEN DATE_SUB('{3}', INTERVAL 65 MINUTE) AND '{3}' AND sr.result_type ='Test' ORDER BY sr.strategy_date DESC LIMIT 1", symbol, strategyCode, period, currentDateTimeStr);

            var sqlQueryStr1H = new StringBuilder();
            sqlQueryStr1H.AppendFormat("SELECT * FROM trade_strategy_result sr where sr.symbol = '{0}' and sr.strategy_code = '{1}' AND sr.period = '{2}' AND sr.strategy_date BETWEEN DATE_SUB('{3}', INTERVAL 130 MINUTE) AND '{3}' AND sr.result_type ='Test' ORDER BY sr.strategy_date DESC LIMIT 1", symbol, strategyCode, period, currentDateTimeStr);
            var sqlQueryStr4H = new StringBuilder();
            sqlQueryStr4H.AppendFormat("SELECT * FROM trade_strategy_result sr where sr.symbol = '{0}' and sr.strategy_code = '{1}' AND sr.period = '{2}' AND sr.strategy_date BETWEEN DATE_SUB('{3}', INTERVAL 500 MINUTE) AND '{3}' AND sr.result_type ='Test' ORDER BY sr.strategy_date DESC LIMIT 1", symbol, strategyCode, period, currentDateTimeStr);

            string sqlQueryStr = sqlQueryStr1H.ToString();

            if (string.Equals(period, "THIRTY_MINS"))
            {
                sqlQueryStr = sqlQueryStr30Min.ToString();
            }
            if (string.Equals(period, "ONE_HOUR"))
            {
                sqlQueryStr = sqlQueryStr1H.ToString();
            }
            if (string.Equals(period, "FOUR_HOURS"))
            {
                sqlQueryStr = sqlQueryStr4H.ToString();
            }
            List<IDictionary<string, string>> datas = DBUtils.query(sqlQueryStr);
            if (datas != null && datas.Count == 1)
            {
                return datas[0];
            }
            return null;
        }
        /// <summary>
        /// 获取K线数据
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="period"></param>
        /// <param name="count"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <returns></returns>
        public static List<Candle> GetRecentCandles(string symbol, string period, int count, string currentDateTimeStr)
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT * FROM (SELECT * FROM trade_kline_data WHERE symbol = '{0}' AND period = '{1}' AND open_time <= TIMESTAMP('{2}') ORDER BY open_time DESC  LIMIT {3}) tab ORDER BY tab.open_time ASC", symbol, period, currentDateTimeStr, count);
            List<IDictionary<string, string>> datas = DBUtils.query(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                var result = new List<Candle>();
                foreach (var dataItem in datas)
                {
                    var time = Convert.ToDateTime(dataItem["open_time"]);
                    var open = Convert.ToDecimal(dataItem["open_price"]);
                    var close = Convert.ToDecimal(dataItem["close_price"]);
                    var high = Convert.ToDecimal(dataItem["high_price"]);
                    var low = Convert.ToDecimal(dataItem["low_price"]);
                    result.Add(new Candle { Time = time, Open = open, High = high, Low = low, Close = close });
                }
                return result;
            }
            return null;
        }


        /// <summary>
        /// 获取截止某时间的最近 N 根 K 线（内部自动计算时间区间）
        /// </summary>
        /// <param name="symbol">交易品种</param>
        /// <param name="period">周期（如 1m, 5m, 15m, 1H, 4H）</param>
        /// <param name="count">K线数量</param>
        /// <param name="endTime">截止时间（包含）</param>
        /// <returns>时间升序K线列表</returns>
        public static List<Candle> GetRecentCandlesByCount(
            string symbol,
            string period,
            int count,
            DateTime endTime)
        {
            var result = new List<Candle>();

            if (count <= 0)
                return result;

            // 1️⃣ 解析周期长度（分钟）
            int minutesPerCandle = GetMinutesFromPeriod(period);

            // 2️⃣ 自动计算开始时间
            DateTime startTime = endTime.AddMinutes(-minutesPerCandle * count);

            string sql = @"
                SELECT open_time,
                       open_price,
                       high_price,
                       low_price,
                       close_price
                FROM trade_kline_data
                WHERE symbol = @symbol
                  AND period = @period
                  AND open_time >= @startTime
                  AND open_time <= @endTime
                ORDER BY open_time ASC";

            var parameters = new Dictionary<string, object>
            {
                { "@symbol", symbol },
                { "@period", period },
                { "@startTime", startTime },
                { "@endTime", endTime }
            };

            var datas = DBUtils.query(sql, parameters);

            if (datas == null || datas.Count == 0)
                return result;

            foreach (var row in datas)
            {
                result.Add(new Candle
                {
                    Time = Convert.ToDateTime(row["open_time"]),
                    Open = Convert.ToDecimal(row["open_price"]),
                    High = Convert.ToDecimal(row["high_price"]),
                    Low = Convert.ToDecimal(row["low_price"]),
                    Close = Convert.ToDecimal(row["close_price"])
                });
            }

            return result;
        }

        // --- 变量定义在类级别，作为缓存 ---
        private static List<Candle> _cache = new List<Candle>();

        /// <summary>
        /// 优化后的数据获取方法（按时间段）
        /// 核心改进：根据 targetKPeriod + count 自动计算时间段，
        /// 返回该时间段内的K线，按时间升序排列。
        /// </summary>
        public static List<Candle> GetRecentCandlesOptimized(
            string symbol,
            string period,
            int count,
            string currentDateTimeStr,
            string targetKPeriod)
        {
            // 1️⃣ 解析当前请求时间
            if (!DateTime.TryParse(currentDateTimeStr, out DateTime currentTime))
            {
                return new List<Candle>();
            }

            // 2️⃣ 计算时间区间
            int minutesPerBar = GetMinutesFromPeriod(targetKPeriod); // targetKPeriod：如 "M5", "H1"
            DateTime startTime = currentTime.AddMinutes(-minutesPerBar * count);
            DateTime endTime = currentTime;

            // 3️⃣ 缓存加载逻辑
            // 如果缓存为空，或者时间区间不在缓存内
            if (_cache.Count == 0
                || startTime < _cache.First().Time
                || endTime > _cache.Last().Time)
            {
                string dbStartTimeStr = startTime.ToString("yyyy-MM-dd HH:mm:ss");

                // 从数据库加载大块数据
                _cache = LoadBigBlockFromDB(symbol, period, dbStartTimeStr, 100000);
            }

            // 4️⃣ 内存过滤：时间区间内的K线
            var result = _cache
                .Where(c => c.Time >= startTime && c.Time <= endTime)
                .OrderBy(c => c.Time) // 保证升序
                .ToList();

            return result;
        }


        public static List<Candle> GetRecentCandlesOptimizedFast(
            string symbol,
            string period,
            int count,
            string currentDateTimeStr,
            string targetKPeriod)
        {
            if (!DateTime.TryParse(currentDateTimeStr, out DateTime currentTime))
                return new List<Candle>();

            int minutesPerBar = GetMinutesFromPeriod(targetKPeriod);
            DateTime startTime = currentTime.AddMinutes(-minutesPerBar * count);
            DateTime endTime = currentTime;

            // 🔥 只在真正超出缓存最大范围时才重新加载
            if (_cache.Count == 0 ||
                endTime > _cache[_cache.Count - 1].Time)
            {
                string dbStartTimeStr = startTime.ToString("yyyy-MM-dd HH:mm:ss");
                _cache = LoadBigBlockFromDB(symbol, period, dbStartTimeStr, 100000);

                // 数据库必须 ORDER BY open_time ASC
                // 如果数据库已排序，这里可以删除 Sort
                _cache.Sort((a, b) => a.Time.CompareTo(b.Time));
            }

            if (_cache.Count == 0)
                return new List<Candle>();

            // 🚀 二分查找
            int startIdx = BinarySearchStartIndex(_cache, startTime);
            int endIdx = BinarySearchEndIndex(_cache, endTime);

            if (startIdx < 0 || endIdx < 0 || startIdx > endIdx)
                return new List<Candle>();

            // 🚀 直接GetRange（O(1)切片）
            return _cache.GetRange(startIdx, endIdx - startIdx + 1);
        }

        // 二分查找 >= startTime 的第一个索引
        private static int BinarySearchStartIndex(List<Candle> list, DateTime startTime)
        {
            int left = 0, right = list.Count - 1;
            int idx = -1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                if (list[mid].Time >= startTime)
                {
                    idx = mid;
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }
            return idx;
        }

        // 二分查找 <= endTime 的最后一个索引
        private static int BinarySearchEndIndex(List<Candle> list, DateTime endTime)
        {
            int left = 0, right = list.Count - 1;
            int idx = -1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                if (list[mid].Time <= endTime)
                {
                    idx = mid;
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }
            return idx;
        }

        /// <summary>
        /// 根据周期字符串获取对应的分钟数
        /// </summary>
        private static int GetMinutesFromPeriod(string period)
        {
            if (string.IsNullOrEmpty(period)) return 1;

            switch (period.ToUpper())
            {
                case "M1": case "ONE_MIN": return 1;
                case "M5": case "FIVE_MIN": return 5;
                case "M15": return 15;
                case "M30": return 30;
                case "H1": case "ONE_HOUR": return 60;
                // 增加对 FOUR_HOURS 的支持
                case "H4": case "FOUR_HOURS": case "FOUR_HOUR": return 240;
                case "D1": case "DAILY": return 1440;
                default: return 1;
            }
        }
        /// <summary>
        /// 从数据库大批量预读数据到内存缓存中
        /// </summary>
        /// <param name="symbol">品种名称</param>
        /// <param name="period">周期</param>
        /// <param name="startTimeStr">开始时间（当前模拟到的时间点）</param>
        /// <param name="blockSize">预读的数量（建议 1000-5000）</param>
        /// <returns>Candle 列表</returns>
        public static List<Candle> LoadBigBlockFromDB(string symbol, string period, string startTimeStr, int blockSize)
        {
            var result = new List<Candle>();
            var sql = new StringBuilder();

            // 逻辑：查询 [当前时间] 之后（包括当前）的 [blockSize] 条记录
            // 这样加载一次，够模拟器跑很久，不用频繁骚扰数据库
            sql.AppendFormat(@"
                SELECT open_time, open_price, high_price, low_price, close_price 
                FROM trade_kline_data 
                WHERE symbol = '{0}' 
                  AND period = '{1}' 
                  AND open_time >= TIMESTAMP('{2}') 
                ORDER BY open_time ASC 
                LIMIT {3}",
                symbol, period, startTimeStr, blockSize);

            // 调用你现有的通用查询工具
            List<IDictionary<string, string>> datas = DBUtils.query(sql.ToString());

            if (datas != null && datas.Count > 0)
            {
                foreach (var dataItem in datas)
                {
                    result.Add(new Candle
                    {
                        Time = Convert.ToDateTime(dataItem["open_time"]),
                        Open = Convert.ToDecimal(dataItem["open_price"]),
                        High = Convert.ToDecimal(dataItem["high_price"]),
                        Low = Convert.ToDecimal(dataItem["low_price"]),
                        Close = Convert.ToDecimal(dataItem["close_price"])
                    });
                }
            }

            // 打印预读日志，方便观察缓存何时触发刷新
            //Console.WriteLine($"[Cache Update] 预读成功: 载入 {result.Count} 条数据，起始时间: {startTimeStr}");
            return result;
        }
    }
}
