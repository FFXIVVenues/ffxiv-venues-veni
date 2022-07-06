using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class Name : IIntentHandler
    {

        private static string[] _nameMessages = new[]
        {
            "My name is Veni!",
            "I'm Veni. :3",
            "It's Veni! Nice to meet you! ♥️",
            "It's Veni. ♥️",
        };

        public Task Handle(MessageContext context) =>
            context.RespondAsync(_nameMessages.PickRandom());

    }
}
