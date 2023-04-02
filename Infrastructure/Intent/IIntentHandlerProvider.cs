using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.Infrastructure.Intent
{
    internal interface IIntentHandlerProvider
    {
        Task HandleIteruptIntent(string interupt, MessageVeniInteractionContext context);

        Task HandleIteruptIntent(string interupt, MessageComponentVeniInteractionContext context);

        Task HandleIteruptIntent(string interupt, SlashCommandVeniInteractionContext context);

        Task HandleIntent(string interupt, MessageVeniInteractionContext context);

        Task HandleIntent(string interupt, MessageComponentVeniInteractionContext context);

        Task HandleIntent(string interupt, SlashCommandVeniInteractionContext context);
    }
}