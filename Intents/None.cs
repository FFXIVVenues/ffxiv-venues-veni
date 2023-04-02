using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Intents
{
    internal class None : IntentHandler
    {
        public override async Task Handle(VeniInteractionContext context) =>
            await context.Interaction.Channel.SendMessageAsync("X.X");
    }
}
