using System;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.UserSupport;
using FFXIVVenues.Veni.Utils;
using Kana.Pipelines;

namespace FFXIVVenues.Veni.Infrastructure.Middleware;

internal class DisplayHelp : IMiddleware<MessageVeniInteractionContext>
{
    private string[] _responses = new[]
    {
        "I don't understand. :smiling_face_with_tear:",
        "Whaaat?",
        "Huh?",
        "Kweh!?!",
        "Kupo?",
        "Sorry, I'm still learning. :smiling_face_with_tear:",
        "I don't know how to answer that. 😓",
        "I don't really know how to respond to that. 😅"
    };

    public async Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
    {
        await context.Interaction.Channel.SendMessageAsync(
            _responses.PickRandom() + "\n" + UserSupportStrings.HelpResponseMessage, 
            embed: new EmbedBuilder().WithDescription(UserSupportStrings.HelpResponseEmbed).Build());
    }
}
