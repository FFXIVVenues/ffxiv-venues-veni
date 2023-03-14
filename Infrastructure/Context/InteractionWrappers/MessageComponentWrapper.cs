using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using NChronicle.Core.Interfaces;

namespace FFXIVVenues.Veni.Infrastructure.Context.InteractionWrappers;

public class MessageComponentWrapper : IInteractionWrapper
{

    public SocketUser User => _messageComponent?.User;
    public ISocketMessageChannel Channel => _messageComponent?.Channel;
    public string Content => null;
    public IInteractionDataWrapper InteractionData { get; set; }
    public bool IsDM => this._messageComponent.IsDMInteraction;

    private SocketMessageComponent _messageComponent { get; }
    private readonly IChronicle _chronicle;


    public MessageComponentWrapper(SocketMessageComponent messageComponent, IChronicle chronicle)
    {
        this._messageComponent = messageComponent;
        this._chronicle = chronicle;
        this.InteractionData = new ComponentDataWrapper(messageComponent.Data);
    }

    public Task RespondAsync(string message = null, MessageComponent component = null, Embed embed = null)
    {
        this._chronicle.Info($"**Veni Ki** [bot]: {message} [Components: {component?.Components?.Count ?? 0}] (Embed: {(embed != null ? "Yes" : "No")})");
        return _messageComponent.HasResponded ? 
            _messageComponent.Channel.SendMessageAsync(message, components: component, embed: embed) : 
            _messageComponent.RespondAsync(message, components: component, embed: embed);
    }

    public string GetArgument(string name) =>
        this.InteractionData.GetArgument(name);
}
