using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Conversation
{
    internal class AffectionIntent : IntentHandler
    {

        private static string[] _responses = new[]
        {
            "♥️",
            "🥰",
            "😘",
            "🤗",
            "😻",
            "You're cute. ♥️",
            "no u. ♥️",
            "*looks away meekly as she blushes*"
        };

        public override Task Handle(VeniInteractionContext context) =>
            context.Interaction.RespondAsync(_responses.PickRandom());

    }
}
