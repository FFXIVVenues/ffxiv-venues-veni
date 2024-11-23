using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using Kana.Pipelines;

namespace FFXIVVenues.Veni.Infrastructure.Middleware;

class StateMiddleware : IMiddleware<MessageVeniInteractionContext>
{

    public async Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
    {
        if (context.Session.StateStack == null)
        {
            await next();
            return;
        }
            
        var handled = await context.HandleMessageAsync();
        if (!handled) await next();
    }

}