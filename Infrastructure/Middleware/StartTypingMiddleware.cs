using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using Kana.Pipelines;

namespace FFXIVVenues.Veni.Infrastructure.Middleware
{
    internal class StartTypingMiddleware : IMiddleware<MessageInteractionContext>
    {
        public async Task ExecuteAsync(MessageInteractionContext context, Func<Task> next)
        {
            await context.Interaction.Channel.TriggerTypingAsync();
            await next();
        }
    }
}
