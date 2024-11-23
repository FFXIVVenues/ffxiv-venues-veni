using System;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionWrappers;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

namespace FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;

public class SlashCommandVeniInteractionContext(
    SocketSlashCommand m, DiscordSocketClient dsc, IServiceProvider sp, Session sc) 
    : VeniInteractionContext<SocketSlashCommand>(m, dsc, sc, sp), IWrappableInteraction
{

    public override ISocketMessageChannel GetChannel() =>
        this.Interaction.Channel;
    
    public override VeniInteractionContext ToWrappedInteraction() =>
        new (new SlashCommandWrapper(this.Interaction),
            this.Client,
            this.Session,
            this.ServiceProvider);

}