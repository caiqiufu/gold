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
        public DateTime LastServerUpdateTime { get; private set; } = DateTime.MinValue;
        private readonly object _sync = new object();
        private readonly List<InFlightIntent> _inFlightBets = new List<InFlightIntent>();
        private readonly List<InFlightIntent> _inFlightEats = new List<InFlightIntent>();
        private double _betCommittedHighWatermark;
        private double _eatCommittedHighWatermark;
        public ComboTradeState(string raceNo, string type, string combo)
        {
            RaceNo = raceNo;
            Type = type;
            Combo = combo;
        }
        public static string BuildDictKey(string raceNo, string type, string combo) => $"{raceNo}_{type}_{combo}";
        public void SetTradeRecordId(int id)
        {
            lock (_sync)
            {
                TradeRecordId = id;
            }
        }
        public void ResetTradeRecordId()
        {
            lock (_sync)
            {
                TradeRecordId = 0;
            }
        }
        public void ApplyServerSnapshot(EatBetInfo serverInfo)
        {
            if (serverInfo == null) return;
            lock (_sync)
            {
                RaceDate = serverInfo.raceDate;
                RaceType = serverInfo.raceType;
                Toto = serverInfo.toto;
                double serverBetExecuted = Math.Max(0, serverInfo.betExecutedAmount);
                double serverEatExecuted = Math.Max(0, serverInfo.eatExecutedAmount);
                double serverBetPending = Math.Max(0, serverInfo.betPendingAmount);
                double serverEatPending = Math.Max(0, serverInfo.eatPendingAmount);
                double serverBetCommitted = serverBetExecuted + serverBetPending;
                double serverEatCommitted = serverEatExecuted + serverEatPending;
                _betCommittedHighWatermark = Math.Max(_betCommittedHighWatermark, serverBetCommitted);
                _eatCommittedHighWatermark = Math.Max(_eatCommittedHighWatermark, serverEatCommitted);
                BetExecuted = Math.Max(BetExecuted, serverBetExecuted);
                EatExecuted = Math.Max(EatExecuted, serverEatExecuted);
                BetPending = serverBetPending;
                BetOdds = serverInfo.betOdds;
                BetLimit = serverInfo.betLimit;
                EatPending = serverEatPending;
                EatOdds = serverInfo.eatOdds;
                EatLimit = serverInfo.eatLimit;
                LastServerUpdateTime = DateTime.Now;
                _inFlightBets.RemoveAll(i =>
                    (BetExecuted + BetPending) >= i.BaselineTotal + i.Amount - EPS);
                _inFlightEats.RemoveAll(i =>
                    (EatExecuted + EatPending) >= i.BaselineTotal + i.Amount - EPS);
            }
        }
        // 🔧 BUG 修复：_betCommittedHighWatermark / _eatCommittedHighWatermark 是"已执行+挂单中"
        // 金额的历史最大值，用来防止过期/乱序的服务器快照把 EffectiveXxxCommitted 错误地往回拉低
        // （例如一笔正确已挂单的金额，因为某次快照请求竞态返回了旧数据而被误判成"消失了"）。
        // 但这个设计没有考虑到"挂单被正当取消/删除"这种情况——挂单一旦被 deleteAll 真正删除，
        // 对应的 pending 金额就应该合法地归零，历史水位线却因为只会取 Max 而永远回不去，
        // 导致 EffectiveEatCommitted 永久卡在一个虚高的值上，GetRemainingEatAmount 从此永远算出 0，
        // 即使 BetExecuted 和 EatExecuted 之间明明还有缺口，该组合也再也无法被判定为"需要强平"。
        // 复现链路：某次挂单尝试（如吃票试探性挂单）把 HWM 顶到 executed+pending 的峰值 →
        // 该挂单一直未成交、随后被 deleteAll 删除 → 下一次服务器快照里 pending 已经合法归零 →
        // 但 Math.Max(HWM, 新值) 仍然保留旧的峰值 → 永久卡死。
        // 修复方式：只在"服务器已确认挂单被删除"这个明确时间点，把水位线显式下调回当前真正
        // 已执行（Executed）的金额——Executed 本身另有单调不减的保护（Math.Max 用在 EatExecuted /
        // BetExecuted 赋值上），所以下调到 Executed 是绝对安全的下限，不会误吞任何真实已成交金额，
        // 只会清除掉"已被取消、从未成交"的那部分虚高历史水位。
        public void ResetCommittedHighWatermarkAfterCancel()
        {
            lock (_sync)
            {
                _betCommittedHighWatermark = BetExecuted;
                _eatCommittedHighWatermark = EatExecuted;
            }
        }
        public double EffectiveBetPending
        {
            get
            {
                lock (_sync)
                {
                    return BetPending + _inFlightBets.Sum(i => i.Amount);
                }
            }
        }
        public double EffectiveEatPending
        {
            get
            {
                lock (_sync)
                {
                    return EatPending + _inFlightEats.Sum(i => i.Amount);
                }
            }
        }
        public double EffectiveBetCommitted
        {
            get
            {
                lock (_sync)
                {
                    double serverCommitted = Math.Max(BetExecuted + BetPending, _betCommittedHighWatermark);
                    double inFlight = _inFlightBets.Sum(i => i.Amount);
                    return serverCommitted + inFlight;
                }
            }
        }
        public double EffectiveEatCommitted
        {
            get
            {
                lock (_sync)
                {
                    double serverCommitted = Math.Max(EatExecuted + EatPending, _eatCommittedHighWatermark);
                    double inFlight = _inFlightEats.Sum(i => i.Amount);
                    return serverCommitted + inFlight;
                }
            }
        }
        public double GetHardRemainingEatAmount()
        {
            lock (_sync)
            {
                double effectiveBet = Math.Max(BetExecuted, _betCommittedHighWatermark);
                double effectiveEat = Math.Max(
                    Math.Max(EatExecuted + EatPending, _eatCommittedHighWatermark),
                    0);
                effectiveEat += _inFlightEats.Sum(i => i.Amount);
                double remaining = effectiveBet - effectiveEat;
                return remaining > EPS ? remaining : 0;
            }
        }
        public bool TryReserveEatIntent(double requestedAmount, TimeSpan ttl, out string intentId, out double reservedAmount)
        {
            lock (_sync)
            {
                intentId = null;
                reservedAmount = 0;
                if (requestedAmount <= EPS) return false;
                double effectiveBet = Math.Max(BetExecuted, _betCommittedHighWatermark);
                double effectiveEat = Math.Max(
                    Math.Max(EatExecuted + EatPending, _eatCommittedHighWatermark),
                    0);
                effectiveEat += _inFlightEats.Sum(i => i.Amount);
                double hardRemaining = effectiveBet - effectiveEat;
                if (hardRemaining <= EPS) return false;
                reservedAmount = Math.Min(requestedAmount, hardRemaining);
                if (reservedAmount <= EPS) return false;
                var intent = new InFlightIntent(reservedAmount, EatExecuted, ttl);
                _inFlightEats.Add(intent);
                intentId = intent.Id;
                return true;
            }
        }
        public IReadOnlyList<(string Id, double Amount, bool IsBet)> GetStaleIntents()
        {
            lock (_sync)
            {
                var result = new List<(string Id, double Amount, bool IsBet)>();
                result.AddRange(_inFlightBets.Where(i => i.IsExpired).Select(i => (i.Id, i.Amount, true)));
                result.AddRange(_inFlightEats.Where(i => i.IsExpired).Select(i => (i.Id, i.Amount, false)));
                return result;
            }
        }
        public void ForceReleaseIntent(string intentId, string operatorReason)
        {
            if (string.IsNullOrWhiteSpace(intentId)) return;
            lock (_sync)
            {
                _inFlightBets.RemoveAll(i => i.Id == intentId);
                _inFlightEats.RemoveAll(i => i.Id == intentId);
            }
        }
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
            lock (_sync)
            {
                _inFlightBets.RemoveAll(i => i.Id == intentId);
            }
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
            lock (_sync)
            {
                _inFlightEats.RemoveAll(i => i.Id == intentId);
            }
        }
        public string DescribeForLog()
        {
            lock (_sync)
            {
                double serverEatCommitted = Math.Max(EatExecuted + EatPending, _eatCommittedHighWatermark);
                double inFlightEat = _inFlightEats.Sum(i => i.Amount);
                double effectiveEat = serverEatCommitted + inFlightEat;
                double serverBetCommitted = Math.Max(BetExecuted + BetPending, _betCommittedHighWatermark);
                double inFlightBet = _inFlightBets.Sum(i => i.Amount);
                double effectiveBet = serverBetCommitted + inFlightBet;
                double remaining = Math.Max(0, Math.Max(BetExecuted, _betCommittedHighWatermark) - effectiveEat);
                return $"[{DictKey}] betExecuted={BetExecuted:F3}, betPending={BetPending:F3}(有效{EffectiveBetPending:F3}), " +
                       $"eatExecuted={EatExecuted:F3}, eatPending={EatPending:F3}(有效{EffectiveEatPending:F3}), " +
                       $"effectiveBet={effectiveBet:F3}, effectiveEat={effectiveEat:F3}, hardRemainingEat={remaining:F3}, " +
                       $"betHWM={_betCommittedHighWatermark:F3}, eatHWM={_eatCommittedHighWatermark:F3}";
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
