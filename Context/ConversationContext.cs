using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.States;
using Discord;

namespace FFXIVVenues.Veni.Context
{
    public class ConversationContext
    {


        public ConcurrentDictionary<string, object> ContextData { get; } = new();
        public IState ActiveState { get; private set; }

        private ConcurrentDictionary<string, Func<MessageContext, Task>> _componentHandlers = new();
        private ConcurrentDictionary<string, Func<MessageContext, Task>> _messageHandlers = new();

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public ConversationContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetService<ILogger>();
        }

        public async Task ShiftState<T>(MessageContext context) where T : IState
        {
            _ = _logger.LogAsync("StateShift", $"[{ActiveState?.GetType().Name}] -> [{typeof(T).Name}]");
            this.ClearComponentHandlers();
            this.ClearMessageHandlers();
            ActiveState = ActivatorUtilities.CreateInstance<T>(_serviceProvider);
            await ActiveState.Init(context);
        }

        public void ClearState()
        {
            this.ContextData.Clear();
            this.ClearComponentHandlers();
            this.ClearMessageHandlers();
            ActiveState = null;
        }

        public T GetItem<T>(string name)
        {
            var itemFound = ContextData.TryGetValue(name, out var item);
            if (!itemFound) return default;
            return (T)item;
        }

        public void ClearItem(string name)
        {
            ContextData.TryRemove(name, out _);
        }

        public void SetItem<T>(string name, T item)
        {
            ContextData.AddOrUpdate(name, item, (s, o) => item);
        }

        public string RegisterComponentHandler(Func<MessageContext, Task> @delegate, ComponentPersistence persistence)
        {
            var key = Guid.NewGuid().ToString();
            this._componentHandlers[key] = persistence switch
            {
                ComponentPersistence.ClearRow => (context) =>
                {
                    _ = context.MessageComponent.ModifyOriginalResponseAsync(props => props.Components = new ComponentBuilder().Build());
                    return @delegate(context);
                },
                ComponentPersistence.DeleteMessage => (context) =>
                {
                    _ = context.MessageComponent.DeleteOriginalResponseAsync();
                    return @delegate(context);
                },
                _ => @delegate,
            };
            return key;
        }

        public void UnregisterComponentHandler(string key)
        {
            this._componentHandlers.TryRemove(key, out _);
        }

        public void ClearComponentHandlers()
        {
            this._componentHandlers.Clear();
        }

        public async Task HandleComponentInteraction(MessageContext context)
        {
            if (this._componentHandlers.TryGetValue(context.MessageComponent.Data.CustomId, out var handler))
                await handler(context);
        }

        public string RegisterMessageHandler(Func<MessageContext, Task> @delegate)
        {
            var key = Guid.NewGuid().ToString();
            this._messageHandlers[key] = @delegate;
            return key;
        }

        public void UnregisterMessageHandler(string key)
        {
            this._messageHandlers.TryRemove(key, out _);
        }


        public void ClearMessageHandlers()
        {
            this._messageHandlers.Clear();
        }

        public async Task<bool> HandleMessageAsync(MessageContext context)
        {
            var handled = false;
            foreach (var handler in this._messageHandlers)
            {
                await handler.Value(context);
                handled = true;
            }
            return handled;
        }

    }
}
