using Newtonsoft.Json;
using NLog;
using System.Collections.Concurrent;
using System.Text;
namespace AutoHorseRace.Utils
{
    /// <summary>
    /// DB 操作
    /// </summary>
    public class DBHelper
    {
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        // 🔧 新增：按业务维度的细粒度锁，防止并发 "先查后写" 产生重复记录。
        // 与 ComboTradeState 里的锁思路保持一致——把 "查询是否已存在" + "决定INSERT还是UPDATE"
        // 这一整段当成一个不可分割的临界区，同一个 key（同一个combo / 同一笔明细）在任意时刻
        // 只允许一个线程执行，避免两个并发线程都查到"不存在"从而各自 INSERT 出重复行。
        private static readonly ConcurrentDictionary<string, object> _recordLocks = new ConcurrentDictionary<string, object>();

        private static object GetLock(string key)
        {
            return _recordLocks.GetOrAdd(key, _ => new object());
        }

        /// <summary>
        /// hr_trade_record 维度的锁：账户+场次+盘口类型+combo 唯一确定一条记录
        /// </summary>
        private static object GetRecordLock(string accountCode, string raceType, string raceDate, string raceNo, string type, string combo)
        {
            return GetLock($"REC|{accountCode}|{raceType}|{raceDate}|{raceNo}|{type}|{combo}");
        }

