using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Conversation
{
    internal class SayWhatIntent : IntentHandler
    {

        private string[] _responses = new[]
        {
            "whaaaat?",
            "whaaat :3",
        };

        public override Task Handle(VeniInteractionContext context) =>
            context.Interaction.RespondAsync(_responses.PickRandom());

    }
}
