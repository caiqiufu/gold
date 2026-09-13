using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace AutoHorseRace.Utils
{
    /// <summary>
    /// HTTP 操作类
    /// </summary>
    public class HTTPHelper
    {
        public static string AutoHorseRaceFastAPI = "127.0.0.1";

        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 是否请求到服务器
        /// </summary>
        private static bool _isUseServer = true;

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="pin"></param>
        /// <returns></returns>
        public static JObject login(string serverAddress, string username, string password, string pin)
        {
            string result;

            var payloadObj = new
            {
                username = username,
                password = password,
                pin = pin
            };
            string url = $"http://{serverAddress}/auth/login";
            string payload = JsonConvert.SerializeObject(payloadObj);
            // 🌟 1. 在外面提前声明耗时变量，给默认值
            string serverProcessTime = "0ms";
            if (_isUseServer)
            {
                result = HTTPUtils.SendSyncRequest(url, "POST", null, null, payload, out serverProcessTime);
                _logger.Debug($"login result: {result}");
            }
            else
            {
                result = File.ReadAllText(@"C:\D\07-svn\01-gold\AutoHorseRace\devdoc\v2\login_response.json");
            }
            try
            {
                JObject jsonResult = JObject.Parse(result);

                // 💡 确保能够成功解析并返回正常状态的余额数据
                if (jsonResult["success"] != null && (bool)jsonResult["success"])
                {
                    _logger.Debug($"✅ [login] 账户 [{username}] 登陆成功 (耗时: {serverProcessTime})");
                }
                else
                {
                    string errorMsg = jsonResult["message"]?.ToString() ?? jsonResult["detail"]?.ToString() ?? "未知错误";
                    _logger.Error($"❌ [login] 账户 [{username}] 登陆返回失败状态: {errorMsg}");
                }

                // 🌟 2. 将服务端耗时注入到返回的 JSON 对象中
                jsonResult["serverProcessTime"] = serverProcessTime;
                return jsonResult;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"❌ [login] JSON 解析异常: {result}");
                return null;
            }
        }

        public static JObject logout(string serverAddress, string username)
        {
            string result;
            string url = $"http://{serverAddress}/auth/logout";
            var payloadObj = new
            {
                username = username
            };
            string payload = JsonConvert.SerializeObject(payloadObj);
            // 🌟 1. 在外面提前声明耗时变量，给默认值
            string serverProcessTime = "0ms";
            if (_isUseServer)
            {
                result = HTTPUtils.SendSyncRequest(url, "POST", null, null, payload, out serverProcessTime);
                _logger.Debug($"logout result: {result}");
            }
            else
            {
                result = File.ReadAllText(@"C:\D\07-svn\01-gold\AutoHorseRace\devdoc\v2\logout_response.json");
            }
            try
            {
                JObject jsonResult = JObject.Parse(result);

                // 💡 确保能够成功解析并返回正常状态的余额数据
                if (jsonResult["success"] != null && (bool)jsonResult["success"])
                {
                    _logger.Debug($"✅ [logout] 账户 [{username}] 登出成功 (耗时: {serverProcessTime})");
                }
                else
                {
                    string errorMsg = jsonResult["message"]?.ToString() ?? jsonResult["detail"]?.ToString() ?? "未知错误";
                    _logger.Error($"❌ [login] 账户 [{username}] 登出返回失败状态: {errorMsg}");
                }

                // 🌟 2. 将服务端耗时注入到返回的 JSON 对象中
                jsonResult["serverProcessTime"] = serverProcessTime;
                return jsonResult;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"❌ [login] JSON 解析异常: {result}");
                return null;
            }
        }


        /// <summary>
        /// 查询余额
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public static JObject queryBalance(string serverAddress, string username)
        {
            string result;
            string url = $"http://{serverAddress}/query/balance";
            var payloadObj = new
            {
                username = username
            };
            string payload = JsonConvert.SerializeObject(payloadObj);

            // 🌟 1. 在外面提前声明耗时变量，给默认值
            string serverProcessTime = "0ms";

            if (_isUseServer)
            {
                result = HTTPUtils.SendSyncRequest(url, "POST", null, null, payload, out serverProcessTime);
                _logger.Debug($"queryBalance result: {result}");
            }
            else
            {
                result = File.ReadAllText(@"C:\D\07-svn\01-gold\AutoHorseRace\devdoc\v2\balance_response.json");
                serverProcessTime = "local_file"; // 离线测试时标记来源
            }

            try
            {
                JObject jsonResult = JObject.Parse(result);

                // 💡 确保能够成功解析并返回正常状态的余额数据
                if (jsonResult["success"] != null && (bool)jsonResult["success"])
                {
                    _logger.Debug($"✅ [queryBalance] 账户 [{username}] 余额查询成功: {jsonResult["data"]?["balance"]} (耗时: {serverProcessTime})");
                }
                else
                {
                    string errorMsg = jsonResult["message"]?.ToString() ?? jsonResult["detail"]?.ToString() ?? "未知错误";
                    _logger.Error($"❌ [queryBalance] 账户 [{username}] 查询余额返回失败状态: {errorMsg}");
                }

                // 🌟 2. 将服务端耗时注入到返回的 JSON 对象中
                jsonResult["serverProcessTime"] = serverProcessTime;
                return jsonResult;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"❌ [queryBalance] JSON 解析异常: {result}");
                return null;
            }
        }

        /// <summary>
        /// 查询盘口数据
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="username"></param>
        /// <param name="rc"></param>
        /// <param name="raceDate"></param>
        /// <param name="raceType"></param>
        /// <returns></returns>

        public static IDictionary<string, List<BetInfo>> queryMarket(string serverAddress, string username, string raceDate, string raceType, string raceNo,
                                    out string serverProcessTime)
        {
            string result;
            string url = $"http://{serverAddress}/query/market";
            var payloadObj = new
            {
                username = username,
                race_type = raceType,
                race_date = raceDate,
                race_num = raceNo
            };
            string payload = JsonConvert.SerializeObject(payloadObj);

            // 💡 必须在分支中对 out 参数赋初始值
            if (_isUseServer)
            {
                result = HTTPUtils.SendSyncRequest(url, "POST", null, null, payload, out serverProcessTime);
                _logger.Debug($"queryMarket result: {result}");
            }
            else
            {
                result = File.ReadAllText(@"C:\D\07-svn\01-gold\AutoHorseRace\devdoc\v2\market_response.json");
                serverProcessTime = "local_file"; // 离线模式默认值
            }

            JObject jsonResult = null;
            try
            {
                jsonResult = JObject.Parse(result);
                // 🌟 将服务端耗时注入到返回的 JSON 对象中
                jsonResult["serverProcessTime"] = serverProcessTime;

                if (jsonResult["success"] != null && (bool)jsonResult["success"])
                {
                    _logger.Debug($"✅ [queryMarket] 账户 [{username}] 查询盘口数据成功 (耗时: {serverProcessTime})");
                }
                else
                {
                    string errorMsg = jsonResult["message"]?.ToString() ?? jsonResult["detail"]?.ToString() ?? "未知错误";
                    _logger.Error($"❌ [queryMarket] 账户 [{username}] 查询盘口数据返回失败状态: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"❌ [queryMarket] JSON 解析异常: {result}");
            }

            if (jsonResult != null && (bool)jsonResult["success"])
            {
                if (jsonResult["data"] != null)
                {
                    IDictionary<string, List<BetInfo>> marketData = new Dictionary<string, List<BetInfo>>();
                    string race_date = (string)jsonResult["data"]["race_date"];
                    string race_type = (string)jsonResult["data"]["race_type"];
                    string curr_rc = (string)jsonResult["data"]["curr_rc"];

                    JArray availableRacesJObject = (JArray)jsonResult["data"]["available_races"];

                    JArray quinellaDataJObject = (JArray)jsonResult["data"]["q_odds"];
                    JArray qplaceDataJObject = (JArray)jsonResult["data"]["qp_odds"];
                    JObject myTradesJObject = (JObject)jsonResult["data"]["my_trades"];

                    JObject columnsJObject = (JObject)jsonResult["data"]["pending_orders"];

                    List<string> availableRaceList = new List<string>();
                    if (availableRacesJObject != null)
                    {
                        availableRaceList = availableRacesJObject.ToObject<List<string>>();
                    }

                    IDictionary<string, TotoInfo> totoData = new Dictionary<string, TotoInfo>();
                    if (quinellaDataJObject != null)
                    {
                        foreach (var item in quinellaDataJObject)
                        {
                            TotoInfo totoInfo = new TotoInfo();
                            totoInfo.raceDate = race_date;
                            totoInfo.raceType = race_type;
                            totoInfo.race = curr_rc;
                            totoInfo.toto = (double)item["odds"];
                            totoInfo.type = (string)item["type"];
                            totoInfo.level = (int)item["level"];
                            totoInfo.combo = (string)item["combo"];
                            totoInfo.key = $"{curr_rc}_{totoInfo.combo}_{totoInfo.type}";
                            totoData.Add(totoInfo.key, totoInfo);
                        }
                    }
                    if (qplaceDataJObject != null)
                    {
                        foreach (var item in qplaceDataJObject)
                        {
                            TotoInfo totoInfo = new TotoInfo();
                            totoInfo.raceDate = race_date;
                            totoInfo.raceType = race_type;
                            totoInfo.race = curr_rc;
                            totoInfo.toto = (double)item["odds"];
                            totoInfo.type = (string)item["type"];
                            totoInfo.level = (int)item["level"];
                            totoInfo.combo = (string)item["combo"];
                            totoInfo.key = $"{curr_rc}_{totoInfo.combo}_{totoInfo.type}";
                            totoData.Add(totoInfo.key, totoInfo);
                        }
                    }

                    if (columnsJObject != null)
                    {
                        JArray Q_BETJArray = (JArray)columnsJObject["Q_BET"];
                        JArray Q_EATJArray = (JArray)columnsJObject["Q_EAT"];
                        JArray QP_BETJArray = (JArray)columnsJObject["QP_BET"];
                        JArray QP_EATJArray = (JArray)columnsJObject["QP_EAT"];

                        if (Q_BETJArray != null && Q_BETJArray.Count > 0)
                        {
                            List<BetInfo> Q_BETBetInfos = new List<BetInfo>();
                            marketData.Add("Q_BETBetInfos", Q_BETBetInfos);
                            foreach (var item in Q_BETJArray)
                            {
                                string totoKey = $"{curr_rc}_{(string)item["combo"]}_Q";
                                double toto = totoData.ContainsKey(totoKey) ? (totoData[totoKey].toto) : 0.0;
                                BetInfo Q_BETBetInfo = new BetInfo();
                                Q_BETBetInfos.Add(Q_BETBetInfo);
                                Q_BETBetInfo.raceNo = (string)item["race"];
                                Q_BETBetInfo.combo = (string)item["combo"];
                                Q_BETBetInfo.amount = (int)item["amount"];
                                Q_BETBetInfo.odds = (int)item["odds"];
                                Q_BETBetInfo.limit = (int)item["limit"];
                                Q_BETBetInfo.action = "BET";
                                Q_BETBetInfo.type = (string)item["type_code"];
                                Q_BETBetInfo.raceDate = race_date;
                                Q_BETBetInfo.raceType = race_type;
                                Q_BETBetInfo.toto = toto;
                            }
                        }
                        if (Q_EATJArray != null && Q_EATJArray.Count > 0)
                        {
                            List<BetInfo> Q_EATBetInfos = new List<BetInfo>();
                            marketData.Add("Q_EATBetInfos", Q_EATBetInfos);
                            foreach (var item in Q_EATJArray)
                            {
                                string totoKey = $"{curr_rc}_{(string)item["combo"]}_Q";
                                double toto = totoData.ContainsKey(totoKey) ? (totoData[totoKey].toto) : 0.0;
                                BetInfo Q_EATBetInfo = new BetInfo();
                                Q_EATBetInfos.Add(Q_EATBetInfo);
                                Q_EATBetInfo.raceNo = (string)item["race"];
                                Q_EATBetInfo.combo = (string)item["combo"];
                                Q_EATBetInfo.amount = (int)item["amount"];
                                Q_EATBetInfo.odds = (int)item["odds"];
                                Q_EATBetInfo.limit = (int)item["limit"];
                                Q_EATBetInfo.action = "EAT";
                                Q_EATBetInfo.type = (string)item["type_code"];
                                Q_EATBetInfo.raceDate = race_date;
                                Q_EATBetInfo.raceType = race_type;
                                Q_EATBetInfo.toto = toto;
                            }
                        }
                        if (QP_BETJArray != null && QP_BETJArray.Count > 0)
                        {
                            List<BetInfo> QP_BETBetInfos = new List<BetInfo>();
                            marketData.Add("QP_BETBetInfos", QP_BETBetInfos);
                            foreach (var item in QP_BETJArray)
                            {
                                string totoKey = $"{curr_rc}_{(string)item["combo"]}_QP";
                                double toto = totoData.ContainsKey(totoKey) ? (totoData[totoKey].toto) : 0.0;
                                BetInfo QP_BETBetInfo = new BetInfo();
                                QP_BETBetInfos.Add(QP_BETBetInfo);
                                QP_BETBetInfo.raceNo = (string)item["race"];
                                QP_BETBetInfo.combo = (string)item["combo"];
                                QP_BETBetInfo.amount = (int)item["amount"];
                                QP_BETBetInfo.odds = (int)item["odds"];
                                QP_BETBetInfo.limit = (int)item["limit"];
                                QP_BETBetInfo.action = "BET";
                                QP_BETBetInfo.type = (string)item["type_code"];
                                QP_BETBetInfo.raceDate = race_date;
                                QP_BETBetInfo.raceType = race_type;
                                QP_BETBetInfo.toto = toto;
                            }
                        }
                        if (QP_EATJArray != null && QP_EATJArray.Count > 0)
                        {
                            List<BetInfo> QP_EATBetInfos = new List<BetInfo>();
                            marketData.Add("QP_EATBetInfos", QP_EATBetInfos);
                            foreach (var item in QP_EATJArray)
                            {
                                string totoKey = $"{curr_rc}_{(string)item["combo"]}_QP";
                                double toto = totoData.ContainsKey(totoKey) ? (totoData[totoKey].toto) : 0.0;
                                BetInfo QP_EATBetInfo = new BetInfo();
                                QP_EATBetInfos.Add(QP_EATBetInfo);
                                QP_EATBetInfo.raceNo = (string)item["race"];
                                QP_EATBetInfo.combo = (string)item["combo"];
                                QP_EATBetInfo.amount = (int)item["amount"];
                                QP_EATBetInfo.odds = (int)item["odds"];
                                QP_EATBetInfo.limit = (int)item["limit"];
                                QP_EATBetInfo.action = "EAT";
                                QP_EATBetInfo.type = (string)item["type_code"];
                                QP_EATBetInfo.raceDate = race_date;
                                QP_EATBetInfo.raceType = race_type;
                                QP_EATBetInfo.toto = toto;
                            }
                        }
                    }

                    if (myTradesJObject != null)
                    {
                        List<BetInfo> myConfirmedBETBetInfos = new List<BetInfo>();
                        List<BetInfo> myPendingBETBetInfos = new List<BetInfo>();

                        var confirmedArray = myTradesJObject["confirmed"] as JArray;
                        if (confirmedArray != null)
                        {
                            foreach (var item in confirmedArray)
                            {
                                BetInfo betInfo = new BetInfo();
                                betInfo.status = (string)item["status"];
                                betInfo.combo = (string)item["combo"];
                                betInfo.stakeAmount = int.TryParse((string)item["amount"], out int amount) ? amount : 0;
                                betInfo.odds = int.TryParse((string)item["odds"], out int odds) ? odds : 0;
                                betInfo.action = string.Equals((string)item["action"], "赌", StringComparison.Ordinal) ? "BET" : "EAT";
                                betInfo.type = (string)item["type_code"];
                                betInfo.raceNo = (string)item["race"];
                                betInfo.raceType = raceType;
                                betInfo.raceDate = raceDate;

                                myConfirmedBETBetInfos.Add(betInfo);
                            }
                        }

                        var pendingArray = myTradesJObject["pending"] as JArray;
                        if (pendingArray != null)
                        {
                            foreach (var item in pendingArray)
                            {
                                BetInfo betInfo = new BetInfo();
                                betInfo.status = (string)item["status"];
                                betInfo.combo = (string)item["combo"];
                                betInfo.stakeAmount = int.TryParse((string)item["amount"], out int amount) ? amount : 0;
                                betInfo.odds = int.TryParse((string)item["odds"], out int odds) ? odds : 0;
                                betInfo.action = string.Equals((string)item["action"], "赌", StringComparison.Ordinal) ? "BET" : "EAT";
                                betInfo.type = (string)item["type_code"];
                                betInfo.raceNo = (string)item["race"];
                                betInfo.mrParams = (string)item["mr_params"];
                                betInfo.raceType = raceType;
                                betInfo.raceDate = raceDate;

                                myPendingBETBetInfos.Add(betInfo);
                            }
                        }

                        marketData.Add("MyConfirmedBETBetInfos", myConfirmedBETBetInfos);
                        marketData.Add("MyPendingBETBetInfos", myPendingBETBetInfos);
                    }
                    return marketData;
                }
            }
            return null;
        }

        /// <summary>
        /// 查询我的交易
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="username"></param>
        /// <param name="raceDate"></param>
        /// <param name="raceType"></param>
        /// <param name="raceNo"></param>
        /// <returns></returns>
        public static IDictionary<string, List<BetInfo>> queryMyTrade(string serverAddress, string username, string raceDate, string raceType, string raceNo, out string serverProcessTime)
        {
            string result;
            string url = $"http://{serverAddress}/query/my_trade";
            var payloadObj = new
            {
                username = username,
                race_type = raceType,
                race_date = raceDate,
                race_num = raceNo
            };
            string payload = JsonConvert.SerializeObject(payloadObj);

            // 💡 必须在所有分支中对 out 参数赋初始值
            if (_isUseServer)
            {
                result = HTTPUtils.SendSyncRequest(url, "POST", null, null, payload, out serverProcessTime);
                _logger.Debug($"queryMyTrade result: {result}");
            }
            else
            {
                result = File.ReadAllText(@"C:\D\07-svn\01-gold\AutoHorseRace\devdoc\v2\market_response.json");
                serverProcessTime = "local_file"; // 离线模式默认值
            }

            JObject jsonResult = null;
            try
            {
                jsonResult = JObject.Parse(result);
                // 🌟 将服务端耗时注入到返回的 JSON 对象中
                jsonResult["serverProcessTime"] = serverProcessTime;
                // 💡 确保能够成功解析并返回正常状态的数据
                if (jsonResult["success"] != null && (bool)jsonResult["success"])
                {
                    _logger.Debug($"✅ [queryMyTrade] 账户 [{username}] 查询我的交易成功 (耗时: {serverProcessTime})");
                }
                else
                {
                    string errorMsg = jsonResult["message"]?.ToString() ?? jsonResult["detail"]?.ToString() ?? "未知错误";
                    _logger.Error($"❌ [queryMyTrade] 账户 [{username}] 查询我的交易返回失败状态: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"❌ [queryMyTrade] JSON 解析异常: {result}");
            }

            if (jsonResult != null && (bool)jsonResult["success"])
            {
                if (jsonResult["data"] != null)
                {
                    IDictionary<string, List<BetInfo>> marketData = new Dictionary<string, List<BetInfo>>();
                    string race_date = (string)jsonResult["data"]["race_date"];
                    string race_type = (string)jsonResult["data"]["race_type"];
                    string curr_rc = (string)jsonResult["data"]["curr_rc"];

                    JObject myTradesJObject = (JObject)jsonResult["data"]["my_trades"];

                    if (myTradesJObject != null)
                    {
                        List<BetInfo> myConfirmedBETBetInfos = new List<BetInfo>();
                        List<BetInfo> myPendingBETBetInfos = new List<BetInfo>();

                        // 解析 confirmed 列表
                        var confirmedArray = myTradesJObject["confirmed"] as JArray;
                        if (confirmedArray != null)
                        {
                            foreach (var item in confirmedArray)
                            {
                                BetInfo betInfo = new BetInfo();
                                betInfo.status = (string)item["status"];
                                betInfo.combo = (string)item["combo"];
                                betInfo.stakeAmount = int.TryParse((string)item["amount"], out int amount) ? amount : 0;
                                betInfo.odds = int.TryParse((string)item["odds"], out int odds) ? odds : 0;
                                string actionCode = (string)item["action_code"];
                                betInfo.action = (string.Equals(actionCode, "B", StringComparison.OrdinalIgnoreCase)) ? "BET" : "EAT";

                                betInfo.type = (string)item["type_code"];
                                betInfo.raceNo = (string)item["race"];
                                betInfo.raceType = raceType;
                                betInfo.raceDate = raceDate;

                                myConfirmedBETBetInfos.Add(betInfo);
                            }
                        }

                        // 解析 pending 列表
                        var pendingArray = myTradesJObject["pending"] as JArray;
                        if (pendingArray != null)
                        {
                            foreach (var item in pendingArray)
                            {
                                BetInfo betInfo = new BetInfo();
                                betInfo.status = (string)item["status"];
                                betInfo.combo = (string)item["combo"];
                                betInfo.stakeAmount = int.TryParse((string)item["amount"], out int amount) ? amount : 0;
                                betInfo.odds = int.TryParse((string)item["odds"], out int odds) ? odds : 0;

                                // 💡 同样使用兼容逻辑
                                string actionCode = (string)item["action_code"] ?? (string)item["action"];
                                betInfo.action = (string.Equals(actionCode, "B", StringComparison.OrdinalIgnoreCase) || string.Equals(actionCode, "赌", StringComparison.Ordinal)) ? "BET" : "EAT";

                                betInfo.type = (string)item["type_code"];
                                betInfo.raceNo = (string)item["race"];
                                betInfo.mrParams = (string)item["mr_params"];

                                betInfo.raceType = raceType;
                                betInfo.raceDate = raceDate;

                                myPendingBETBetInfos.Add(betInfo);
                            }
                        }

                        marketData.Add("MyConfirmedBETBetInfos", myConfirmedBETBetInfos);
                        marketData.Add("MyPendingBETBetInfos", myPendingBETBetInfos);
                    }
                    return marketData;
                }
            }
            return null;
        }

        /// <summary>
        /// 查询已结算的历史交易记录
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public static List<SettledHistoryInfo> querySettledHistoryInfo(string serverAddress, string username, out string serverProcessTime)
        {
            string result;
            string url = $"http://{serverAddress}/query/settled-history?username={username}";

            if (_isUseServer)
            {
                result = HTTPUtils.SendSyncRequest(url, "GET", null, null, null, out serverProcessTime);
                _logger.Debug($"querySettledHistoryInfo result: {result}");
            }
            else
            {
                result = File.ReadAllText(@"C:\D\07-svn\01-gold\AutoHorseRace\devdoc\v2\market_response.json");
                serverProcessTime = "local_file";
            }

            JObject jsonResult = null;
            try
            {
                jsonResult = JObject.Parse(result);
                jsonResult["serverProcessTime"] = serverProcessTime; // 注入耗时

                if (jsonResult["success"] != null && (bool)jsonResult["success"])
                {
                    _logger.Debug($"✅ [querySettledHistoryInfo] 账户 [{username}] 查询已结算的历史交易记录成功 (耗时: {serverProcessTime})");
                }
                else
                {
                    string errorMsg = jsonResult["message"]?.ToString() ?? jsonResult["detail"]?.ToString() ?? "未知错误";
                    _logger.Error($"❌ [querySettledHistoryInfo] 账户 [{username}] 查询已结算的历史交易记录返回失败状态: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"❌ [querySettledHistoryInfo] JSON 解析异常: {result}");
            }

            if (jsonResult != null && (bool)jsonResult["success"])
            {
                if (jsonResult["data"] != null)
                {
                    JArray dataJArray = (JArray)jsonResult["data"];
                    if (dataJArray != null)
                    {
                        List<SettledHistoryInfo> SettledHistoryInfos = new List<SettledHistoryInfo>();
                        foreach (var item in dataJArray)
                        {
                            SettledHistoryInfo SettledHistoryInfo = new SettledHistoryInfo();
                            SettledHistoryInfos.Add(SettledHistoryInfo);
                            SettledHistoryInfo.date = (string)item["date"];
                            SettledHistoryInfo.location = (string)item["location"];
                            SettledHistoryInfo.oddsType = (string)item["odds_type"];
                            SettledHistoryInfo.stake = (string)item["stake"];
                            SettledHistoryInfo.tax = (string)item["tax"];
                            SettledHistoryInfo.winLoss = (string)item["win_loss"];
                            SettledHistoryInfo.raceDate = (string)item["rd"];
                            SettledHistoryInfo.raceType = (string)item["type"];
                        }
                        return SettledHistoryInfos;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 查询已结算的汇总信息
        /// </summary>
        public static IDictionary<string, Object> querySettledInfo(string serverAddress, string username, string raceDate, string raceType, out string serverProcessTime)
        {
            string result;
            var payloadObj = new
            {
                username = username,
                race_type = raceType,
                race_date = raceDate,
            };
            string url = $"http://{serverAddress}/query/transaction-details";
            string payload = JsonConvert.SerializeObject(payloadObj);

            if (_isUseServer)
            {
                result = HTTPUtils.SendSyncRequest(url, "POST", null, null, payload, out serverProcessTime);
                _logger.Debug($"querySettledInfo result: {result}");
            }
            else
            {
                result = File.ReadAllText(@"C:\D\07-svn\01-gold\AutoHorseRace\devdoc\v2\market_response.json");
                serverProcessTime = "local_file";
            }

            JObject jsonResult = null;
            try
            {
                jsonResult = JObject.Parse(result);
                jsonResult["serverProcessTime"] = serverProcessTime; // 注入耗时

                if (jsonResult["success"] != null && (bool)jsonResult["success"])
                {
                    _logger.Debug($"✅ [querySettledInfo] 账户 [{username}] 查询已结算的汇总信息成功 (耗时: {serverProcessTime})");
                }
                else
                {
                    string errorMsg = jsonResult["message"]?.ToString() ?? jsonResult["detail"]?.ToString() ?? "未知错误";
                    _logger.Error($"❌ [querySettledInfo] 账户 [{username}] 查询已结算的汇总信息返回失败状态: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"❌ [querySettledInfo] JSON 解析异常: {result}");
            }

            if (jsonResult != null && jsonResult["success"] != null && (bool)jsonResult["success"])
            {
                JObject dataJObject = (JObject)jsonResult["data"];
                if (dataJObject != null)
                {
                    IDictionary<string, Object> SettledInfo = new Dictionary<string, Object>();

                    // 1. 解析 summary_rows
                    JArray summaryRowsJArray = (JArray)dataJObject["summary_rows"];
                    if (summaryRowsJArray != null && summaryRowsJArray.Count > 0)
                    {
                        List<SettledSummaryInfo> SettledSummaryInfos = new List<SettledSummaryInfo>();
                        foreach (var item in summaryRowsJArray)
                        {
                            SettledSummaryInfo settledSummaryInfo = new SettledSummaryInfo();
                            settledSummaryInfo.raceNo = (string)item["race"];
                            settledSummaryInfo.betStake = (string)item["bet_stake"];
                            settledSummaryInfo.betAmount = (string)item["bet_amount"];
                            settledSummaryInfo.betTax = (string)item["bet_tax"];
                            settledSummaryInfo.betRefund = (string)item["bet_refund"];
                            settledSummaryInfo.eatStake = (string)item["eat_stake"];
                            settledSummaryInfo.eatAmount = (string)item["eat_amount"];
                            settledSummaryInfo.eatTax = (string)item["eat_tax"];
                            settledSummaryInfo.eatRefund = (string)item["eat_refund"];
                            settledSummaryInfo.winLoss = (string)item["win_loss"];

                            SettledSummaryInfos.Add(settledSummaryInfo);
                        }
                        SettledInfo.Add("summary_rows", SettledSummaryInfos);
                    }

                    // 2. 解析 forecast_races
                    JObject forecastRacesJObject = (JObject)dataJObject["forecast_races"];
                    if (forecastRacesJObject != null && forecastRacesJObject.Count > 0)
                    {
                        IDictionary<string, List<RaceBettingRecordInfo>> RaceBettingRecordInfo = new Dictionary<string, List<RaceBettingRecordInfo>>();
                        foreach (var kvp in forecastRacesJObject)
                        {
                            JArray raceJArray = (JArray)kvp.Value;
                            if (raceJArray != null)
                            {
                                List<RaceBettingRecordInfo> RaceBettingRecordInfos = new List<RaceBettingRecordInfo>();
                                RaceBettingRecordInfo.Add("R" + kvp.Key, RaceBettingRecordInfos);
                                foreach (var race in raceJArray)
                                {
                                    RaceBettingRecordInfo raceBettingRecordInfo = new RaceBettingRecordInfo();
                                    raceBettingRecordInfo.isSub = race["is_sub"] != null && (bool)race["is_sub"];
                                    raceBettingRecordInfo.type = (string)race["type"];
                                    raceBettingRecordInfo.horses = (string)race["horses"];
                                    raceBettingRecordInfo.stake = (string)race["stake"];
                                    raceBettingRecordInfo.pct = (string)race["pct"];
                                    raceBettingRecordInfo.limit = (string)race["limit"];
                                    raceBettingRecordInfo.tax = (string)race["tax"];
                                    raceBettingRecordInfo.betPayout = (string)race["bet_payout"];
                                    raceBettingRecordInfo.odds = (string)race["odds"];
                                    raceBettingRecordInfo.eatPayout = (string)race["eat_payout"];
                                    raceBettingRecordInfo.winLoss = (string)race["win_loss"];
                                    raceBettingRecordInfo.action = (string)race["action"];
                                    raceBettingRecordInfo.actDate = (string)race["act_date"];
                                    raceBettingRecordInfo.actType = (string)race["act_type"];
                                    raceBettingRecordInfo.actHorses = (string)race["act_horses"];
                                    raceBettingRecordInfo.actStake = (string)race["act_stake"];
                                    raceBettingRecordInfo.actStatus = (string)race["act_status"];
                                    RaceBettingRecordInfos.Add(raceBettingRecordInfo);
                                }
                            }
                        }
                        SettledInfo.Add("forecast_races", RaceBettingRecordInfo);
                    }
                    // 如果你需要把 serverProcessTime 也带进返回的 IDictionary 中，可以按需添加
                    SettledInfo.Add("serverProcessTime", serverProcessTime);
                    return SettledInfo;
                }
            }
            return null;
        }

        /// <summary>
        /// 提交挂单
        /// </summary>
        public static JObject? submitOrder(string serverAddress, Account account, string raceType, string raceDate, int stakeAmount, IDictionary<string, string> marketData, out string serverProcessTime)
        {
            string result;
            string url = $"http://{serverAddress}/trade/submit_q";

            var payloadObj = new
            {
                username = account.UserCode,
                race_type = raceType,
                race_date = raceDate,
                race_num = marketData["race"],
                q_type = marketData["q_type"],
                action = marketData["action"],
                horses = Utils.getHorses(marketData["combo"]),
                amount = marketData["odds"],
                tix = stakeAmount,
                fclmt = marketData["limit"],
            };

            string payload = JsonConvert.SerializeObject(payloadObj);
            _logger.Debug($"submitOrder payload: {payload}");

            if (_isUseServer)
            {
                result = HTTPUtils.SendSyncRequest(url, "POST", null, null, payload, out serverProcessTime);
                _logger.Debug($"submitOrder result: {result}");
            }
            else
            {
                result = File.ReadAllText(@"C:\D\07-svn\01-gold\AutoHorseRace\devdoc\v2\submit_q_response.json");
                serverProcessTime = "local_file";
            }

            // 🔧 这里判断的是"HTTP 请求本身没有正确完成"（连接层直接报错、返回的不是合法响应体），
            // 属于真正的网络/传输层异常，跟"服务器已正常处理但业务上拒绝"是两回事，继续返回 null 是合理的。
            if (result.Contains("Error: Response status code does not indicate success: 400 (Bad Request).") || result.Contains("400 Bad Request"))
            {
                _logger.Error($"❌ [submitOrder] 400 Bad Request 错误，账户 [{account.UserCode}] 提交交易失败。服务器返回原始内容: {result}");
                return null;
            }

            try
            {
                JObject resultJObject = JObject.Parse(result);
                resultJObject["serverProcessTime"] = serverProcessTime; // 注入耗时

                string status = resultJObject["status"]?.ToString();
                bool isSuccess = resultJObject["success"]?.Value<bool>() ?? false;

                if (string.Equals(status, "SUCCESS", StringComparison.OrdinalIgnoreCase) || isSuccess)
                {
                    return resultJObject;
                }
                else
                {
                    string errorMsg = resultJObject["message"]?.ToString() ?? resultJObject["detail"]?.ToString() ?? "未知错误";
                    _logger.Error($"submitOrder failed: {errorMsg}");
                    // 🔧 关键修复：服务器已经给出了完整、明确的拒绝响应（HTTP 请求成功完成，
                    // 只是业务上判定失败），必须把这个 resultJObject 原样返回，而不是吞成 null。
                    // 上层 ExecuteTrade/AutoBetProcess/AutoEatProcess/ClosePositionByDeadline
                    // 需要靠"result != null 但 success=false"这个区别，
                    // 来判断到底是"服务器明确拒绝"（应立即释放预占）还是"请求异常/网络超时"
                    // （才应该保留预占、等下一轮快照核实）。之前这里没有 return，
                    // 会导致两种完全不同的情况在上层被误判成同一种"null（可能是网络问题）"，
                    // 掩盖了服务器真实的拒绝原因，也让本该释放的预占被错误地保留了 60 秒。
                    return resultJObject;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ [submitOrder] 解析响应 JSON 失败: {ex.Message}, 原始内容: {result}");
            }
            // 只有 HTTP 请求异常、或响应体根本不是合法 JSON（真正的传输/解析层面问题）才会走到这里，返回 null。
            return null;
        }

        /// <summary>
        /// 提交对冲下注
        /// </summary>
        public static JObject? singleAutoTrade(string serverAddress, Account account, string raceType, string raceDate, int stakeAmount, IDictionary<string, string> marketData, out string serverProcessTime)
        {
            string result;
            string url = $"http://{serverAddress}/trade/single_auto_trade";

            var payloadObj = new
            {
                username = account.UserCode,
                race_type = raceType,
                race_date = raceDate,
                race_num = marketData["race"],
                col_name = marketData["col_name"],
                bet_info = new
                {
                    limit = marketData["limit"],
                    combo = marketData["combo"],
                    odds = marketData["odds"]
                },
                stake_amount = stakeAmount
            };

            string payload = JsonConvert.SerializeObject(payloadObj);
            _logger.Debug($"singleAutoTrade payload: {payload}");

            if (_isUseServer)
            {
                result = HTTPUtils.SendSyncRequest(url, "POST", null, null, payload, out serverProcessTime);
                _logger.Debug($"singleAutoTrade result: {result}");
            }
            else
            {
                result = File.ReadAllText(@"C:\D\07-svn\01-gold\AutoHorseRace\devdoc\v2\submit_q_response.json");
                serverProcessTime = "local_file";
            }

            // 🔧 同样，只有 HTTP 请求本身没有正确完成才属于真正的传输层异常，应返回 null。
            if (result.Contains("Error: Response status code does not indicate success: 400 (Bad Request).") || result.Contains("400 Bad Request"))
            {
                _logger.Error($"❌ [singleAutoTrade] 400 Bad Request 错误，账户 [{account.UserCode}] 提交交易失败。");
                return null;
            }

            try
            {
                JObject resultJObject = JObject.Parse(result);
                resultJObject["serverProcessTime"] = serverProcessTime; // 注入耗时

                string status = resultJObject["status"]?.ToString();
                bool isSuccess = resultJObject["success"]?.Value<bool>() ?? false;

                if (string.Equals(status, "SUCCESS", StringComparison.OrdinalIgnoreCase) || isSuccess)
                {
                    return resultJObject;
                }
                else
                {
                    string errorMsg = resultJObject["message"]?.ToString() ?? resultJObject["detail"]?.ToString() ?? "未知错误";
                    _logger.Error($"singleAutoTrade failed: {errorMsg}");
                    // 🔧 关键修复：与 submitOrder 同理。这类响应（如 {"code":400,"message":"单笔交易未确认: 
                    // 对不起！ 你的交易不成功。","success":false}）是服务器正常处理完成后给出的明确业务拒绝，
                    // HTTP 请求本身没有任何异常（能被成功 JObject.Parse 就说明是合法完整的响应），
                    // 必须原样返回，让上层能区分"明确拒绝"和"网络异常"这两种完全不同的情况：
                    // 前者应立即释放预占、允许下一轮强平重新判断该 combo；
                    // 后者才应该保留预占、等待服务器快照核实，避免误判后重复下单。
                    // 之前这里没有 return，导致这两种情况在上层被统一误判成
                    // "原始响应: null(请求异常)"，是造成强平重复吃注的直接根因之一。
                    return resultJObject;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ [singleAutoTrade] 解析响应 JSON 失败: {ex.Message}");
            }
            // 只有 HTTP 请求异常、或响应体根本不是合法 JSON 才会走到这里，返回 null，
            // 代表"结果未知"，上层应保留预占、等待服务器快照核实。
            return null;
        }

        /// <summary>
        /// 删除所有挂单
        /// </summary>
        public static JObject deleteAll(string serverAddress, string username, string raceDate, string raceType, string rc, out string serverProcessTime)
        {
            string result;
            var payloadObj = new
            {
                username = username,
                race_date = raceDate,
                race_type = raceType,
                race_num = rc,
                batch_bet_mr = $"{rc},11,{raceType},{raceDate},{raceType},{rc}",
                batch_eat_mr = $"{rc},6,{raceType},{raceDate},{raceType},{rc}"
            };
            string url = $"http://{serverAddress}/trade/delete_all";
            string payload = JsonConvert.SerializeObject(payloadObj);
            _logger.Debug($"deleteAll request: {payload}");

            if (_isUseServer)
            {
                result = HTTPUtils.SendSyncRequest(url, "POST", null, null, payload, out serverProcessTime);
                _logger.Debug($"deleteAll result: {result}");
            }
            else
            {
                result = File.ReadAllText(@"C:\D\07-svn\01-gold\AutoHorseRace\devdoc\v2\submit_q_response.json");
                serverProcessTime = "local_file";
            }

            JObject jsonResult = null;
            try
            {
                jsonResult = JObject.Parse(result);
                jsonResult["serverProcessTime"] = serverProcessTime; // 注入耗时

                if (jsonResult["success"] != null && (bool)jsonResult["success"])
                {
                    _logger.Debug($"✅ [deleteAll] 账户 [{username}] 删除所有挂单成功 (耗时: {serverProcessTime})");
                }
                else
                {
                    string errorMsg = jsonResult["message"]?.ToString() ?? jsonResult["detail"]?.ToString() ?? "未知错误";
                    _logger.Error($"❌ [deleteAll] 账户 [{username}] 删除所有挂单返回失败状态: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"❌ [deleteAll] JSON 解析异常: {result}");
            }
            return jsonResult;
        }

        /// <summary>
        /// 查询当天赛事信息列表
        /// </summary>
        public static JObject queryTodayRacesInfo(string serverAddress, string username)
        {
            string result;
            var payloadObj = new
            {
                username = username
            };
            string url = $"http://{serverAddress}/query/active-races";
            string payload = JsonConvert.SerializeObject(payloadObj);
            string serverProcessTime = "0ms";

            if (_isUseServer)
            {
                result = HTTPUtils.SendSyncRequest(url, "POST", null, null, payload, out serverProcessTime);
                _logger.Debug($"queryTodayRacesInfo result: {result}");
            }
            else
            {
                result = File.ReadAllText(@"C:\D\07-svn\01-gold\AutoHorseRace\devdoc\v2\active-races_response.json");
                serverProcessTime = "local_file";
            }

            try
            {
                JObject jsonResult = JObject.Parse(result);
                jsonResult["serverProcessTime"] = serverProcessTime; // 注入耗时

                if (jsonResult["success"] != null && (bool)jsonResult["success"])
                {
                    _logger.Debug($"✅ [queryTodayRacesInfo] 账户 [{username}] 查询当天赛事信息列表成功 (耗时: {serverProcessTime})");
                }
                else
                {
                    string errorMsg = jsonResult["message"]?.ToString() ?? jsonResult["detail"]?.ToString() ?? "未知错误";
                    _logger.Error($"❌ [queryTodayRacesInfo] 账户 [{username}] 查询返回失败状态: {errorMsg}");
                }

                return jsonResult;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"❌ [queryTodayRacesInfo] JSON 解析异常: {result}");
                return null;
            }
        }

        /// <summary>
        /// 查询指定赛事信息,当前场次,开赛时间,总场次数
        /// </summary>
        public static JObject queryRaceInfo(string serverAddress, string username, string raceDate, string raceType, string raceNo)
        {
            string result;
            var payloadObj = new
            {
                username = username,
                race_type = raceType,
                race_date = raceDate
            };
            string url = $"http://{serverAddress}/query/race-times";
            string payload = JsonConvert.SerializeObject(payloadObj);
            string serverProcessTime = "0ms";

            if (_isUseServer)
            {
                result = HTTPUtils.SendSyncRequest(url, "POST", null, null, payload, out serverProcessTime);
                _logger.Debug($"queryRaceInfo result: {result}");
            }
            else
            {
                result = File.ReadAllText(@"C:\D\07-svn\01-gold\AutoHorseRace\devdoc\v2\venue-race-info_response.json");
                serverProcessTime = "local_file";
            }

            try
            {
                JObject jsonResult = JObject.Parse(result);
                jsonResult["serverProcessTime"] = serverProcessTime; // 注入耗时

                if (jsonResult["success"] != null && (bool)jsonResult["success"])
                {
                    _logger.Debug($"✅ [queryRaceInfo] 账户 [{username}] 查询赛事信息列表成功 (耗时: {serverProcessTime})");
                }
                else
                {
                    string errorMsg = jsonResult["message"]?.ToString() ?? jsonResult["detail"]?.ToString() ?? "未知错误";
                    _logger.Error($"❌ [queryRaceInfo] 账户 [{username}] 查询返回失败状态: {errorMsg}");
                }

                return jsonResult;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"❌ [queryRaceInfo] JSON 解析异常: {result}");
                return null;
            }
        }
    }
}