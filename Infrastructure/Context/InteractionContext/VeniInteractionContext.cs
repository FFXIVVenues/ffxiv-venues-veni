using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.AI.Clu.CluModels;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionWrappers;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;

public abstract class VeniInteractionContext<T>(T message, DiscordSocketClient client, Session conversation, IServiceProvider serviceProvider)
    : IVeniInteractionContext
    where T : class
{

    public T Interaction { get; } = message;
    public DiscordSocketClient Client { get; } = client;
    public Session Session { get; } = conversation;
    public IDisposable TypingHandle { get; set; }
        
    protected readonly IServiceProvider ServiceProvider = serviceProvider;
        
    private int _backClearance = 2;

    internal void SetBackClearanceAmount(int amount) =>
        this._backClearance = amount;
        
    #region State handling

    public async Task NewSessionAsync<TSessionStateType>() where TSessionStateType : ISessionState
    {
        await this.ClearSessionAsync();
        await this.MoveSessionToStateAsync<TSessionStateType>();
    }
    
    public async Task NewSessionAsync<TSessionState, TSessionStateContext>(TSessionStateContext arg) where TSessionState : ISessionState<TSessionStateContext>
    {
        await this.ClearSessionAsync();
        await this.MoveSessionToStateAsync<TSessionState, TSessionStateContext>(arg);
    }
    
    public async Task MoveSessionToStateAsync<TSessionStateType>() where TSessionStateType : ISessionState
    {
        var newState = ActivatorUtilities.CreateInstance<TSessionStateType>(ServiceProvider);
        await this.MoveSessionToStateAsync(newState);
    }
        
    public async Task MoveSessionToStateAsync<TSessionState, TSessionStateContext>(TSessionStateContext arg) where TSessionState : ISessionState<TSessionStateContext>
    {
        var newState = ActivatorUtilities.CreateInstance<TSessionState>(ServiceProvider, arg);
        await this.MoveSessionToStateAsync(newState);
    }
        
    public async Task MoveSessionToStateAsync<TSessionState>(TSessionState state) where TSessionState : ISessionStateBase
    {
        await this.ClearComponentHandlers();
        this.ClearMessageHandlers();
        this.Session.UpdateState(state);
        if (this.Session.StateStack.TryPeek(out var currentState))
            Log.Debug("Set state from [{PreviousState}] to {State}", currentState?.GetType().Name, typeof(T).Name);
        else
            Log.Debug("Set state to [{State}]", typeof(T).Name);
            
        await state.EnterState(this.ToWrappedInteraction());
    }
    
    public async Task<bool> TryBackStateAsync()
    {
        var (previousState, newState) = this.Session.RewindState();
        if (previousState == null || newState == null)
            return false;
        
        await this.ClearComponentHandlers();
        this.ClearMessageHandlers();
        
        Log.Debug("Back state from [{PreviousState}] to [{State}]",
            previousState?.GetType().Name, newState?.GetType().Name);
        
        await newState.EnterState(this.ToWrappedInteraction());
        return true;
    }
    
    public async Task ClearSessionAsync()
    {
        await this.ClearPreviousComponents();
        this.Session.Clear();
    }
    #endregion
    
    #region Component Handling
    public string RegisterComponentHandler(Func<ComponentVeniInteractionContext, Task> @delegate, ComponentPersistence persistence)
    {
        var key = Guid.NewGuid().ToString();
        var registration = new ComponentSessionHandlerRegistration(@delegate, persistence);
        this.Session.RegisterComponentHandler(key, registration);
        return key;
    }
        
    public Task HandleComponentInteraction(ComponentVeniInteractionContext context)
    {
        var key = context.Interaction.Data.CustomId.Split(":");
        if (key[0] == ComponentBroker.ValuesToHandlersKey)
            key = context.Interaction.Data.Values?.FirstOrDefault()?.Split(":");

        var handler = this.Session.GetComponentHandler(key![0]);
        if (handler == null)
            return Task.CompletedTask;

        if (handler.Persistence == ComponentPersistence.ClearRow)
            _ = context.Interaction.ModifyOriginalResponseAsync(props => props.Components = new ComponentBuilder().Build());
        if (handler.Persistence == ComponentPersistence.DeleteMessage)
            _ = context.Interaction.DeleteOriginalResponseAsync();

        return handler.Delegate(context);
    }
        
    public async Task ClearComponentHandlers()
    {
        await this.ClearPreviousComponents();
        this.Session.ClearComponentHandlers();
    }
        
    private async Task ClearPreviousComponents()
    {
        var channel = this.GetChannel();
        var messages = await channel.GetMessagesAsync(this._backClearance).FirstAsync();
        foreach (var message in messages)
        {
            if (message.Author.Id == this.Client.CurrentUser.Id && message.Components.Any())
                foreach (var wrappingComponent in message.Components)
                {
                    if (!(wrappingComponent is ActionRowComponent actionRow))
                        continue;

                    var messageDeleted = false;
                    foreach (var component in actionRow.Components)
                    {
                        var handler = this.Session.GetComponentHandler(component.CustomId);
                        if (handler?.Persistence == ComponentPersistence.ClearRow)
                        {
                            _ = channel.ModifyMessageAsync(message.Id, m => m.Components = new ComponentBuilder().Build());
                            break;
                        }
                        if (handler?.Persistence == ComponentPersistence.DeleteMessage)
                        {
                            _ = channel.DeleteMessageAsync(message.Id);
                            messageDeleted = true;  
                            break;
                        }
                    }

                    if (messageDeleted)
                        break;
                }
        }
    }
    #endregion
 
    #region Message Handling
    public string RegisterMessageHandler(Func<MessageVeniInteractionContext, Task> @delegate)
    {
        var key = Guid.NewGuid().ToString();
        this.Session.RegisterMessageHandler(key, @delegate);
        return key;
    }
        
    public async Task<bool> HandleMessageAsync(MessageVeniInteractionContext context)
    {
        var handled = false;
        foreach (var handler in this.Session.GetMessageHandlers())
        {
            await handler(context);
            handled = true;
        }
        return handled;
    }
        
    public void ClearMessageHandlers() =>
        this.Session.ClearMessageHandlers();
    #endregion
        
    public abstract ISocketMessageChannel GetChannel();

    public abstract VeniInteractionContext ToWrappedInteraction();
}

