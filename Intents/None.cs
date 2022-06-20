using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents
{
    internal class None : IIntentHandler
    {

        private string[] _responses = new[]
        {
            "I don't understand. :smiling_face_with_tear:",
            "Whaaat?",
            "Huh?",
            "Kweh!?!",
            "Kupo?",
            "Sorry, I'm still learning. :smiling_face_with_tear:",
            "I don't know how to answer that. 😓",
            "I don't really know how to respond to that. 😅"
        };

        public Task Handle(MessageContext context) =>
            context.SendMessageAsync(_responses.PickRandom());

    }
}
