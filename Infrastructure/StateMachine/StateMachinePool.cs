using System.Collections.Concurrent;
using System.Threading.Tasks;
using Stateless;

namespace FFXIVVenues.Veni.Infrastructure.StateMachine;

public class StateMachinePool<TState, TTrigger>
{

    private ConcurrentDictionary<string, StateMachine<TState, TTrigger>> _stateMachines;

    public Task AddToPool(string key, StateMachine<TState, TTrigger> stateMachine)
    {
        this._stateMachines.TryAdd(key, stateMachine);
        return Task.CompletedTask;
    }

    public Task<StateMachine<TState, TTrigger>> Get(string key)
    {
        this._stateMachines.TryGetValue(key, out var stateMachine);
        return Task.FromResult(stateMachine);
    }
    
}