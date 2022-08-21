using Discord.WebSocket;
using FFXIVVenues.Veni.Context;
using Kana.Pipelines;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Middleware
{
    class StateMiddleware : IMiddleware<MessageInteractionContext>
    {

        public async Task ExecuteAsync(MessageInteractionContext context, Func<Task> next)
        {
            if (context.Session.StateStack == null)
            {
                await next();
                return;
            }
            
            var handled = await context.Session.HandleMessageAsync(context);
            if (!handled) await next();
        }

    }
}
