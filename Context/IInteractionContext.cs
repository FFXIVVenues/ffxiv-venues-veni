using Discord.WebSocket;

namespace FFXIVVenues.Veni.Context
{
    public interface IInteractionContext
    {
        DiscordSocketClient Client { get; }
        SessionContext Session { get; }

    }
}