using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;

namespace FFXIVVenues.Veni.Infrastructure.Components;

public interface IComponentHandler
{
    Task HandleAsync(ComponentVeniInteractionContext context, string[] args);
}