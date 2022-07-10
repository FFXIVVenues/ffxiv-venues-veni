using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Open : IIntentHandler
    {

        private readonly IApiService _apiService;

        public Open(IApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task Handle(MessageContext context)
        {
            var user = context.Message.Author.Id;
            var venues = await this._apiService.GetAllVenuesAsync(user);

            if (venues == null || !venues.Any())
                await context.RespondAsync("You don't seem to be an assigned contact for any venues. 🤔");
            else if (venues.Count() > 1)
            {
                if (venues.Count() > 25)
                    venues = venues.Take(25);
                context.Conversation.SetItem("venues", venues);
                await context.Conversation.ShiftState<SelectVenueToOpenState>(context);
            }
            else
            {
                await _apiService.OpenVenueAsync(venues.Single().Id);
                await context.RespondAsync(MessageRepository.VenueOpenMessage.PickRandom());
            }
        }

    }
}
