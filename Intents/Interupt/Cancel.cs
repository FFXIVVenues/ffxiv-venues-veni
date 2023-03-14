using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Intents.Interupt
{
    internal class Cancel : IntentHandler
    {

        public override Task Handle(InteractionContext context)
        {
            if (context.Session.StateStack == null)
                return context.Interaction.RespondAsync("Huh? We're not in the middle of anything. :shrug:");

            _ = context.Session.ClearState(context);
            return context.Interaction.RespondAsync(MessageRepository.StoppedMessage.PickRandom());
        }

    }
}
