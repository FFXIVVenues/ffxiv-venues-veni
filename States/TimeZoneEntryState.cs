using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class TimeZoneEntryState : IState
    {

        private static string[] _messages = new[]
        {
            "What time zone would you like to give opening times in?",
            "What time zone would the venues opening times be in?"
        };

        private static Dictionary<string, TimeZoneInfo> _timezones = new()
        {
            { "Eastern Standard Time (EST)", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") },
            { "Central Standard Time (CST)", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time") },
            { "Mountain Standard Time (MST)", TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time") },
            { "Pacific Standard Time (PST)", TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time") },
            { "Atlantic Standard Time (AST)", TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time") },
            { "Greenwich Mean Time (GMT)", TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time") },
            { "Coordinated Universal Time (UTC)", TimeZoneInfo.Utc }
        };

        public Task Init(MessageContext c)
        {
            var component = new ComponentBuilder();
            var timezoneOptions = _timezones.Select(dc => new SelectMenuOptionBuilder(dc.Key, dc.Key)).ToList();
            var selectMenu = new SelectMenuBuilder();
            selectMenu.WithOptions(timezoneOptions);
            selectMenu.WithCustomId(c.Conversation.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
            return c.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} { _messages.PickRandom()}",
                                  component.WithSelectMenu(selectMenu).Build());
        }

        public Task Handle(MessageContext c)
        {
            var selectedTimezone = c.MessageComponent.Data.Values.Single();
            var timezone = _timezones[selectedTimezone];

            c.Conversation.SetItem("timeZoneKey", selectedTimezone);
            c.Conversation.SetItem("timeZoneId", timezone.Id);
            var offset = timezone.GetUtcOffset(DateTime.UtcNow);
            c.Conversation.SetItem("timezoneOffset", offset.Hours);
            return c.Conversation.ShiftState<DaysEntryState>(c);
        }
    }
}
