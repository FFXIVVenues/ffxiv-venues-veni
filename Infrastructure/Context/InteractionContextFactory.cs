using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context.Abstractions;
using NChronicle.Core.Interfaces;

namespace FFXIVVenues.Veni.Infrastructure.Context;

public class InteractionContextFactory : IInteractionContextFactory
{
    private readonly ISessionProvider _sessionProvider;
    private readonly DiscordSocketClient _client;
    private readonly IChronicle _chronicle;

    public InteractionContextFactory(ISessionProvider sessionProvider, DiscordSocketClient client, IChronicle chronicle)
    {
        this._sessionProvider = sessionProvider;
        this._client = client;
        this._chronicle = chronicle;
    }

    public MessageVeniInteractionContext Create(SocketMessage message)
    {
        var conversationContext = _sessionProvider.GetSession(message);
        return new MessageVeniInteractionContext(message, this._client, conversationContext, this._chronicle);
    }

    public MessageComponentVeniInteractionContext Create(SocketMessageComponent message)
    {
        var session = _sessionProvider.GetSession(message);
        return new MessageComponentVeniInteractionContext(message, _client, session, this._chronicle);
    }

    public SlashCommandVeniInteractionContext Create(SocketSlashCommand message)
    {
        var sessionContext = _sessionProvider.GetSession(message);
        return new SlashCommandVeniInteractionContext(message, _client, sessionContext, this._chronicle);
    }
    
}