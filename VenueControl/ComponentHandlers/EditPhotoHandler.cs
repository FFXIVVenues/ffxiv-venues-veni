using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Abstractions;
using FFXIVVenues.Veni.People;
using FFXIVVenues.Veni.Services.Api;
using FFXIVVenues.Veni.SessionStates;

namespace FFXIVVenues.Veni.VenueControl.ComponentHandlers;

public class EditPhotoHandler : IComponentHandler
{
    public static string Key => "CONTROL_EDIT_PHOTO";
    
    private readonly IStaffService _staffService;
    private readonly IApiService _apiService;

    public EditPhotoHandler(IStaffService staffService, IApiService apiService)
    {
        this._staffService = staffService;
        this._apiService = apiService;
    }
    
    public async Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args)
    {
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
                    props.Components = new ComponentBuilder().Build());
        
        var user = context.Interaction.User.Id;
        var venueId = args[0];
        var venue = await this._apiService.GetVenueAsync(venueId);
        if (!this._staffService.IsPhotographer(user) && !this._staffService.IsEditor(user) && !venue.Managers.Contains(user.ToString()))
            return;
        
        context.Session.SetItem("venue", venue);
        await context.Session.MoveStateAsync<BannerEntrySessionState>(context);
    }
    
}