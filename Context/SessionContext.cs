﻿
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
using System.Linq;

namespace FFXIVVenues.Veni.Context
{
    public class SessionContext
    {

        public Stack<IState> StateStack { get; private set; } = new();
        public ConcurrentDictionary<string, object> Data { get; } = new();


        private readonly IServiceProvider _serviceProvider;
        private IChronicle _chronicle;
        private ConcurrentDictionary<string, ComponentHandlerRegistration> _componentHandlers = new();
        private int _backClearance = 2;

        internal void SetBackClearanceAmount(int amount) =>
            this._backClearance = amount;

        private ConcurrentDictionary<string, Func<MessageInteractionContext, Task>> _messageHandlers = new();

        public SessionContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _chronicle = serviceProvider.GetService<IChronicle>();
        }

        public async Task MoveStateAsync<T>(InteractionContext context) where T : IState
        {
            if (StateStack.TryPeek(out var currentState))
                this._chronicle.Debug($"Set state from [{currentState?.GetType().Name}] to [{typeof(T).Name}]");
            else
                this._chronicle.Debug($"Set state to [{typeof(T).Name}]");

            await this.ClearComponentHandlers(context);
            this.ClearMessageHandlers();
            var newState = ActivatorUtilities.CreateInstance<T>(_serviceProvider);
            StateStack.Push(newState);
            await newState.Enter(context);
        }

        public Task MoveStateAsync<T>(IWrappableInteraction context) where T : IState =>
            this.MoveStateAsync<T>(context.ToWrappedInteraction());

        public async Task<bool> TryBackStateAsync(InteractionContext context)
        {
            if (!StateStack.TryPop(out var currentState))
                return false;
            if (!StateStack.TryPeek(out var newState))
                return false;
            this._chronicle.Debug($"Back state from [{currentState?.GetType().Name}] to [{newState?.GetType().Name}]");
            await this.ClearComponentHandlers(context);
            this.ClearMessageHandlers();
            await newState.Enter(context);
            return true;
        }

        public Task<bool> TryBackStateAsync(IWrappableInteraction context) =>
            this.TryBackStateAsync(context.ToWrappedInteraction());

        public async Task ClearState(InteractionContext context)
        {
            this.Data.Clear();
            await this.ClearComponentHandlers(context);
            this.ClearMessageHandlers();
            StateStack = new();
        }

        public Task ClearState(IWrappableInteraction context) =>
            this.ClearState(context.ToWrappedInteraction());

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
            var registration = new ComponentHandlerRegistration(@delegate, persistence);
            this._componentHandlers[key] = registration;
            return key;
        }

        public void UnregisterComponentHandler(string key)
        {
            this._componentHandlers.TryRemove(key, out _);
        }

        public async Task ClearComponentHandlers(InteractionContext context)
        {
            await this.ClearPreviousComponents(context.Interaction.Channel, context.Client.CurrentUser.Id);
            this._componentHandlers.Clear();
        }

        public Task HandleComponentInteraction(MessageComponentInteractionContext context)
        {
            if (!this._componentHandlers.TryGetValue(context.Interaction.Data.CustomId, out var handler))
                return Task.CompletedTask;

            if (handler.Persistence == ComponentPersistence.ClearRow)
                _ = context.Interaction.ModifyOriginalResponseAsync(props => props.Components = new ComponentBuilder().Build());
            if (handler.Persistence == ComponentPersistence.DeleteMessage)
                _ = context.Interaction.DeleteOriginalResponseAsync();

            return handler.Delegate(context);
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

        // TODO: Move this to ClearComponentHandlers method
        private async Task ClearPreviousComponents(ISocketMessageChannel channel, ulong currentUser)
        {
            var messages = await channel.GetMessagesAsync(this._backClearance).FirstAsync();
            foreach (var message in messages)
            {
                if (message.Author.Id == currentUser && message.Components.Any())
                foreach (var wrappingComponent in message.Components)
                {
                    if (!(wrappingComponent is ActionRowComponent actionRow))
                        continue;

                    var rowCleared = false;
                    var messageDeleted = false;
                    foreach (var component in actionRow.Components)
                        if (this._componentHandlers.TryGetValue(component.CustomId, out var handler))
                            if (!rowCleared && handler.Persistence == ComponentPersistence.ClearRow)
                            {
                                _ = channel.ModifyMessageAsync(message.Id, m => m.Components = new ComponentBuilder().Build());
                                rowCleared = true;
                                break;
                            }
                            else if (handler.Persistence == ComponentPersistence.DeleteMessage)
                            {
                                _ = channel.DeleteMessageAsync(message.Id);
                                messageDeleted = true;  
                                break;
                            }

                    if (messageDeleted)
                        break;
                }
            }
        }
    }
}
