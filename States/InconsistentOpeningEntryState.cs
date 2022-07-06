using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels.V2022;
using Venue = FFXIVVenues.Veni.Api.Models.Venue;

namespace FFXIVVenues.Veni.States
{
    class InconsistentOpeningEntryState : IState
    {

        private static string[] _openingMessages = new[]
        {
            "What time do you _open_ on {0}? (for example 8:30pm, 9pm or 1:30am)"
        };

        private static string[] _closingMessages = new[]
        {
            "What time do you _close_ on {0}? (for example 8:30pm, 9pm or 1:30am)"
        };

        private static Regex _regex = new Regex("(?<hour>[0-9]|(1[0-2]))(:?(?<minute>[0-5][0-9]))? ?(?<meridiem>am|pm)");

        private Venue _venue;
        private string _timeZoneId;
        private int _venueDayEnd;
        private bool nowSettingClosing = false;
        private int currentIndex = 0;

        public Task Init(MessageContext c)
        {
            _venue = c.Conversation.GetItem<Venue>("venue");
            _timeZoneId = c.Conversation.GetItem<string>("timeZoneId");
            _venueDayEnd = 11 + c.Conversation.GetItem<int>("timeZoneOffset");

            string openingForDayMessage = string.Format(_openingMessages.PickRandom(), _venue.Openings[0].Day);
            return c.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {openingForDayMessage}");
        }

        public Task OnMessageReceived(MessageContext c)
        {
            var message = c.Message.Content.StripMentions().ToLower();
            var match = _regex.Match(message);
            if (!match.Success)
                return c.RespondAsync($"Sorry, I didn't understand that, could you write in 12-hour format? Like 12am, or 7:30pm?");

            var hour = ushort.Parse(match.Groups["hour"].Value);
            var minute = match.Groups["minute"].Success ? ushort.Parse(match.Groups["minute"].Value) : (ushort)0;
            var meridiem = match.Groups["meridiem"].Value;

            if (meridiem == "am" && hour == 12)
                hour = 0;
            else if (meridiem == "pm" && hour != 12)
                hour += 12;

            var opening = _venue.Openings[currentIndex];
            if (!nowSettingClosing)
            {
                // setting opening time per day
                opening.Start = new Time { Hour = hour, Minute = minute, NextDay = hour < _venueDayEnd, TimeZone = _timeZoneId };

                nowSettingClosing = true;
                var closingForDayMessage = string.Format(_closingMessages.PickRandom(), _venue.Openings[currentIndex].Day);
                return c.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {closingForDayMessage}");
            }

            // setting closing time per day
            opening.End = new Time { Hour = hour, Minute = minute, NextDay = opening.Start.NextDay || hour < _venueDayEnd, TimeZone = _timeZoneId };

            currentIndex++;
            nowSettingClosing = false;

            if (currentIndex < _venue.Openings.Count)
            {
                var openingForDayMessage = string.Format(_openingMessages.PickRandom(), _venue.Openings[currentIndex].Day);
                return c.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {openingForDayMessage}");
            }

            if (c.Conversation.GetItem<bool>("modifying"))
                return c.Conversation.ShiftState<ConfirmVenueState>(c);
            return c.Conversation.ShiftState<BannerInputState>(c);
        }
    }
}
