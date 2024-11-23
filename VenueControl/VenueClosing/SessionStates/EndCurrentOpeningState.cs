using System;
using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

namespace FFXIVVenues.Veni.VenueControl.VenueClosing.SessionStates;

internal class EndCurrentOpeningState(IApiService apiService, IAuthorizer authorizer) : ISessionState
{
    public async Task EnterState(VeniInteractionContext interactionContext)
    {
        var venue = interactionContext.Session.GetVenue();
        var authorize = authorizer.Authorize(interactionContext.Interaction.User.Id, Permission.CloseVenue, venue);
        if (!authorize.Authorized)
        {
            await interactionContext.Interaction.Channel.SendMessageAsync(
                "Sorry, you do not have permission to close this venue. 😢");
            return;
        }
        
        var activeSchedule = venue.Schedule.FirstOrDefault(s => s.Resolution.IsNow);
        var scheduleOverrides = venue.ScheduleOverrides.FirstOrDefault(s => s.Open && s.IsNow);
        if (activeSchedule is not null)
            await apiService.CloseVenueAsync(venue.Id, DateTimeOffset.UtcNow, activeSchedule.Resolution.End);
        if (scheduleOverrides is not null)
            await apiService.RemoveOverridesAsync(venue.Id, DateTimeOffset.UtcNow, scheduleOverrides.End);
        await interactionContext.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueOpeningEnded);
        await interactionContext.ClearSessionAsync();
    }
}

