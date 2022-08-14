using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using Kana.Pipelines;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Middleware
{
    class StopCallingMeMommyMiddleware : IMiddleware<MessageInteractionContext>
    {

        private static readonly Regex _match = new ("\\bm+o+m+y+\\b", RegexOptions.IgnoreCase);

        private static readonly string[] _responses = new[]
        {
            "Please don't call me mommy. :facepalm:",
            "Staawp! I'm nobody's mommy! 😆",
            "Mommy? 😑"
        };

        public async Task ExecuteAsync(MessageInteractionContext context, Func<Task> next)
        {
            var match = _match.Match(context.Interaction.Content);
            if (match.Success)
                await context.Interaction.Channel.SendMessageAsync(_responses.PickRandom());

            await next();
        }

    }
}
