using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class HowAreYou : IntentHandler
    {

        private static string[] _messages = new[]
        {
            "I'm doing great! Ready to play. ♥️",
            "Good, thanks! ♥️",
            "Excited! How can I help?",
            "Really well, thanks! :smile:",
            "Better now you're here. :wink:"
        };

        public override Task Handle(InteractionContext context) =>
            context.Interaction.RespondAsync(_messages.PickRandom());

    }
}
