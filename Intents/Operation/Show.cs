using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Services.Api;
using FFXIVVenues.Veni.SessionStates;
using FFXIVVenues.Veni.VenueControl;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Show : IntentHandler
    {

        private readonly IApiService _apiService;
        private readonly IVenueRenderer _venueRenderer;

        public Show(IApiService apiService, IVenueRenderer venueRenderer)
        {
            this._apiService = apiService;
            this._venueRenderer = venueRenderer;
        }

        public override async Task Handle(VeniInteractionContext context)
        {
            var asker = context.Interaction.User.Id;
            var venues = await this._apiService.GetAllVenuesAsync(asker);

            if (venues == null || !venues.Any())
                await context.Interaction.RespondAsync("You don't seem to be an assigned manager for any venues. 🤔");
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
