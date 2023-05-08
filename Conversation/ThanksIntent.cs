using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Conversation
{
    internal class ThanksIntent : IntentHandler
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
