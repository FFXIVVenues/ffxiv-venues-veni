﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueClosing.SessionStates;

internal class CloseNowOrLaterEntryState(IApiService apiService) : ISessionState
{
    public Task Enter(VeniInteractionContext c)
    {
        var component = this.BuildCloseComponent(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskIfClosingIsNowOrLater, component.Build());
    }

    private ComponentBuilder BuildCloseComponent(VeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var isOpen = venue.Resolution?.IsNow ?? false;
        var isClosed = venue.ScheduleOverrides.Any(s => s.IsNow && s.Open is false);
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnSelect, ComponentPersistence.ClearRow));
            
        if (isOpen)
            selectComponent
                .AddOption("End current opening", "EndOpening")
                .AddOption("Close Now", "Now")
                .AddOption("Close Later", "Later");
        else if (isClosed)
            selectComponent
                .AddOption("End current closure", "EndClosure")
                .AddOption("Extend current closure", "Extend")
                .AddOption("Close Later", "Later");
        else
            selectComponent
                .AddOption("Close Now", "Now")
                .AddOption("Close Later", "Later");
        
        return new ComponentBuilder().WithSelectMenu(selectComponent).WithBackButton(c);
    }

    private Task OnSelect(ComponentVeniInteractionContext c)
    {
        c.Session.ClearItem(SessionKeys.OPENING_DATE);
        c.Session.ClearItem(SessionKeys.OPENING_HOUR);
        var selection = c.Interaction.Data.Values.Single();
        return selection switch
        {
            "EndOpening" => this.EndCurrentOpening(c),
            "EndClosure" => this.EndCurrentClosure(c),
            "Extend" => c.Session.MoveStateAsync<CloseHowLongWhenEntryState>(c),
            "Now" => c.Session.MoveStateAsync<CloseHowLongWhenEntryState>(c),
            "Later" => c.Session.MoveStateAsync<CloseTimeZoneEntryState>(c),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private async Task EndCurrentOpening(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var activeSchedule = venue.Schedule.FirstOrDefault(s => s.Resolution.IsNow);
        var scheduleOverrides = venue.ScheduleOverrides.FirstOrDefault(s => s.Open && s.IsNow);
        if (activeSchedule is not null)
            await apiService.CloseVenueAsync(venue.Id, DateTimeOffset.UtcNow, venue.Resolution.End);
        else if (scheduleOverrides is not null)
            await apiService.RemoveOverridesAsync(venue.Id, DateTimeOffset.UtcNow, scheduleOverrides.End);
        await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueOpeningEnded);
    }
    
    private async Task EndCurrentClosure(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var closure = venue.ScheduleOverrides.FirstOrDefault(s => s.IsNow && s.Open is false);
        if (closure is not null)
            await apiService.RemoveOverridesAsync(venue.Id, closure.Start, closure.End);
        await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueClosureEnded);
    }
}

