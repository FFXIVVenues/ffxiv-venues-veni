using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Close : IIntentHandler
    {

        private static string[] _responses = new[]
        {
            "The doors are closed! I can't wait til next time. ♥️",
            "We're no longer green! We'll close up. ",
            "Okay! I'll lock up. 😉"
        };

        private readonly IApiService _apiService;
        private readonly IIndexersService _indexersService;

        public Close(IApiService apiService, IIndexersService indexersService)
        {
            this._apiService = apiService;
            this._indexersService = indexersService;
        }

        public async Task Handle(MessageContext context)
        {
            var user = context.Message.Author.Id;
            var isIndexer = this._indexersService.IsIndexer(user);
            IEnumerable<Venue> venues;
            if (isIndexer)
                venues = await this._apiService.GetAllVenuesAsync();
            else
                venues = await this._apiService.GetAllVenuesAsync(user);

            if (venues == null || !venues.Any())
                await context.RespondAsync("You don't seem to be an assigned contact for any venues. 🤔");
            else if (venues.Count() > 1)
            {
                context.Conversation.SetItem("venues", venues);
                await context.Conversation.ShiftState<SelectVenueToCloseState>(context);
            }
            else
            {
                await _apiService.CloseVenueAsync(venues.Single().Id);
                await context.RespondAsync(_responses.PickRandom());
            }
        }

    }
}