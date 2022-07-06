using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class Thanks : IIntentHandler
    {

        private static string[] _responses = new[]
        {
            "Anytime!",
            "Let me know if you need anything!",
            "You're very welcome! ♥️",
            "Ofcourse, anytime!"
        };

        public Task Handle(MessageContext context) =>
            context.RespondAsync(_responses.PickRandom());

    }
}
