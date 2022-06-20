using System.Threading.Tasks;
using FFXIVVenues.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Create : IIntentHandler
    {

        private const string CREATE_VALUE_KEY = "venue";

        public async Task Handle(MessageContext context)
        {
            var venue = new Venue();
            venue.Contacts.Add(context.Message.Author.Id.ToString());
            context.Conversation.ContextData.AddOrUpdate(CREATE_VALUE_KEY, (s, v) => v, (s, e, v) => v, venue);
            await context.SendMessageAsync(MessageRepository.CreateVenueMessage.PickRandom());
            await context.Conversation.ShiftState<NameEntryState>(context);
        }

    }
}
