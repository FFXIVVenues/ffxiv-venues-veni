using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueAuditing.ComponentHandlers;

public abstract class BaseAuditHandler : IComponentHandler
{
    
    public abstract Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args);
    
    protected async Task UpdateSentMessages(DiscordSocketClient client, IVenueRenderer render, Venue venue, 
                                            IUser responder, IEnumerable<AuditMessage> sentMessages, 
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
    
    protected void CompleteAudit(MessageComponentVeniInteractionContext context, in VenueAuditRecord audit, Venue venue, VenueAuditStatus status, string message)
    {
        if (audit.RoundId == null)
        {
            var channel = context.Client.GetChannel(audit.RequestedIn);
            if (channel is SocketDMChannel dmChannel)
                dmChannel.SendMessageAsync(
                    $"Heyo! The audit of {venue.Name} you requested has been completed. \n{message} 😘");
            if (channel is SocketTextChannel textChannel)
                textChannel.SendMessageAsync(
                    $"Hey {MentionUtils.MentionUser(audit.RequestedBy)}! The audit of {venue.Name} you requested has been completed. \n{message}");
        }

        audit.Log(message);
        audit.Status = status;
        audit.CompletedBy = context.Interaction.User.Id;
        audit.CompletedAt = DateTime.UtcNow;
    }

}