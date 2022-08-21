using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Interupt
{
    internal class Cancel : IntentHandler
    {

        public override Task Handle(InteractionContext context)
        {
            if (context.Session.StateStack == null)
                return context.Interaction.RespondAsync("Huh? We're not in the middle of anything. :shrug:");

            context.Session.ClearState();
            return context.Interaction.RespondAsync(MessageRepository.StoppedMessage.PickRandom());
        }

    }
}
