using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.MareEntry;

class HasMareEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{

    public Task EnterState(VeniInteractionContext interactionContext)
    {
        return interactionContext.Interaction.RespondAsync(VenueControlStrings.AskIfHasMareMessage,
            new ComponentBuilder()
                .WithBackButton(interactionContext)
                .WithButton("Yes", interactionContext.RegisterComponentHandler(cm =>
                    cm.MoveSessionToStateAsync<MareIdEntryState, VenueAuthoringContext>(authoringContext),
                    ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("No",
                     interactionContext.RegisterComponentHandler(cm =>
                        {
                            var venue = interactionContext.Session.GetVenue();
                            venue.MareCode = venue.MarePassword = null;

                            if (cm.Session.InEditing())
                                return cm.MoveSessionToStateAsync<ConfirmVenueSessionState, VenueAuthoringContext>(authoringContext);
                            return cm.MoveSessionToStateAsync<SfwEntrySessionState, VenueAuthoringContext>(authoringContext);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
    }


}