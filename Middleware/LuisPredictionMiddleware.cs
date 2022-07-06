using Kana.Pipelines;
using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Luis;
using FFXIVVenues.Veni.Context;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using FFXIVVenues.Veni.Intents;

namespace FFXIVVenues.Veni.Middleware
{
    internal class LuisPredictionMiddleware : IMiddleware<MessageContext>
    {
        private readonly ILuisClient _luisClient;

        public LuisPredictionMiddleware(ILuisClient luisClient)
        {
            _luisClient = luisClient;
        }

        public async Task ExecuteAsync(MessageContext context, Func<Task> next)
        {
            if (string.IsNullOrWhiteSpace(context.Message.Content))
            {
                context.Prediction = new Prediction { TopIntent = IntentNames.None };
                await next();
                return;
            }

            var query = context.Message.Content.StripMentions();
            context.Prediction = await _luisClient.PredictAsync(query);
            await next();
        }
    }
}
