using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.VenueControl.VenueDeletion.SessionStates;

namespace FFXIVVenues.Veni.VenueControl.VenueDeletion.ConversationalIntent
{
    internal class DeleteIntent : IntentHandler
    {

        private readonly IApiService _apiService;

        public DeleteIntent(IApiService apiService)
        {
            this._apiService = apiService;
        }

        // todo: change to stateless handlers (like edit)
        public override async Task Handle(VeniInteractionContext context)
        {
            var user = context.Interaction.User.Id;
            var venues = await this._apiService.GetAllVenuesAsync(user);

            if (venues == null || !venues.Any())
                await context.Interaction.RespondAsync("You don't seem to be an assigned manager for any venues. 🤔");
            else if (venues.Count() > 1)
            {
                if (venues.Count() > 25)
                    venues = venues.Take(25);
                context.Session.SetItem("venues", venues);
                await context.Session.MoveStateAsync<SelectVenueToDeleteSessionState>(context);
            }
            else
            {
                context.Session.SetVenue(venues.Single());
                await context.Session.MoveStateAsync<DeleteVenueSessionState>(context);
            }
        }

    }
}
