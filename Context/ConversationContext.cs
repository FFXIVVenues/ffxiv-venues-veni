using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.States;

namespace FFXIVVenues.Veni.Context
{
    public class ConversationContext
    {


        public ConcurrentDictionary<string, object> ContextData { get; } = new();
        public IState ActiveState { get; private set; }

        private ConcurrentDictionary<string, Func<MessageContext, Task>> _componentHandlers = new();

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
            ActiveState = ActivatorUtilities.CreateInstance<T>(_serviceProvider); ;
            await ActiveState.Init(context);
        }

        public void ClearState()
        {
            ActiveState = null;
        }

        public void ClearData()
        {
            ContextData.Clear();
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

        public string RegisterComponentHandler(Func<MessageContext, Task> @delegate)
        {
            var key = Guid.NewGuid().ToString();
            this._componentHandlers[key] = @delegate;
            return key;
        }

        public void UnregisterComponentHandler(string key)
        {
            this._componentHandlers.TryRemove(key, out _);
        }

        public async Task RunComponentHandlerAsync(MessageContext context)
        {
            if (this._componentHandlers.TryGetValue(context.MessageComponent.Data.CustomId, out var handler))
                await handler(context);
        }

    }
}
