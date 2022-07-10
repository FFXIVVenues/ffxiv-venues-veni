using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class ShowForManager : IIntentHandler
    {

        private readonly IApiService _apiService;

        public ShowForManager(IApiService apiService)
        {
            this._apiService = apiService;
        }

        public async Task Handle(MessageContext c)
        {
            var discordIdStr = (c.Prediction.Entities["discord-id"] as JArray)?.First.Value<string>();

            if (string.IsNullOrWhiteSpace(discordIdStr))
            {
                await c.RespondAsync("Which manager am I getting venues for? 🤔");
                return;
            }

            var discordId = ulong.Parse(discordIdStr);
            var venues = await this._apiService.GetAllVenuesAsync(discordId);

            if (venues == null || !venues.Any())
            {
                await c.RespondAsync("Couldn't find any venues for that manager. 😔");
                return;
            }

            if (venues.Count() > 25)
                venues = venues.Take(25);
            c.Conversation.SetItem("venues", venues);
            await c.Conversation.ShiftState<SelectVenueToShowState>(c);
        }

    }
}
