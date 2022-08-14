using Kana.Pipelines;
using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Context;

namespace FFXIVVenues.Veni.Middleware
{
    class InteruptIntentMiddleware : IMiddleware<MessageInteractionContext>
    {

        private readonly IIntentHandlerProvider intentHandlerProvider;

        public InteruptIntentMiddleware(IIntentHandlerProvider intentHandlerProvider)
        {
            this.intentHandlerProvider = intentHandlerProvider;
        }

        public Task ExecuteAsync(MessageInteractionContext context, Func<Task> next)
        {
            var handleTask = intentHandlerProvider.HandleIteruptIntent(context.Prediction.TopIntent, context);
            if (handleTask == null)
                return next();

            return handleTask;
        }

    }
}
