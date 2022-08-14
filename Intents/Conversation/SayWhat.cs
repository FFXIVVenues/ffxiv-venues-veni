using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Session
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
