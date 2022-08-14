using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using PrettyPrintNet;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Session
{
    internal class Age : IntentHandler
    {

        private static string[] _messages = new[]
        {
            "I'm {0} old! ♥️",
            "Today, I'm {0} old ♥️",
            "{0} old 🙂",
            "{0}",
        };

        public override Task Handle(InteractionContext context)
        {
            var age = DateTime.Now - DateTime.Parse("06-11-2021 21:40:00");
            return context.Interaction.RespondAsync(string.Format(_messages.PickRandom(), age.ToPrettyString()));
        }

    }
}
