using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;

namespace FFXIVVenues.Veni.States
{
    class DeleteVenueState : IState
    {

        private static string[] _messages = new[]
        {
            "Are you super sure you want to **delete {0}**? 😢",
            "Are you really sure you want to **delete {0}**?"
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

        public Task Init(MessageContext c)
        {
            this._venue = c.Conversation.GetItem<Venue>("venue");
            c.Conversation.RegisterMessageHandler(this.OnMessageReceived);
            return c.RespondAsync(string.Format(_messages.PickRandom(), _venue.Name));
        }

        public async Task OnMessageReceived(MessageContext c)
        {
            if (c.Prediction.TopIntent == IntentNames.Response.Yes)
                await _apiService.DeleteVenueAsync(_venue.Id);
            else if (c.Prediction.TopIntent != IntentNames.Response.No)
                await c.RespondAsync(MessageRepository.DontUnderstandResponses.PickRandom());

            c.Conversation.ClearData();
            c.Conversation.ClearState();
            await c.RespondAsync(_deleteMessages.PickRandom());
        }
    }
}
