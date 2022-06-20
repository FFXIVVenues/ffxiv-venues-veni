using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class Bye : IIntentHandler
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

        public Task Handle(MessageContext context)
        {
            if (context.Message.Author.Id == 236852510688542720)
                return context.SendMessageAsync(_kanaMessages.PickRandom());
            if (context.Message.Author.Id == 339219022774272000)
                return context.SendMessageAsync(_aaryxMessages.PickRandom());
            if (context.Message.Author.Id == 685561823943983125)
                return context.SendMessageAsync(_nikoMessages.PickRandom());

            return context.SendMessageAsync(_messages.PickRandom());
        }

    }
}
