using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class Hello : IIntentHandler
    {
        private static string[] _helloMessage = new string[]
        {
            "Hey",
            "Hi",
            "Hi hi",
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
            "Suuumiiii! ♥️  Aether's newest indexer!",
            "Suuumiiii! ♥️  How goes indexing?",
            "Sumi! ♥️",
            "Hi! ♥️ You came for me, Sumichan?",
            "Heya sis!",
            "Sumi :heart_eyes:",
        };

        private static string[] _aaryxMessages = new[]
        {
            "Heya Bunny girl! ♥️ How's the crew?",
            "Aaryx! There's my bunny! I missed you! ♥️",
            "Aaryx, come to call me mommy again? 😑"
        };

        private static string[] _nikoMessages = new[]
        {
            "Heyo Niko! ♥️ When are you becoming kitty?",
            "Niko! ♥️ Did you come to test me again? I don't know if I can take anymore. 😅"
        };

        private static string[] _fluffyMessages = new[]
        {
            "Heyo Fluffy! ❤️",
            "Fluffy! ♥️ Still buying out venue's stock? 😅",
            "Fluffy! How's my greedy catboi? ❤️"
        };

        public Task Handle(MessageContext context)
        {
            if (context.Message.Author.Id == People.Kana)
                return context.RespondAsync(_kanaMessages.PickRandom());
            if (context.Message.Author.Id == People.Sumi)
                return context.RespondAsync(_sumiMessages.PickRandom());
            if (context.Message.Author.Id == People.Fluffy)
                return context.RespondAsync(_fluffyMessages.PickRandom());

            return context.RespondAsync(_helloMessage.PickRandom());
        }

    }
}
