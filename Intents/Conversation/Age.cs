using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Context;
using PrettyPrintNet;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class Age : IIntentHandler
    {

        private static string[] _messages = new[]
        {
            "I'm {0} old! ♥️",
            "Today, I'm {0} old ♥️",
            "{0} old 🙂",
            "{0}",
        };

        public Task Handle(MessageContext context)
        {
            var age = DateTime.Now - DateTime.Parse("06-11-2021 21:40:00");
            return context.SendMessageAsync(string.Format(_messages.PickRandom(), age.ToPrettyString()));
        }

    }
}
