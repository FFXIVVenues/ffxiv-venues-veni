using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;

namespace FFXIVVenues.Veni.Infrastructure.Components;

public interface IComponentBroker
{
    void Add<THandler>(string key) where THandler : IComponentHandler;
    Task HandleAsync(ComponentVeniInteractionContext component);
}