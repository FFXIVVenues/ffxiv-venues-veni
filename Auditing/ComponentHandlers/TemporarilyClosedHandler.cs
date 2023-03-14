using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Components;

namespace FFXIVVenues.Veni.Auditing.ComponentHandlers;

public class TemporarilyClosedHandler : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "AUDIT_TEMP_CLOSED";
    
    public Task HandleAsync(SocketMessageComponent component, string[] args)
    {
        return component.Message.Channel.SendMessageAsync("Meow - Temporarily Closed");
    }
    
}