public class VeniInteractionContext(IInteractionWrapper m,
    DiscordSocketClient dsc,
    Session sc,
    IServiceProvider sp,
    CluPrediction prediction = null) 
    : VeniInteractionContext<IInteractionWrapper>(m, dsc, sc, sp)
{
    public CluPrediction Prediction { get; set; } = prediction;
        
    public override VeniInteractionContext ToWrappedInteraction() => 
        this;
        
    public override ISocketMessageChannel GetChannel() =>
        this.Interaction.Channel;
}

public class MessageVeniInteractionContext(
    SocketMessage m,
    DiscordSocketClient dsc,
    IServiceProvider sp,
    Session sc)
    : VeniInteractionContext<SocketMessage>(m, dsc, sc, sp), IWrappableInteraction
{
    
    public override VeniInteractionContext ToWrappedInteraction() =>
        new (new MessageWrapper(this.Interaction),
            this.Client,
            this.Session,
            this.ServiceProvider);
    
    public override ISocketMessageChannel GetChannel() =>
        this.Interaction.Channel;
    
    #region Message Handling
    public async Task<bool> HandleMessageAsync()
    {
        var handled = false;
        foreach (var handler in this.Session.GetMessageHandlers())
        {
            await handler(this);
            handled = true;
        }
        return handled;
    }
    #endregion
    
}

public class ComponentVeniInteractionContext(
    SocketMessageComponent m,
    DiscordSocketClient dsc,
    IServiceProvider sp,
    Session sc)
    : VeniInteractionContext<SocketMessageComponent>(m, dsc, sc, sp), IWrappableInteraction
{
    public override VeniInteractionContext ToWrappedInteraction() =>
        new (new MessageComponentWrapper(this.Interaction),
            this.Client,
            this.Session,
            this.ServiceProvider);

    public override ISocketMessageChannel GetChannel() =>
        this.Interaction.Channel;
}