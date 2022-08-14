using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Session
{
    internal class HitOn : IntentHandler
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

        public override async Task Handle(InteractionContext context)
        {
            if (context.Interaction.User.Id == People.Kana)
                await context.Interaction.RespondAsync(_kanaMessages.PickRandom());
            else
                await context.Interaction.RespondAsync(_responses.PickRandom());
        }

    }
}
