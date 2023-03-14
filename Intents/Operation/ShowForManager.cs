using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.SessionStates;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class ShowForManager : IIntentHandler
    {

        private readonly IApiService _apiService;

        public ShowForManager(IApiService apiService)
        {
            this._apiService = apiService;
        }

        public Task Handle(MessageInteractionContext context)
        {
            var discordIdStr = (context.Prediction?.Entities["discord-id"] as JArray)?.First.Value<string>();

            if (string.IsNullOrWhiteSpace(discordIdStr))
                return context.Interaction.Channel.SendMessageAsync("Which manager am I getting venues for? 🤔");

            var discordId = ulong.Parse(discordIdStr);

            return this.Handle(context.ToWrappedInteraction(), discordId);
        }

        public Task Handle(MessageComponentInteractionContext context) =>
            Task.CompletedTask;

        public Task Handle(SlashCommandInteractionContext context)
        {
            var user = context.Interaction.Data?.Options?.FirstOrDefault(o => o.Name == "user")?.Value as SocketUser;
            return this.Handle(context.ToWrappedInteraction(), user.Id);
        }

        private async Task Handle(InteractionContext c, ulong discordId)
        {
            var venues = await this._apiService.GetAllVenuesAsync(discordId);

            if (venues == null || !venues.Any())
            {
                await c.Interaction.RespondAsync("Couldn't find any venues for that manager. 😔");
                return;
            }

            if (venues.Count() > 25)
                venues = venues.Take(25);
            c.Session.SetItem("venues", venues);
            await c.Session.MoveStateAsync<SelectVenueToShowSessionState>(c);
        }

    }
}
