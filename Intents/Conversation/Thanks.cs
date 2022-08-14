using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Session
{
    internal class Thanks : IntentHandler
    {

        private static string[] _responses = new[]
        {
            "Anytime!",
            "Let me know if you need anything!",
            "You're very welcome! ♥️",
            "Ofcourse, anytime!"
        };

        public override Task Handle(InteractionContext context) =>
            context.Interaction.RespondAsync(_responses.PickRandom());

    }
}
