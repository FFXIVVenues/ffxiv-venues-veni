using FFXIVVenues.Veni.Context;
using Kana.Pipelines;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Middleware
{
    class StateMiddleware : IMiddleware<MessageContext>
    {

        public Task ExecuteAsync(MessageContext context, Func<Task> next)
        {
            if (context.Conversation.ActiveState == null)
                return next();

            return context.Conversation.HandleMessage(context);
        }

    }
}
