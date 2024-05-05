using System;
using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

namespace FFXIVVenues.Veni.VenueControl.VenueOpening.SessionStates;

internal class EndCurrentOpeningState(IApiService apiService) : ISessionState
{
    public async Task Enter(VeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var activeSchedule = venue.Schedule.FirstOrDefault(s => s.Resolution.IsNow);
        var scheduleOverrides = venue.ScheduleOverrides.FirstOrDefault(s => s.Open && s.IsNow);
        if (activeSchedule is not null)
            await apiService.CloseVenueAsync(venue.Id, DateTimeOffset.UtcNow, activeSchedule.Resolution.End);
        if (scheduleOverrides is not null)
            await apiService.RemoveOverridesAsync(venue.Id, DateTimeOffset.UtcNow, scheduleOverrides.End);
        await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueOpeningEnded);
        await c.Session.ClearStateAsync(c);
    }
}

