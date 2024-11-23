using System.Linq;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.StateMachine.InteractionModels;
using FFXIVVenues.Veni.Infrastructure.StateMachine.Models;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl;

namespace FFXIVVenues.Veni.Infrastructure.StateMachine;

public partial class VenueAuthoringStateMachine
{
    private void ConfigureNameEntry()
    {
        this.Configure(VenueAuthoringState.TakingName)
            
            .OnEntry((interaction, _) => 
                interaction.Channel.SendMessageAsync(VenueControlStrings.AskForNameDirectMessage))
            
            .InternalTransitionIf(TakeInteractionTrigger,
                i => i.Is<SocketMessage>(out var message) && 
                     message.Channel is SocketDMChannel ||
                     message.MentionedUsers.Any(u => u.Id == this._client.CurrentUser.Id),
                
                (interaction, _) =>
                {
                    if (!interaction.Is<SocketMessageComponent>(out var messageInteraction))
                        return;

                    this.Venue.Name = messageInteraction.Message.Content.StripMentions();
                    this.Fire(this.MoveNext, interaction);
                })
            
            .Permit(VenueAuthoringTrigger.MoveNext, VenueAuthoringState.TakingDescription);
    }
}