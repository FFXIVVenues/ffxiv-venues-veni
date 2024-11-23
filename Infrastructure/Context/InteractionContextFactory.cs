using System;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context.Abstractions;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;


namespace FFXIVVenues.Veni.Infrastructure.Context;

public class InteractionContextFactory(ISessionProvider sessionProvider, IServiceProvider serviceProvider, DiscordSocketClient client)
    : IInteractionContextFactory
{
    public MessageVeniInteractionContext Create(SocketMessage message)
    {
        var conversationContext = sessionProvider.GetSession(message);
        return new MessageVeniInteractionContext(message, client, serviceProvider, conversationContext);
    }

    public ComponentVeniInteractionContext Create(SocketMessageComponent message)
    {
        var session = sessionProvider.GetSession(message);
        return new ComponentVeniInteractionContext(message, client, serviceProvider, session);
    }

    public SlashCommandVeniInteractionContext Create(SocketSlashCommand message)
    {
        var sessionContext = sessionProvider.GetSession(message);
        return new SlashCommandVeniInteractionContext(message, client, serviceProvider, sessionContext);
    }
    
}