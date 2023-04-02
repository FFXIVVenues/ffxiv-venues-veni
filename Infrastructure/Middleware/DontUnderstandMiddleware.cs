using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using Kana.Pipelines;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Middleware
{
    internal class DontUnderstandMiddleware : IMiddleware<MessageInteractionContext>
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

        public async Task ExecuteAsync(MessageInteractionContext context, Func<Task> next)
        {
            await context.Interaction.Channel.SendMessageAsync(_responses.PickRandom());

        }
    
    }
}
