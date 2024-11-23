using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

class IsSubdivisionEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{
    public Task EnterState(VeniInteractionContext interactionContext)
    {
        return interactionContext.Interaction.RespondAsync(MessageRepository.AskForSubdivisionMessage.PickRandom(), new ComponentBuilder()
            .WithBackButton(interactionContext)
            .WithButton("Yes, it's subdivision", interactionContext.RegisterComponentHandler(cm =>
            {
                var venue = cm.Session.GetVenue();
                venue.Location.Subdivision = true;
                return cm.MoveSessionToStateAsync<ApartmentEntrySessionState, VenueAuthoringContext>(authoringContext);
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
            .WithButton("No, the first division", interactionContext.RegisterComponentHandler(cm =>
            {
                var venue = cm.Session.GetVenue();
                venue.Location.Subdivision = false;
                return cm.MoveSessionToStateAsync<ApartmentEntrySessionState, VenueAuthoringContext>(authoringContext);
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
            .Build());
    }
}