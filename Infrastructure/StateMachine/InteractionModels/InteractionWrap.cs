using System;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.Infrastructure.StateMachine.InteractionModels;

public sealed class InteractionWrap
{
    public object Interaction;
    
    public InteractionWrap(SocketMessage message) => Interaction = message;
    public InteractionWrap(SocketMessageComponent component) => Interaction = component;
    public InteractionWrap(SocketSlashCommand command) => Interaction = command;

    public bool Is<T>() => this.Interaction is T;

    public bool Is<T>(out T interaction)
    {
        if (this.Interaction is T casted)
        {
            interaction = casted;
            return true;
        }
        interaction = default;
        return false;
    }

    public TReturn Match<TReturn>(Func<SocketMessage, TReturn> messageMatch,
        Func<SocketMessageComponent, TReturn> componentMatch, Func<SocketSlashCommand, TReturn> commandMatch,
        TReturn @default = default) => this.Interaction switch
    {
        SocketMessage m => messageMatch(m),
        SocketMessageComponent c => componentMatch(c),
        SocketSlashCommand c => commandMatch(c),
        _ => @default
    };

    public void Match(Action<SocketMessage> messageMatch,
        Action<SocketMessageComponent> componentMatch, Action<SocketSlashCommand> commandMatch)
    {
        switch (this.Interaction)
        {
            case SocketMessage message:
                messageMatch(message);
                break;
            case SocketMessageComponent component:
                componentMatch(component);
                break;
            case SocketSlashCommand command:
                commandMatch(command);
                break;
        }
    }
    
    public ISocketMessageChannel Channel => Match(m => m.Channel, c => c.Channel, c => c.Channel);
    public SocketUser Author =>  Match(m => m.Author, c => c.User, c => c.User);

    public Task<RestUserMessage> RespondAsync(string text = null,
        bool isTts = false,
        Embed embed = null,
        RequestOptions options = null,
        AllowedMentions allowedMentions = null,
        MessageReference messageReference = null,
        MessageComponent components = null,
        ISticker[] stickers = null,
        Embed[] embeds = null,
        MessageFlags flags = MessageFlags.None) => Channel.SendMessageAsync(text, isTts, embed, options,
        allowedMentions, messageReference, components, stickers, embeds, flags);
    
    public static implicit operator SocketMessage(InteractionWrap wrap) => wrap.Interaction as SocketMessage;
    public static implicit operator SocketMessageComponent(InteractionWrap wrap) => wrap.Interaction as SocketMessageComponent;
    public static implicit operator SocketSlashCommand(InteractionWrap wrap) => wrap.Interaction as SocketSlashCommand;
    
    public static implicit operator InteractionWrap(SocketMessage message) => new (message);
    public static implicit operator InteractionWrap(SocketMessageComponent component) => new (component);
    public static implicit operator InteractionWrap(SocketSlashCommand command) => new (command);
    
}