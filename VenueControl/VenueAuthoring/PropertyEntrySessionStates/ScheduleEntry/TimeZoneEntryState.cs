using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry;

class TimeZoneEntrySessionState : ISessionState
{

    private static string[] _messages = new[]
    {
        "What **time zone** would you like to give opening times in?",
        "What **time zone** would the venues opening times be in?"
    };

    private static Dictionary<string, string> _timezones = new()
    {
        { "America/New_York", "Eastern Standard Time (EST)" },
        { "America/Chicago", "Central Standard Time (CST)" },
        { "America/Denver", "Mountain Standard Time (MST)" },
        { "America/Los_Angeles", "Pacific Standard Time (PST)" },
        { "America/Halifax", "Atlantic Standard Time (AST)" },
        { "Europe/Budapest", "Central European Time (CEST)" },
        { "Europe/Chisinau", "Eastern European Time (EEST)" },
        { "Europe/London", "GMT Time (GMT)" },
        { "UTC", "Server Time (UTC)" },
        { "Australia/Perth", "Australian Western Time (AWST)" },
        { "Australia/Adelaide", "Australian Central Time (ACST)" },
        { "Australia/Sydney", "Australian Eastern Time (AEST)" }
    };

    public Task Enter(VeniInteractionContext c)
    {
        var component = new ComponentBuilder();
        var timezoneOptions = _timezones.Select(dc => new SelectMenuOptionBuilder(dc.Value, dc.Key)).ToList();
        var selectMenu = new SelectMenuBuilder();
        selectMenu.WithOptions(timezoneOptions);
        selectMenu.WithCustomId(c.Session.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
        return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} { _messages.PickRandom()}",
            component.WithSelectMenu(selectMenu).WithBackButton(c).Build());
    }

    private Task Handle(MessageComponentVeniInteractionContext c)
    {
        var selectedTimezone = c.Interaction.Data.Values.Single();
        c.Session.SetItem("timeZoneId", selectedTimezone);
        return c.Session.MoveStateAsync<DaysEntrySessionState>(c);
    }
}