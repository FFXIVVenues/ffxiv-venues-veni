using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;
using System.Linq;
using FFXIVVenues.Veni.Api;
using Newtonsoft.Json.Linq;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.States.Abstractions;

namespace FFXIVVenues.Veni.States
{
    class ManagerEntryState : IState
    {
        private readonly IIndexersService _indexersService;

        public ManagerEntryState(IIndexersService indexersService)
        {
            this._indexersService = indexersService;
        }

        public Task Init(InteractionContext c)
        {
            if (!this._indexersService.IsIndexer(c.Interaction.User.Id))
                return c.Session.ShiftState<ConfirmVenueState>(c);

            c.Session.RegisterMessageHandler(this.OnMessageReceived);
            return c.Interaction.RespondAsync("Who is/are the manager(s)? :heart:");
        }

        public Task OnMessageReceived(MessageInteractionContext c)
        {
            var venue = c.Session.GetItem<Venue>("venue");

            JArray discordIds = null;
            if (c.Prediction.Entities.ContainsKey("discord-id"))
                discordIds = c.Prediction.Entities["discord-id"] as JArray;

            if (discordIds == null || discordIds.Count() == 0)
                return c.Interaction.Channel.SendMessageAsync(MessageRepository.DontUnderstandResponses.PickRandom());

            venue.Managers = discordIds.Select(id => id.Value<string>()).ToList();
            return c.Session.ShiftState<ConfirmVenueState>(c);
        }

    }
}
