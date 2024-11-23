using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.TagsEntry;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates
{
    class SfwEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
    {
        public Task EnterState(VeniInteractionContext interactionContext)
        {
            return interactionContext.Interaction.RespondAsync(MessageRepository.AskForSfwMessage.PickRandom(), new ComponentBuilder()
                .WithBackButton(interactionContext)
                .WithButton("Yes, it's safe on entry", interactionContext.RegisterComponentHandler(cm =>
                {
                    var venue = cm.Session.GetVenue();
                    venue.Sfw = true;
                    if (cm.Session.InEditing())
                        return cm.MoveSessionToStateAsync<ConfirmVenueSessionState, VenueAuthoringContext>(authoringContext);
                    return cm.MoveSessionToStateAsync<CategoryEntrySessionState, VenueAuthoringContext>(authoringContext);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("No, we're openly NSFW", interactionContext.RegisterComponentHandler(cm =>
                {
                    var venue = cm.Session.GetVenue();
                    venue.Sfw = false;
                    if (cm.Session.InEditing())
                        return cm.MoveSessionToStateAsync<ConfirmVenueSessionState, VenueAuthoringContext>(authoringContext);
                    return cm.MoveSessionToStateAsync<CategoryEntrySessionState, VenueAuthoringContext>(authoringContext);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }

    }

}
