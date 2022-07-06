using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class TimeZoneEntryState : IState
    {

        private static string[] _messages = new[]
        {
            "What time zone would you like to give opening times in? (EST, CST, MST, PST, AST or GMT)",
            "What time zone would the venues opening times be in? (EST, CST, MST, PST, AST or GMT)"
        };

        private static Dictionary<string, TimeZoneInfo> _timezones = new()
        {
            { "est", TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") },
            { "cst", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time") },
            { "mst", TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time") },
            { "pst", TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time") },
            { "ast", TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time") },
            { "gmt", TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time") },
            { "utc", TimeZoneInfo.Utc }
        };

        public Task Init(MessageContext c)
        {
            c.Conversation.RegisterMessageHandler(this.OnMessageReceived);
            return c.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} { _messages.PickRandom()}");
        }

        public Task OnMessageReceived(MessageContext c)
        {
            var message = c.Message.Content.StripMentions().ToLower();
            foreach (var timezone in _timezones)
                if (new Regex("\\b" + timezone.Key + "\\b").IsMatch(message))
                {
                    c.Conversation.SetItem("timeZoneKey", timezone.Key);
                    c.Conversation.SetItem("timeZoneId", timezone.Value.Id);
                    var offset = timezone.Value.BaseUtcOffset;
                    if (timezone.Value.IsDaylightSavingTime(DateTime.UtcNow))
                        offset += new TimeSpan(1, 0, 0);
                    c.Conversation.SetItem("timezoneOffset", offset.Hours);
                    return c.Conversation.ShiftState<DaysEntryState>(c);
                }

            return c.RespondAsync($"Sorry, I don't know that time zone. 😭");

        }
    }
}
