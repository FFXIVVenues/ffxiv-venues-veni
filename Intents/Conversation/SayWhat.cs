using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class SayWhat : IntentHandler
    {

        private string[] _responses = new[]
        {
            "whaaaat?",
            "whaaat :3",
        };

        public override Task Handle(InteractionContext context) =>
            context.Interaction.RespondAsync(_responses.PickRandom());

    }
}
