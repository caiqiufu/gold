namespace AutoHorseRace
{
    public class ComboTradeState
    {
        private const double EPS = 0.001;

        public string RaceNo { get; }
        public string Type { get; }
        public string Combo { get; }
        public string DictKey => BuildDictKey(RaceNo, Type, Combo);

        public string RaceDate { get; private set; } = string.Empty;
        public string RaceType { get; private set; } = string.Empty;
        public double Toto { get; private set; }

        public double BetExecuted { get; private set; }
        public double BetPending { get; private set; }
        public double BetOdds { get; private set; }
        public double BetLimit { get; private set; }

        public double EatExecuted { get; private set; }
        public double EatPending { get; private set; }
        public double EatOdds { get; private set; }
        public double EatLimit { get; private set; }
        public int TradeRecordId { get; private set; }
        public void SetTradeRecordId(int id) { lock (_sync) { TradeRecordId = id; } }
        public DateTime LastServerUpdateTime { get; private set; } = DateTime.MinValue;

        private readonly object _sync = new object();
        private readonly List<InFlightIntent> _inFlightBets = new List<InFlightIntent>();
        private readonly List<InFlightIntent> _inFlightEats = new List<InFlightIntent>();

        public ComboTradeState(string raceNo, string type, string combo)
        {
            RaceNo = raceNo;
            Type = type;
            Combo = combo;
        }

        public static string BuildDictKey(string raceNo, string type, string combo) => $"{raceNo}_{type}_{combo}";

        public void ApplyServerSnapshot(EatBetInfo serverInfo)
        {
            if (serverInfo == null) return;

            lock (_sync)
            {
                RaceDate = serverInfo.raceDate;
                RaceType = serverInfo.raceType;
                Toto = serverInfo.toto;

                BetExecuted = serverInfo.betExecutedAmount;
                BetPending = serverInfo.betPendingAmount;
                BetOdds = serverInfo.betOdds;
                BetLimit = serverInfo.betLimit;

                EatExecuted = serverInfo.eatExecutedAmount;
                EatPending = serverInfo.eatPendingAmount;
                EatOdds = serverInfo.eatOdds;
                EatLimit = serverInfo.eatLimit;

                LastServerUpdateTime = DateTime.Now;

                // 🔧 关键修复：TTL 到期不再是"可以清除预占"的理由。
                // 唯一允许清除的条件是服务器快照已经证明这笔金额被体现出来了。
                // 这样即使 GetBatBetInfo/服务器确认链路有延迟或 bug，也绝不会因为
                // "等够60秒"就误判成"可以再下一单"——这是"同一个combo一次bet或者
                // eat只能一单，无论赔率多少"这条硬性规则的底线。
                _inFlightBets.RemoveAll(i => (BetExecuted + BetPending) >= i.BaselineTotal + i.Amount - EPS);
                _inFlightEats.RemoveAll(i => (EatExecuted + EatPending) >= i.BaselineTotal + i.Amount - EPS);
            }
        }

        /// <summary>决策要看的"有效待处理"= 服务器 pending + 尚未被服务器确认体现的本地预占（不再按 TTL 过滤）。</summary>
        public double EffectiveBetPending
        {
            // 🔧 去掉 .Where(i => !i.IsExpired)：过期只是"该报警了"，不代表"可以当它不存在"。
            get { lock (_sync) { return BetPending + _inFlightBets.Sum(i => i.Amount); } }
        }

        public double EffectiveEatPending
        {
            get { lock (_sync) { return EatPending + _inFlightEats.Sum(i => i.Amount); } }
        }

        /// <summary>决策要看的"有效已吃"= 服务器已确认 + 尚未被服务器确认体现的本地预占。</summary>
        public double EffectiveEatCommitted
        {
            get { lock (_sync) { return EatExecuted + _inFlightEats.Sum(i => i.Amount); } }
        }

        /// <summary>
        /// 供轮询代码（RefreshMyTradeSnapshot / QueryMyTradeInfListDataAsync）在每次
        /// ApplyServerSnapshot 之后调用，把"预占已经超过TTL但服务器仍未体现"这件事
        /// 大声报出来——这通常意味着 GetBatBetInfo 或 queryMyTrade 链路本身有问题，
        /// 需要人工介入，而不是让系统自己悄悄放行新的下单。
        /// </summary>
        public IReadOnlyList<(string Id, double Amount, bool IsBet)> GetStaleIntents()
        {
            lock (_sync)
            {
                var result = new List<(string, double, bool)>();
                result.AddRange(_inFlightBets.Where(i => i.IsExpired).Select(i => (i.Id, i.Amount, true)));
                result.AddRange(_inFlightEats.Where(i => i.IsExpired).Select(i => (i.Id, i.Amount, false)));
                return result;
            }
        }

        /// <summary>
        /// 唯一的人工解锁出口：确认某个预占确实已经死掉（比如你手动核对过交易所后台，
        /// 这笔单子确实没有成交也不会成交）之后，显式调用它来解除预占。
        /// 绝不应该被任何定时器/轮询自动调用。
        /// </summary>
        public void ForceReleaseIntent(string intentId, string operatorReason)
        {
            lock (_sync)
            {
                _inFlightBets.RemoveAll(i => i.Id == intentId);
                _inFlightEats.RemoveAll(i => i.Id == intentId);
            }
        }
        public void ResetTradeRecordId() { lock (_sync) { TradeRecordId = 0; } }
        public string ReserveBetIntent(double amount, TimeSpan ttl)
        {
            lock (_sync)
            {
                var intent = new InFlightIntent(amount, BetExecuted + BetPending, ttl);
                _inFlightBets.Add(intent);
                return intent.Id;
            }
        }

        public void ReleaseBetIntent(string intentId)
        {
            lock (_sync) { _inFlightBets.RemoveAll(i => i.Id == intentId); }
        }

        public string ReserveEatIntent(double amount, TimeSpan ttl)
        {
            lock (_sync)
            {
                var intent = new InFlightIntent(amount, EatExecuted, ttl);
                _inFlightEats.Add(intent);
                return intent.Id;
            }
        }

        public void ReleaseEatIntent(string intentId)
        {
            lock (_sync) { _inFlightEats.RemoveAll(i => i.Id == intentId); }
        }

        public string DescribeForLog()
        {
            lock (_sync)
            {
                return $"[{DictKey}] betExecuted={BetExecuted}, betPending={BetPending}(有效{EffectiveBetPending}), " +
                       $"eatExecuted={EatExecuted}, eatPending={EatPending}(有效{EffectiveEatPending}), effectiveEat={EffectiveEatCommitted}";
            }
        }

        public EatBetInfo ToEatBetInfo()
        {
            lock (_sync)
            {
                return new EatBetInfo
                {
                    raceDate = RaceDate,
                    raceType = RaceType,
                    raceNo = RaceNo,
                    type = Type,
                    combo = Combo,
                    toto = Toto,
                    betExecutedAmount = BetExecuted,
                    betPendingAmount = BetPending,
                    betOdds = BetOdds,
                    betLimit = BetLimit,
                    eatExecutedAmount = EatExecuted,
                    eatPendingAmount = EatPending,
                    eatOdds = EatOdds,
                    eatLimit = EatLimit,
                    totalBetAmount = BetExecuted + BetPending,
                    totalEatAmount = EatExecuted + EatPending
                };
            }
        }
    }
}