using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Intents.Operation;
using FFXIVVenues.Veni.People;
using FFXIVVenues.Veni.Services.Api;
using FFXIVVenues.Veni.VenueControl.SessionStates;

namespace FFXIVVenues.Veni.VenueControl.ComponentHandlers;

public class CloseHandler : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "CONTROL_CLOSE";
    
    private readonly IAuthorizer _authorizer;
    private readonly IApiService _apiService;

    public CloseHandler(IAuthorizer authorizer, IApiService apiService)
    {
        this._authorizer = authorizer;
        this._apiService = apiService;
    }
    
    public async Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = args[0];
        var venue = await this._apiService.GetVenueAsync(venueId);
        
        if (!this._authorizer.Authorize(user, Permission.CloseVenue, venue).Authorized)
            return;
        
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());
        
        context.Session.SetItem("venue", venue);
        await context.Session.MoveStateAsync<CloseEntrySessionState>(context);
    }
    
}