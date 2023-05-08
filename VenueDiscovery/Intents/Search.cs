using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.VenueDiscovery.SessionStates;
using FFXIVVenues.Veni.VenueRendering;

namespace FFXIVVenues.Veni.VenueDiscovery.Intents
{
    internal class Search : IntentHandler
    {

        private readonly IApiService _apiService;
        private readonly IVenueRenderer _venueRenderer;

        public Search(IApiService apiService, IVenueRenderer venueRenderer)
        {
            this._apiService = apiService;
            this._venueRenderer = venueRenderer;
        }

        public override async Task Handle(VeniInteractionContext context)
        {
            var asker = context.Interaction.User.Id;
            var query = context.GetArgument("search-query");

            if (string.IsNullOrWhiteSpace(query))
            {
                await context.Interaction.RespondAsync("What am I looking for? 🤔");
                return;
            }

            var venues = await this._apiService.GetAllVenuesAsync(query);

            if (venues == null || !venues.Any())
                await context.Interaction.RespondAsync("Could find any venues with that name. 😔");
            else if (venues.Count() > 1)
            {
                if (venues.Count() > 25)
                    venues = venues.Take(25);
                context.Session.SetItem("venues", venues);
                await context.Session.MoveStateAsync<SelectVenueToShowSessionState>(context);
            }
            else
            {
                var venue = venues.Single();
                await context.Interaction.RespondAsync(embed: this._venueRenderer.RenderEmbed(venue).Build(),
                    component: this._venueRenderer.RenderActionComponents(context, venue, asker).Build());
            }
        }

    }
}
