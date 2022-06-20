using Kana.Pipelines;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Luis;
using FFXIVVenues.Veni.Context;

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
            var query = context.Message.Content.StripMentions();
            context.Prediction = await _luisClient.PredictAsync(query);
            await next();
        }
    }
}
