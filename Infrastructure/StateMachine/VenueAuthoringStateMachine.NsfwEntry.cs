using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using FFXIVVenues.Veni.Infrastructure.StateMachine.Models;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Infrastructure.StateMachine;

public partial class VenueAuthoringStateMachine
{
    private void ConfigureNsfwEntry()
    {
        // todo: Unserializable state
        Task<RestUserMessage> message = null;
        
        this.Configure(VenueAuthoringState.TakingSfwStatus)
            
            // todo: handle MoveBack
            .OnEntryFrom(this.MoveNext, 
                (interaction, _) =>
                    message = interaction.RespondAsync(MessageRepository.AskForSfwMessage.PickRandom(),
                        components: new ComponentBuilder()
                            .WithButton(label: "️Back", customId: "back", ButtonStyle.Secondary, new Emoji("◀️"))
                            .WithButton(label: "️Yes, it's safe on entry", customId: "yes", ButtonStyle.Secondary, new Emoji("💚"))
                            .WithButton(label: "No, we're openly NSFW", customId: "no", ButtonStyle.Secondary, new Emoji("💋"))
                            .Build()))
            
            .InternalTransition(TakeComponentTrigger,
                (interaction, _) =>
                {
                    switch (interaction.Data.CustomId)
                    {
                        case "back":
                            this.Fire(this.MoveBack, interaction);
                            return;
                        case "yes":
                            Venue.Sfw = true;
                            this.Fire(this.MoveNext, interaction);
                            break;
                        case "no":
                            Venue.Sfw = false;
                            this.Fire(this.MoveNext, interaction);
                            break;
                    }
                })
            
            .PermitIf(VenueAuthoringTrigger.MoveBack, VenueAuthoringState.TakingName, () => this.Phase == VenueAuthoringPhase.FullCreation)
            .PermitIf(VenueAuthoringTrigger.MoveBack, VenueAuthoringState.ConfirmingVenue, () => this.Phase == VenueAuthoringPhase.PropertyEdit)
            
            .PermitIf(VenueAuthoringTrigger.MoveNext, VenueAuthoringState.TakingCategory, () => this.Phase == VenueAuthoringPhase.FullCreation)
            .PermitIf(VenueAuthoringTrigger.MoveNext, VenueAuthoringState.ConfirmingVenue, () => this.Phase == VenueAuthoringPhase.PropertyEdit)
            
            .OnExit(_ => message.Result?.DeleteAsync());
    }
}