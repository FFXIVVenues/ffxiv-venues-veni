using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels.V2022;
using Venue = FFXIVVenues.Veni.Api.Models.Venue;

namespace FFXIVVenues.Veni.States
{
    class ConsistentOpeningEntryState : IState
    {

        private static string[] _openingMessages = new[]
        {
            "What time do you _open_? (for example 8:30pm, 9pm or 1:30am)"
        };

        private static string[] _closingMessages = new[]
        {
            "What time do you _close_? (for example 8:30pm, 9pm or 1:30am)"
        };

        private static Regex _regex = new Regex("(?<hour>[0-9]|(1[0-2]))(:?(?<minute>[0-5][0-9]))? ?(?<meridiem>am|pm)");

        private Venue _venue;
        private string _timeZoneId;
        private int _venueDayEnd;
        private bool _nowSettingClosing = false;

        public Task Init(MessageContext c)
        {
            this._venue = c.Conversation.GetItem<Venue>("venue");
            this._timeZoneId = c.Conversation.GetItem<string>("timeZoneId");
            this._venueDayEnd = 11 + c.Conversation.GetItem<int>("timeZoneOffset");
            c.Conversation.RegisterMessageHandler(this.OnMessageReceived);
            return c.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {_openingMessages.PickRandom()}");
        }

        public Task OnMessageReceived(MessageContext c)
        {
            var message = c.Message.Content.StripMentions().ToLower();
            var match = _regex.Match(message);
            if (!match.Success)
                return c.RespondAsync($"I don't get it 😓 Could you write in 12-hour format? Like 12am, or 7:30pm?");

            var hour = ushort.Parse(match.Groups["hour"].Value);
            var minute = match.Groups["minute"].Success ? ushort.Parse(match.Groups["minute"].Value) : (ushort)0;
            var meridiem = match.Groups["meridiem"].Value;

            if (meridiem == "am" && hour == 12)
                hour = 0;
            else if (meridiem == "pm" && hour != 12)
                hour += 12;

            if (!_nowSettingClosing)
            {
                // setting opening times
                foreach (var opening in _venue.Openings)
                    opening.Start = new Time { Hour = hour, Minute = minute, NextDay = hour < _venueDayEnd, TimeZone = _timeZoneId };

                _nowSettingClosing = true;
                return c.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {_closingMessages.PickRandom()}");
            }

            // setting closing times
            foreach (var opening in _venue.Openings)
                opening.End = new Time { Hour = hour, Minute = minute, NextDay = opening.Start.NextDay || hour < _venueDayEnd, TimeZone = _timeZoneId };

            if (c.Conversation.GetItem<bool>("modifying"))
                return c.Conversation.ShiftState<ConfirmVenueState>(c);
            return c.Conversation.ShiftState<BannerInputState>(c);
        }
    }
}
