namespace AutoHorseRace
{
    public class InFlightIntent
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public double Amount { get; }
        /// <summary>预占那一刻，Executed+Pending 的合计——用来判断服务器数据后续是否已经把这笔吸收进去了。</summary>
        public double BaselineTotal { get; }
        private readonly DateTime _expireAt;
        public bool IsExpired => DateTime.Now > _expireAt;

        public InFlightIntent(double amount, double baselineTotal, TimeSpan ttl)
        {
            Amount = amount;
            BaselineTotal = baselineTotal;
            _expireAt = DateTime.Now + ttl;
        }
    }
}