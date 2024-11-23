using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry;

class MonthlyCommencementEntryState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{

    private Venue _venue;
    private Day? _currentDay;
    private Dictionary<Day, List<Schedule>> _schedules = new ();

    public Task EnterState(VeniInteractionContext interactionContext)
    {
        this._venue = interactionContext.Session.GetVenue();
        var timezone = interactionContext.Session.GetItem<string>(SessionKeys.TIMEZONE_ID);
        this._schedules = interactionContext.Session.GetItem<Dictionary<Day, List<Schedule>>>(SessionKeys.MONTHLY_SCHEDULE_BY_DAY);
        if (this._schedules is null)
        {
            this._schedules = _venue.Schedule.DistinctBy(s => s.Day).OrderBy(s => s.Day).ToDictionary(s => s.Day, _ => new List<Schedule>());
            interactionContext.Session.SetItem(SessionKeys.MONTHLY_SCHEDULE_BY_DAY, this._schedules);
        }
        
        // Assumes that there is not multiple schedules for the same day with differing times/location
        if (this._currentDay == null)
        {
            this._currentDay = interactionContext.Session.GetItem<Day?>(SessionKeys.NOW_SETTING_DAY);
            interactionContext.Session.ClearItem(SessionKeys.NOW_SETTING_DAY);
        }
        this._currentDay ??= this._schedules.First()!.Key;
        
        var select = new SelectMenuBuilder()
            .WithCustomId(interactionContext.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow))
            .WithMinValues(1).WithMaxValues(8)
            .AddOption($"1st {this._currentDay} of the month", "1")
            .AddOption($"2nd {this._currentDay} of the month", "2")
            .AddOption($"3rd {this._currentDay} of the month", "3")
            .AddOption($"4th {this._currentDay} of the month", "4")
            .AddOption($"Last {this._currentDay} of the month", "-1")
            .AddOption($"2nd last {this._currentDay} of the month", "-2")
            .AddOption($"3rd last {this._currentDay} of the month", "-3")
            .AddOption($"4th last {this._currentDay} of the month", "-4");
            
        var message = string.Format(VenueControlStrings.AskWhenInMonthMessage, this._currentDay);
        return interactionContext.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {message}",
            new ComponentBuilder().WithSelectMenu(select).WithBackButton(interactionContext).Build());
    }

    private Task Handle(ComponentVeniInteractionContext c)
    {
        var selections = c.Interaction.Data.Values;
        var originalForDay = this._venue.Schedule.First(s => s.Day == this._currentDay);
        var scheduleListForDay = selections
            .Select(int.Parse)
            .Select(selection => new Schedule
            {
                Day = originalForDay.Day,
                Commencing = null,
                Start = originalForDay.Start,
                End = originalForDay.End,
                Location = originalForDay.Location,
                Interval = new() { IntervalType = IntervalType.EveryXthDayOfTheMonth, IntervalArgument = selection }
            })
            .ToList();
        _schedules[this._currentDay!.Value] = scheduleListForDay;
        
        return NextState(c);
    }
    
    private Task NextState(ComponentVeniInteractionContext c)
    {
        var thisWasLastDay = this._currentDay == this._schedules.Last().Key;
        if (!thisWasLastDay)
        {
            var nextDay = this._schedules.Keys.SkipWhile(k => k != this._currentDay).Skip(1).Take(1).Single();
            c.Session.SetItem(SessionKeys.NOW_SETTING_DAY, nextDay);
            return c.MoveSessionToStateAsync<MonthlyCommencementEntryState, VenueAuthoringContext>(authoringContext);
        }

        this._venue.Schedule = this._schedules.SelectMany(s => s.Value).ToList();
        c.Session.ClearItem(SessionKeys.NOW_SETTING_DAY);
        if (c.Session.InEditing())
            return c.MoveSessionToStateAsync<ConfirmVenueSessionState, VenueAuthoringContext>(authoringContext);
        return c.MoveSessionToStateAsync<BannerEntrySessionState, VenueAuthoringContext>(authoringContext);
    }
}
