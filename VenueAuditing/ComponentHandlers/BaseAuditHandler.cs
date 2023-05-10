using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueAuditing.ComponentHandlers;

public abstract class BaseAuditHandler : IComponentHandler
{

    public abstract Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args);

    protected async Task UpdateSentMessages(DiscordSocketClient client, IVenueRenderer render, Venue venue,
        IUser responder, IEnumerable<BroadcastMessageReceipt> sentMessages,
        string responderMessage, string othersMessage)
    {
        foreach (var message in sentMessages)
        {
            if (message.Status != MessageStatus.Sent) continue;
            var newMessage = responderMessage;
            if (message.UserId != responder.Id)
                newMessage = othersMessage;

            var channel = await client.GetChannelAsync(message.ChannelId);
            (channel as IDMChannel)?.ModifyMessageAsync(message.MessageId, props =>
            {
                props.Components = new ComponentBuilder().Build();
                props.Embeds = new[]
                {
                    render.RenderEmbed(venue).Build(),
                    new EmbedBuilder().WithDescription(newMessage).Build()
                };
            });
        }
    }

    protected async Task NotifyRequesterAsync(MessageComponentVeniInteractionContext context, VenueAuditRecord audit,
        Venue venue, string message) 
    {
        var channel = await context.Client.GetChannelAsync(audit.RequestedIn);
        if (channel == null)
            channel = await context.Client.GetDMChannelAsync(audit.RequestedIn);

        if (channel is SocketTextChannel sTextChannel)
            await sTextChannel.SendMessageAsync(
                $"Hey {MentionUtils.MentionUser(audit.RequestedBy)}! The audit of **{venue.Name}** you requested has been completed. \n{message}");
        if (channel is RestTextChannel rTextChannel)
            await rTextChannel.SendMessageAsync(
                $"Hey {MentionUtils.MentionUser(audit.RequestedBy)}! The audit of **{venue.Name}** you requested has been completed. \n{message}");
        if (channel is RestDMChannel rdmChannel)
            await rdmChannel.SendMessageAsync($"Heyo! The audit of **{venue.Name}** you requested has been completed. \n{message}");
        if (channel is SocketDMChannel sdmChannel)
            await sdmChannel.SendMessageAsync($"Heyo! The audit of **{venue.Name}** you requested has been completed. \n{message}");
    }

    protected void UpdateAudit(MessageComponentVeniInteractionContext context, in VenueAuditRecord audit, VenueAuditStatus status, string message)
    {   audit.Log(message);
        audit.Status = status;
        audit.CompletedBy = context.Interaction.User.Id;
        audit.CompletedAt = DateTime.UtcNow;
    }

}