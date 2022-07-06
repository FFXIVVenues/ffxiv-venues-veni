using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Context;
using Kana.Pipelines;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Middleware
{
    class StopCallingMeMommyMiddleware : IMiddleware<MessageContext>
    {

        private static readonly Regex _match = new Regex("\\bmom+y\\b", RegexOptions.IgnoreCase);

        private static string[] _responses = new[]
        {
            "Please don't call me mommy. :facepalm:",
            "Staawp! I'm nobody's mommy! 😆",
            "Mommy? 😑"
        };

        public async Task ExecuteAsync(MessageContext context, Func<Task> next)
        {
            var match = _match.Match(context.Message.Content);
            if (match.Success)
                await context.RespondAsync(_responses.PickRandom());

            await next();
        }

    }
}
