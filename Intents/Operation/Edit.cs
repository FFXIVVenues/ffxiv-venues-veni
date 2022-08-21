using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Edit : IntentHandler
    {

        private readonly IApiService _apiService;

        public Edit(IApiService apiService)
        {
            this._apiService = apiService;
        }

        public override async Task Handle(InteractionContext context)
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
                await context.Session.SetStateAsync<SelectVenueToModifyState>(context);
            }
            else
            {
                context.Session.SetItem("venue", venues.Single());
                await context.Session.SetStateAsync<ModifyVenueState>(context);
            }
        }

    }
}
