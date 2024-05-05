using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

namespace FFXIVVenues.Veni.VenueControl.VenueOpening.SessionStates;

internal class EndCurrentClosureState(IApiService apiService) : ISessionState
{
    public async Task Enter(VeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var closure = venue.ScheduleOverrides.FirstOrDefault(s => s.IsNow && s.Open is false);
        if (closure is not null)
            await apiService.RemoveOverridesAsync(venue.Id, closure.Start, closure.End);
        await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueClosureEnded);
        await c.Session.ClearStateAsync(c);
    }
}

