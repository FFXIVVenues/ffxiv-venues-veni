using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.VenueControl.VenueDeletion.SessionStates;

namespace FFXIVVenues.Veni.VenueControl.VenueDeletion.ComponentHandlers;

public class DeleteHandler : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "CONTROL_DELETE";
    
    private readonly IAuthorizer _authorizer;
    private readonly IApiService _apiService;

    public DeleteHandler(IAuthorizer authorizer, IApiService apiService)
    {
        this._authorizer = authorizer;
        this._apiService = apiService;
    }
    
    public async Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = args[0];
        var venue = await this._apiService.GetVenueAsync(venueId);
        if (!this._authorizer.Authorize(user, Permission.DeleteVenue, venue).Authorized)
            return;
        
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());
        
        context.Session.SetVenue(venue);
        await context.Session.MoveStateAsync<DeleteVenueSessionState>(context);
    }
    
}