using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueOpening.SessionStates;

internal class OpenDayEntryState : ISessionState
{
    public Task Enter(VeniInteractionContext c)
    {
        var component = this.BuildOpenComponent(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskForDayOfOpening, component.WithBackButton(c).Build()); //change text later
    }

    private ComponentBuilder BuildOpenComponent(VeniInteractionContext c)
    {
        var timezone = c.Session.GetItem<string>(SessionKeys.TIMEZONE_ID);
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnSelect, ComponentPersistence.ClearRow));
        foreach (var date in this.NextNDates(21, timezone))
            selectComponent.AddOption(date.ToString("dddd dd MMMM"), date.ToString());
        return new ComponentBuilder().WithSelectMenu(selectComponent);
    }

    private Task OnSelect(ComponentVeniInteractionContext c)
    {
        var date = c.Interaction.Data.Values.Single(); 
        c.Session.SetItem(SessionKeys.OPENING_DATE, DateTimeOffset.Parse(date));
        return c.Session.MoveStateAsync<OpenTimeEntryState>(c);
    }
    
    private IEnumerable<DateTimeOffset> NextNDates(int n, string timeZoneId)
    {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var now = DateTime.UtcNow;
        var offset = timezone.GetUtcOffset(now);
        now = now.Add(offset);
        var current = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, offset);

        yield return current; 
        for (var i = 0; i < n; i++)
        {
            current = current.AddDays(1);
            yield return current;
        }
    }
}

