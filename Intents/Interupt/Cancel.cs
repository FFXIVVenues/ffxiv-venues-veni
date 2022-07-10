using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Interupt
{
    internal class Cancel : IIntentHandler
    {

        public Task Handle(MessageContext context)
        {
            if (context.Conversation.ActiveState == null)
                return context.RespondAsync("Huh? We're not in the middle of anything. :shrug:");

            context.Conversation.ClearState();
            return context.RespondAsync(MessageRepository.StoppedMessage.PickRandom());
        }

    }
}
