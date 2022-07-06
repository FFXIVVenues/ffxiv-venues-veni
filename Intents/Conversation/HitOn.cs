using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class HitOn : IIntentHandler
    {

        private static string[] _kanaMessages = new[]
        {
            "*Snuggles with Kana*",
            "Mhm!",
            "♥️"
        };

        private static string[] _responses = new[]
        {
            "I-- *hides behind Kana*",
            "I-- will have to ask mom.",
            "*looks away meekly as she blushes*"
        };

        public async Task Handle(MessageContext context)
        {
            if (context.Message.Author.Id == People.Kana)
                await context.RespondAsync(_kanaMessages.PickRandom());
            else
                await context.RespondAsync(_responses.PickRandom());
        }

    }
}
