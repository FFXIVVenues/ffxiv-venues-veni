using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class Meow : IntentHandler
    {

        private static string[] _meowMessages = new[]
        {
            "Meow!",
            "Meeeooooow!",
            "meeooow",
            "Mmmeeeeeeeeooooooooowwww!",
            "nya. :3"
        };

        public override Task Handle(VeniInteractionContext context) =>
            context.Interaction.RespondAsync(_meowMessages.PickRandom());

    }
}
