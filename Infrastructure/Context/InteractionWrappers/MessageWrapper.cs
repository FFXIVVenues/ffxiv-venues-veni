using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using NChronicle.Core.Interfaces;

namespace FFXIVVenues.Veni.Infrastructure.Context.InteractionWrappers;

public class MessageWrapper : IInteractionWrapper
{

    public SocketUser User => _message?.Author;
    public ISocketMessageChannel Channel => _message?.Channel;
    public string Content => _message?.CleanContent;
    public IInteractionDataWrapper InteractionData => null;
    public bool IsDM => this._message.Channel is IDMChannel;

    private readonly SocketMessage _message;
    private readonly IChronicle _chronicle;

    public MessageWrapper(SocketMessage message, IChronicle chronicle)
    {
        this._message = message;
        this._chronicle = chronicle;
    }

    public Task RespondAsync(string message = null, MessageComponent component = null, Embed embed = null)
    {
        this._chronicle.Info($"**Veni Ki** [bot]: {message} (Components: {component?.Components?.Count ?? 0}) (Embeds: {(embed != null ? "Yes" : "No")})");
        return _message.Channel.SendMessageAsync(message, components: component, embed: embed);
    }

    public string GetArgument(string name) =>
        null;
}
