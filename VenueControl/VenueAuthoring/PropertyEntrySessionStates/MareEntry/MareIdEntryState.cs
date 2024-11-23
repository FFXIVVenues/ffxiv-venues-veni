using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.MareEntry;

class MareIdEntryState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{

    public Task EnterState(VeniInteractionContext interactionContext)
    {
        interactionContext.RegisterMessageHandler(this.OnMessageReceived);
        var isDm = interactionContext.Interaction.Channel is IDMChannel;
        return interactionContext.Interaction.RespondAsync(isDm ? 
                VenueControlStrings.AskForMareIdDirectMessage :
                VenueControlStrings.AskForMareIdMessage,
            new ComponentBuilder()
                .WithBackButton(interactionContext)
                .Build());
    }

    public Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        
        venue.MareCode = c.Interaction.Content.StripMentions();
        return c.MoveSessionToStateAsync<MarePasswordEntryState, VenueAuthoringContext>(authoringContext);
    }


}