using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class HowAreYou : IIntentHandler
    {

        private static string[] _messages = new[]
        {
            "I'm doing great! Ready to play. ♥️",
            "Good, thanks! ♥️",
            "Excited! How can I help?",
            "Really well, thanks! :smile:",
            "Better now you're here. :wink:"
        };

        public Task Handle(MessageContext context) =>
            context.SendMessageAsync(_messages.PickRandom());

    }
}
