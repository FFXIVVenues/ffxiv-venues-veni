using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context.Session;

namespace FFXIVVenues.Veni.Infrastructure.Context
{
    public interface IVeniInteractionContext
    {
        DiscordSocketClient Client { get; }
        SessionContext Session { get; }

    }
}