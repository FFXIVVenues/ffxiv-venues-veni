using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class Meow : IntentHandler
    {

        private static string[] _meowMessages = new[]
        {
            "Meow!",
            "Meeeooooow!",
            "meeooow",
            "Mmmeeeeeeeeooooooooowwww!",
            "nya. :3"
        };

        public override Task Handle(InteractionContext context) =>
            context.Interaction.RespondAsync(_meowMessages.PickRandom());

    }
}
