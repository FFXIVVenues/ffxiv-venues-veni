using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Intents.Conversation
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

        public override Task Handle(VeniInteractionContext context) =>
            context.Interaction.RespondAsync(_responses.PickRandom());

    }
}
