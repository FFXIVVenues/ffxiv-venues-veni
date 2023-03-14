using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Components;

namespace FFXIVVenues.Veni.Auditing.ComponentHandlers;

public class EditVenueHandler : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "AUDIT_EDIT_VENUE";
    
    public Task HandleAsync(SocketMessageComponent component, string[] args)
    {
        return component.Message.Channel.SendMessageAsync("Meow - Edit Venue");
    }
    
}