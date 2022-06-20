using Kana.Pipelines;
using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Context;

namespace FFXIVVenues.Veni.Middleware
{
    class InteruptIntentMiddleware : IMiddleware<MessageContext>
    {

        private readonly IIntentHandlerProvider intentHandlerProvider;

        public InteruptIntentMiddleware(IIntentHandlerProvider intentHandlerProvider)
        {
            this.intentHandlerProvider = intentHandlerProvider;
        }

        public Task ExecuteAsync(MessageContext context, Func<Task> next)
        {
            var intentHandler = intentHandlerProvider.ActivateInteruptIntentHandler(context.Prediction.TopIntent);
            if (intentHandler == null)
                return next();

            return intentHandler.Handle(context);
        }

    }
}
