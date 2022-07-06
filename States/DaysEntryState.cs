using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels.V2022;
using System.Threading.Tasks;
using Venue = FFXIVVenues.Veni.Api.Models.Venue;

namespace FFXIVVenues.Veni.States
{
    class DaysEntryState : IState
    {

        private static string[] _messages = new[]
        {
            "What days are you open? (please list them in one message for me)",
            "What days is the venue open each week? (please list them in one message for me)"
        };

        public Task Init(MessageContext c) =>
            c.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {_messages.PickRandom()}");

        public Task OnMessageReceived(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            var message = c.Message.Content.StripMentions().ToLower();

            if (message.Contains("mon"))
                venue.Openings.Add(new Opening { Day = Day.Monday });
            if (message.Contains("tue"))
                venue.Openings.Add(new Opening { Day = Day.Tuesday });
            if (message.Contains("wed"))
                venue.Openings.Add(new Opening { Day = Day.Wednesday });
            if (message.Contains("thur"))
                venue.Openings.Add(new Opening { Day = Day.Thursday });
            if (message.Contains("fri"))
                venue.Openings.Add(new Opening { Day = Day.Friday });
            if (message.Contains("sat"))
                venue.Openings.Add(new Opening { Day = Day.Saturday });
            if (message.Contains("sun"))
                venue.Openings.Add(new Opening { Day = Day.Sunday });

            if (venue.Openings.Count == 0)
                return c.RespondAsync($"Sorry, I don't understand. Which days of the week is your venue open?");

            if (venue.Openings.Count > 1)
                return c.Conversation.ShiftState<AskIfConsistentTimeEntryState>(c);

            return c.Conversation.ShiftState<ConsistentOpeningEntryState>(c);
        }
    }
}
