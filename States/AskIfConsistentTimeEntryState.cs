using System.Threading.Tasks;
using FFXIVVenues.Utils;
using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;

namespace FFXIVVenues.Veni.States
{
    class AskIfConsistentTimeEntryState : IState
    {

        private static string[] _messages = new[]
        {
            "Are you open at the same time for each these days? (yes/no)",
            "Is the venue open the same time on each day? (yes/no)"
        };

        public Task Enter(MessageContext c) =>
            c.SendMessageAsync($"{MessageRepository.ConfirmMessage.PickRandom()} { _messages.PickRandom()}");

        public Task Handle(MessageContext c)
        {
            if (c.Prediction.TopIntent == IntentNames.Response.Yes)
                return c.Conversation.ShiftState<ConsistentOpeningEntryState>(c);
            else if (c.Prediction.TopIntent == IntentNames.Response.No)
                return c.Conversation.ShiftState<InconsistentOpeningEntryState>(c);
            else
                return c.SendMessageAsync(MessageRepository.DontUnderstandResponses.PickRandom());
        }
    }
}
