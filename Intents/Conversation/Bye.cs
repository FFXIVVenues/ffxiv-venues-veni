using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class Bye : IntentHandler
    {

        private static string[] _messages = new[]
        {
            "Cyaaa ♥️",
            "Bi bi",
            "Don't forget me, oki?",
            "Until next time!",
            "/wave"
        };

        private static string[] _kanaMessages = new[]
        {
            "Bi bi kannaaa! ♥️  I'll miss you!",
            "Cya kana"
        };

        private static string[] _aaryxMessages = new[]
        {
            "Cya bunny girl! Miss you! ♥️",
            "Miss you bunny! ♥️"
        };

        private static string[] _nikoMessages = new[]
        {
            "Bye Niko! ♥️",
            "Cya Niko! ♥️ Next time I see you, you better be a kitty!",
            "Cya Niko! ♥️ No more tests, alright? 😅"
        };

        public override Task Handle(VeniInteractionContext context)
        {
            if (context.Interaction.User.Id == 236852510688542720)
                return context.Interaction.RespondAsync(_kanaMessages.PickRandom());
            if (context.Interaction.User.Id == 339219022774272000)
                return context.Interaction.RespondAsync(_aaryxMessages.PickRandom());
            if (context.Interaction.User.Id == 685561823943983125)
                return context.Interaction.RespondAsync(_nikoMessages.PickRandom());

            return context.Interaction.RespondAsync(_messages.PickRandom());
        }

    }
}
