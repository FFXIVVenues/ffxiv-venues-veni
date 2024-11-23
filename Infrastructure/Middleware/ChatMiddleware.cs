using FFXIVVenues.Veni.Utils;
using Kana.Pipelines;
using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.AI.Davinci;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using Serilog;

namespace FFXIVVenues.Veni.Infrastructure.Middleware;

internal class ChatMiddleware(IAiHandler aIHandler) : IMiddleware<MessageVeniInteractionContext>
{
    private static string[] _emotes = new[]
    {
        " ",
        " :3",
        " 💕",
        " 💖",
        " ❤️",
        " 💜",
        " 💞"
    };

    public async Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
    {
        try
        {
            string response = await aIHandler.ResponseHandler(context);
            await context.Interaction.Channel.SendMessageAsync(response + _emotes.PickRandom());
        }
        catch (Exception ex)
        {
            Log.Warning(ex.Message, ex);
            await next();
        }

    }

}