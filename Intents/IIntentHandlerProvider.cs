using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents
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