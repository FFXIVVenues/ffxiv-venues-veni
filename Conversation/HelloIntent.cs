using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Conversation
{
    internal class HelloIntent : IntentHandler
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
        
        public override Task Handle(VeniInteractionContext context)
        {
            if (context.Interaction.User.Id == People.People.Kana)
                return context.Interaction.RespondAsync(_kanaMessages.PickRandom());
            if (context.Interaction.User.Id == People.People.Sumi)
                return context.Interaction.RespondAsync(_sumiMessages.PickRandom());

            return context.Interaction.RespondAsync(_helloMessage.PickRandom());
        }

    }
}
