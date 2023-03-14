using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.Infrastructure.Components;

public class ComponentBroker : IComponentBroker
{
    
    private readonly TypeMap<IComponentHandler> _handlers;

    public ComponentBroker(IServiceProvider serviceProvider) =>
        this._handlers = new(serviceProvider);

    public void Add<THandler>(string key) where THandler : IComponentHandler =>
        this._handlers.Add<THandler>(key);

    public Task HandleAsync(SocketMessageComponent component)
    {
        var key = component.Data.CustomId.Split(":");
        var handler = this._handlers.Activate(key[0]);
        if (handler == default)
            return Task.CompletedTask;
        return handler.HandleAsync(component, key[1..]);
    }

}
