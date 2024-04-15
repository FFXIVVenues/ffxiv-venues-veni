using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.EditPropertyHandlers;

public class EditPhotoHandler(IAuthorizer authorizer, IApiService apiService) : IComponentHandler
{
    public static string Key => "CONTROL_EDIT_PHOTO";

    public async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = args[0];
        
        var alreadyModifying = context.Session.InEditing();
        var venue = alreadyModifying ? context.Session.GetVenue() : await apiService.GetVenueAsync(venueId);
        
        if (! authorizer.Authorize(user, Permission.EditPhotography, venue).Authorized)
        {
            await context.Interaction.FollowupAsync(VenueControlStrings.NoPermission);
            return;
        }
        
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
                    props.Components = new ComponentBuilder().Build());
        
        await context.Session.ClearState(context);
        context.Session.SetVenue(venue);
        context.Session.SetEditing(true);
        await context.Session.MoveStateAsync<BannerEntrySessionState>(context);
    }
    
}