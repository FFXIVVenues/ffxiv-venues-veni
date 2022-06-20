using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.States;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Close : IIntentHandler
    {

        private static string[] _responses = new[]
        {
            "The doors are closed! I can't wait til next time. ♥️",
            "We're no longer green! We'll close up. ",
            "Okay! I'll lock up. 😉"
        };

        private IApiService _apiService;

        public Close(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task Handle(MessageContext context)
        {
            var contact = context.Message.Author.Id;
            var venuesForContact = await _apiService.GetAllVenuesAsync(contact);
            if (venuesForContact == null || !venuesForContact.Any())
                await context.SendMessageAsync("You don't seem to be an assigned contact for any venues. 🤔");
            else if (venuesForContact.Count() > 1)
            {
                context.Conversation.SetItem("venues", venuesForContact);
                await context.Conversation.ShiftState<SelectVenueToCloseState>(context);
            }
            else
            {
                await _apiService.CloseVenue(venuesForContact.Single().Id);
                await context.SendMessageAsync(_responses.PickRandom());
            }
        }

    }
}