using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Intents.Conversation
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

        public override async Task Handle(VeniInteractionContext context)
        {
            if (context.Interaction.User.Id == People.People.Kana)
                await context.Interaction.RespondAsync(_kanaMessages.PickRandom());
            else
                await context.Interaction.RespondAsync(_responses.PickRandom());
        }

    }
}
