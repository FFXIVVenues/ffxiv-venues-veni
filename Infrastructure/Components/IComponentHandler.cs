using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.Infrastructure.Components;

public interface IComponentHandler
{
    Task HandleAsync(SocketMessageComponent component, string[] args);
}