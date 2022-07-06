using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class SayWhat : IIntentHandler
    {

        private string[] _responses = new[]
        {
            "whaaaat?",
            "whaaat :3",
        };

        public Task Handle(MessageContext context) =>
            context.RespondAsync(_responses.PickRandom());

    }
}
