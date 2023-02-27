using Kana.Pipelines;
using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Context;

namespace FFXIVVenues.Veni.Middleware
{
    class IntentMiddleware : IMiddleware<MessageInteractionContext>
    {
        private readonly IIntentHandlerProvider intentHandlerProvider;

        public IntentMiddleware(IIntentHandlerProvider intentHandlerProvider)
        {
            this.intentHandlerProvider = intentHandlerProvider;
        }

        public Task ExecuteAsync(MessageInteractionContext context, Func<Task> next)
        {
            if (context.Prediction.TopIntent == IntentNames.None) 
                return next();
            
            return intentHandlerProvider.HandleIntent(context.Prediction.TopIntent, context);
        }

    }
}
