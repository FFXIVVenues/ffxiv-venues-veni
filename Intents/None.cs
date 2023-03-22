using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Intents
{
    internal class None : IntentHandler
    {

        private string[] _responses = new[]
        {
            "I don't understand. :smiling_face_with_tear:",
            "Whaaat?",
            "Huh?",
            "Kweh!?!",
            "Kupo?",
            "Sorry, I'm still learning. :smiling_face_with_tear:",
            "I don't know how to answer that. 😓",
            "I don't really know how to respond to that. 😅"
        };

        public override Task Handle(VeniInteractionContext context) =>
            context.Interaction.RespondAsync(_responses.PickRandom());

    }
}
