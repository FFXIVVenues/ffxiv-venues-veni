using System.Threading.Tasks;
using FFXIVVenues.Veni.Api.Models;
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

        public Task Init(MessageContext c) =>
            c.RespondAsync(_messages.PickRandom());

        public Task OnMessageReceived(MessageContext c)
        {
            if (c.Prediction.TopIntent == IntentNames.Response.Yes)
                return c.Conversation.ShiftState<TimeZoneEntryState>(c);
            else if (c.Prediction.TopIntent == IntentNames.Response.No)
            {
                var venue = c.Conversation.GetItem<Venue>("venue");
                venue.Openings = new();

                if (c.Conversation.GetItem<bool>("modifying"))
                    return c.Conversation.ShiftState<ConfirmVenueState>(c);
                return c.Conversation.ShiftState<BannerInputState>(c);
            }
            else
                return c.RespondAsync(MessageRepository.DontUnderstandResponses.PickRandom());
        }

    }
}
