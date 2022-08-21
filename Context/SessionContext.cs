
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Utils;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.States.Abstractions;
using System.Collections.Generic;
using NChronicle.Core.Interfaces;

namespace FFXIVVenues.Veni.Context
{
    public class SessionContext
    {

        public Stack<IState> StateStack { get; private set; } = new();
        public ConcurrentDictionary<string, object> Data { get; } = new();


        private readonly IServiceProvider _serviceProvider;
        private IChronicle _chronicle;
        private ConcurrentDictionary<string, Func<MessageComponentInteractionContext, Task>> _componentHandlers = new();
        private ConcurrentDictionary<string, Func<MessageInteractionContext, Task>> _messageHandlers = new();

        public SessionContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _chronicle = serviceProvider.GetService<IChronicle>();
        }

        public async Task SetStateAsync<T>(InteractionContext context) where T : IState
        {
            if (StateStack.TryPeek(out var currentState))
                this._chronicle.Debug($"Set state from [{currentState?.GetType().Name}] to [{typeof(T).Name}]");
            else
                this._chronicle.Debug($"Set state to [{typeof(T).Name}]");

            this.ClearComponentHandlers();
            this.ClearMessageHandlers();
            var newState = ActivatorUtilities.CreateInstance<T>(_serviceProvider);
            StateStack.Push(newState);
            await newState.Init(context);
        }

        public Task ShiftState<T>(IWrappableInteraction context) where T : IState =>
            this.SetStateAsync<T>(context.ToWrappedInteraction());

        public async Task<bool> TryBackStateAsync(InteractionContext context)
        {
            if (!StateStack.TryPop(out var currentState))
                return false;
            if (!StateStack.TryPeek(out var newState))
                return false;
            this._chronicle.Debug($"Back state from [{currentState?.GetType().Name}] to [{newState?.GetType().Name}]");
            this.ClearComponentHandlers();
            this.ClearMessageHandlers();
            await newState.Init(context);
            return true;
        }

        public Task<bool> TryBackStateAsync(IWrappableInteraction context) =>
            this.TryBackStateAsync(context.ToWrappedInteraction());


        public void ClearState()
        {
            this.Data.Clear();
            this.ClearComponentHandlers();
            this.ClearMessageHandlers();
            StateStack = new();
        }

        public T GetItem<T>(string name)
        {
            var itemFound = Data.TryGetValue(name, out var item);
            if (!itemFound) return default;
            return (T)item;
        }

        public void ClearItem(string name)
        {
            Data.TryRemove(name, out _);
        }

        public void SetItem<T>(string name, T item)
        {
            Data.AddOrUpdate(name, item, (s, o) => item);
        }

        public string RegisterComponentHandler(Func<MessageComponentInteractionContext, Task> @delegate, ComponentPersistence persistence)
        {
            var key = Guid.NewGuid().ToString();
            this._componentHandlers[key] = persistence switch
            {
                ComponentPersistence.ClearRow => (context) =>
                {
                    _ = context.Interaction.ModifyOriginalResponseAsync(props => props.Components = new ComponentBuilder().Build());
                    return @delegate(context);
                },
                ComponentPersistence.DeleteMessage => (context) =>
                {
                    _ = context.Interaction.DeleteOriginalResponseAsync();
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

        public async Task HandleComponentInteraction(MessageComponentInteractionContext context)
        {
            if (this._componentHandlers.TryGetValue(context.Interaction.Data.CustomId, out var handler))
                await handler(context);
        }

        public string RegisterMessageHandler(Func<MessageInteractionContext, Task> @delegate)
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

        public async Task<bool> HandleMessageAsync(MessageInteractionContext context)
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
