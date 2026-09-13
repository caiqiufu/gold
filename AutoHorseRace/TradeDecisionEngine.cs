namespace AutoHorseRace
{
    public class TradeDecisionEngine
    {
        private const double EPS = 0.001;
        public double GetRemainingEatAmount(ComboTradeState state)
        {
            if (state == null) return 0;
            double remaining = state.BetExecuted - state.EffectiveEatCommitted;
            return remaining > EPS ? remaining : 0;
        }
        public bool ShouldSkip(ComboTradeState state, out string reason)
        {
            if (state == null)
            {
                reason = "交易状态为空";
                return true;
            }
            if (state.EffectiveBetPending > EPS || state.EffectiveEatPending > EPS)
            {
                reason = $"存在待处理金额(Bet:{state.EffectiveBetPending:F3}, Eat:{state.EffectiveEatPending:F3})";
                return true;
            }
            double remaining = GetRemainingEatAmount(state);
            if (remaining <= EPS)
            {
                reason = $"吃单已达到或超过赌单执行量(betExecuted={state.BetExecuted:F3}, effectiveEat={state.EffectiveEatCommitted:F3})";
                return true;
            }
            reason = null;
            return false;
        }
        public bool ShouldSkipBet(ComboTradeState state, out string reason)
        {
            if (state == null)
            {
                reason = "交易状态为空";
                return true;
            }
            if (state.EffectiveBetPending > EPS || state.EffectiveEatPending > EPS)
            {
                reason = $"存在待处理金额(Bet:{state.EffectiveBetPending:F3}, Eat:{state.EffectiveEatPending:F3})";
                return true;
            }
            bool isBalanced =
                Math.Abs((state.BetExecuted + state.BetPending) - (state.EatExecuted + state.EatPending)) <= EPS &&
                Math.Abs(state.EatExecuted - state.BetExecuted) <= EPS;
            if (!isBalanced)
            {
                reason = $"赌吃不平衡(betExecuted={state.BetExecuted:F3}, eatExecuted={state.EatExecuted:F3})，暂缓下新赌注";
                return true;
            }
            reason = null;
            return false;
        }
    }
}