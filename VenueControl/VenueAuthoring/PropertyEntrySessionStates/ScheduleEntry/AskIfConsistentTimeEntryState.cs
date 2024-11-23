using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry;

class AskIfConsistentTimeEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{

    private static string[] _messages = new[]
    {
        "Is the venue **open the same time** across each these days?",
        "Is the scheduled **opening time the same** across all those days?"
    };

    public Task EnterState(VeniInteractionContext interactionContext) =>
        interactionContext.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {_messages.PickRandom()}",
            new ComponentBuilder()
                .WithBackButton(interactionContext)
                .WithButton("Yes, each day has the same opening/closing time",
                    interactionContext.RegisterComponentHandler(cm => cm.MoveSessionToStateAsync<ConsistentOpeningTimeEntrySessionState, VenueAuthoringContext>(authoringContext), 
                        ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("No, it's different opening times between days",
                    interactionContext.RegisterComponentHandler(cm => cm.MoveSessionToStateAsync<InconsistentOpeningTimeEntrySessionState, VenueAuthoringContext>(authoringContext), 
                        ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());

}