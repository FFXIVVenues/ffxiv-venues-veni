using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates;

class DescriptionEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{
    public Task EnterState(VeniInteractionContext interactionContext)
    {
        interactionContext.RegisterMessageHandler(this.OnMessageReceived);
        var isDm = interactionContext.Interaction.Channel is IDMChannel;
        return interactionContext.Interaction.RespondAsync(isDm ? 
                VenueControlStrings.AskForDescriptionDirectMessage :
                VenueControlStrings.AskForDescriptionMessage,
            new ComponentBuilder()
                .WithBackButton(interactionContext)
                .WithSkipButton<LocationTypeEntrySessionState, ConfirmVenueSessionState>(interactionContext, authoringContext)
                .Build());
    }

    private Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Description = c.Interaction.Content.StripMentions().AsListOfParagraphs();
        if (c.Session.InEditing())
            return c.MoveSessionToStateAsync<ConfirmVenueSessionState, VenueAuthoringContext>(authoringContext);
        return c.MoveSessionToStateAsync<LocationTypeEntrySessionState, VenueAuthoringContext>(authoringContext);
    }
}

