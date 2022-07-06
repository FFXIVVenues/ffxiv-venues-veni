using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Delete : IIntentHandler
    {

        private readonly IApiService _apiService;
        private readonly IIndexersService _indexersService;

        public Delete(IApiService apiService, IIndexersService indexersService)
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
                await context.Conversation.ShiftState<SelectVenueToDeleteState>(context);
            }
            else
            {
                context.Conversation.SetItem("venue", venues.Single());
                context.Conversation.SetItem("prexisting", true);
                await context.Conversation.ShiftState<DeleteVenueState>(context);
            }
        }

    }
}
