using System.Threading.Tasks;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Create : IIntentHandler
    {

        private const string CREATE_VALUE_KEY = "venue";

        public async Task Handle(MessageContext context)
        {
            context.Conversation.SetItem("isNewVenue", true);

            var venue = new Venue();
            venue.Managers.Add(context.Message.Author.Id.ToString());
            context.Conversation.ContextData.AddOrUpdate(CREATE_VALUE_KEY, (s, v) => v, (s, e, v) => v, venue);
            await context.RespondAsync(MessageRepository.CreateVenueMessage.PickRandom());
            await context.Conversation.ShiftState<NameEntryState>(context);
        }

    }
}
