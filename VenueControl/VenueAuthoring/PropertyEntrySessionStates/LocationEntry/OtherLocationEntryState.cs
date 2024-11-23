using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.MareEntry;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

internal class OtherLocationEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{
    
    public Task EnterState(VeniInteractionContext interactionContext)
    {
        interactionContext.RegisterMessageHandler(this.MessageHandler);
        return interactionContext.Interaction.RespondAsync("Ooo, interesting! In as few characters as possible, where is your venue **located**? 🥰", new ComponentBuilder()
            .WithBackButton(interactionContext).Build());
    }

    private Task MessageHandler(MessageVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Location = new Location { Override = c.Interaction.Content.StripMentions() };
            
        if (c.Session.InEditing())
            return c.MoveSessionToStateAsync<ConfirmVenueSessionState, VenueAuthoringContext>(authoringContext);

        return c.MoveSessionToStateAsync<HasMareEntrySessionState, VenueAuthoringContext>(authoringContext);
    }
    
}