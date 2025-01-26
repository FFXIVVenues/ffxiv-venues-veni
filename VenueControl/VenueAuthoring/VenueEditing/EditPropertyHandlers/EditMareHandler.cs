using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.MareEntry;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.EditPropertyHandlers;

public class EditMareHandler(IAuthorizer authorizer, IApiService apiService) : IComponentHandler
{
    public static string Key => "CONTROL_EDIT_MARE";

    public async Task HandleAsync(ComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = args[0];

        var isNewVenue = context.Session.IsNewVenue();
        var alreadyModifying = context.Session.InEditing();
        var venue = alreadyModifying ? context.Session.GetVenue() : await apiService.GetVenueAsync(venueId);
        
        if (!authorizer.Authorize(user, Permission.EditVenue, venue).Authorized)
        {
            await context.Interaction.FollowupAsync(VenueControlStrings.NoPermission);
            return;
        }

        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
                    props.Components = new ComponentBuilder().Build());
        
        await context.Session.ClearStateAsync(context);
        context.Session.SetVenue(venue);
        context.Session.SetEditing(true);
        context.Session.SetIsNewVenue(isNewVenue);
        await context.Session.MoveStateAsync<HasMareEntrySessionState>(context);
    }
    
}