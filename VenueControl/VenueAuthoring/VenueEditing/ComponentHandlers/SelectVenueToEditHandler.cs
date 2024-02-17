using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.SessionStates;
using FFXIVVenues.Veni.VenueRendering;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.ComponentHandlers;

public class SelectVenueToEditHandler : IComponentHandler
{
    public static string Key => "CONTROL_SELECT_EDIT";
    
    private readonly IAuthorizer _authorizer;
    private readonly IApiService _apiService;
    private readonly IVenueRenderer _venueRenderer;

    public SelectVenueToEditHandler(IAuthorizer authorizer, IApiService apiService, IVenueRenderer venueRenderer)
    {
        this._authorizer = authorizer;
        this._apiService = apiService;
        this._venueRenderer = venueRenderer;
    }
    
    public async Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = context.Interaction.Data.Values.First();
        var venue = await this._apiService.GetVenueAsync(venueId);
        if (! this._authorizer.Authorize(user, Permission.EditVenue, venue).Authorized)
            return;
        
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
                    props.Components = new ComponentBuilder().Build());
        
        context.Session.SetVenue(venue);
        await context.Session.MoveStateAsync<EditVenueSessionState>(context);
    }
    
}