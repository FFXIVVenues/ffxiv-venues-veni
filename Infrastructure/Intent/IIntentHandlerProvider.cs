using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.Infrastructure.Intent
{
    internal interface IIntentHandlerProvider
    {
        Task HandleIteruptIntent(string interupt, MessageInteractionContext context);

        Task HandleIteruptIntent(string interupt, MessageComponentInteractionContext context);

        Task HandleIteruptIntent(string interupt, SlashCommandInteractionContext context);

        Task HandleIntent(string interupt, MessageInteractionContext context);

        Task HandleIntent(string interupt, MessageComponentInteractionContext context);

        Task HandleIntent(string interupt, SlashCommandInteractionContext context);
    }
}