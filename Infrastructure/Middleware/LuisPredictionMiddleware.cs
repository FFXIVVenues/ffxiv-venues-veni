using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.AI.Luis;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Utils;
using Kana.Pipelines;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;

namespace FFXIVVenues.Veni.Infrastructure.Middleware
{
    internal class LuisPredictionMiddleware : IMiddleware<MessageVeniInteractionContext>
    {
        private readonly ILuisClient _luisClient;

        public LuisPredictionMiddleware(ILuisClient luisClient)
        {
            _luisClient = luisClient;
        }

        public async Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
        {
            var query = context.Interaction.Content.StripMentions(context.Client.CurrentUser.Id);
            if (string.IsNullOrWhiteSpace(query))
            {
                context.Prediction = new Prediction { TopIntent = IntentNames.None };
                await next();
                return;
            }

            context.Prediction = await _luisClient.PredictAsync(query);
            await next();
        }
    }
}
