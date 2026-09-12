using NLog;
using System;
using System.Collections.Generic;
using System.Text;



namespace uClient.Comm
{
    /// <summary>
    /// DB 操作
    /// </summary>
    public class DBHelper
    {
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// MT4保存交易日志
        /// </summary>
        /// <param name="platformCode"></param>
        /// <param name="brokerCode"></param>
        /// <param name="brokerName"></param>
        /// <param name="accountCode"></param>
        /// <param name="autoFlag"></param>
        /// <param name="orderId"></param>
        /// <param name="eaInfo"></param>
        /// <param name="orderType"></param>
        /// <param name="orderTypeDetail"></param>
        /// <param name="volume"></param>
        /// <param name="openPrice"></param>
        /// <param name="closePrice"></param>
        /// <param name="profitPoint"></param>
        /// <param name="profitAmount"></param>
        /// <param name="accountBalance"></param>
        /// <param name="accountEquity"></param>
        /// <param name="accountMargin"></param>
        /// <param name="accountFreemargin"></param>
        public static void saveTradeLogMT41(string platformCode, string brokerCode, string brokerName, string accountCode, string autoFlag, string orderId, string eaInfo, string orderType, string orderTypeDetail, double volume, double openPrice, double closePrice, double profitPoint, double profitAmount, double accountBalance, double accountEquity, double accountMargin, double accountFreemargin, string remark)
        {
            var sqlStr = new StringBuilder();
            sqlStr.Append("INSERT INTO trade_record(platform_code,broker_code,broker_name,account_code,auto_flag,order_id, ea_info,order_type,order_type_detail,volume,open_price,close_price,profit_point,profit_amount,account_balance,account_equity,account_margin,account_freemargin,create_time,server_id,remark) VALUES");
            sqlStr.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}',{18},'{19}','{20}')", platformCode, brokerCode, brokerName, accountCode, autoFlag, orderId, eaInfo, orderType, orderTypeDetail, volume, openPrice, closePrice, profitPoint, profitAmount, accountBalance, accountEquity, accountMargin, accountFreemargin, "sysdate()", uClient.Comm.Utils.GetLocalIP(), remark);
            DBUtils.eaExecuteSql(sqlStr.ToString());
        }
        /// <summary>
        /// 使用参数化查询安全地保存 MT4 交易日志。
        /// </summary>
        public static void saveTradeLogMT4(
            string platformCode, string brokerCode, string brokerName, string accountCode, string autoFlag,
            string orderId, string eaInfo, string orderType, string orderTypeDetail,
            double volume, double openPrice, double closePrice, double profitPoint,
            double profitAmount, double accountBalance, double accountEquity, double accountMargin,
            double accountFreemargin, string remark)
        {
            // 1. 定义 SQL 插入语句，使用命名参数 (@paramName)
            const string sql = @"
            INSERT INTO trade_record (
                platform_code, broker_code, broker_name, account_code, auto_flag,
                order_id, ea_info, order_type, order_type_detail, volume,
                open_price, close_price, profit_point, profit_amount, account_balance,
                account_equity, account_margin, account_freemargin, create_time, server_id, remark
            )
            VALUES (
                @platformCode, @brokerCode, @brokerName, @accountCode, @autoFlag,
                @orderId, @eaInfo, @orderType, @orderTypeDetail, @volume,
                @openPrice, @closePrice, @profitPoint, @profitAmount, @accountBalance,
                @accountEquity, @accountMargin, @accountFreemargin, SYSDATE(), @serverId, @remark
            )";

            // 2. 创建参数字典或列表，将参数名和值映射起来
            // 注意：SYSDATE() 或 GETDATE() 等数据库函数直接保留在 SQL 中，不需要作为参数。
            var parameters = new Dictionary<string, object>
        {
            { "@platformCode", platformCode },
            { "@brokerCode", brokerCode },
            { "@brokerName", brokerName },
            { "@accountCode", accountCode },
            { "@autoFlag", autoFlag },
            { "@orderId", orderId },
            { "@eaInfo", eaInfo },
            { "@orderType", orderType },
            { "@orderTypeDetail", orderTypeDetail },
            // 数值类型参数
            { "@volume", volume },
            { "@openPrice", openPrice },
            { "@closePrice", closePrice },
            { "@profitPoint", profitPoint },
            { "@profitAmount", profitAmount },
            { "@accountBalance", accountBalance },
            { "@accountEquity", accountEquity },
            { "@accountMargin", accountMargin },
            { "@accountFreemargin", accountFreemargin },
            // 其他字符串参数
            { "@serverId", uClient.Comm.Utils.GetLocalIP() },
            { "@remark", remark }
        };

            // 3. 调用 DBUtils 中执行参数化查询的方法
            // 假设 DBUtils 有一个方法可以接受 SQL 语句和参数字典
            DBUtils.ExecuteParameterizedSql(sql, parameters);
        }
        /// <summary>
        ///  MF4保存交易日志
        /// </summary>
        /// <param name="platformCode"></param>
        /// <param name="brokerCode"></param>
        /// <param name="brokerName"></param>
        /// <param name="accountCode"></param>
        /// <param name="autoFlag"></param>
        /// <param name="orderId"></param>
        /// <param name="eaInfo"></param>
        /// <param name="orderType"></param>
        /// <param name="orderTypeDetail"></param>
        /// <param name="volume"></param>
        /// <param name="openPrice"></param>
        /// <param name="closePrice"></param>
        /// <param name="profitPoint"></param>
        /// <param name="accountBalance"></param>
        /// <param name="accountEquity"></param>
        /// <param name="accountMargin"></param>
        /// <param name="accountFreemargin"></param>
        public static void saveTradeLogMF4(string platformCode, string brokerCode, string brokerName, string accountCode, string autoFlag, string orderId, string eaInfo, string orderType, string orderTypeDetail, double volume, double openPrice, double closePrice, double profitPoint, double accountBalance, double accountEquity, double accountMargin, double accountFreemargin, string remark)
        {
            var sqlStr = new StringBuilder();
            sqlStr.Append("INSERT INTO trade_record(platform_code,broker_code,broker_name,account_code,auto_flag,order_id, ea_info,order_type,order_type_detail,volume,open_price,close_price,profit_point,profit_amount,account_balance,account_equity,account_margin,account_freemargin,create_time,server_id,remark) VALUES");
            sqlStr.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}',{13},'{14}','{15}','{16}','{17}',{18},'{19}','{20}')", platformCode, brokerCode, brokerName, accountCode, autoFlag, orderId, eaInfo, orderType, orderTypeDetail, volume, openPrice, closePrice, profitPoint,
                "ROUND((" + accountBalance + "-(SELECT tr.account_balance FROM trade_record tr WHERE tr.id = (SELECT max(tt.id) FROM trade_record tt WHERE tt.account_code = '" + accountCode + "'))),2)",
                accountBalance, accountEquity, accountMargin, accountFreemargin, "sysdate()", uClient.Comm.Utils.GetLocalIP(), remark);
            DBUtils.eaExecuteSql(sqlStr.ToString());
        }
        /// <summary>
        /// 更新账户余额
        /// </summary>
        /// <param name="brokerCode"></param>
        /// <param name="accountCode"></param>
        /// <param name="balance"></param>
        /// <param name="totalEquity"></param>
        /// <param name="usedMargin"></param>
        /// <param name="usableMargin"></param>
        public static void updateAccountBalance(string brokerCode, string accountCode, double balance, double totalEquity, double usedMargin, double usableMargin)
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT CAST(ta.id AS CHAR) AS ID FROM trade_user_account ta WHERE ta.account_code = '{0}' AND ta.broker_code =  '{1}' and ta.activate_flag = 'Y'", accountCode, brokerCode);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                foreach (IDictionary<string, string> data in datas)
                {
                    String id = data["ID"];
                    var sqlStr = new StringBuilder();
                    sqlStr.AppendFormat("UPDATE trade_user_account ta SET ta.account_balance = {0},ta.account_equity = {1},ta.account_margin = {2},ta.account_freemargin = {3},ta.modify_by='{4}',ta.modify_date= sysdate() WHERE ta.id = {5}",
                        balance, totalEquity, usedMargin, usableMargin, "unieap", id);
                    DBUtils.eaExecuteSql(sqlStr.ToString());
                }
            }
        }
        /// <summary>
        /// 根据绑定的IP,formNo查询对应的账户信息
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="formNo"></param>
        /// <returns></returns>
        public static Broker getBrokerInfo(string ip, string formNo)
        {
            var sqlBrokerStr = new StringBuilder();
            sqlBrokerStr.AppendFormat("SELECT b.platform as PlatformCode, b.broker_code AS BrokerCode,b.broker_name AS BrokerName,ta.form_no AS FormNo,b.live_symbol AS LiveSymbol,b.live_ip AS LiveIP,b.live_port AS LivePort,b.demo_symbol  AS DemoSymbol,b.demo_ip AS DemoIP,b.demo_port AS DemoPort,b.time_zone AS Timezone, b.remark as Remark FROM trade_user_account ua,trade_auto ta,trade_broker b WHERE ua.account_code = ta.account_code AND ua.broker_code = b.broker_code AND ua.activate_flag = 'Y' AND ua.auto_flag = 'Y' AND ta.execute_ip = '{0}' AND ta.form_no ='{1}'", ip, formNo);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlBrokerStr.ToString());
            Broker broker = null;
            if (datas != null && datas.Count > 0)
            {
                broker = new Broker();
                IDictionary<string, string> data = datas[0];
                broker.PlatformNo = string.Equals(data["PlatformCode"], "MT4") ? 2 : 4;
                broker.PlatformCode = data["PlatformCode"];
                broker.BrokerCode = data["BrokerCode"];
                broker.BrokerName = data["BrokerName"];
                broker.FormNo = data["FormNo"];
                broker.DemoIP = data["DemoIP"];
                broker.DemoPort = data["DemoPort"];
                broker.LiveIP = data["LiveIP"];
                broker.LivePort = data["LivePort"];
                var sqlAccountStr = new StringBuilder();
                sqlAccountStr.AppendFormat("SELECT ua.account_code AS UserCode,ua.password AS Password,ua.account_type AS Type,ua.valumn as Valumn,ua.sms_notify AS SMSNotify,ua.email_notify AS EmailNotify FROM trade_user_account ua,trade_auto ta WHERE ua.account_code = ta.account_code AND ua.activate_flag = 'Y' AND ua.auto_flag = 'Y' AND ta.execute_ip = '{0}' AND ta.form_no ='{1}'", ip, formNo);
                List<IDictionary<string, string>> datasAccount = DBUtils.eaQuery(sqlAccountStr.ToString());
                if (datasAccount != null && datasAccount.Count > 0)
                {
                    Account account = new Account();
                    IDictionary<string, string> dataAccount = datasAccount[0];
                    account.BrokerCode = broker.BrokerCode;
                    account.Type = dataAccount["Type"];
                    if (string.Equals(account.Type, "Demo"))
                    {
                        broker.DemoAccount = account;
                        broker.DefaultAccountType = "Demo";
                        broker.Symbol = data["DemoSymbol"].Split(',');
                        broker.DefaultSymbol = broker.Symbol[0];
                    }
                    if (string.Equals(account.Type, "Live"))
                    {
                        broker.LiveAccount = account;
                        broker.DefaultAccountType = "Live";
                        broker.Symbol = data["LiveSymbol"].Split(',');
                        broker.DefaultSymbol = broker.Symbol[0];
                    }
                    account.UserCode = dataAccount["UserCode"];
                    account.Password = dataAccount["Password"];
                    TradeParametre tradePara = new TradeParametre();
                    broker.TradePara = tradePara;
                    account.TradePara = tradePara;
                    tradePara.SellLots = Convert.ToDecimal(dataAccount["Valumn"]);
                    tradePara.BuyLots = Convert.ToDecimal(dataAccount["Valumn"]);
                    tradePara.Timezone = Convert.ToDouble(data["Timezone"]);

                    //托管自动交易标识
                    string autoTradeFlag = DBHelper.getAutoTradeFlag(account.UserCode);
                    if (autoTradeFlag != null && autoTradeFlag != "")
                    {
                        if (string.Equals(autoTradeFlag, "Y"))
                        {
                            tradePara.AutoTrade = true;
                        }
                        else
                        {
                            tradePara.AutoTrade = false;
                        }
                    }

                    //增加默认参数
                    tradePara.Slippage = 10;
                    tradePara.Stoploss = 2;
                    tradePara.AutoLock = false;
                    tradePara.AutoLockPoint = 1;
                    tradePara.AutoLockTimeDuration = 3;
                    tradePara.AutoCloseTimeDuration = "1Mins";
                    tradePara.AutoChecked = false;
                    tradePara.IncreaseFlag = false;
                    tradePara.IncreaseLots = 1;
                    tradePara.NotifyFlag = false;
                }
            }
            return broker;
        }
        /// <summary>
        /// 获取平台列表
        /// </summary>
        /// <returns></returns>
        public static Broker[] getBrokerList()
        {
            var sqlBrokerStr = new StringBuilder();
            sqlBrokerStr.AppendFormat("SELECT b.platform,b.broker_code AS BrokerCode,b.broker_name AS BrokerName,b.live_symbol AS LiveSymbol,b.live_ip AS LiveIP,b.live_port AS LivePort,b.demo_symbol  AS DemoSymbol,b.demo_ip AS DemoIP,b.demo_port AS DemoPort,b.time_zone AS Timezone FROM trade_broker b WHERE b.activate_flag = 'Y'");
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlBrokerStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                Broker[] brokerList = new Broker[datas.Count];
                for (int i = 0; i < datas.Count; i++)
                {
                    IDictionary<string, string> data = datas[i];
                    Broker broker = new Broker();
                    brokerList[i] = broker;
                    broker.PlatformNo = string.Equals(data["platform"], "MT4") ? 2 : 1;
                    broker.BrokerCode = data["BrokerCode"];
                    broker.BrokerName = data["BrokerName"];
                    broker.DemoIP = data["DemoIP"];
                    broker.DemoPort = data["DemoPort"];
                    broker.LiveIP = data["LiveIP"];
                    broker.LivePort = data["LivePort"];
                    //默认品类列表为Live环境
                    broker.Symbol = data["LiveSymbol"].Split(',');
                    broker.DefaultSymbol = broker.Symbol[0];
                    broker.DefaultAccountType = "Live";
                    var sqlAccountStr = new StringBuilder();
                    sqlAccountStr.AppendFormat("SELECT ua.account_code AS UserCode,ua.password AS Password,ua.account_type AS Type,ua.valumn as Valumn,ua.sms_notify AS SMSNotify,ua.email_notify AS EmailNotify FROM trade_user_account ua WHERE ua.activate_flag = 'Y' and ua.broker_code = '" + broker.BrokerCode + "'");
                    List<IDictionary<string, string>> datasAccount = DBUtils.eaQuery(sqlAccountStr.ToString());
                    if (datasAccount != null && datasAccount.Count > 0)
                    {
                        Account account = new Account();
                        IDictionary<string, string> dataAccount = datasAccount[0];
                        account.BrokerCode = broker.BrokerCode;
                        account.Type = dataAccount["Type"];
                        //默认交易品类Live 环境覆盖Demo环境
                        if (string.Equals(account.Type, "Demo"))
                        {
                            broker.DemoAccount = account;
                            broker.DefaultAccountType = "Demo";
                            broker.Symbol = data["DemoSymbol"].Split(',');
                            broker.DefaultSymbol = broker.Symbol[0];
                        }
                        if (string.Equals(account.Type, "Live"))
                        {
                            broker.LiveAccount = account;
                            broker.DefaultAccountType = "Live";
                            broker.Symbol = data["LiveSymbol"].Split(',');
                            broker.DefaultSymbol = broker.Symbol[0];
                        }
                        account.UserCode = dataAccount["UserCode"];
                        account.Password = dataAccount["Password"];
                        TradeParametre tradePara = new TradeParametre();
                        broker.TradePara = tradePara;
                        account.TradePara = tradePara;
                        tradePara.SellLots = Convert.ToDecimal(dataAccount["Valumn"]);
                        tradePara.BuyLots = Convert.ToDecimal(dataAccount["Valumn"]);
                        tradePara.Timezone = Convert.ToDouble(data["Timezone"]);

                        //托管自动交易标识
                        string autoTradeFlag = DBHelper.getAutoTradeFlag(account.UserCode);
                        if (autoTradeFlag != null && autoTradeFlag != "")
                        {
                            if (string.Equals(autoTradeFlag, "Y"))
                            {
                                tradePara.AutoTrade = true;
                            }
                            else
                            {
                                tradePara.AutoTrade = false;
                            }
                        }

                        //增加默认参数
                        tradePara.Slippage = 10;
                        tradePara.Stoploss = 2;
                        tradePara.AutoLock = false;
                        tradePara.AutoLockPoint = 1;
                        tradePara.AutoLockTimeDuration = 3;
                        tradePara.AutoCloseTimeDuration = "1Mins";
                        tradePara.AutoChecked = false;
                        tradePara.IncreaseFlag = false;
                        tradePara.IncreaseLots = 1;
                        tradePara.NotifyFlag = false;
                    }
                }
                return brokerList;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 更新账户登陆/登出信息
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="formNo"></param>
        /// <param name="accountCode"></param>
        /// <param name="loginFlag"></param>
        public static void updateLoginInfo(string ip, string formNo, string accountCode, string loginFlag)
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT ID FROM trade_auto ta WHERE ta.execute_ip = '{0}' AND ta.form_no ='{1}' AND ta.account_code = '{2}'", ip, formNo, accountCode);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                foreach (IDictionary<string, string> data in datas)
                {
                    String id = data["ID"];
                    var sqlStr = new StringBuilder();
                    sqlStr.AppendFormat("UPDATE trade_auto ta SET ta.login_status = '{0}', ta.login_date =  sysdate() where ID = {1}", loginFlag, id);
                    DBUtils.eaExecuteSql(sqlStr.ToString());
                }
            }
        }
        /// <summary>
        /// 更新自动交易信息,启动自动交易/停止自动交易
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="formNo"></param>
        /// <param name="accountCode"></param>
        /// <param name="autoFlag"></param>
        public static void updateAutoInfo(string ip, string formNo, string accountCode, string autoFlag)
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT ID FROM trade_auto ta WHERE ta.execute_ip = '{0}' AND ta.form_no ='{1}' AND ta.account_code = '{2}'", ip, formNo, accountCode);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                foreach (IDictionary<string, string> data in datas)
                {
                    String id = data["ID"];
                    var sqlStr = new StringBuilder();
                    sqlStr.AppendFormat("UPDATE trade_auto ta SET ta.auto_status = '{0}', ta.auto_date =  sysdate() where ID = {1}", autoFlag, id);
                    DBUtils.eaExecuteSql(sqlStr.ToString());
                }
            }
        }
        /// <summary>
        /// 获取策略IP
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="formNo"></param>
        /// <returns></returns>
        public static IDictionary<string, string> getAutoInfo(string ip, string formNo)
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT ta.ea_ip,ua.broker_code,CONCAT('EA_',ua.ea_type) as ea_type FROM trade_auto ta, trade_user_account ua WHERE ta.account_code = ua.account_code AND ta.execute_ip = '{0}' AND ta.form_no ='{1}' ", ip, formNo);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                return datas[0];
            }
            return null;
        }
        /// <summary>
        /// 获取托管交易配置信息
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static List<IDictionary<string, string>> getAutoList(string ip)
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT ta.ea_ip,ta.form_no,ua.broker_code FROM trade_auto ta, trade_user_account ua WHERE ta.account_code = ua.account_code AND ta.execute_ip = '{0}' AND ua.activate_flag = 'Y' AND ua.auto_flag = 'Y' AND ta.auto_trade = 'Y' ", ip);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            return datas;
        }
        /// <summary>
        /// 查询Broker信息
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="formNo"></param>
        /// <returns></returns>
        public static IDictionary<string, string> getAutoBrokerInfo(string ip, string formNo)
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT b.platform, b.broker_code,b.broker_name FROM trade_auto a, trade_user_account ua,trade_broker b WHERE a.execute_ip = '{0}' AND a.form_no = '{1}' AND a.account_code = ua.account_code AND ua.broker_code = b.broker_code ", ip, formNo);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                return datas[0];
            }
            return null;
        }

        /// <summary>
        /// 获取托管交易手数
        /// </summary>
        /// <param name="accountCode"></param>
        /// <returns></returns>
        public static String getAutoValumn(string accountCode)
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT ua.valumn FROM trade_user_account ua WHERE ua.account_code = '{0}' ", accountCode);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                return datas[0]["valumn"];
            }
            return null;
        }
        /// <summary>
        /// 获取自动交易标识
        /// </summary>
        /// <param name="accountCode"></param>
        /// <returns></returns>
        public static String getAutoTradeFlag(string accountCode)
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT a.auto_trade FROM trade_auto a WHERE a.account_code = '{0}' ", accountCode);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                return datas[0]["auto_trade"];
            }
            return null;
        }

        /// <summary>
        /// 获取托管策略
        /// </summary>
        /// <param name="accountCode"></param>
        /// <returns></returns>
        public static String getAutoEAType(string accountCode)
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT ua.ea_type FROM trade_user_account ua WHERE ua.account_code = '{0}' ", accountCode);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                return datas[0]["ea_type"];
            }
            return null;
        }
        /// <summary>
        /// 保存Daily数据
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="buyValume"></param>
        /// <param name="sellValume"></param>
        public static void SaveDailyData(string pt, int buyValume, int sellValume)
        {
            var sqlStr = new StringBuilder();
            sqlStr.Append("insert into trade_daily_sum(symbol,pt,buy_valume,sell_valume,create_time,ds_name) VALUES");
            sqlStr.AppendFormat("('{0}','{1}','{2}','{3}',{4},'{5}')", "gold", pt, buyValume, sellValume, "sysdate()", "JR");
            DBUtils.executeSql(sqlStr.ToString());
        }
        /// <summary>
        /// 保存Hourly数据
        /// </summary>
        /// <param name="time"></param>
        /// <param name="pt"></param>
        /// <param name="buyValume"></param>
        /// <param name="sellValume"></param>
        public static void SaveHourlyData(string time, string pt, int buyValume, int sellValume)
        {
            var sqlStr = new StringBuilder();
            sqlStr.Append("insert into trade_daily_detail(symbol,time,pt,buy_valume,sell_valume,create_time,ds_name) VALUES");
            sqlStr.AppendFormat("('{0}','{1}','{2}','{3}','{4}',{5},'{6}')", "gold", time, pt, buyValume, sellValume, "sysdate()", "JR");
            DBUtils.executeSql(sqlStr.ToString());
        }
        /// <summary>
        /// 保存EA指令
        /// </summary>
        /// <param name="eaNo"></param>
        /// <param name="autoFlag"></param>
        /// <param name="eaParas"></param>
        /// <param name="eaValue"></param>
        /// <param name="eaResult"></param>
        /// <param name="eaCommand"></param>
        /// <param name="eaType"></param>
        public static void saveEACommand(string eaNo, string autoFlag, string eaParas, string eaValue, string eaResult, string eaCommand, string eaType, string eaTradeId)
        {
            // ------------------------------------------------------------------
            // 核心修改：使用传统的 null 检查和 if 语句来安全处理 eaResult
            // ------------------------------------------------------------------
            string safeEaResult;

            if (string.IsNullOrEmpty(eaResult))
            {
                // 如果 eaResult 是 null 或空字符串，则使用空字符串
                safeEaResult = string.Empty;
            }
            else
            {
                // 否则，执行替换操作
                safeEaResult = eaResult.Replace("\r\n", " ").Replace("\n", " ");
            }
            // 1. 定义 SQL 插入语句，使用命名参数 (@paramName)
            const string sql = @"
            INSERT INTO trade_ea (
                ea_no, auto_flag, ea_paras, ea_value, ea_result, 
                ea_command, create_time, server_id, ea_type,ea_tradeid
            )
            VALUES (
                @eaNo, @autoFlag, @eaParas, @eaValue, @eaResult, 
                @eaCommand, @createTime, @serverId, @eaType , @eaTradeId
            )";

            // 2. 创建参数字典/列表，将参数名和值映射起来
            // 注意：DBUtils.getDateTime() 和 uClient.Comm.Utils.GetLocalIP() 应该在 C# 客户端获取。
            var parameters = new Dictionary<string, object>
        {
            // 字符串/JSON 数据参数
            { "@eaNo", eaNo },
            { "@autoFlag", autoFlag },
            { "@eaParas", eaParas }, // 您的 JSON 数据，这里应该使用数据库支持的大文本类型（如 MEDIUMTEXT）
            { "@eaValue", eaValue }, // 您的 JSON 数据
            { "@eaResult", safeEaResult }, // 您的 JSON 数据
            { "@eaCommand", eaCommand },
            { "@eaType", eaType },
            { "@eaTradeId", eaTradeId },
            
            // 时间和服务器 ID 参数
            { "@createTime", DBUtils.getDateTime() },         // 假设 getDateTime() 返回一个可被数据库识别的日期/时间字符串或 DateTime 对象
            { "@serverId", uClient.Comm.Utils.GetLocalIP() }
        };

            // 3. 调用 DBUtils 中执行参数化查询的方法
            // 假设 DBUtils 有一个方法可以接受 SQL 语句和参数字典
            DBUtils.ExecuteParameterizedSql(sql, parameters);
        }
        /// <summary>
        /// 保存回测数据
        /// </summary>
        /// <param name="eaNo"></param>
        /// <param name="eaParas"></param>
        /// <param name="eaCommand"></param>
        /// <param name="price"></param>
        /// <param name="quotaTime"></param>
        /// <param name="eaValue"></param>
        /// <param name="eaType"></param>
        /// <param name="eaTime"></param>
        /// <param name="profitPoint"></param>
        public static void saveEATestCommand(string eaNo, string eaParas, string eaValue, string eaResult, string eaCommand, string price, string quotaTime, string testDuration, string keepProfitDiff, string eaType, double profitPoint, string batchNo, string eaTradeId)
        {
            // ------------------------------------------------------------------
            // 核心修改：使用传统的 null 检查和 if 语句来安全处理 eaResult
            // ------------------------------------------------------------------
            string safeEaResult;

            if (string.IsNullOrEmpty(eaResult))
            {
                // 如果 eaResult 是 null 或空字符串，则使用空字符串
                safeEaResult = string.Empty;
            }
            else
            {
                // 否则，执行替换操作
                safeEaResult = eaResult.Replace("\r\n", " ").Replace("\n", " ");
            }

            // 1. 定义 SQL 插入语句，使用命名参数 (@paramName)
            const string sql = @"
            INSERT INTO trade_test_result (
                ea_no, ea_paras, ea_value, ea_result, price, quota_time, 
                test_duration, keep_profit_diff, ea_type, ea_command, 
                profit_point, create_time, batch_no,ea_tradeid
            )
            VALUES (
                @eaNo, @eaParas, @eaValue, @eaResult, @price, @quotaTime, 
                @testDuration, @keepProfitDiff, @eaType, @eaCommand, 
                @profitPoint, SYSDATE(), @batchNo, @eaTradeId
            )";

            // 2. 将字符串参数 quotaTime 转换为 DateTime 对象（如果数据库接受 DateTime 类型）
            DateTime quotaDateTime;
            if (!DateTime.TryParse(quotaTime, out quotaDateTime))
            {
                // 失败处理：如果转换失败，可以使用当前时间或抛出异常
                quotaDateTime = DateTime.Now;
            }

            // 3. 创建参数字典/列表，将参数名和值映射起来
            var parameters = new Dictionary<string, object>
        {
            // 字符串/JSON 参数 (eaParas, eaValue, eaResult 通常包含大量文本或 JSON)
            { "@eaNo", eaNo },
            { "@eaParas", eaParas },
            { "@eaValue", eaValue },
            { "@eaResult", safeEaResult }, 
            
            // 数值类型参数
            { "@price", price },
            { "@keepProfitDiff", keepProfitDiff },
            { "@profitPoint", profitPoint },

            // 其他参数
            { "@quotaTime", quotaDateTime }, // 传递 DateTime 对象
            { "@testDuration", testDuration },
            { "@eaType", eaType },
            { "@eaCommand", eaCommand },
            { "@batchNo", batchNo },
            { "@eaTradeId", eaTradeId }
            
            // 注意：create_time 使用 SQL 函数 SYSDATE() 或 GETDATE()，不作为参数传递
        };

            // 4. 调用 DBUtils 中执行参数化查询的方法
            // 假设 DBUtils 有一个方法可以接受 SQL 语句和参数字典
            DBUtils.ExecuteParameterizedSql(sql, parameters);
        }
        /// <summary>
        /// 删除回测数据
        /// </summary>
        /// <param name="testDuration"></param>
        /// <param name="eaType"></param>
        public static void deleteTestData(string batchNo, int retestStartNum, int retestEndNum)
        {
            string deletedata = "delete from trade_test_result where batch_no = '" + batchNo + "' and ea_no>=" + retestStartNum + " and ea_no<= " + retestEndNum + " and id>0";
            //删除数据
            DBUtils.executeSql(deletedata);
        }
        /// <summary>
        /// 获取测试结果
        /// </summary>
        /// <param name="timeDuration"></param>
        /// <returns></returns>
        public static List<IDictionary<string, string>> getTestResult(string eaType, string timeDuration, string keepProfitDiff, string testBatchNo)
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT r.ea_no, count(id) as orderCount, SUM(IF(profit_point>0,1,0)) as profitOrderCount,SUM(IF(profit_point<=0,1,0)) as lostOrderCount,GROUP_CONCAT(profit_point SEPARATOR ', ') AS orderList, ROUND(sum(profit_point),2) as points FROM trade_test_result r WHERE r.ea_type = '{0}' AND r.test_duration = '{1}' AND r.keep_profit_diff = '{2}' AND r.batch_no = '{3}' AND SUBSTRING(r.ea_command, 1, 6) = 'CLOSE_' GROUP BY r.ea_no", eaType, timeDuration, keepProfitDiff, testBatchNo);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            return datas;
        }
        /// <summary>
        /// 生成提醒
        /// </summary>
        /// <param name="notifyType"></param>
        /// <param name="notifyId">EA Type</param>
        /// <param name="notifyContent"></param>
        /// <param name="userCode"></param>
        /// <param name="accountCode"></param>
        public static void saveNotify(string notifyType, string notifyId, string notifyContent, string userCode, string accountCode)
        {
            var sqlStr = new StringBuilder();
            sqlStr.Append("INSERT INTO trade_notify (notify_type,notify_id,notify_content,user_code,account_code,email_status,sms_status,create_by,create_date,tenant_id)  VALUES");
            sqlStr.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}',{8},{9})", notifyType, notifyId, notifyContent, userCode, accountCode, "1", "1", "system", "sysdate()", "1");
            DBUtils.eaExecuteSql(sqlStr.ToString());
        }
        /// <summary>
        /// 查询时间段内有多少订单
        /// </summary>
        /// <param name="eaType"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static int getOrderCountForDuration(string eaType, string startDate, string endDate)
        {

            var sqlStr = new StringBuilder();
            sqlStr.AppendFormat("SELECT COUNT(1) AS ordercount FROM trade_ea ea WHERE ea.ea_type = '{0}' AND (ea.ea_command like '%:CLOSE_BUY:%' OR ea.ea_command like '%:CLOSE_SELL:%') AND ea.create_time BETWEEN '{1}' AND '{2}'", eaType, startDate, endDate);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                return int.Parse(datas[0]["ordercount"]);
            }
            return 0;
        }
        public static int getOrderCountForDurationForTest(string testBatchNo, string selectedStrategy, string eaType, string startDate, string endDate)
        {

            var sqlStr = new StringBuilder();
            sqlStr.AppendFormat("SELECT COUNT(1) AS ordercount FROM trade_test_result ea WHERE ea.batch_no = '{0}' AND ea.ea_no = '{1}' AND ea.ea_type = '{2}' AND (ea.ea_command like '%:CLOSE_BUY:%' OR ea.ea_command like '%:CLOSE_SELL:%') AND ea.quota_time BETWEEN '{3}' AND '{4}'", testBatchNo, selectedStrategy, eaType, startDate, endDate);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                return int.Parse(datas[0]["ordercount"]);
            }
            return 0;
        }
        /// <summary>
        /// 判断当天总亏损单数是否已经超过给定值
        /// </summary>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="lossCount"></param>
        /// <returns></returns>
        public static bool getOrderCountForContinuousLossesDaily(string eaType, string currentDateTimeStr, int lossCount)
        {
            var sqlStr = new StringBuilder();
            sqlStr.AppendFormat("SELECT SUM(CASE WHEN tab.loss_value < 0 THEN 1 ELSE 0 END) AS negative_count FROM (SELECT SUBSTRING_INDEX(SUBSTRING_INDEX(ea.ea_command, ':', -3), ':', 1) AS loss_value FROM trade_ea ea WHERE ea.ea_type = '{0}' AND (ea.ea_command like '%:CLOSE_BUY:%' OR ea.ea_command like '%:CLOSE_SELL:%') AND ea.create_time BETWEEN  DATE(STR_TO_DATE('{1}', '%Y-%m-%d %H:%i:%s')) AND DATE(STR_TO_DATE('{1}', '%Y-%m-%d %H:%i:%s')) + INTERVAL 1 DAY - INTERVAL 1 SECOND ORDER BY ea.create_time DESC  LIMIT {2}) tab", eaType, currentDateTimeStr, lossCount);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                if (string.IsNullOrEmpty(datas[0]["negative_count"]))
                {
                    return false;
                }
                int negativeCount = int.Parse(datas[0]["negative_count"]);
                if (negativeCount == lossCount)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断当天总亏损单数是否已经超过给定值
        /// </summary>
        /// <param name="testBatchNo"></param>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="lossCount"></param>
        /// <returns></returns>
        public static bool getOrderCountForContinuousLossesDailyForTest(string testBatchNo, string selectedStrategy, string eaType, string currentDateTimeStr, int lossCount)
        {
            var sqlStr = new StringBuilder();
            sqlStr.AppendFormat("SELECT SUM(CASE WHEN tab.loss_value < 0 THEN 1 ELSE 0 END) AS negative_count FROM (SELECT SUBSTRING_INDEX(SUBSTRING_INDEX(ea.ea_command, ':', -3), ':', 1) AS loss_value FROM trade_test_result ea WHERE  ea.batch_no = '{0}' AND ea.ea_no = '{1}' AND ea.ea_type = '{2}' AND (ea.ea_command like '%:CLOSE_BUY:%'  OR ea.ea_command like '%:CLOSE_SELL:%') AND ea.quota_time BETWEEN  DATE(STR_TO_DATE('{3}', '%Y-%m-%d %H:%i:%s')) AND DATE(STR_TO_DATE('{3}', '%Y-%m-%d %H:%i:%s')) + INTERVAL 1 DAY - INTERVAL 1 SECOND ORDER BY ea.quota_time DESC  LIMIT {4}) tab", testBatchNo, selectedStrategy, eaType, currentDateTimeStr, lossCount);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                if (string.IsNullOrEmpty(datas[0]["negative_count"]))
                {
                    return false;
                }
                int negativeCount = int.Parse(datas[0]["negative_count"]);
                if (negativeCount == lossCount)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断本周总亏损单数是否已经超过给定值
        /// </summary>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="lossCount"></param>
        /// <returns></returns>
        public static bool getOrderCountForTotalLossesWeekly(string eaType, string currentDateTimeStr, int lossCount)
        {
            var sqlStr = new StringBuilder();
            sqlStr.AppendFormat("SELECT SUM(CASE WHEN tab.loss_value < 0 THEN 1 ELSE 0 END) AS negative_count FROM (SELECT SUBSTRING_INDEX(SUBSTRING_INDEX(ea.ea_command, ':', -3), ':', 1) AS loss_value FROM trade_ea ea WHERE ea.ea_type = '{0}' AND (ea.ea_command like '%:CLOSE_BUY:%' OR ea.ea_command like '%:CLOSE_SELL:%') AND ea.create_time BETWEEN  DATE_FORMAT(DATE_SUB(STR_TO_DATE('{1}', '%Y-%m-%d %H:%i:%s'), INTERVAL WEEKDAY(STR_TO_DATE('{1}', '%Y-%m-%d %H:%i:%s')) DAY),'%Y-%m-%d 00:00:00') AND DATE_ADD( DATE_SUB(STR_TO_DATE('{1}', '%Y-%m-%d %H:%i:%s'), INTERVAL WEEKDAY(STR_TO_DATE('{1}', '%Y-%m-%d %H:%i:%s')) DAY),  INTERVAL 6 DAY) + INTERVAL '23:59:59' HOUR_SECOND ORDER BY ea.create_time DESC  LIMIT {2}) tab", eaType, currentDateTimeStr, lossCount);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                if (string.IsNullOrEmpty(datas[0]["negative_count"]))
                {
                    return false;
                }
                int negativeCount = int.Parse(datas[0]["negative_count"]);
                if (negativeCount == lossCount)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断本周总亏损单数是否已经超过给定值
        /// </summary>
        /// <param name="testBatchNo"></param>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="lossCount"></param>
        /// <returns></returns>
        public static bool getOrderCountForTotalLossesWeeklyForTest(string testBatchNo, string selectedStrategy, string eaType, string currentDateTimeStr, int lossCount)
        {
            var sqlStr = new StringBuilder();
            sqlStr.AppendFormat("SELECT SUM(CASE WHEN tab.loss_value < 0 THEN 1 ELSE 0 END) AS negative_count FROM (SELECT SUBSTRING_INDEX(SUBSTRING_INDEX(ea.ea_command, ':', -3), ':', 1) AS loss_value FROM trade_test_result ea WHERE  ea.batch_no = '{0}' AND ea.ea_no ='{1}' AND ea.ea_type = '{2}' AND (ea.ea_command like '%:CLOSE_BUY:%'  or ea.ea_command like '%:CLOSE_SELL:%') AND ea.quota_time BETWEEN DATE_FORMAT(DATE_SUB(STR_TO_DATE('{3}', '%Y-%m-%d %H:%i:%s'), INTERVAL WEEKDAY(STR_TO_DATE('{3}', '%Y-%m-%d %H:%i:%s')) DAY),'%Y-%m-%d 00:00:00') AND DATE_ADD( DATE_SUB(STR_TO_DATE('{3}', '%Y-%m-%d %H:%i:%s'), INTERVAL WEEKDAY(STR_TO_DATE('{3}', '%Y-%m-%d %H:%i:%s')) DAY),  INTERVAL 6 DAY) + INTERVAL '23:59:59' HOUR_SECOND ORDER BY ea.quota_time DESC  LIMIT {4}) tab", testBatchNo, selectedStrategy, eaType, currentDateTimeStr, lossCount);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                if (string.IsNullOrEmpty(datas[0]["negative_count"]))
                {
                    return false;
                }
                int negativeCount = int.Parse(datas[0]["negative_count"]);
                if (negativeCount == lossCount)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断本周连续亏损单数是否已经超过给定值
        /// </summary>
        /// <param name="testBatchNo"></param>
        /// <param name="selectedStrategy"></param>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="lossCount"></param>
        /// <returns></returns>
        public static bool getOrderCountForContinuousLossesWeeklyForTest(string testBatchNo, string selectedStrategy, string eaType, string currentDateTimeStr, int lossCount)
        {
            var sqlStr = new StringBuilder();
            sqlStr.AppendFormat("SELECT SUBSTRING_INDEX(SUBSTRING_INDEX(ea.ea_command, ':', -3), ':', 1) AS loss_value FROM trade_test_result ea WHERE  ea.batch_no = '{0}' AND ea.ea_no ='{1}' AND ea.ea_type = '{2}' AND (ea.ea_command like '%:CLOSE_BUY:%'  or ea.ea_command like '%:CLOSE_SELL:%') AND ea.quota_time BETWEEN DATE_FORMAT(DATE_SUB(STR_TO_DATE('{3}', '%Y-%m-%d %H:%i:%s'), INTERVAL WEEKDAY(STR_TO_DATE('{3}', '%Y-%m-%d %H:%i:%s')) DAY),'%Y-%m-%d 00:00:00') AND DATE_ADD( DATE_SUB(STR_TO_DATE('{3}', '%Y-%m-%d %H:%i:%s'), INTERVAL WEEKDAY(STR_TO_DATE('{3}', '%Y-%m-%d %H:%i:%s')) DAY),  INTERVAL 6 DAY) + INTERVAL '23:59:59' HOUR_SECOND ORDER BY ea.quota_time DESC  LIMIT {4}", testBatchNo, selectedStrategy, eaType, currentDateTimeStr, lossCount);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlStr.ToString());
            if (datas != null && datas.Count == lossCount)
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    double negativeCount = Double.Parse(datas[i]["loss_value"]);
                    if (negativeCount > 0)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断本周连续亏损单数是否已经超过给定值
        /// </summary>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="lossCount"></param>
        /// <returns></returns>
        public static bool getOrderCountForContinuousLossesWeekly(string eaType, string currentDateTimeStr, int lossCount)
        {
            var sqlStr = new StringBuilder();
            sqlStr.AppendFormat("SELECT SUBSTRING_INDEX(SUBSTRING_INDEX(ea.ea_command, ':', -3), ':', 1) AS loss_value FROM trade_ea ea WHERE ea.ea_type = '{0}' AND (ea.ea_command like '%:CLOSE_BUY:%' OR ea.ea_command like '%:CLOSE_SELL:%') AND ea.create_time BETWEEN  DATE_FORMAT(DATE_SUB(STR_TO_DATE('{1}', '%Y-%m-%d %H:%i:%s'), INTERVAL WEEKDAY(STR_TO_DATE('{1}', '%Y-%m-%d %H:%i:%s')) DAY),'%Y-%m-%d 00:00:00') AND DATE_ADD( DATE_SUB(STR_TO_DATE('{1}', '%Y-%m-%d %H:%i:%s'), INTERVAL WEEKDAY(STR_TO_DATE('{1}', '%Y-%m-%d %H:%i:%s')) DAY),  INTERVAL 6 DAY) + INTERVAL '23:59:59' HOUR_SECOND ORDER BY ea.create_time DESC  LIMIT {2}", eaType, currentDateTimeStr, lossCount);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlStr.ToString());
            if (datas != null && datas.Count == lossCount)
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    double negativeCount = Double.Parse(datas[i]["loss_value"]);
                    if (negativeCount > 0)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 本周盈亏值
        /// </summary>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="sumLossPoint"></param>
        /// <returns></returns>
        public static double getOrderCountForSumLossesWeekly(string eaType, string currentDateTimeStr)
        {
            var sqlStr = new StringBuilder();
            sqlStr.AppendFormat("SELECT SUM(tab.profit_value) AS sum_profit FROM (SELECT SUBSTRING_INDEX(SUBSTRING_INDEX(ea.ea_command, ':', -3), ':', 1) AS profit_value FROM trade_ea ea WHERE ea.ea_type = '{0}' AND (ea.ea_command like '%:CLOSE_BUY:%' OR ea.ea_command like '%:CLOSE_SELL:%') AND ea.create_time BETWEEN  DATE_FORMAT(DATE_SUB(STR_TO_DATE('{1}', '%Y-%m-%d %H:%i:%s'), INTERVAL WEEKDAY(STR_TO_DATE('{1}', '%Y-%m-%d %H:%i:%s')) DAY),'%Y-%m-%d 00:00:00') AND DATE_ADD( DATE_SUB(STR_TO_DATE('{1}', '%Y-%m-%d %H:%i:%s'), INTERVAL WEEKDAY(STR_TO_DATE('{1}', '%Y-%m-%d %H:%i:%s')) DAY),  INTERVAL 6 DAY) + INTERVAL '23:59:59' HOUR_SECOND ORDER BY ea.create_time DESC) tab", eaType, currentDateTimeStr);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                if (string.IsNullOrEmpty(datas[0]["sum_profit"]))
                {
                    return 0;
                }
                else
                {
                    return Double.Parse(datas[0]["sum_profit"]); ;
                }
            }
            return 0;
        }
        /// <summary>
        /// 本周盈亏值
        /// </summary>
        /// <param name="testBatchNo"></param>
        /// <param name="selectedStrategy"></param>
        /// <param name="eaType"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <param name="sumLossPoint"></param>
        /// <returns></returns>
        public static double getOrderCountForSumLossesWeeklyForTest(string testBatchNo, string selectedStrategy, string eaType, string currentDateTimeStr)
        {
            var sqlStr = new StringBuilder();
            sqlStr.AppendFormat("SELECT SUM(tab.profit_value) AS sum_profit FROM (SELECT SUBSTRING_INDEX(SUBSTRING_INDEX(ea.ea_command, ':', -3), ':', 1) AS profit_value FROM trade_test_result ea WHERE  ea.batch_no = '{0}' AND ea.ea_no ='{1}' AND ea.ea_type = '{2}' AND (ea.ea_command like '%:CLOSE_BUY:%' OR ea.ea_command like '%:CLOSE_SELL:%') AND ea.quota_time BETWEEN DATE_FORMAT(DATE_SUB(STR_TO_DATE('{3}', '%Y-%m-%d %H:%i:%s'), INTERVAL WEEKDAY(STR_TO_DATE('{3}', '%Y-%m-%d %H:%i:%s')) DAY),'%Y-%m-%d 00:00:00') AND DATE_ADD( DATE_SUB(STR_TO_DATE('{3}', '%Y-%m-%d %H:%i:%s'), INTERVAL WEEKDAY(STR_TO_DATE('{3}', '%Y-%m-%d %H:%i:%s')) DAY),  INTERVAL 6 DAY) + INTERVAL '23:59:59' HOUR_SECOND ORDER BY ea.quota_time DESC) tab", testBatchNo, selectedStrategy, eaType, currentDateTimeStr);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                if (string.IsNullOrEmpty(datas[0]["sum_profit"]))
                {
                    return 0;
                }
                else
                {
                    return Double.Parse(datas[0]["sum_profit"]); ;
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取系统配置参数
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> getSysConfig()
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT sc.name,sc.value FROM unieap_sys_config sc where sc.activate_flag = '{0}'", "Y");
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            Dictionary<string, string> data = new Dictionary<string, string>();
            if (datas != null && datas.Count > 0)
            {
                foreach (var dataItem in datas)
                {
                    data.Add(dataItem["name"], dataItem["value"]);
                }
            }
            return data;
        }

        /// <summary>
        /// 保存黄金和白银报价
        /// </summary>
        /// <param name="goldPrices"></param>
        /// <param name="silverPrices"></param>
        /// <param name="dsType"></param>
        public static void saveQuoteGS(double goldPrices, double silverPrices, string dsType)
        {
            dsType = "dukascopy";
            string dateTime = DBUtils.getDateTime();
            var sqlStrGold = new StringBuilder();
            sqlStrGold.Append("INSERT INTO TRADE_QUOTE(SYMBOL,PRICE,TIME,CREATE_TIME,DS_NAME,GOLD_PRICE,SILVER_PRICE) VALUES");
            sqlStrGold.AppendFormat("('{0}','{1}','{2}','{3}','{4}','{5}','{6}')", "gold", goldPrices, dateTime, dateTime, dsType, goldPrices, silverPrices);
            DBUtils.executeSql(sqlStrGold.ToString());
        }
        /// <summary>
        /// 清除情绪指数数据
        /// </summary>
        public static void ClearData()
        {
            _logger.Info("清除数据开始");
            DBUtils.executeSql("SET SQL_SAFE_UPDATES = 0");
            DBUtils.executeSql("delete from trade_daily_detail");
            DBUtils.executeSql("delete from trade_daily_sum");
            DBUtils.executeSql("delete from trade_latest");
            DBUtils.executeSql("delete from trade_price");
            DBUtils.executeSql("delete from trade_sum");
            DBUtils.executeSql("delete from trade_single_sum");
            DBUtils.executeSql("delete from trade_single_transaction");
            _logger.Info("清除数据完成");
        }

        /// <summary>
        /// 保存下注记录,先删除全部数据再保存
        /// </summary>
        /// <param name="data"></param>
        /// <param name="raceId"></param>
        /// <param name="currentRaceDate"></param>
        public static void saveTickets(List<Dictionary<string, object>> data, string raceId, string currentRaceDate)
        {
            DBUtils.executeSql("delete from TRADE_HORSE_TICKETS where race_id = '" + raceId + "' AND race_date= '" + currentRaceDate + "'");
            DBUtils.bulkInsert("TRADE_HORSE_TICKETS", data);
        }


        /// <summary>
        /// 获取K线数据
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="period"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<Candle> GetRecentCandles(string symbol, string period, int count)
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT * FROM (SELECT * FROM trade_kline_data WHERE symbol = '{0}' AND period = '{1}' ORDER BY open_time DESC  LIMIT {2}) tab ORDER BY tab.open_time ASC", symbol, period, count);
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
            sqlQueryStr.AppendFormat("SELECT * FROM (SELECT * FROM trade_kline_data WHERE symbol = '{0}' AND period = '{1}' AND open_time >= TIMESTAMP('{2}') ORDER BY open_time DESC  LIMIT {3}) tab ORDER BY tab.open_time ASC", symbol, period, currentDateTimeStr, count);
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
        /// 获取最新的报价数据
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="period"></param>
        /// <param name="count"></param>
        /// <param name="currentDateTimeStr"></param>
        /// <returns></returns>
        public static List<IDictionary<string, string>> GetQuotaStatus(string dsName, int count, string currentDateTimeStr)
        {
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat("SELECT * FROM trade_quote WHERE ds_name = '{0}' AND create_time >= TIMESTAMP('{1}') ORDER BY create_time DESC LIMIT {2}", dsName, currentDateTimeStr, count);
            List<IDictionary<string, string>> datas = DBUtils.query(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                return datas;
            }
            return null;
        }
    }
}
