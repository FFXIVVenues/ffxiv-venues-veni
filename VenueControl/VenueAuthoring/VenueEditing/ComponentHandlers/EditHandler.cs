using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueRendering;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.ComponentHandlers;

public class EditHandler : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "CONTROL_EDIT";
    
    private readonly IAuthorizer _authorizer;
    private readonly IApiService _apiService;
    private readonly IVenueRenderer _venueRenderer;

    public EditHandler(IAuthorizer authorizer, IApiService apiService, IVenueRenderer venueRenderer)
    {
        this._authorizer = authorizer;
        this._apiService = apiService;
        this._venueRenderer = venueRenderer;
    }
    
    public async Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = args[0];
        var venue = await this._apiService.GetVenueAsync(venueId);
        if (! this._authorizer.Authorize(user, Permission.EditVenue, venue).Authorized)
            return;
        
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());
        
        await context.Interaction.Channel.SendMessageAsync(MessageRepository.EditVenueMessage.PickRandom(),
            components: this._venueRenderer.RenderEditComponents(venue, context.Interaction.User.Id).Build());
    }
    
}