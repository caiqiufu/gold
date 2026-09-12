using AutoHorseRace.Utils;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace AutoHorseRace
{
    public class TradeStateStore
    {
        private readonly ConcurrentDictionary<string, ComboTradeState> _states = new ConcurrentDictionary<string, ComboTradeState>();
        private readonly Channel<string> _writeQueue = Channel.CreateUnbounded<string>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

        private CancellationTokenSource _cts;
        private Account _account;
        private Config _config;
        private Action<string> _logInfo;
        private Action<string> _logError;

        public ComboTradeState GetOrCreate(string raceNo, string type, string combo)
        {
            string dictKey = ComboTradeState.BuildDictKey(raceNo, type, combo);
            return _states.GetOrAdd(dictKey, _ => new ComboTradeState(raceNo, type, combo));
        }

        public bool TryGet(string dictKey, out ComboTradeState state) => _states.TryGetValue(dictKey, out state);

        public IEnumerable<ComboTradeState> GetAll() => _states.Values;

        /// <summary>切换到下一场时调用，只清掉指定场次的旧状态，不影响别的场次（正常情况下也不会有别的场次残留）。</summary>
        public void ClearRace(string raceNo)
        {
            string prefix = raceNo + "_";
            foreach (var key in _states.Keys.Where(k => k.StartsWith(prefix)).ToList())
            {
                _states.TryRemove(key, out _);
            }
        }

        public void MarkDirty(string dictKey) => _writeQueue.Writer.TryWrite(dictKey);

        public void Start(Account account, Config config, Action<string> logInfo = null, Action<string> logError = null)
        {
            _account = account;
            _config = config;
            _logInfo = logInfo ?? (_ => { });
            _logError = logError ?? (_ => { });
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => WriterLoopAsync(_cts.Token));
            _logInfo("[TradeStateStore] 异步落库线程已启动");
        }

        public void Stop()
        {
            _cts?.Cancel();
            _writeQueue.Writer.TryComplete();
            _logInfo?.Invoke("[TradeStateStore] 异步落库线程已停止");
        }

        public void WarmUpFromDatabase(IEnumerable<EatBetInfo> historyRows)
        {
            var rows = historyRows.ToList();
            foreach (var row in rows)
            {
                var state = GetOrCreate(row.raceNo, row.type, row.combo);
                state.ApplyServerSnapshot(row);
            }
            _logInfo?.Invoke($"[TradeStateStore] 冷启动加载完成，共恢复 {rows.Count} 条组合状态");
        }

        private async Task WriterLoopAsync(CancellationToken token)
        {
            await foreach (var dictKey in _writeQueue.Reader.ReadAllAsync(token))
            {
                if (!_states.TryGetValue(dictKey, out var state)) continue;
                try
                {
                    // 🔧 关键修复：落库用的 raceType/raceDate/raceNo 必须跟着 state 自己走，
                    // 不能用 _config.CurrentRaceType/CurrentRaceDate/CurrentRaceNo（当前UI选中的场次）。
                    // 这个写队列是异步的，MarkDirty 入队和这里真正处理之间必然有延迟，
                    // 一旦这段延迟窗口内场次发生切换（AutoToNextRace/手动切场次下拉框），
                    // _config.Current* 就已经指向新场次了，但这条消息对应的仍然是旧场次的组合，
                    // 用当前场次的参数去定位/更新数据库记录，会导致查不到匹配的旧行、
                    // 从而插入一条错位的新记录——这正是"多出一条记录但没有真正提交到服务器"的成因。
                    // state.ToEatBetInfo() 自己就带着正确的 raceType/raceDate/raceNo，直接用它。
                    var info = state.ToEatBetInfo();
                    int tradeRecordId = DBHelper.UpdateTradeRecord(_account, info.raceType, info.raceDate,
                        info.raceNo, info, "Y", "Auto Refresh(异步)");
                    if (tradeRecordId > 0)
                    {
                        state.SetTradeRecordId(tradeRecordId);
                    }
                }
                catch (Exception ex)
                {
                    _logError($"[{dictKey}] 异步落库失败，重新入队重试: {ex.Message}");
                    _writeQueue.Writer.TryWrite(dictKey);
                }
            }
        }
    }
}