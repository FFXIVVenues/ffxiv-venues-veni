using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.States
{
    class DeleteVenueState : IState
    {

        private static string[] _messages = new[]
        {
            "Are you super sure you want to **delete {0}**? 😢",
            "Are you really sure you want to **delete {0}**?",
            "R-really? Are you sure you want me to **delete {0}**?"
        };

        private static string[] _deleteMessages = new[]
        {
            "Okay, that's done. 😢",
            "It's gone. 😢"
        };

        private readonly IApiService _apiService;
        private Venue _venue;

        public DeleteVenueState(IApiService apiService)
        {
            this._apiService = apiService;
        }

        public Task Init(InteractionContext c)
        {
            this._venue = c.Session.GetItem<Venue>("venue");
            return c.Interaction.RespondAsync(string.Format(_messages.PickRandom(), _venue.Name), new ComponentBuilder()
                .WithButton("Yes, delete it 😢", c.Session.RegisterComponentHandler(cm =>
                    {
                        _ = c.Interaction.RespondAsync(_deleteMessages.PickRandom());
                        return _apiService.DeleteVenueAsync(_venue.Id);
                    }, 
                    ComponentPersistence.ClearRow), ButtonStyle.Danger)
                .WithButton("No, don't! I've changed my mind. 🙂", c.Session.RegisterComponentHandler(cm => cm.Interaction.RespondAsync("Phew 😅"), ComponentPersistence.ClearRow))
                .Build());
        }
    }
}
