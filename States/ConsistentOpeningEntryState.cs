using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels.V2022;
using Venue = FFXIVVenues.Veni.Api.Models.Venue;

namespace FFXIVVenues.Veni.States
{
    class ConsistentOpeningEntryState : IState
    {

        private static string[] _openingMessages = new[]
        {
            "What time do you **open**? (for example 8:30pm, 9pm or 1:30am)"
        };

        private static string[] _closingMessages = new[]
        {
            "What time do you **close**? (for example 8:30pm, 9pm or 1:30am)"
        };

        private static Regex _regex = new Regex("(?<hour>[0-9]|(1[0-2]))(:?(?<minute>[0-5][0-9]))? ?(?<meridiem>am|pm)");

        private Venue _venue;
        private string _timeZoneId;
        private int _venueDayEnd;
        private bool? _nowSettingClosing;

        public Task Enter(InteractionContext c)
        {
            this._venue = c.Session.GetItem<Venue>("venue");
            this._timeZoneId = c.Session.GetItem<string>("timeZoneId");
            this._venueDayEnd = 11 + c.Session.GetItem<int>("timeZoneOffset");

            if (this._nowSettingClosing == null)
            {
                this._nowSettingClosing = c.Session.GetItem<bool?>("nowSettingClosing");
                c.Session.ClearItem("nowSettingClosing");
            }

            if (this._nowSettingClosing == null)
                this._nowSettingClosing = false;

            c.Session.RegisterMessageHandler(this.OnMessageReceived);

            var messages = !this._nowSettingClosing.Value ? _openingMessages : _closingMessages;
            return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {messages.PickRandom()}",
                                                new ComponentBuilder().WithBackButton(c).Build());
        }

        public Task OnMessageReceived(MessageInteractionContext c)
        {
            var message = c.Interaction.Content.StripMentions().ToLower();
            var match = _regex.Match(message);
            if (!match.Success)
                return c.Interaction.Channel.SendMessageAsync($"I don't get it 😓 Could you write in 12-hour format? Like 12am, or 7:30pm?");

            var hour = ushort.Parse(match.Groups["hour"].Value);
            var minute = match.Groups["minute"].Success ? ushort.Parse(match.Groups["minute"].Value) : (ushort)0;
            var meridiem = match.Groups["meridiem"].Value;

            if (meridiem == "am" && hour == 12)
                hour = 0;
            else if (meridiem == "pm" && hour != 12)
                hour += 12;

            if (!this._nowSettingClosing.Value)
            {
                // setting opening times
                foreach (var opening in _venue.Openings)
                {
                    if (hour < _venueDayEnd)
                        opening.Day = opening.Day != 0 ? opening.Day - 1 : (Day) 6;  
                    opening.Start = new Time { Hour = hour, Minute = minute, NextDay = hour < _venueDayEnd, TimeZone = _timeZoneId };
                }

                c.Session.SetItem("nowSettingClosing", true);
                return c.Session.MoveStateAsync<ConsistentOpeningEntryState>(c);
            }

            // setting closing times
            foreach (var opening in _venue.Openings)
                opening.End = new Time { Hour = hour, Minute = minute, NextDay = opening.Start.NextDay || hour < _venueDayEnd, TimeZone = _timeZoneId };

            c.Session.ClearItem("nowSettingClosing");

            if (c.Session.GetItem<bool>("modifying"))
                return c.Session.MoveStateAsync<ConfirmVenueState>(c);
            return c.Session.MoveStateAsync<BannerEntryState>(c);
        }
    }
}
