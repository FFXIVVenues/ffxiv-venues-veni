using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context.Abstractions;
using NChronicle.Core.Interfaces;

namespace FFXIVVenues.Veni.Infrastructure.Context;

public class InteractionContextFactory : IInteractionContextFactory
{
    private readonly ISessionProvider _sessionProvider;
    private readonly DiscordSocketClient _client;

    public InteractionContextFactory(ISessionProvider sessionProvider, DiscordSocketClient client)
    {
        this._sessionProvider = sessionProvider;
        this._client = client;
    }

    public MessageVeniInteractionContext Create(SocketMessage message)
    {
        var conversationContext = _sessionProvider.GetSession(message);
        return new MessageVeniInteractionContext(message, this._client, conversationContext);
    }

    public MessageComponentVeniInteractionContext Create(SocketMessageComponent message)
    {
        var session = _sessionProvider.GetSession(message);
        return new MessageComponentVeniInteractionContext(message, _client, session);
    }

    public SlashCommandVeniInteractionContext Create(SocketSlashCommand message)
    {
        var sessionContext = _sessionProvider.GetSession(message);
        return new SlashCommandVeniInteractionContext(message, _client, sessionContext);
    }
    
}