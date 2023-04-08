using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Services.Api;
using FFXIVVenues.Veni.SessionStates;

namespace FFXIVVenues.Veni.VenueControl.ComponentHandlers;

public class EditManagersHandler : IComponentHandler
{
    public static string Key => "CONTROL_EDIT_MANAGERS";
    
    private readonly IAuthorizer _authorizer;
    private readonly IApiService _apiService;

    public EditManagersHandler(IAuthorizer authorizer, IApiService apiService)
    {
        this._authorizer = authorizer;
        this._apiService = apiService;
    }
    
    public async Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args)
    {
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
                    props.Components = new ComponentBuilder().Build());
        
        var user = context.Interaction.User.Id;
        var venueId = args[0];
        var venue = await this._apiService.GetVenueAsync(venueId);
        if (! this._authorizer.Authorize(user, Permission.EditManagers, venue).Authorized)
            return;
        
        context.Session.SetItem("venue", venue);
        await context.Session.MoveStateAsync<ManagerEntrySessionState>(context);
    }
    
}