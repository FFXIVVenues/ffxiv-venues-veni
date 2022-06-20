using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class ConfirmVenueState : IState
    {
        private readonly IApiService _apiService;

        private static string[] _preexisingResponse = new[]
        {
            "Wooo! All updated!",
            "All done for you! 🥳",
            "Ok, that's updated for you! 😊"
        };

        private static string[] _successfulResponse = new[]
        {
            "Wooo! I've sent it. Once it's approved it'll show on the index!",
            "All done! Once Kana approves it, it'll be live! 🥳",
            "Ok! We'll get that approved and get it live soon! 🎉"
        };

        public ConfirmVenueState(IApiService apiService)
        {
            _apiService = apiService;
        }

        public Task Enter(MessageContext c) =>
            c.SendMessageAsync($"Here's a summary of your venue!\n {c.Conversation.GetItem<Venue>("venue")} **Would you like to make any edits?**");

        public async Task Handle(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");

            if (c.Prediction.TopIntent == IntentNames.Response.No)
            {
                var response = await _apiService.PutVenueAsync(venue);
                if (!response.IsSuccessStatusCode)
                {
                    await c.SendMessageAsync("Ooops! Something went wrong. 😢");
                    return;
                }
                c.Conversation.ClearState();
                c.Conversation.ClearData();
                var preexisting = c.Conversation.GetItem<bool>("prexisting");
                if (preexisting)
                    await c.SendMessageAsync(_preexisingResponse.PickRandom());
                else
                    await c.SendMessageAsync(_successfulResponse.PickRandom());
            }
            else if (c.Prediction.TopIntent == IntentNames.Response.Yes)
                await c.Conversation.ShiftState<ModifyVenueState>(c);
            else
                await c.SendMessageAsync(MessageRepository.DontUnderstandResponses.PickRandom());
        }
    }
}
