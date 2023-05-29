using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using TimeZoneConverter;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry
{
    class TimeZoneEntrySessionState : ISessionState
    {

        private static string[] _messages = new[]
        {
            "What **time zone** would you like to give opening times in?",
            "What **time zone** would the venues opening times be in?"
        };

        private static TimeZoneInfo _gmt = TZConvert.GetTimeZoneInfo("GMT Standard Time");
        private static Dictionary<string, TimeZoneInfo> _timezones = new()
        {
            { "Eastern Standard Time (EST)", TZConvert.GetTimeZoneInfo("Eastern Standard Time") },
            { "Central Standard Time (CST)", TZConvert.GetTimeZoneInfo("Central Standard Time") },
            { "Mountain Standard Time (MST)", TZConvert.GetTimeZoneInfo("Mountain Standard Time") },
            { "Pacific Standard Time (PST)", TZConvert.GetTimeZoneInfo("Pacific Standard Time") },
            { "Atlantic Standard Time (AST)", TZConvert.GetTimeZoneInfo("Atlantic Standard Time") },
            { "Central European Time (CEST)", TZConvert.GetTimeZoneInfo("Central Europe Standard Time") },
            { "Eastern European Time (EEST)", TZConvert.GetTimeZoneInfo("E. Europe Standard Time") },
            { _gmt.IsDaylightSavingTime(DateTime.UtcNow) ? "GMT Summer Time (GMT)" : "GMT Standard Time", _gmt },
            { "Server Time (UTC)", TimeZoneInfo.Utc }
        };

        public Task Enter(VeniInteractionContext c)
        {
            var component = new ComponentBuilder();
            var timezoneOptions = _timezones.Select(dc => new SelectMenuOptionBuilder(dc.Key, dc.Key)).ToList();
            var selectMenu = new SelectMenuBuilder();
            selectMenu.WithOptions(timezoneOptions);
            selectMenu.WithCustomId(c.Session.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
            return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} { _messages.PickRandom()}",
                                  component.WithSelectMenu(selectMenu).WithBackButton(c).Build());
        }

        public Task Handle(MessageComponentVeniInteractionContext c)
        {
            var selectedTimezone = c.Interaction.Data.Values.Single();
            var timezone = _timezones[selectedTimezone];

            c.Session.SetItem("timeZoneKey", selectedTimezone);
            c.Session.SetItem("timeZoneId", timezone.StandardName);
            var offset = timezone.GetUtcOffset(DateTime.UtcNow);
            c.Session.SetItem("timezoneOffset", offset.Hours);
            return c.Session.MoveStateAsync<DaysEntrySessionState>(c);
        }
    }
}
