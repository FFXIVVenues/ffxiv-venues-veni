using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Services.Api;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.ComponentHandlers;
using FFXIVVenues.Veni.VenueRendering;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.ConversationalIntents;

internal class ModifyIntentHandler : IntentHandler
{

    private readonly IApiService _apiService;
    private readonly IVenueRenderer _venueRenderer;

    public ModifyIntentHandler(IApiService apiService, IVenueRenderer venueRenderer)
    {
        this._apiService = apiService;
        this._venueRenderer = venueRenderer;
    }

    public override async Task Handle(VeniInteractionContext context)
    {
        var user = context.Interaction.User.Id;
        var venues = await this._apiService.GetAllVenuesAsync(user);

        if (venues == null || !venues.Any())
        {
            await context.Interaction.RespondAsync("You don't seem to be an assigned manager for any venues. 🤔");
            return;
        }

        // ReSharper disable once PossibleMultipleEnumeration
        // Enumerating next once for the Any is better than enumerating all on a chance
        venues = venues.ToList();
        if (venues.Count() == 1)
        {
            var venue = venues.Single();
            await context.Interaction.RespondAsync(embed: this._venueRenderer.RenderEmbed(venue).Build(),
                component: this._venueRenderer.RenderEditComponents(venue, user).Build());
        }
                
        if (venues.Count() > 25)
            venues = venues.Take(25);
                
        await context.Interaction.RespondAsync(VenueControlStrings.SelectVenueToEdit,
            component: this._venueRenderer.RenderVenueSelection(venues, SelectVenueToEditHandler.Key).Build());
    }

}