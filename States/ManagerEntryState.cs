using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;
using System.Linq;
using FFXIVVenues.Veni.Api;
using Newtonsoft.Json.Linq;
using FFXIVVenues.Veni.Api.Models;

namespace FFXIVVenues.Veni.States
{
    class ManagerEntryState : IState
    {
        private readonly IIndexersService _indexersService;

        public ManagerEntryState(IIndexersService indexersService)
        {
            this._indexersService = indexersService;
        }

        public Task Init(MessageContext c)
        {
            if (!this._indexersService.IsIndexer(c.User.Id))
                return c.Conversation.ShiftState<ConfirmVenueState>(c);

            c.Conversation.RegisterMessageHandler(this.OnMessageReceived);
            return c.RespondAsync("Who is/are the manager(s)? :heart:");
        }

        public Task OnMessageReceived(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");

            JArray discordIds = null;
            if (c.Prediction.Entities.ContainsKey("discord-id"))
                discordIds = c.Prediction.Entities["discord-id"] as JArray;

            if (discordIds == null || discordIds.Count() == 0)
                return c.RespondAsync(MessageRepository.DontUnderstandResponses.PickRandom());

            venue.Managers = discordIds.Select(id => id.Value<string>()).ToList();
            return c.Conversation.ShiftState<ConfirmVenueState>(c);
        }

    }
}
