using Discord.WebSocket;
using FFXIVVenues.Veni.Context;
using Kana.Pipelines;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Middleware
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
