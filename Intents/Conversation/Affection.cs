using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class Affection : IIntentHandler
    {

        private static string[] _responses = new[]
        {
            "♥️",
            "🥰",
            "😘",
            "🤗",
            "😻",
            "You're cute. ♥️",
            "no u. ♥️",
            "*looks away meekly as she blushes*"
        };

        public Task Handle(MessageContext context) =>
            context.SendMessageAsync(_responses.PickRandom());

    }
}
