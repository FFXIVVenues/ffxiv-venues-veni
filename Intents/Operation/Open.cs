using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Open : IIntentHandler
    {

        private static string[] _responses = new[]
        {
            "Woo! The doors are open. You're green and announcements have been sent! Let's have fun today! ♥️",
            "Yay! It's that time again. 😀 You're all green on the index, and everyone's been notified. ♥️",
            "Let's do it! We... are... live!!! We're green on the index and the pings are flying! So excited. 🙂"
        };

        private readonly IApiService _apiService;
        private readonly IIndexersService _indexersService;

        public Open(IApiService apiService, IIndexersService indexersService)
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
                await context.Conversation.ShiftState<SelectVenueToOpenState>(context);
            }
            else
            {
                await _apiService.OpenVenueAsync(venues.Single().Id);
                await context.RespondAsync(_responses.PickRandom());
            }
        }

    }
}
