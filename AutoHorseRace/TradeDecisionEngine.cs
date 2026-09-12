namespace AutoHorseRace
{
    public class TradeDecisionEngine
    {
        private const double EPS = 0.001;

        public double GetRemainingEatAmount(ComboTradeState state)
        {
            double remaining = state.BetExecuted - state.EffectiveEatCommitted;
            return remaining > EPS ? remaining : 0;
        }

        /// <summary>吃票要不要跳过。</summary>
        public bool ShouldSkip(ComboTradeState state, out string reason)
        {
            if (state.EffectiveBetPending > EPS || state.EffectiveEatPending > EPS)
            {
                reason = $"存在待处理金额(Bet:{state.EffectiveBetPending}, Eat:{state.EffectiveEatPending})";
                return true;
            }
            double remaining = GetRemainingEatAmount(state);
            if (remaining <= 0)
            {
                reason = $"吃单已达到或超过赌单执行量(betExecuted={state.BetExecuted}, effectiveEat={state.EffectiveEatCommitted})";
                return true;
            }
            reason = null;
            return false;
        }

        /// <summary>要不要开一笔新的赌注——只有这个 combo 当前"干净"（没有挂单、赌吃平衡）才允许。</summary>
        public bool ShouldSkipBet(ComboTradeState state, out string reason)
        {
            if (state.EffectiveBetPending > EPS || state.EffectiveEatPending > EPS)
            {
                reason = $"存在待处理金额(Bet:{state.EffectiveBetPending}, Eat:{state.EffectiveEatPending})";
                return true;
            }
            bool isBalanced = Math.Abs((state.BetExecuted + state.BetPending) - (state.EatExecuted + state.EatPending)) <= EPS
                               && Math.Abs(state.EatExecuted - state.BetExecuted) <= EPS;
            if (!isBalanced)
            {
                reason = $"赌吃不平衡(betExecuted={state.BetExecuted}, eatExecuted={state.EatExecuted})，暂缓下新赌注";
                return true;
            }
            reason = null;
            return false;
        }
    }
}