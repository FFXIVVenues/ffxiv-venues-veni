using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Context;

namespace FFXIVVenues.Veni.States
{
    class AskIfConsistentTimeEntryState : IState
    {

        private static string[] _messages = new[]
        {
            "Is the venue open the same time across each these days?",
            "Is the scheduled opening time the same across all those days?"
        };

        public Task Init(MessageContext c) =>
            c.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {_messages.PickRandom()}",
                new ComponentBuilder()
                    .WithButton("Yes, each day has the same opening/closing time",
                        c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<ConsistentOpeningEntryState>(cm), 
                    ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("No, it's different opening times between days",
                        c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<InconsistentOpeningEntryState>(cm), 
                    ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());

    }
}
