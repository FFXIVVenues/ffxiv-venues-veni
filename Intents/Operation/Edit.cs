using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.States;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Edit : IIntentHandler
    {

        private readonly IApiService _apiService;
        private readonly IIndexersService _indexersService;

        public Edit(IApiService apiService, IIndexersService indexersService)
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
                if (isIndexer)
                    await context.RespondAsync("There doesn't seem to be any venues to edit!? Kupo?!");
                else
                    await context.RespondAsync("You don't seem to be an assigned manager for any venues. 🤔");

            else if (venues.Count() > 1)
            {
                context.Conversation.SetItem("venues", venues);
                await context.Conversation.ShiftState<SelectVenueToModifyState>(context);
            }
            else
            {
                context.Conversation.SetItem("venue", venues.Single());
                context.Conversation.SetItem("prexisting", true); // different to "modifying" since you can modifying a not-yet-sent venue
                await context.Conversation.ShiftState<ModifyVenueState>(context);
            }
        }

    }
}
