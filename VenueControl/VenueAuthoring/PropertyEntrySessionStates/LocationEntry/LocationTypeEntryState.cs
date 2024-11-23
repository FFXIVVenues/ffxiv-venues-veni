using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

class LocationTypeEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{
    public Task EnterState(VeniInteractionContext interactionContext)
    {
        return interactionContext.Interaction.RespondAsync(MessageRepository.AskForHouseOrApartmentMessage.PickRandom(), new ComponentBuilder()
            .WithBackButton(interactionContext)
            .WithButton("A house", interactionContext.RegisterComponentHandler(cm =>
            {
                cm.Session.SetItem("locationType", "house");
                return cm.MoveSessionToStateAsync<DataCenterEntrySessionState, VenueAuthoringContext>(authoringContext);
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
            .WithButton("A room in a house", interactionContext.RegisterComponentHandler(cm =>
            {
                cm.Session.SetItem("locationType", "room");
                return cm.MoveSessionToStateAsync<DataCenterEntrySessionState, VenueAuthoringContext>(authoringContext);
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
            .WithButton("An apartment", interactionContext.RegisterComponentHandler(cm =>
            {
                cm.Session.SetItem("locationType", "apartment");
                return cm.MoveSessionToStateAsync<DataCenterEntrySessionState, VenueAuthoringContext>(authoringContext);
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
            .WithButton("Other", interactionContext.RegisterComponentHandler(cm =>
            {
                cm.Session.SetItem("locationType", "other");
                return cm.MoveSessionToStateAsync<OtherLocationEntrySessionState, VenueAuthoringContext>(authoringContext);
            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
            .Build());
    }
}