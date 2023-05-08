using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Utils;
using PrettyPrintNet;

namespace FFXIVVenues.Veni.Conversation
{
    internal class AgeIntent : IntentHandler
    {

        private static string[] _messages = new[]
        {
            "I'm {0} old! ♥️",
            "Today, I'm {0} old ♥️",
            "{0} old 🙂",
            "{0}",
        };

        public override Task Handle(VeniInteractionContext context)
        {
            var age = DateTime.Now - new DateTime(2021, 11, 6, 21, 40, 0);
            return context.Interaction.RespondAsync(string.Format(_messages.PickRandom(), age.ToPrettyString()));
        }

    }
}
