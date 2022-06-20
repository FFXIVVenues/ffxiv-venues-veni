using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Show : IIntentHandler
    {
        private readonly IApiService _apiService;

        public Show(IApiService apiService)
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
                await context.Conversation.ShiftState<SelectVenueToShowState>(context);
            }
            else
                await context.SendMessageAsync(venuesForContact.Single().ToString());
        }

    }
}
