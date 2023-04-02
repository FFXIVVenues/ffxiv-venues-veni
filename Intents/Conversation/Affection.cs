using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class Affection : IntentHandler
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
