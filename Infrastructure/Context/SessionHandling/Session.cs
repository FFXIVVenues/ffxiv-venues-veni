using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;

namespace FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

public class Session
{

    public Stack<ISessionStateBase> StateStack { get; private set; } = new();
        
    private ConcurrentDictionary<string, object> _data { get; } = new();
    private ConcurrentDictionary<string, ComponentSessionHandlerRegistration> _componentHandlers = new();
    private ConcurrentDictionary<string, Func<MessageVeniInteractionContext, Task>> _messageHandlers = new();

    public ISessionStateBase GetState() =>
        StateStack.TryPeek(out var result) ? result : null;

    #region State handling
    public void UpdateState<T>(T state) where T : ISessionStateBase
    {
        this.ClearMessageHandlers();
        StateStack.Push(state);
    }

    public (ISessionStateBase previous, ISessionStateBase @new) RewindState()
    {
        if (!this.StateStack.TryPop(out var currentState))
            return (null, null);
        if (!this.StateStack.TryPeek(out var newState) || newState is null)
            return (currentState, null);
        return (currentState, newState);
    }
    #endregion

    public void Clear()
    {
        this.StateStack.Clear();
        this._data.Clear();
        this.ClearComponentHandlers();
        this.ClearMessageHandlers();
    }
        
    #region SessionStorage        
    public void SetItem<T>(string name, T item) =>
        _data.AddOrUpdate(name, item, (s, o) => item);
        
    public T GetItem<T>(string name)
    {
        var itemFound = _data.TryGetValue(name, out var item);
        if (!itemFound) return default;
        return (T)item;
    }

    public void ClearItem(string name) =>
        _data.TryRemove(name, out _);
    #endregion

    #region ComponentHandling
    public void RegisterComponentHandler(string key, ComponentSessionHandlerRegistration handlerRegistration) =>
        this._componentHandlers[key] = handlerRegistration;

    public ComponentSessionHandlerRegistration GetComponentHandler(string key) =>
        this._componentHandlers.GetValueOrDefault(key);
        
    public void UnregisterComponentHandler(string key) =>
        this._componentHandlers.TryRemove(key, out _);
        
    public void ClearComponentHandlers() =>
        this._componentHandlers.Clear();
    #endregion        

    #region MessageHandling
    public void RegisterMessageHandler(string key, Func<MessageVeniInteractionContext, Task> @delegate) =>
        this._messageHandlers[key] = @delegate;

    public void UnregisterMessageHandler(string key) =>
        this._messageHandlers.TryRemove(key, out _);

    public Func<MessageVeniInteractionContext, Task>[] GetMessageHandlers() =>
        this._messageHandlers.Values.ToArray();
        
    public void ClearMessageHandlers() =>
        this._messageHandlers.Clear();
    #endregion

}