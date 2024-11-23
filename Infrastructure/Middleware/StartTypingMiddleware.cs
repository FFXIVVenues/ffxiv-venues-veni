using System;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using Kana.Pipelines;

namespace FFXIVVenues.Veni.Infrastructure.Middleware;

internal class StartTypingMiddleware : IMiddleware<MessageVeniInteractionContext>
{
    public async Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
    {
        context.TypingHandle = context.Interaction.Channel.EnterTypingState(new RequestOptions());
        await next();
    }
}