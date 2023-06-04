using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.EditPropertyHandlers;

public class EditScheduleHandler : IComponentHandler
{
    public static string Key => "CONTROL_EDIT_SCHEDULE";
    
    private readonly IAuthorizer _authorizer;
    private readonly IApiService _apiService;

    public EditScheduleHandler(IAuthorizer authorizer, IApiService apiService)
    {
        this._authorizer = authorizer;
        this._apiService = apiService;
    }
    
    public async Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = args[0];   
        
        var alreadyModifying = context.Session.GetItem<bool>("modifying");
        var venue = alreadyModifying ? context.Session.GetVenue() : await this._apiService.GetVenueAsync(venueId);
        
        if (!this._authorizer.Authorize(user, Permission.EditVenue, venue).Authorized)
        {
            await context.Interaction.FollowupAsync(
                "Aaaah. You'll need to speak to my owners at FFXIV Venues to change the schedule for your venue. 🥲");
            return;
        }

        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
                    props.Components = new ComponentBuilder().Build());
        
        context.Session.SetVenue(venue);
        context.Session.SetItem("modifying", true);
        await context.Session.MoveStateAsync<HaveScheduleEntrySessionState>(context);
    }
    
}