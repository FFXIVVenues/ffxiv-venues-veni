using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Edit : IIntentHandler
    {

        private readonly IApiService _apiService;

        public Edit(IApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task Handle(MessageContext context)
        {
            var user = context.Message.Author.Id;
            var venues = await this._apiService.GetAllVenuesAsync(user);

            if (venues == null || !venues.Any())
                await context.RespondAsync("You don't seem to be an assigned manager for any venues. 🤔");

            else if (venues.Count() > 1)
            {
                if (venues.Count() > 25)
                    venues = venues.Take(25);
                context.Conversation.SetItem("venues", venues);
                await context.Conversation.ShiftState<SelectVenueToModifyState>(context);
            }
            else
            {
                context.Conversation.SetItem("venue", venues.Single());
                await context.Conversation.ShiftState<ModifyVenueState>(context);
            }
        }

    }
}
