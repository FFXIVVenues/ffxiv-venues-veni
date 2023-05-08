using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry
{
    class InconsistentOpeningTimeEntrySessionState : ISessionState
    {

        private static string[] _openingMessages = new[]
        {
            "What time do you **open** on **{0}**? (for example 8:30pm, 9pm or 1:30am)"
        };

        private static string[] _closingMessages = new[]
        {
            "What time do you **close** on **{0}**? (for example 8:30pm, 9pm or 1:30am)"
        };

        private static Regex _regex = new Regex("(?<hour>[0-9]|(1[0-2]))(:?(?<minute>[0-5][0-9]))? ?(?<meridiem>am|pm)");

        private Venue _venue;
        private string _timeZoneId;
        private bool? _nowSettingClosing;
        private int? _nowSettingDay;

        public Task Enter(VeniInteractionContext c)
        {
            this._venue = c.Session.GetVenue();
            this._timeZoneId = c.Session.GetItem<string>("timeZoneId");

            if (this._nowSettingClosing == null)
            {
                this._nowSettingClosing = c.Session.GetItem<bool?>("nowSettingClosing");
                c.Session.ClearItem("nowSettingClosing");
            }

            if (this._nowSettingClosing == null)
                this._nowSettingClosing = false;

            if (this._nowSettingDay == null)
            {
                this._nowSettingDay = c.Session.GetItem<int?>("nowSettingDay");
                c.Session.ClearItem("nowSettingDay");
            }

            if (this._nowSettingDay == null)
                this._nowSettingDay = 0;

            c.Session.RegisterMessageHandler(this.OnMessageReceived);

            var messages = !this._nowSettingClosing.Value ? _openingMessages : _closingMessages;
            var openingForDayMessage = string.Format(messages.PickRandom(), _venue.Openings[this._nowSettingDay.Value].Day);
            
            return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {openingForDayMessage}",
                                               new ComponentBuilder().WithBackButton(c).Build());
        }

        public Task OnMessageReceived(MessageVeniInteractionContext c)
        {
            var message = c.Interaction.Content.StripMentions().ToLower();
            var match = _regex.Match(message);
            if (!match.Success)
                return c.Interaction.Channel.SendMessageAsync($"Sorry, I didn't understand that, could you write in 12-hour format? Like 12am, or 7:30pm?");

            var hour = ushort.Parse(match.Groups["hour"].Value);
            var minute = match.Groups["minute"].Success ? ushort.Parse(match.Groups["minute"].Value) : (ushort)0;
            var meridiem = match.Groups["meridiem"].Value;

            if (meridiem == "am" && hour == 12)
                hour = 0;
            else if (meridiem == "pm" && hour != 12)
                hour += 12;

            var opening = _venue.Openings[this._nowSettingDay.Value];
            if (!this._nowSettingClosing.Value)
            {
                opening.Start = new Time { Hour = hour, Minute = minute, NextDay = false, TimeZone = _timeZoneId };
                c.Session.SetItem("nowSettingDay", this._nowSettingDay);
                c.Session.SetItem("nowSettingClosing", true);
                return c.Session.MoveStateAsync<InconsistentOpeningTimeEntrySessionState>(c);
            }

            // setting closing time per day
            opening.End = new Time { Hour = hour, Minute = minute, NextDay = hour < opening.Start.Hour, TimeZone = _timeZoneId };

            var thisWasLastDay = this._nowSettingDay + 1 == _venue.Openings.Count;
            if (!thisWasLastDay)
            {
                c.Session.SetItem("nowSettingDay", this._nowSettingDay + 1);
                c.Session.SetItem("nowSettingClosing", false);
                return c.Session.MoveStateAsync<InconsistentOpeningTimeEntrySessionState>(c);
            }

            c.Session.ClearItem("nowSettingDay");
            c.Session.ClearItem("nowSettingClosing");

            if (c.Session.GetItem<bool>("modifying"))
                return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);
            return c.Session.MoveStateAsync<BannerEntrySessionState>(c);
        }
    }
}
