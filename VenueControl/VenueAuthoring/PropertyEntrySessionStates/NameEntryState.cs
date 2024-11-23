using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates;

class NameEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{
    public Task EnterState(VeniInteractionContext interactionContext)
    {
        interactionContext.RegisterMessageHandler(this.OnMessageReceived);
            
        var isDm = interactionContext.Interaction.Channel is IDMChannel;
        return interactionContext.Interaction.RespondAsync(isDm
            ? VenueControlStrings.AskForNameDirectMessage
            : VenueControlStrings.AskForNameMessage);
    }

    private Task OnMessageReceived(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Name = c.Interaction.Content.StripMentions();
        if (c.Session.InEditing())
            return c.MoveSessionToStateAsync<ConfirmVenueSessionState, VenueAuthoringContext>(authoringContext);
        return c.MoveSessionToStateAsync<DescriptionEntrySessionState, VenueAuthoringContext>(authoringContext);
    }
}