        /// <summary>
        /// hr_trade_detail 维度的锁：与 checkBettingDetailExist 的判重条件保持一致
        /// </summary>
        private static object GetDetailLock(int tradeRecordId, string autoFlag, string action, string odds, string status)
        {
            return GetLock($"DET|{tradeRecordId}|{autoFlag}|{action}|{odds}|{status}");
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
        /// 保存交易日志
        /// </summary>
        /// <param name="account"></param>
        /// <param name="raceType"></param>
        /// <param name="raceDate"></param>
        /// <param name="raceNo"></param>
        /// <param name="localAllOrders"></param>
        /// <param name="autoFlag"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static int SaveTradeLogsBatch(
            Account account,
            string raceType,
            string raceDate,
            string raceNo,
            ConcurrentDictionary<string, EatBetInfo> localAllOrders,
            IDictionary<string, string> betInfo,
            string autoFlag,
            string remark)
        {
            if (localAllOrders == null || localAllOrders.Count == 0)
            {
                return 0;
            }
            string userCode = account.UserCode;
            int processedCount = 0;
            string serverIp = Utils.GetLocalIP();
            string defaultRemark = string.IsNullOrEmpty(remark) ? "Batch Trade Sync" : remark;
            try
            {
                // 🔥 修正：遍历 localAllOrders 的 Values（EatBetInfo 对象集合）
                foreach (var info in localAllOrders.Values)
                {
                    if (info == null) continue;

                    // 🔧 关键修复：把"查询是否存在" + "INSERT或UPDATE"整体锁住，
                    // 防止并发下（例如BET/EAT两条流程同时处理同一个combo）出现
                    // 两个线程都查到 record == null，从而各自INSERT出重复行。
                    object recordLock = GetRecordLock(userCode, raceType, raceDate, raceNo, info.type, info.combo);
                    lock (recordLock)
                    {
                        // 1. 查询数据库中是否已存在该账户、场次、盘口类型和组合的记录
                        IDictionary<string, string> record = queryBettingInfo(
                            userCode,
                            raceType,
                            raceDate,
                            raceNo,
                            info.type,
                            info.combo
                        );
                        int tradeRecordId = 0;
                        if (record != null)
                        {
                            // ==================== 2. 已存在：执行更新操作 (UPDATE) ====================
                            tradeRecordId = Convert.ToInt32(record["seq"]);
                            string sqlUpdate = @"UPDATE hr_trade_record                              
                                     SET eat_pending_amount = @eat_pending_amount,                                  
                                         eat_executed_amount = @eat_executed_amount,                                  
                                         total_eat_amount = @total_eat_amount,                                  
                                         eat_odds = @eat_odds,                                  
                                         bet_pending_amount = @bet_pending_amount,                                  
                                         bet_executed_amount = @bet_executed_amount,                                  
                                         total_bet_amount = @total_bet_amount,                                  
                                         bet_odds = @bet_odds,                                  
                                         modify_time = SYSDATE(),                                  
                                         remark = @remark                              
                                     WHERE id = @id";
                            var updateParams = new Dictionary<string, object>
                                {
                                    { "@eat_pending_amount",   info.eatPendingAmount },
                                    { "@eat_executed_amount",  info.eatExecutedAmount },
                                    { "@total_eat_amount",     info.totalEatAmount },
                                    { "@eat_odds",             info.eatOdds },
                                    { "@bet_pending_amount",   info.betPendingAmount },
                                    { "@bet_executed_amount",  info.betExecutedAmount },
                                    { "@total_bet_amount",     info.totalBetAmount },
                                    { "@bet_odds",             info.betOdds },
                                    { "@remark",               defaultRemark },
                                    { "@id",                   tradeRecordId }
                                };
                            DBUtils.ExecuteParameterizedSql(sqlUpdate, updateParams);
                            _logger.Debug($"[更新][hr_trade_record]完成,Info:{JsonConvert.SerializeObject(info, Formatting.Indented)}");
                        }
                        else
                        {
                            string intertSql = @"
                            INSERT INTO `hr_trade_record` (
                                `platform_code`, `broker_code`, `broker_name`, `account_code`, 
                                `auto_flag`, `race_date`, `race_type`, `race_no`, 
                                `type`, `combo`, `toto`, `eat_odds`, `bet_odds`, 
                                `eat_executed_amount`, `eat_pending_amount`, `bet_executed_amount`, `bet_pending_amount`, 
                                `total_eat_amount`, `total_bet_amount`, `create_time`, `server_id`, `remark`
                            ) VALUES (
                                @platform_code, @broker_code, @broker_name, @account_code, 
                                @auto_flag, @race_date, @race_type, @race_no, 
                                @type, @combo, @toto, @eat_odds, @bet_odds, 
                                @eat_executed_amount, @eat_pending_amount, @bet_executed_amount, @bet_pending_amount, 
                                @total_eat_amount, @total_bet_amount, @create_time, @server_id, @remark
                            )";
                            // ==================== 3. 不存在：执行单条插入操作 (INSERT) ====================
                            var singleInsert = new Dictionary<string, object>
                            {
                                { "platform_code",       account.BrokerName },
                                { "broker_code",         account.BrokerCode },
                                { "broker_name",         account.BrokerName },
                                { "account_code",        userCode },
                                { "auto_flag",           autoFlag },
                                { "race_date",           raceDate },
                                { "race_type",           raceType },
                                { "race_no",             raceNo },
                                { "type",                info.type },
                                { "combo",               info.combo },
                                { "toto",                info.toto },
                                { "eat_odds",            info.eatOdds },
                                { "bet_odds",            info.betOdds },
                                { "eat_executed_amount", info.eatExecutedAmount },
                                { "eat_pending_amount",  info.eatPendingAmount },
                                { "bet_executed_amount", info.betExecutedAmount },
                                { "bet_pending_amount",  info.betPendingAmount },
                                { "total_eat_amount",    info.totalEatAmount },
                                { "total_bet_amount",    info.totalBetAmount },
                                { "create_time",         DateTime.Now },
                                { "server_id",           serverIp },
                                { "remark",              defaultRemark }
                            };
                            tradeRecordId = DBUtils.ExecuteInsertAndGetId(intertSql, singleInsert);
                            _logger.Debug($"[插入][hr_trade_record]完成,Info:{JsonConvert.SerializeObject(info, Formatting.Indented)}");
                            if (betInfo != null)
                            {
                                // 1. 安全提取基础字段
                                string tradeType = betInfo.TryGetValue("trade_type", out var tType) ? tType : string.Empty;
                                string combo = betInfo.TryGetValue("combo", out var cb) ? cb : string.Empty;
                                string status = betInfo.TryGetValue("status", out var st) ? st : string.Empty;
                                double oddsVal = betInfo.TryGetValue("odds", out var odStr) && double.TryParse(odStr, out var od) ? od : 0;
                                if (string.Equals(combo, info.combo))
                                {
                                    // 3. 校验该明细是否已经存在（防重）
                                    bool isExisting = checkBettingDetailExist(tradeRecordId, autoFlag, info.type, oddsVal.ToString(), status);
                                    if (!isExisting)
                                    {
                                        // 4. 定义 SQL 插入语句
                                        const string sql = @"
                                    INSERT INTO hr_trade_detail (
                                        trade_record_id, auto_flag, race_no, type, combo, toto, 
                                        action, odds, stake_amount, amount, limit_amount, 
                                        status, order_type, timestamp, create_time, server_id, remark
                                    ) VALUES (
                                        @trade_record_id, @auto_flag, @race_no, @type, @combo, @toto, 
                                        @action, @odds, @stake_amount, @amount, @limit_amount, 
                                        @status, @order_type, SYSDATE(), SYSDATE(), @server_id, @remark
                                    )";
                                        // 5. 优化参数组装（复用前面已安全解析的值，减少重复判断）
                                        var parameters = new Dictionary<string, object>
                                        {
                                            { "@trade_record_id", tradeRecordId },
                                            { "@auto_flag",       autoFlag },
                                            { "@race_no",         betInfo.TryGetValue("race", out var race) ? race : (object)DBNull.Value },
                                            { "@type",            betInfo.TryGetValue("type", out var type) ? type : (object)DBNull.Value },
                                            { "@combo",           combo },
                                            { "@toto",            betInfo.TryGetValue("toto", out var toto) && double.TryParse(toto, out var totoVal) ? totoVal : (object)DBNull.Value },
                                            { "@action",          tradeType },
                                            { "@odds",            oddsVal > 0 ? (object)oddsVal : DBNull.Value },
                                            { "@stake_amount",    betInfo.TryGetValue("stake_amount", out var sa) && double.TryParse(sa, out var saVal) ? saVal : (object)DBNull.Value },
                                            { "@amount",          betInfo.TryGetValue("amount", out var am) && double.TryParse(am, out var amVal) ? amVal : (object)DBNull.Value },
                                            { "@limit_amount",    betInfo.TryGetValue("limit", out var lm) && double.TryParse(lm, out var lmVal) ? lmVal : (object)DBNull.Value },
                                            { "@status",          status },
                                            { "@order_type",      betInfo.TryGetValue("order_type", out var ot) ? ot : (object)DBNull.Value },
                                            { "@server_id",       Utils.GetLocalIP() },
                                            { "@remark",          remark ?? (object)DBNull.Value }
                                        };
                                        // 6. 执行入库
                                        DBUtils.ExecuteParameterizedSql(sql, parameters);
                                        _logger.Debug($"[插入][hr_trade_detail]完成,Info:{JsonConvert.SerializeObject(betInfo, Formatting.Indented)}");
                                    }
                                }
                            }
                        }
                    }
                    processedCount++;
                }
                if (processedCount > 0)
                {
                    _logger.Debug($"[处理][SaveTradeLogsBatch][{processedCount}]完成");
                }
                return processedCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SaveTradeLogsBatch] 异常: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// 保存交易记录,不存在再插入新记录
        /// </summary>
        /// <param name="account"></param>
        /// <param name="raceType"></param>
        /// <param name="raceDate"></param>
        /// <param name="raceNo"></param>
        /// <param name="eatBetInfo"></param>
        /// <param name="autoFlag"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static int SaveTradeRecord(
            Account account,
            string raceType,
            string raceDate,
            string raceNo,
            EatBetInfo eatBetInfo,
            string autoFlag,
            string remark)
        {
            if (eatBetInfo == null)
            {
                return 0;
            }
            string userCode = account.UserCode;
            string serverIp = Utils.GetLocalIP();
            string defaultRemark = string.IsNullOrEmpty(remark) ? "Batch Trade Sync" : remark;

            // 🔧 关键修复：把"查询是否存在" + "INSERT或UPDATE"整体锁住，
            // 防止并发下两个线程都查到 record == null 从而各自 INSERT 出重复行。
            object recordLock = GetRecordLock(userCode, raceType, raceDate, raceNo, eatBetInfo.type, eatBetInfo.combo);
            lock (recordLock)
            {
                // 1. 查询数据库中是否已存在该账户、场次、盘口类型和组合的记录
                IDictionary<string, string> record = queryBettingInfo(
                    userCode,
                    raceType,
                    raceDate,
                    raceNo,
                    eatBetInfo.type,
                    eatBetInfo.combo
                );
                int tradeRecordId = 0;
                try
                {
                    if (record != null)
                    {
                        // ==================== 2. 已存在：执行更新操作 (UPDATE) ====================
                        tradeRecordId = Convert.ToInt32(record["seq"]);
                        string sqlUpdate = @"UPDATE hr_trade_record                              
                                     SET eat_pending_amount = @eat_pending_amount,                                  
                                         eat_executed_amount = @eat_executed_amount,                                  
                                         total_eat_amount = @total_eat_amount,                                  
                                         eat_odds = @eat_odds,                                  
                                         bet_pending_amount = @bet_pending_amount,                                  
                                         bet_executed_amount = @bet_executed_amount,                                  
                                         total_bet_amount = @total_bet_amount,                                  
                                         bet_odds = @bet_odds,                                  
                                         modify_time = SYSDATE(),                                  
                                         remark = @remark                              
                                     WHERE id = @id";
                        var updateParams = new Dictionary<string, object>
                            {
                                { "@eat_pending_amount",   eatBetInfo.eatPendingAmount },
                                { "@eat_executed_amount",  eatBetInfo.eatExecutedAmount },
                                { "@total_eat_amount",     eatBetInfo.totalEatAmount },
                                { "@eat_odds",             eatBetInfo.eatOdds },
                                { "@bet_pending_amount",   eatBetInfo.betPendingAmount },
                                { "@bet_executed_amount",  eatBetInfo.betExecutedAmount },
                                { "@total_bet_amount",     eatBetInfo.totalBetAmount },
                                { "@bet_odds",             eatBetInfo.betOdds },
                                { "@remark",               defaultRemark },
                                { "@id",                   tradeRecordId }
                            };
                        DBUtils.ExecuteParameterizedSql(sqlUpdate, updateParams);
                        _logger.Debug($"[更新][hr_trade_record]完成,Info:{JsonConvert.SerializeObject(eatBetInfo, Formatting.Indented)}");
                    }
                    else
                    {
                        string intertSql = @"
                            INSERT INTO `hr_trade_record` (
                                `platform_code`, `broker_code`, `broker_name`, `account_code`, 
                                `auto_flag`, `race_date`, `race_type`, `race_no`, 
                                `type`, `combo`, `toto`, `eat_odds`, `bet_odds`, 
                                `eat_executed_amount`, `eat_pending_amount`, `bet_executed_amount`, `bet_pending_amount`, 
                                `total_eat_amount`, `total_bet_amount`, `create_time`, `server_id`, `remark`
                            ) VALUES (
                                @platform_code, @broker_code, @broker_name, @account_code, 
                                @auto_flag, @race_date, @race_type, @race_no, 
                                @type, @combo, @toto, @eat_odds, @bet_odds, 
                                @eat_executed_amount, @eat_pending_amount, @bet_executed_amount, @bet_pending_amount, 
                                @total_eat_amount, @total_bet_amount, @create_time, @server_id, @remark
                            )";
                        // ==================== 3. 不存在：执行单条插入操作 (INSERT) ====================
                        var singleInsert = new Dictionary<string, object>
                        {
                            { "platform_code",       account.BrokerName },
                            { "broker_code",         account.BrokerCode },
                            { "broker_name",         account.BrokerName },
                            { "account_code",        userCode },
                            { "auto_flag",           autoFlag },
                            { "race_date",           raceDate },
                            { "race_type",           raceType },
                            { "race_no",             raceNo },
                            { "type",                eatBetInfo.type },
                            { "combo",               eatBetInfo.combo },
                            { "toto",                eatBetInfo.toto },
                            { "eat_odds",            eatBetInfo.eatOdds },
                            { "bet_odds",            eatBetInfo.betOdds },
                            { "eat_executed_amount", eatBetInfo.eatExecutedAmount },
                            { "eat_pending_amount",  eatBetInfo.eatPendingAmount },
                            { "bet_executed_amount", eatBetInfo.betExecutedAmount },
                            { "bet_pending_amount",  eatBetInfo.betPendingAmount },
                            { "total_eat_amount",    eatBetInfo.totalEatAmount },
                            { "total_bet_amount",    eatBetInfo.totalBetAmount },
                            { "create_time",         DateTime.Now },
                            { "server_id",           serverIp },
                            { "remark",              defaultRemark }
                        };
                        tradeRecordId = DBUtils.ExecuteInsertAndGetId(intertSql, singleInsert);
                        _logger.Debug($"[插入][hr_trade_record]完成,Info:{JsonConvert.SerializeObject(eatBetInfo, Formatting.Indented)}");
                    }
                    return tradeRecordId;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SaveTradeRecord] 异常: {ex.Message}");
                    throw;
                }
            }
        }
        /// <summary>
        /// 更新交易记录
        /// </summary>
        /// <param name="account"></param>
        /// <param name="raceType"></param>
        /// <param name="raceDate"></param>
        /// <param name="raceNo"></param>
        /// <param name="eatBetInfo"></param>
        /// <param name="autoFlag"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        public static int UpdateTradeRecord(
            Account account,
            string raceType,
            string raceDate,
            string raceNo,
            EatBetInfo eatBetInfo,
            string autoFlag,
            string remark)
        {
            if (eatBetInfo == null)
            {
                return 0;
            }
            string userCode = account.UserCode;
            string serverIp = Utils.GetLocalIP();
            string defaultRemark = string.IsNullOrEmpty(remark) ? "Batch Trade Sync" : remark;

            // 🔧 关键修复：同样加锁，避免与 SaveTradeRecord/SaveTradeLogsBatch 并发时
            // 对同一个 combo 的记录产生竞态（例如这边判断"已存在"去 UPDATE 的同时，
            // 另一边正在对同一个 combo 执行 INSERT）。
            object recordLock = GetRecordLock(userCode, raceType, raceDate, raceNo, eatBetInfo.type, eatBetInfo.combo);
            lock (recordLock)
            {
                // 1. 查询数据库中是否已存在该账户、场次、盘口类型和组合的记录
                IDictionary<string, string> record = queryBettingInfo(
                    userCode,
                    raceType,
                    raceDate,
                    raceNo,
                    eatBetInfo.type,
                    eatBetInfo.combo
                );
                int tradeRecordId = 0;
                try
                {
                    if (record != null)
                    {
                        // ==================== 2. 已存在：执行更新操作 (UPDATE) ====================
                        tradeRecordId = Convert.ToInt32(record["seq"]);
                        string sqlUpdate = @"UPDATE hr_trade_record                              
                                     SET eat_pending_amount = @eat_pending_amount,                                  
                                         eat_executed_amount = @eat_executed_amount,                                  
                                         total_eat_amount = @total_eat_amount,                                  
                                         eat_odds = @eat_odds,                                  
                                         bet_pending_amount = @bet_pending_amount,                                  
                                         bet_executed_amount = @bet_executed_amount,                                  
                                         total_bet_amount = @total_bet_amount,                                  
                                         bet_odds = @bet_odds,                                  
                                         modify_time = SYSDATE(),                                  
                                         remark = @remark                              
                                     WHERE id = @id";
                        var updateParams = new Dictionary<string, object>
                            {
                                { "@eat_pending_amount",   eatBetInfo.eatPendingAmount },
                                { "@eat_executed_amount",  eatBetInfo.eatExecutedAmount },
                                { "@total_eat_amount",     eatBetInfo.totalEatAmount },
                                { "@eat_odds",             eatBetInfo.eatOdds },
                                { "@bet_pending_amount",   eatBetInfo.betPendingAmount },
                                { "@bet_executed_amount",  eatBetInfo.betExecutedAmount },
                                { "@total_bet_amount",     eatBetInfo.totalBetAmount },
                                { "@bet_odds",             eatBetInfo.betOdds },
                                { "@remark",               defaultRemark },
                                { "@id",                   tradeRecordId }
                            };
                        DBUtils.ExecuteParameterizedSql(sqlUpdate, updateParams);
                        _logger.Debug($"[更新][hr_trade_record]完成,Info:tradeRecordId={tradeRecordId},{JsonConvert.SerializeObject(eatBetInfo, Formatting.Indented)}");
                    }
                    return tradeRecordId;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[UpdateTradeRecord] 异常: {ex.Message}");
                    throw;
                }
            }
        }
        /// <summary>
        /// 保存交易明细,不存在再插入新记录
        /// </summary>
        /// <param name="account"></param>
        /// <param name="raceType"></param>
        /// <param name="raceDate"></param>
        /// <param name="raceNo"></param>
        /// <param name="tradeRecordId"></param>
        /// <param name="orderType"></param>
        /// <param name="autoFlag"></param>
        /// <param name="betInfo"></param>
        /// <param name="remark"></param>
        public static void saveTradeDetailLog(
            Account account,
            string raceType,
            string raceDate,
            string raceNo, int tradeRecordId,
            string orderType,
            string autoFlag,
            BetInfo betInfo,
            string remark)
        {
            if (betInfo == null) return;

            // 🔧 关键修复：把"判重" + "插入"整体锁住，key 与 checkBettingDetailExist
            // 的判重条件完全一致，避免并发下两次调用都判断"不存在"从而插入两条重复明细。
            object detailLock = GetDetailLock(tradeRecordId, autoFlag, betInfo.action, betInfo.odds.ToString(), betInfo.status);
            lock (detailLock)
            {
                // 3. 校验该明细是否已经存在（防重）
                bool isExisting = checkBettingDetailExist(tradeRecordId, autoFlag, betInfo.action, betInfo.odds.ToString(), betInfo.status);
                if (!isExisting)
                {
                    // 4. 定义 SQL 插入语句
                    const string sql = @"
                    INSERT INTO hr_trade_detail (
                        trade_record_id, auto_flag, race_no, type, combo, toto, 
                        action, odds, stake_amount, amount, limit_amount, 
                        status, order_type, timestamp, create_time, server_id, remark
                    ) VALUES (
                        @trade_record_id, @auto_flag, @race_no, @type, @combo, @toto, 
                        @action, @odds, @stake_amount, @amount, @limit_amount, 
                        @status, @order_type, SYSDATE(), SYSDATE(), @server_id, @remark
                    )";
                    // 5. 优化参数组装（复用前面已安全解析的值，减少重复判断）
                    var parameters = new Dictionary<string, object>
                    {
                        { "@trade_record_id", tradeRecordId },
                        { "@auto_flag",       autoFlag },
                        { "@race_no",         betInfo.raceNo },
                        { "@type",            betInfo.type},
                        { "@combo",           betInfo.combo },
                        { "@toto",            betInfo.toto },
                        { "@action",          betInfo.action },
                        { "@odds",            betInfo.odds },
                        { "@stake_amount",    betInfo.stakeAmount},
                        { "@amount",          betInfo.amount},
                        { "@limit_amount",    betInfo.limit},
                        { "@status",          betInfo.status },
                        { "@order_type",      orderType },
                        { "@server_id",       Utils.GetLocalIP() },
                        { "@remark",          remark ?? (object)DBNull.Value }
                    };
                    // 6. 执行入库
                    DBUtils.ExecuteParameterizedSql(sql, parameters);
                    _logger.Debug($"[插入][hr_trade_detail]完成,Info:tradeRecordId={tradeRecordId},{JsonConvert.SerializeObject(betInfo, Formatting.Indented)}");
                }
            }
        }
        public static List<EatBetInfo> queryBettingInfoList(string accountCode, string raceType, string raceDate, string raceNo)
        {
            var sqlQueryStr = new StringBuilder();
            // 🚀 优化点：改用范围查询，让数据库能够精准走 create_time 字段的索引，避免全表扫描
            sqlQueryStr.AppendFormat(@"SELECT * FROM hr_trade_record WHERE account_code = '{0}' AND race_type = '{1}' AND race_date = '{2}' AND race_no = '{3}'", accountCode, raceType, raceDate, raceNo);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                List<EatBetInfo> bettingInfos = new List<EatBetInfo>();
                foreach (var dataItem in datas)
                {
                    // 安全解析数值类型，防止数据库返回空字串或脏数据导致崩溃
                    double.TryParse(dataItem.ContainsKey("toto") ? dataItem["toto"] : "0", out double parsedToto);
                    double.TryParse(dataItem.ContainsKey("eat_executed_amount") ? dataItem["eat_executed_amount"] : "0", out double parsedEatExecuted);
                    double.TryParse(dataItem.ContainsKey("eat_pending_amount") ? dataItem["eat_pending_amount"] : "0", out double parsedEatPending);
                    double.TryParse(dataItem.ContainsKey("bet_executed_amount") ? dataItem["bet_executed_amount"] : "0", out double parsedBetExecuted);
                    double.TryParse(dataItem.ContainsKey("bet_pending_amount") ? dataItem["bet_pending_amount"] : "0", out double parsedBetPending);
                    double.TryParse(dataItem.ContainsKey("eat_odds") ? dataItem["eat_odds"] : "0", out double eatOdds);
                    double.TryParse(dataItem.ContainsKey("bet_odds") ? dataItem["bet_odds"] : "0", out double betOdds);
                    EatBetInfo betInfo = new EatBetInfo
                    {
                        // 2. 基础标识与核心赛事元数据 (使用 ContainsKey 确保键值存在，防止抛出 KeyNotFoundException)
                        seq = dataItem.ContainsKey("id") ? dataItem["id"] : string.Empty,
                        raceDate = dataItem.ContainsKey("race_date") ? dataItem["race_date"] : string.Empty,
                        raceType = dataItem.ContainsKey("action") ? dataItem["action"] : string.Empty,
                        raceNo = dataItem.ContainsKey("race_no") ? dataItem["race_no"] : string.Empty,
                        // 3. 盘口组合与赔率
                        type = dataItem.ContainsKey("type") ? dataItem["type"] : string.Empty,
                        combo = dataItem.ContainsKey("combo") ? dataItem["combo"] : string.Empty,
                        toto = parsedToto,
                        eatOdds = eatOdds,
                        betOdds = betOdds,
                        // 4. 吃票（EAT）状态下的已成交与待成交金额
                        eatExecutedAmount = parsedEatExecuted,
                        eatPendingAmount = parsedEatPending,
                        // 5. 下注（BET）状态下的已成交与待成交金额
                        betExecutedAmount = parsedBetExecuted,
                        betPendingAmount = parsedBetPending
                    };
                    bettingInfos.Add(betInfo);
                }
                return bettingInfos;
            }
            return null;
        }
        /// <summary>
        /// 获取下注明细列表
        /// </summary>
        /// <param name="tradeRecordId"></param>
        /// <returns></returns>
        public static List<BetInfo> queryBettingDetailList(string tradeRecordId)
        {
            var sqlQueryStr = new StringBuilder();
            // 🚀 优化点：改用范围查询，让数据库能够精准走 create_time 字段的索引，避免全表扫描
            sqlQueryStr.AppendFormat(@"SELECT * FROM hr_trade_detail WHERE trade_record_id = '{0}' order by action, create_time", tradeRecordId);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                List<BetInfo> bettingInfos = new List<BetInfo>();
                foreach (var dataItem in datas)
                {
                    // 安全解析数值类型，防止数据库返回空字串或脏数据导致崩溃
                    double.TryParse(dataItem.ContainsKey("toto") ? dataItem["toto"] : "0", out double parsedToto);
                    double.TryParse(dataItem.ContainsKey("stake_amount") ? dataItem["stake_amount"] : "0", out double parsedStakeAmount);
                    double.TryParse(dataItem.ContainsKey("odds") ? dataItem["odds"] : "0", out double parsedOdds);
                    double.TryParse(dataItem.ContainsKey("limit_amount") ? dataItem["limit_amount"] : "0", out double parsedLimit);
                    BetInfo betInfo = new BetInfo
                    {
                        // 2. 基础标识与核心赛事元数据 (使用 ContainsKey 确保键值存在，防止抛出 KeyNotFoundException)
                        seq = dataItem.ContainsKey("id") ? dataItem["id"] : string.Empty,
                        tradeRecordId = dataItem.ContainsKey("trade_record_id") ? dataItem["trade_record_id"] : string.Empty,
                        raceDate = dataItem.ContainsKey("race_date") ? dataItem["race_date"] : string.Empty,
                        raceType = dataItem.ContainsKey("action") ? dataItem["action"] : string.Empty,
                        raceNo = dataItem.ContainsKey("race_no") ? dataItem["race_no"] : string.Empty,
                        // 3. 盘口组合与赔率
                        type = dataItem.ContainsKey("type") ? dataItem["type"] : string.Empty,
                        combo = dataItem.ContainsKey("combo") ? dataItem["combo"] : string.Empty,
                        odds = (int)parsedOdds,
                        toto = parsedToto,
                        limit = (int)parsedLimit,
                        stakeAmount = (int)parsedStakeAmount,
                        action = dataItem.ContainsKey("action") ? dataItem["action"] : string.Empty,
                        status = dataItem.ContainsKey("status") ? dataItem["status"] : string.Empty,
                        timestamp = dataItem.ContainsKey("timestamp") ? dataItem["timestamp"] : string.Empty,
                        remark = dataItem.ContainsKey("remark") ? dataItem["remark"] : string.Empty,
                    };
                    bettingInfos.Add(betInfo);
                }
                return bettingInfos;
            }
            return null;
        }
        /// <summary>
        /// 查询下注明细是否存在，用于避免重复插入
        /// </summary>
        /// <param name="tradeRecordId"></param>
        /// <param name="autoFlag"></param>
        /// <param name="tradeType"></param>
        /// <param name="odds"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static bool checkBettingDetailExist(int tradeRecordId, string autoFlag, string tradeType, string odds, string status)
        {
            var sqlQueryStr = new StringBuilder();
            // 直接拼接所有条件，要求同时满足
            sqlQueryStr.AppendFormat(
                "SELECT id FROM hr_trade_detail WHERE trade_record_id = '{0}' AND auto_flag = '{1}' AND action= '{2}' AND odds = {3} AND status = '{4}'",
                tradeRecordId, autoFlag, tradeType, odds, status
            );
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 查询自动下注状态列表
        /// </summary>
        /// <param name="accountCode">账号</param>
        /// <param name="raceType">赛事类型</param>
        /// <param name="raceDate">赛事日期</param>
        /// <param name="raceNo">场次，0 表示全部</param>
        /// <param name="combos">组合，支持 "2-11" 或 "1-2,2-3,2-11"</param>
        /// <returns>自动下注状态列表</returns>
        public static List<BetInfo> queryAutoBettingStatusList(
            string accountCode,
            string raceType,
            string raceDate,
            string raceNo,
            string combos)
        {
            var sqlQueryStr = new StringBuilder();

            // ============================================================
            // 1. 基础 SQL
            // ============================================================
            sqlQueryStr.AppendFormat(@"
        SELECT 
            d.id AS detail_id, 
            d.trade_record_id AS d_trade_record_id, 
            d.type AS detail_type, 
            d.combo AS detail_combo, 
            d.odds AS detail_odds, 
            d.toto AS detail_toto, 
            d.limit_amount AS detail_limit, 
            d.stake_amount AS detail_stake, 
            d.action AS detail_action, 
            d.status AS detail_status, 
            d.timestamp AS detail_timestamp, 
            d.remark AS detail_remark, 

            r.id AS record_id,
            r.race_date AS record_race_date, 
            r.race_no AS record_race_no, 
            r.race_type AS record_race_type,
            r.account_code AS record_account_code

        FROM hr_trade_detail d

        INNER JOIN hr_trade_record r 
            ON d.trade_record_id = r.id

        WHERE r.account_code = '{0}'
          AND r.race_date = '{1}'
          AND r.race_type = '{2}'",
                accountCode,
                raceDate,
                raceType);

            // ============================================================
            // 2. Race No 条件
            // ============================================================
            if (!string.IsNullOrWhiteSpace(raceNo) && raceNo != "0")
            {
                sqlQueryStr.AppendFormat(
                    " AND r.race_no = '{0}'",
                    raceNo.Trim());
            }

            // ============================================================
            // 3. Combo 条件
            //
            // 支持：
            // 2-11
            //
            // 或：
            // 1-2,2-3,2-11
            //
            // 使用精确匹配 IN。
            // ============================================================
            if (!string.IsNullOrWhiteSpace(combos))
            {
                var comboList = combos
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (comboList.Count > 0)
                {
                    var escapedCombos = comboList
                        .Select(c => "'" + c.Replace("'", "''") + "'")
                        .ToList();

                    sqlQueryStr.AppendFormat(
                        " AND d.combo IN ({0})",
                        string.Join(",", escapedCombos));
                }
            }

            // ============================================================
            // 4. ORDER BY 必须放在所有 WHERE 条件之后
            // ============================================================
            sqlQueryStr.Append(
                " ORDER BY d.combo, d.action, d.create_time");

            // ============================================================
            // 5. 执行查询
            // ============================================================
            List<IDictionary<string, string>> datas =
                DBUtils.eaQuery(sqlQueryStr.ToString());

            if (datas != null && datas.Count > 0)
            {
                List<BetInfo> bettingInfos = new List<BetInfo>();

                foreach (var dataItem in datas)
                {
                    // ====================================================
                    // 安全解析数值
                    // ====================================================
                    double.TryParse(
                        dataItem.ContainsKey("detail_toto")
                            ? dataItem["detail_toto"]
                            : "0",
                        out double parsedToto);

                    double.TryParse(
                        dataItem.ContainsKey("detail_stake")
                            ? dataItem["detail_stake"]
                            : "0",
                        out double parsedStakeAmount);

                    double.TryParse(
                        dataItem.ContainsKey("detail_odds")
                            ? dataItem["detail_odds"]
                            : "0",
                        out double parsedOdds);

                    double.TryParse(
                        dataItem.ContainsKey("detail_limit")
                            ? dataItem["detail_limit"]
                            : "0",
                        out double parsedLimit);

                    // ====================================================
                    // BetInfo
                    // ====================================================
                    BetInfo betInfo = new BetInfo
                    {
                        seq = dataItem.ContainsKey("detail_id")
                            ? dataItem["detail_id"]
                            : string.Empty,

                        tradeRecordId = dataItem.ContainsKey("d_trade_record_id")
                            ? dataItem["d_trade_record_id"]
                            : string.Empty,

                        raceDate = dataItem.ContainsKey("record_race_date")
                            ? dataItem["record_race_date"]
                            : string.Empty,

                        raceType = dataItem.ContainsKey("record_race_type")
                            ? dataItem["record_race_type"]
                            : string.Empty,

                        raceNo = dataItem.ContainsKey("record_race_no")
                            ? dataItem["record_race_no"]
                            : string.Empty,

                        type = dataItem.ContainsKey("detail_type")
                            ? dataItem["detail_type"]
                            : string.Empty,

                        combo = dataItem.ContainsKey("detail_combo")
                            ? dataItem["detail_combo"]
                            : string.Empty,

                        odds = (int)parsedOdds,

                        toto = parsedToto,

                        limit = (int)parsedLimit,

                        stakeAmount = (int)parsedStakeAmount,

                        action = dataItem.ContainsKey("detail_action")
                            ? dataItem["detail_action"]
                            : string.Empty,

                        status = dataItem.ContainsKey("detail_status")
                            ? dataItem["detail_status"]
                            : string.Empty,

                        timestamp = dataItem.ContainsKey("detail_timestamp")
                            ? dataItem["detail_timestamp"]
                            : string.Empty,

                        remark = dataItem.ContainsKey("detail_remark")
                            ? dataItem["detail_remark"]
                            : string.Empty,
                    };

                    bettingInfos.Add(betInfo);
                }

                return bettingInfos;
            }

            return new List<BetInfo>();
        }
        /// <summary>
        /// 获取下注信息,不判断是否完全平仓
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="raceType"></param>
        /// <param name="raceDate"></param>
        /// <param name="raceNo"></param>
        /// <param name="type"></param>
        /// <param name="combo"></param>
        /// <returns></returns>
        public static IDictionary<string, string> queryBettingInfo(string accountCode, string raceType, string raceDate, string raceNo, string type, string combo)
        {
            // ====== 第一步：根据多条件精准查出这条记录 ======
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat(@"SELECT id as seq, eat_odds, bet_odds, eat_executed_amount, eat_pending_amount, 
                    bet_executed_amount, bet_pending_amount, total_eat_amount, total_bet_amount 
                    FROM hr_trade_record t                        
                    WHERE t.account_code = '{0}'                         
                      AND t.race_type = '{1}'                         
                      AND t.race_date = '{2}'                         
                      AND t.race_no = '{3}'                         
                      AND t.type = '{4}'                         
                      AND t.combo = '{5}'", accountCode, raceType, raceDate, raceNo, type, combo);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            //System.Diagnostics.Debug.WriteLine(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                return datas[0];
            }
            return null;
        }
        /// <summary>
        /// 查询未完全平仓信息
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="raceType"></param>
        /// <param name="raceDate"></param>
        /// <param name="raceNo"></param>
        /// <param name="type"></param>
        /// <param name="combo"></param>
        /// <returns></returns>
        public static IDictionary<string, string> queryBettingOpenInfo(string accountCode, string raceType, string raceDate, string raceNo, string type, string combo)
        {
            // ====== 第一步：根据多条件精准查出这条记录 ======
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat(@"SELECT id as seq, eat_odds, bet_odds, eat_executed_amount, eat_pending_amount, 
                    bet_executed_amount, bet_pending_amount, total_eat_amount, total_bet_amount 
                    FROM hr_trade_record t                        
                    WHERE t.account_code = '{0}'                         
                      AND t.race_type = '{1}'                         
                      AND t.race_date = '{2}'                         
                      AND t.race_no = '{3}'                         
                      AND t.type = '{4}'                         
                      AND t.combo = '{5}'                         
                      AND NOT EXISTS (
                          SELECT 1                                             
                          FROM hr_trade_record d                                             
                          WHERE d.id = t.id 
                            AND d.account_code = '{0}'                                                                      
                            AND d.race_type = '{1}'                                                                      
                            AND d.race_date = '{2}'                                                                      
                            AND d.race_no = '{3}'
                            AND d.type = '{4}'
                            AND d.combo = '{5}'                                            
                            AND d.total_eat_amount = d.total_bet_amount                                             
                            AND d.total_eat_amount = d.eat_executed_amount                                             
                            AND d.total_bet_amount = d.bet_executed_amount                                             
                            AND d.eat_pending_amount = 0                                             
                            AND d.bet_pending_amount = 0                                        
                      )", accountCode, raceType, raceDate, raceNo, type, combo);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            //System.Diagnostics.Debug.WriteLine(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                return datas[0];
            }
            return null;
        }
        /// <summary>
        /// 查询完全平仓信息
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="raceType"></param>
        /// <param name="raceDate"></param>
        /// <param name="raceNo"></param>
        /// <param name="type"></param>
        /// <param name="combo"></param>
        /// <returns></returns>
        public static IDictionary<string, string> queryBettingClosedInfo(string accountCode, string raceType, string raceDate, string raceNo, string type, string combo)
        {
            // ====== 第一步：根据多条件精准查出这条记录 ======
            var sqlQueryStr = new StringBuilder();
            sqlQueryStr.AppendFormat(@"SELECT id as seq, eat_odds, bet_odds, eat_executed_amount, eat_pending_amount, 
                    bet_executed_amount, bet_pending_amount, total_eat_amount, total_bet_amount 
                    FROM hr_trade_record t                        
                    WHERE t.account_code = '{0}'                         
                      AND t.race_type = '{1}'                         
                      AND t.race_date = '{2}'                         
                      AND t.race_no = '{3}'                         
                      AND t.type = '{4}'                         
                      AND t.combo = '{5}'  
                      AND t.total_eat_amount = t.total_bet_amount                                             
                      AND t.total_eat_amount = t.eat_executed_amount                                             
                      AND t.total_bet_amount = t.bet_executed_amount                                             
                      AND t.eat_pending_amount = 0                                             
                      AND t.bet_pending_amount = 0 ", accountCode, raceType, raceDate, raceNo, type, combo);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            //System.Diagnostics.Debug.WriteLine(sqlQueryStr.ToString());
            if (datas != null && datas.Count > 0)
            {
                return datas[0];
            }
            return null;
        }
        /// <summary>
        /// 查询该查赛次下所有未平仓下注信息
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="raceType"></param>
        /// <param name="raceDate"></param>
        /// <param name="raceNo"></param>
        /// <returns></returns>
        public static Dictionary<string, IDictionary<string, string>> queryAllOpenBettingInfoForRance(string accountCode, string raceType, string raceDate, string raceNo, string type, string combo = null)
        {
            var sqlQueryStr = new StringBuilder();
            // 1. 基础 SQL 查询构建（不包含 type 和 combo）
            sqlQueryStr.AppendFormat(@"SELECT id as seq, combo, eat_odds, bet_odds, eat_executed_amount, eat_pending_amount,bet_executed_amount, bet_pending_amount, total_eat_amount, total_bet_amount FROM hr_trade_record WHERE account_code = '{0}' AND race_type = '{1}' AND race_date = '{2}' AND race_no = '{3}'",
                accountCode, raceType, raceDate, raceNo);
            // 2. 如果传入了具体的 type，则在 SQL 中追加该过滤条件
            if (!string.IsNullOrEmpty(type))
            {
                sqlQueryStr.AppendFormat(" AND type = '{0}'", type);
            }
            // 3. 如果传入了具体的 combo，则在 SQL 中进行精准过滤
            if (!string.IsNullOrEmpty(combo))
            {
                sqlQueryStr.AppendFormat(" AND combo = '{0}'", combo);
            }
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            if (datas == null || datas.Count == 0)
            {
                return null;
            }
            // 4. 使用 LINQ 将结果转换为字典
            return datas
                .Where(data => data != null && data.TryGetValue("combo", out var comboObj) && comboObj != null)
                .GroupBy(data => data["combo"].ToString())
                .ToDictionary(
                    group => group.Key,
                    group => group.First() // 若存在重复 combo 取第一条
                );
        }
        /// <summary>
        /// 查询本场比赛是否有未平仓记录
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="raceType"></param>
        /// <param name="raceDate"></param>
        /// <param name="raceNo"></param>
        /// <returns></returns>
        public static Dictionary<string, IDictionary<string, string>> QueryExistPendingBettingInfoForRance(
            string accountCode,
            string raceType,
            string raceDate,
            string raceNo,
            string type)
        {
            var sqlQueryStr = new StringBuilder();
            // 💡 优化：移除聚合函数，避免 Group By 报错；如果需要保证每组只取最新一条，可以加上 ORDER BY id DESC
            sqlQueryStr.AppendFormat(@"
                SELECT id as seq, combo, eat_odds, bet_odds, 
                       eat_executed_amount, eat_pending_amount,                
                       bet_executed_amount, bet_pending_amount, 
                       total_eat_amount, total_bet_amount              
                FROM   hr_trade_record                  
                WHERE  account_code           = '{0}'                      
                  AND  race_type              = '{1}'                      
                  AND  race_date              = '{2}'                      
                  AND  race_no                = '{3}'                      
                  AND  type                   = '{4}'                      
                  AND  total_bet_amount       = bet_executed_amount                      
                  AND  total_bet_amount       = total_eat_amount                      
                  AND  total_eat_amount      <> eat_executed_amount                      
                  AND  eat_pending_amount     > 0
                ORDER BY id DESC", // 优先取最新插入的记录
                accountCode, raceType, raceDate, raceNo, type);
            List<IDictionary<string, string>> datas = DBUtils.eaQuery(sqlQueryStr.ToString());
            if (datas == null || datas.Count == 0)
            {
                return null;
            }
            // 使用 LINQ 转为字典（因为前面加了 ORDER BY id DESC，这里的 First() 自然就是最新的一条）
            return datas
                .Where(data => data != null && data.TryGetValue("combo", out var comboObj) && comboObj != null)
                .GroupBy(data => data["combo"].ToString())
                .ToDictionary(
                    group => group.Key,
                    group => group.First()
                );
        }
        /// <summary>
        /// 删除未成交的赌注
        /// </summary>
        /// <param name="accountCode"></param>
        /// <param name="raceType"></param>
        /// <param name="raceDate"></param>
        /// <param name="raceNo"></param>
        public static void deleteOpenBetRecord(string accountCode, string raceDate, string raceType, string raceNo)
        {
            // ====== 1. 先删除子表 (hr_trade_detail)，使用 INNER JOIN 关联主表条件 ======
            var sqlDetailStr = new StringBuilder();
            sqlDetailStr.AppendFormat(
                @"DELETE d FROM hr_trade_detail d
                  INNER JOIN hr_trade_record r ON d.trade_record_id = r.id
                  WHERE r.account_code = '{0}' 
                    AND r.race_date    = '{1}' 
                    AND r.race_type    = '{2}' 
                    AND r.race_no      = '{3}' 
                    AND r.bet_executed_amount = 0 
                    AND r.total_bet_amount    = r.bet_pending_amount",
                        accountCode, raceDate, raceType, raceNo
                    );
            _logger.Debug("Delete hr_trade_detail: " + sqlDetailStr.ToString());
            DBUtils.executeSql(sqlDetailStr.ToString());
            // ====== 2. 再删除主表 (hr_trade_record) ======
            var sqlRecordStr = new StringBuilder();
            sqlRecordStr.AppendFormat(
                @"DELETE FROM hr_trade_record 
                  WHERE account_code = '{0}' 
                    AND race_date    = '{1}' 
                    AND race_type    = '{2}' 
                    AND race_no      = '{3}' 
                    AND bet_executed_amount = 0 
                    AND total_bet_amount    = bet_pending_amount",
                        accountCode, raceDate, raceType, raceNo
                    );
            _logger.Debug("Delete hr_trade_record: " + sqlRecordStr.ToString());
            DBUtils.executeSql(sqlRecordStr.ToString());
        }
    }
}