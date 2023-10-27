using Discord.WebSocket;
using NChronicle.Core.Model;

namespace FFXIVVenues.Veni.Infrastructure.Logging
{
    public interface IDiscordChronicleLibrary
    {
        void Clear();
        bool IsSubscribed(ISocketMessageChannel channel);
        void Subscribe(ISocketMessageChannel channel, ChronicleLevel verbosity);
        void Unsubscribe(ISocketMessageChannel channel);
    }
}