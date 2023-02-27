using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents
{
    internal class None : IntentHandler
    {
        public override async Task Handle(InteractionContext context) =>
            await context.Interaction.Channel.SendMessageAsync("X.X");
    }
}
