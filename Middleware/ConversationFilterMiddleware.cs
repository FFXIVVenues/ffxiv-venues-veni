using Discord.WebSocket;
using FFXIVVenues.Veni.Context;
using Kana.Pipelines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Middleware
{
    class ConversationFilterMiddleware : IMiddleware<MessageContext>
    {

        public Task ExecuteAsync(MessageContext context, Func<Task> next)
        {
            if (context.Message.Channel is SocketDMChannel || context.Message.MentionedUsers.Any(u => u.Id == context.Client.CurrentUser.Id))
                return next();
            return Task.CompletedTask;
        }

    }
}
