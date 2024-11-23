using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;

namespace FFXIVVenues.Veni.Infrastructure.Commands
{
    public interface ICommandHandler
    {
        Task HandleAsync(SlashCommandVeniInteractionContext slashCommand);
    }

}
