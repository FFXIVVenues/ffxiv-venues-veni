using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Session
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

        public override Task Handle(InteractionContext context) =>
            context.Interaction.RespondAsync(_responses.PickRandom());

    }
}
