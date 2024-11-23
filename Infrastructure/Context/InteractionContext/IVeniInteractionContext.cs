using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

namespace FFXIVVenues.Veni.Infrastructure.Context;

public interface IVeniInteractionContext
{
    DiscordSocketClient Client { get; }
    Session Session { get; }

    #region State handling
    Task MoveSessionToStateAsync<TSessionType>() where TSessionType : ISessionState;
    Task MoveSessionToStateAsync<TSessionType, TSessionArgType>(TSessionArgType arg) where TSessionType : ISessionState<TSessionArgType>;
    Task MoveSessionToStateAsync<TSessionType>(TSessionType state) where TSessionType : ISessionStateBase;
    Task<bool> TryBackStateAsync();
    Task ClearSessionAsync();
    #endregion
    
    #region Component Handling
    string RegisterComponentHandler(Func<ComponentVeniInteractionContext, Task> @delegate, ComponentPersistence persistence);
    Task HandleComponentInteraction(ComponentVeniInteractionContext context);
    Task ClearComponentHandlers();
    #endregion
 
    #region Message Handling
    string RegisterMessageHandler(Func<MessageVeniInteractionContext, Task> @delegate);
    Task<bool> HandleMessageAsync(MessageVeniInteractionContext context);
    void ClearMessageHandlers();
    #endregion
        
    ISocketMessageChannel GetChannel();
    VeniInteractionContext ToWrappedInteraction();
}
