using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using Kana.Pipelines;

namespace FFXIVVenues.Veni.Infrastructure.Middleware;

internal class ConversationFilterMiddleware : IMiddleware<MessageVeniInteractionContext>
{

    public Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
    {
        if (context.Interaction.Channel is SocketDMChannel || context.Interaction.MentionedUsers.Any(u => u.Id == context.Client.CurrentUser.Id))
            return next();
        return Task.CompletedTask;
    }

}