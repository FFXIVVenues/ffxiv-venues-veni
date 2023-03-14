using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class Name : IntentHandler
    {

        private static string[] _nameMessages = new[]
        {
            "My name is Veni!",
            "I'm Veni. :3",
            "It's Veni! Nice to meet you! ♥️",
            "It's Veni. ♥️",
        };

        public override Task Handle(InteractionContext context) =>
            context.Interaction.RespondAsync(_nameMessages.PickRandom());

    }
}
