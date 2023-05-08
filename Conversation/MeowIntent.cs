using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Conversation
{
    internal class MeowIntent : IntentHandler
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
