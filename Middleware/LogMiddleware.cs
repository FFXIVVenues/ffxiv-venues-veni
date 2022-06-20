using Discord;
using FFXIVVenues.Veni.Context;
using Kana.Pipelines;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Middleware
{
    internal class LogMiddleware : IMiddleware<MessageContext>
    {
        public Task ExecuteAsync(MessageContext context, Func<Task> next)
        {
            var activeState = context.Conversation.ActiveState != null ? "[" + context.Conversation.ActiveState.GetType().Name + "] " : string.Empty;
            Console.WriteLine(new LogMessage(LogSeverity.Info, "Message",
                $"{activeState}{context.Message.Author}: {context.Message.Content}"));
            return next();
        }
    }
}
