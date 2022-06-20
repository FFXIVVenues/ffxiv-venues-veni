using System.Threading.Tasks;
using FFXIVVenues.Utils;
using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;

namespace FFXIVVenues.Veni.States
{
    class HaveScheduleEntryState : IState
    {

        private static string[] _messages = new[]
        {
            "Does your venue have a set opening schedule? (yes/no)",
            "Is your venue generally open at the same days and times every week? (yes/no)",
        };

        public Task Enter(MessageContext c) =>
            c.SendMessageAsync(_messages.PickRandom());

        public Task Handle(MessageContext c)
        {
            if (c.Prediction.TopIntent == IntentNames.Response.Yes)
                return c.Conversation.ShiftState<TimeZoneEntryState>(c);
            else if (c.Prediction.TopIntent == IntentNames.Response.No)
                return c.Conversation.ShiftState<ConfirmVenueState>(c);
            else
                return c.SendMessageAsync(MessageRepository.DontUnderstandResponses.PickRandom());
        }

    }
}
