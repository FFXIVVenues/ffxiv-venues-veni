using Discord.WebSocket;
using NChronicle.Core.Model;

namespace FFXIVVenues.Veni.Logging
{
    internal interface IDiscordChronicleLibrary
    {
        void Clear();
        bool IsSubscribed(ISocketMessageChannel channel);
        void Subscribe(ISocketMessageChannel channel, ChronicleLevel verbosity);
        void Unsubscribe(ISocketMessageChannel channel);
    }
}