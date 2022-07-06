using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Conversation
{
    internal class Meow : IIntentHandler
    {

        private static string[] _meowMessages = new[]
        {
            "Meow!",
            "Meeeooooow!",
            "meeooow",
            "Mmmeeeeeeeeooooooooowwww!",
            "nya. :3"
        };

        public Task Handle(MessageContext context) =>
            context.RespondAsync(_meowMessages.PickRandom());

    }
}
