using FFXIVVenues.Veni.Context;
using Kana.Pipelines;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Middleware
{
    internal class StartTypingMiddleware : IMiddleware<MessageContext>
    {
        public async Task ExecuteAsync(MessageContext context, Func<Task> next)
        {
            await context.Message.Channel.TriggerTypingAsync();
            await next();
        }
    }
}
