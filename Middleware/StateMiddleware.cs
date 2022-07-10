using FFXIVVenues.Veni.Context;
using Kana.Pipelines;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Middleware
{
    class StateMiddleware : IMiddleware<MessageContext>
    {

        public async Task ExecuteAsync(MessageContext context, Func<Task> next)
        {
            if (context.Conversation.ActiveState == null)
            {
                await next();
                return;
            }
            
            var handled = await context.Conversation.HandleMessageAsync(context);
            if (!handled) await next();
        }

    }
}
