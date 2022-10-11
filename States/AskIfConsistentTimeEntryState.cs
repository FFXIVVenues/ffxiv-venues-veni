using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.States
{
    class AskIfConsistentTimeEntryState : IState
    {

        private static string[] _messages = new[]
        {
            "Is the venue **open the same time** across each these days?",
            "Is the scheduled **opening time the same** across all those days?"
        };

        public Task Enter(InteractionContext c) =>
            c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {_messages.PickRandom()}",
                new ComponentBuilder()
                    .WithBackButton(c)
                    .WithButton("Yes, each day has the same opening/closing time",
                        c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<ConsistentOpeningTimeEntryState>(cm), 
                    ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("No, it's different opening times between days",
                        c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<InconsistentOpeningTimeEntryState>(cm), 
                    ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .Build());

    }
}
