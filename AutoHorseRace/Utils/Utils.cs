using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace AutoHorseRace.Utils
{
    public class Utils
    {
        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIP()
        {
            string name = System.Net.Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            foreach (IPAddress ipa in ipadrlist)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ipa.ToString();
                }
            }
            return "";
        }

        // 1. 提取安全的键值提取方法（局部辅助函数）
        public static string GetDictValue(IDictionary<string, string> dict, string key)
        {
            return dict != null && dict.TryGetValue(key, out string value) ? value : string.Empty;
        }


        /// <summary>
        /// 获取投注状态的文本描述
        /// </summary>
        public static string GetBettingStatusDescription(int status)
        {
            return status switch
            {
                0 => "初始化或未定义状态",
                1 => "全新没有下注",
                2 => "已下注并完全平仓",
                3 => "已下过吃注,待下赌注",
                4 => "已下过赌注,待下吃注",
                -1 => "存在未平下注",
                _ => "异常状态"
            };
        }

        public static int[] getHorses(string combo)
        {
            // 假设 marketData["combo"] 的值可能是 "2-4" 或 "(2-4)" 或 " ( 2-4 ) "
            string rawCombo = combo.ToString();

            // 1. 去除所有空格、左右括号
            string cleanCombo = Regex.Replace(rawCombo, @"[\s\(\)]+", "");

            // 2. 按 '-' 分割并转换为整数数组
            int[] horses = cleanCombo
                .Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(h => int.Parse(h))
                .ToArray();
            return horses;
        }

        public static HashSet<string> GetAllowedSet(string configStr)
        {
            if (string.IsNullOrWhiteSpace(configStr)) return null;

            string cleaned = configStr.Replace('，', ',').Replace(" ", "");
            return new HashSet<string>(cleaned.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
        }
        /// <summary>
        /// 获取最新的下注信息
        /// </summary>
        /// <param name="allMyTrades"></param>
        /// <param name="eatBetInfosList"></param>
        /// <param name="eatBetInfoDict"></param>
        public static void GetBatBetInfo(
            List<BetInfo> allMyTrades,
            out List<EatBetInfo> eatBetInfosList,
            out Dictionary<string, EatBetInfo> eatBetInfoDict)
        {
            // 1. 初始化 out 参数
            eatBetInfosList = new List<EatBetInfo>();
            eatBetInfoDict = new Dictionary<string, EatBetInfo>();

            if (allMyTrades != null && allMyTrades.Count > 0)
            {
                // 2. 按照 combo 和 type 进行分组聚合，生成 List
                eatBetInfosList = allMyTrades
                    .GroupBy(b => new { b.combo, b.type })
                    .Select(g =>
                    {
                        var firstItem = g.First();

                        var eatInfo = new EatBetInfo
                        {
                            seq = firstItem.seq,
                            raceDate = firstItem.raceDate,
                            raceType = firstItem.raceType,
                            raceNo = firstItem.raceNo,
                            type = g.Key.type,
                            combo = g.Key.combo,
                            toto = firstItem.toto,

                            // ==================== 1. BET (下注) 相关字段 ====================
                            // 修正：使用 stakeAmount，并用 StringComparison 忽略 status 大小写
                            betExecutedAmount = g.Where(x => x.action == "BET" && string.Equals(x.status, "confirmed", StringComparison.OrdinalIgnoreCase))
                                                 .Sum(x => (double)x.stakeAmount),
                            betPendingAmount = g.Where(x => x.action == "BET" && string.Equals(x.status, "pending", StringComparison.OrdinalIgnoreCase))
                                                .Sum(x => (double)x.stakeAmount),
                            betOdds = g.Where(x => x.action == "BET")
                                       .Select(x => (double)x.odds)
                                       .LastOrDefault(),
                            betLimit = g.Where(x => x.action == "BET")
                                        .Select(x => (double)x.limit)
                                        .LastOrDefault(),
                            totalBetAmount = g.Where(x => x.action == "BET")
                                              .Sum(x => (double)x.stakeAmount),

                            // ==================== 2. EAT (吃票) 相关字段 ====================
                            eatExecutedAmount = g.Where(x => x.action == "EAT" && string.Equals(x.status, "confirmed", StringComparison.OrdinalIgnoreCase))
                                                 .Sum(x => (double)x.stakeAmount),
                            eatPendingAmount = g.Where(x => x.action == "EAT" && string.Equals(x.status, "pending", StringComparison.OrdinalIgnoreCase))
                                                .Sum(x => (double)x.stakeAmount),
                            eatOdds = g.Where(x => x.action == "EAT")
                                       .Select(x => (double)x.odds)
                                       .LastOrDefault(),
                            eatLimit = g.Where(x => x.action == "EAT")
                                        .Select(x => (double)x.limit)
                                        .LastOrDefault(),
                            totalEatAmount = g.Where(x => x.action == "EAT")
                                              .Sum(x => (double)x.stakeAmount)
                        };

                        return eatInfo;
                    })
                    .ToList();

                // 3. 将 List 转换为以 "Type_Combo" 为 Key 的 Dictionary
                foreach (var item in eatBetInfosList)
                {
                    string dictKey = $"{item.type}_{item.combo}";
                    eatBetInfoDict[dictKey] = item;
                }
            }
        }

        /// <summary>
        /// 获取最新的下注信息,并刷新本地内存数据（返回元组形式）
        /// </summary>
        /// <param name="BetBatInfos"></param>
        /// <returns></returns>
        public static (List<BetInfo> allTrades, List<EatBetInfo> eatBetInfosList, Dictionary<string, EatBetInfo> eatBetInfoDict) GetBatBetInfo(IDictionary<string, List<BetInfo>> BetBatInfos)
        {
            List<BetInfo> allMyTrades = new List<BetInfo>();
            var eatBetInfosList = new List<EatBetInfo>();
            var eatBetInfoDict = new Dictionary<string, EatBetInfo>();

            // 1. 判断并提取确认和未确认的下注信息
            if (BetBatInfos != null && (BetBatInfos.ContainsKey("MyConfirmedBETBetInfos") || BetBatInfos.ContainsKey("MyPendingBETBetInfos")))
            {
                if (BetBatInfos.TryGetValue("MyConfirmedBETBetInfos", out var confirmedList) && confirmedList != null)
                {
                    allMyTrades.AddRange(confirmedList);
                }

                if (BetBatInfos.TryGetValue("MyPendingBETBetInfos", out var pendingList) && pendingList != null)
                {
                    allMyTrades.AddRange(pendingList);
                }
            }

            // 2. 如果有数据，进行分组聚合
            if (allMyTrades.Count > 0)
            {
                eatBetInfosList = allMyTrades
                    .GroupBy(b => new { b.combo, b.type })
                    .Select(g =>
                    {
                        var firstItem = g.First();

                        // ==================== 1. BET (下注) 相关字段计算 ====================
                        // 注意：这里不再因为"该组合曾有confirmed记录"就把pending清零——
                        // 一个组合下可以同时存在多笔独立的BET订单（不同水折），有的已confirmed、
                        // 有的还在等撮合，必须如实反映，否则决策引擎会看不见真正挂着的新订单。
                        double betExecuted = g.Where(x => x.action == "BET" && string.Equals(x.status, "confirmed", StringComparison.OrdinalIgnoreCase))
                                              .Sum(x => (double)x.stakeAmount);
                        double betPending = g.Where(x => x.action == "BET" && string.Equals(x.status, "pending", StringComparison.OrdinalIgnoreCase))
                                              .Sum(x => (double)x.stakeAmount);

                        // ==================== 2. EAT (吃票) 相关字段计算 ====================
                        double eatExecuted = g.Where(x => x.action == "EAT" && string.Equals(x.status, "confirmed", StringComparison.OrdinalIgnoreCase))
                                              .Sum(x => (double)x.stakeAmount);
                        double eatPending = g.Where(x => x.action == "EAT" && string.Equals(x.status, "pending", StringComparison.OrdinalIgnoreCase))
                                              .Sum(x => (double)x.stakeAmount);

                        var eatInfo = new EatBetInfo
                        {
                            seq = firstItem.seq,
                            raceDate = firstItem.raceDate,
                            raceType = firstItem.raceType,
                            raceNo = firstItem.raceNo,
                            type = g.Key.type,
                            combo = g.Key.combo,
                            toto = firstItem.toto,

                            // BET 赋值
                            betExecutedAmount = betExecuted,
                            betPendingAmount = betPending,
                            totalBetAmount = betExecuted + betPending, // 严格遵循：总额 = 已执行 + 待处理
                            betOdds = g.Where(x => x.action == "BET")
                                       .Select(x => (double)x.odds)
                                       .LastOrDefault(),
                            betLimit = g.Where(x => x.action == "BET")
                                        .Select(x => (double)x.limit)
                                        .LastOrDefault(),

                            // EAT 赋值
                            eatExecutedAmount = eatExecuted,
                            eatPendingAmount = eatPending,
                            totalEatAmount = eatExecuted + eatPending, // 同理
                            eatOdds = g.Where(x => x.action == "EAT")
                                       .Select(x => (double)x.odds)
                                       .LastOrDefault(),
                            eatLimit = g.Where(x => x.action == "EAT")
                                        .Select(x => (double)x.limit)
                                        .LastOrDefault()
                        };

                        return eatInfo;
                    })
                    .ToList();

                // 3. 转换为以 "{raceNo}_{type}_{combo}" 为 Key 的 Dictionary
                foreach (var item in eatBetInfosList)
                {
                    string dictKey = $"{item.raceNo}_{item.type}_{item.combo}";
                    eatBetInfoDict[dictKey] = item;
                }
            }

            // 4. 返回 3 个结果
            return (allMyTrades, eatBetInfosList, eatBetInfoDict);
        }
    }
}
