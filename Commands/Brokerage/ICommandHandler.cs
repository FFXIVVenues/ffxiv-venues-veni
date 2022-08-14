using Discord.WebSocket;
using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Commands.Brokerage
{
    internal interface ICommandHandler
    {
        Task HandleAsync(SlashCommandInteractionContext slashCommand);
    }

}
