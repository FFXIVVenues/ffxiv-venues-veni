using Discord.WebSocket;
using FFXIVVenues.Veni.Context;
using Kana.Pipelines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Middleware
{
    class ConversationFilterMiddleware : IMiddleware<MessageInteractionContext>
    {

        public Task ExecuteAsync(MessageInteractionContext context, Func<Task> next)
        {
            if (context.Interaction.Channel is SocketDMChannel || context.Interaction.MentionedUsers.Any(u => u.Id == context.Client.CurrentUser.Id))
                return next();
            return Task.CompletedTask;
        }

    }
}
