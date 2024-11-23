using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

namespace FFXIVVenues.Veni.VenueControl.VenueOpening.SessionStates;

internal class EndCurrentClosureState(IApiService apiService, IAuthorizer authorizer) : ISessionState
{
    public async Task EnterState(VeniInteractionContext interactionContext)
    {
        var venue = interactionContext.Session.GetVenue();
        var authorize = authorizer.Authorize(interactionContext.Interaction.User.Id, Permission.OpenVenue, venue);
        if (!authorize.Authorized)
        {
            await interactionContext.Interaction.Channel.SendMessageAsync(
                "Sorry, you do not have permission to close this venue. 😢");
            return;
        }
        
        var closure = venue.ScheduleOverrides.FirstOrDefault(s => s.IsNow && s.Open is false);
        if (closure is not null)
            await apiService.RemoveOverridesAsync(venue.Id, closure.Start, closure.End);
        await interactionContext.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueClosureEnded);
        await interactionContext.ClearSessionAsync();
    }
}

