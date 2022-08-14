using Kana.Pipelines;
using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Luis;
using FFXIVVenues.Veni.Context;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using FFXIVVenues.Veni.Intents;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.Middleware
{
    internal class LuisPredictionMiddleware : IMiddleware<MessageInteractionContext>
    {
        private readonly ILuisClient _luisClient;

        public LuisPredictionMiddleware(ILuisClient luisClient)
        {
            _luisClient = luisClient;
        }

        public async Task ExecuteAsync(MessageInteractionContext context, Func<Task> next)
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
