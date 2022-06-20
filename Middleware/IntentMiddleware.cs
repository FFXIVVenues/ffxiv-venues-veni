using Kana.Pipelines;
using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Context;

namespace FFXIVVenues.Veni.Middleware
{
    class IntentMiddleware : IMiddleware<MessageContext>
    {
        private readonly IIntentHandlerProvider intentHandlerProvider;

        public IntentMiddleware(IIntentHandlerProvider intentHandlerProvider)
        {
            this.intentHandlerProvider = intentHandlerProvider;
        }

        public Task ExecuteAsync(MessageContext context, Func<Task> next)
        {
            var intentHandler = intentHandlerProvider.ActivateIntentHandler(context.Prediction.TopIntent);
            if (intentHandler == null)
                return new None().Handle(context);
            return intentHandler.Handle(context);
        }

    }
}
