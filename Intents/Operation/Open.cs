using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Open : IIntentHandler
    {

        private static string[] _responses = new[]
        {
            "Woo! The doors are open. You're green and announcements have been sent! Let's have fun today! ♥️",
            "Yay! It's that time again. 😀 You're all green on the index, and everyone's been notified. ♥️",
            "Let's do it! We... are... live!!! We're green on the index and the pings are flying! So excited. 🙂"
        };

        private IApiService _apiService;

        public Open(IApiService apiService)
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
                await context.Conversation.ShiftState<SelectVenueToOpenState>(context);
            }
            else
            {
                await _apiService.OpenVenue(venuesForContact.Single().Id);
                await context.SendMessageAsync(_responses.PickRandom());
            }
        }

    }
}
