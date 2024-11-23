using System;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation.Blacklist;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using Kana.Pipelines;

namespace FFXIVVenues.Veni.Infrastructure.Middleware;

class BlacklistMiddleware(IRepository repository) : IMiddleware<MessageVeniInteractionContext>
{
    public async Task ExecuteAsync(MessageVeniInteractionContext context, Func<Task> next)
    {
        if (await repository.ExistsAsync<BlacklistEntry>(context.Interaction.Author.Id.ToString()))
        {
            var dm = await context.Client.GetUser(context.Interaction.Author.Id).CreateDMChannelAsync();
            await dm.SendMessageAsync($"Sorry, my family said I'm not allowed to speak to you. 😢" +
                                      $" If you think this was a mistake please let my family know.");
            return;
        }

        await next();
    }

}