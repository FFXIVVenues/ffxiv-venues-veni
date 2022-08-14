using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Context;
using Kana.Pipelines;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Middleware
{
    internal class LogMiddleware : IMiddleware<MessageInteractionContext>
    {
        public Task ExecuteAsync(MessageInteractionContext context, Func<Task> next)
        {
            var activeState = context.Session.State != null ? "[" + context.Session.State.GetType().Name + "] " : string.Empty;
            Console.WriteLine(new LogMessage(LogSeverity.Info, "Message",
                $"{activeState}{context.Interaction.Author}: {context.Interaction.Content}"));
            return next();
        }
    }
}
