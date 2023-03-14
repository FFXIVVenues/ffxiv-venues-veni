using System.Threading.Tasks;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.Infrastructure.Components;

public interface IComponentBroker
{
    void Add<THandler>(string key) where THandler : IComponentHandler;
    Task HandleAsync(SocketMessageComponent component);
}