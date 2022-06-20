using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Delete : IIntentHandler
    {

        private readonly IApiService _apiService;

        public Delete(IApiService apiService)
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
                await context.Conversation.ShiftState<SelectVenueToDeleteState>(context);
            }
            else
            {
                context.Conversation.SetItem("venue", venuesForContact.Single());
                context.Conversation.SetItem("prexisting", true);
                await context.Conversation.ShiftState<DeleteVenueState>(context);
            }
        }

    }
}
