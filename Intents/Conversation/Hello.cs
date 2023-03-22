using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class Hello : IntentHandler
    {
        private static string[] _helloMessage = new string[]
        {
            "Hey",
            "Hi",
            "Hi there",
            "Hi hi",
            "Nyyyaaaa ♥",
            "Meow",
            "Hiya",
            "Heya",
            "Meowdi",
            "Nyalo"
        };

        private static string[] _kanaMessages = new[]
        {
            "Kannaaa! ♥️  I missed you!",
            "Kana! ♥️",
            "Hi! ♥️ You came for me, Kana?",
            "Heya mom!",
            "Kana :heart_eyes:",
        };

        private static string[] _sumiMessages = new[]
        {
            "Suuumiiii! ♥️",
            "Siiis! ♥️  How goes indexing?",
            "Sumi! ♥️",
            "Hi! ♥️ You came for me, Sumichan?",
            "Heya sis!",
            "Sumi :heart_eyes:",
            "Sis ♥️",
        };

        private static string[] _fluffyMessages = new[]
        {
            "Heyo Fluffy! ❤️",
            "Fluffy! ♥️ Still buying out venue's stock? 😅",
            "Fluffy! How's my greedy catboi? ❤️"
        };

        public override Task Handle(VeniInteractionContext context)
        {
            if (context.Interaction.User.Id == People.People.Kana)
                return context.Interaction.RespondAsync(_kanaMessages.PickRandom());
            if (context.Interaction.User.Id == People.People.Sumi)
                return context.Interaction.RespondAsync(_sumiMessages.PickRandom());
            if (context.Interaction.User.Id == People.People.Fluffy)
                return context.Interaction.RespondAsync(_fluffyMessages.PickRandom());

            return context.Interaction.RespondAsync(_helloMessage.PickRandom());
        }

    }
}
