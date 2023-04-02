using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.People;
using FFXIVVenues.Veni.Services.Api;
using FFXIVVenues.Veni.SessionStates;
using FFXIVVenues.Veni.VenueControl.SessionStates;

namespace FFXIVVenues.Veni.VenueControl.ComponentHandlers;

public class OpenHandler : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "CONTROL_OPEN";
    
    private readonly IStaffService _staffService;
    private readonly IApiService _apiService;

    public OpenHandler(IStaffService staffService, IApiService apiService)
    {
        this._staffService = staffService;
        this._apiService = apiService;
    }

    public async Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = args[0];
        var venue = await this._apiService.GetVenueAsync(venueId);
        if (!this._staffService.IsEditor(user) && !venue.Managers.Contains(user.ToString()))
            return;
        
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());
        
        context.Session.SetItem("venue", venue);
        await context.Session.MoveStateAsync<OpenEntrySessionState>(context);
    }
    
}