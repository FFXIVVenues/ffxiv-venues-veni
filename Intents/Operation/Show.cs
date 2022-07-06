using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Show : IIntentHandler
    {

        private readonly IApiService _apiService;
        private readonly IIndexersService _indexersService;
        private readonly string _uiUrl;
        private readonly string _apiUrl;

        public Show(IApiService apiService,
                    UiConfiguration uiConfig,
                    ApiConfiguration apiConfig,
                    IIndexersService indexersService)
        {
            this._apiService = apiService;
            this._uiUrl = uiConfig.BaseUrl;
            this._apiUrl = apiConfig.BaseUrl;
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
                await context.RespondAsync("You don't seem to be an assigned manager for any venues. 🤔");
            else if (venues.Count() > 1)
            {
                context.Conversation.SetItem("venues", venues);
                await context.Conversation.ShiftState<SelectVenueToShowState>(context);
            }
            else
            {
                var venue = venues.Single();
                await context.RespondAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build());
            }
        }

    }
}
