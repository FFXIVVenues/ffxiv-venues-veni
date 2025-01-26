using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context.Abstractions;


namespace FFXIVVenues.Veni.Infrastructure.Context;

public class InteractionContextFactory(ISessionProvider sessionProvider, DiscordSocketClient client)
    : IInteractionContextFactory
{
    public MessageVeniInteractionContext Create(SocketMessage message)
    {
        var conversationContext = sessionProvider.GetSession(message);
        return new MessageVeniInteractionContext(message, client, conversationContext);
    }

    public ComponentVeniInteractionContext Create(SocketMessageComponent message)
    {
        var session = sessionProvider.GetSession(message);
        return new ComponentVeniInteractionContext(message, client, session);
    }

    public SlashCommandVeniInteractionContext Create(SocketSlashCommand message)
    {
        var sessionContext = sessionProvider.GetSession(message);
        return new SlashCommandVeniInteractionContext(message, client, sessionContext);
    }
    
}