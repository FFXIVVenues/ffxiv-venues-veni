using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace FFXIVVenues.Veni.Api
{
    public class IndexersService : IIndexersService
    {

        public ulong[] Indexers { get; private init; }
        public ulong[] Photographers { get; private init; }

        private readonly DiscordSocketClient _client;
        private readonly ConcurrentDictionary<string, Broadcast> _broadcasts = new();

        public IndexersService(DiscordSocketClient client, IConfiguration config)
        {
            var indexerStrings = config.GetSection("Indexers")?.GetChildren()?.Select(x => x.Value)?.ToArray();
            this.Indexers = indexerStrings.Select(s => ulong.Parse(s)).ToArray();
            var photographerStrings = config.GetSection("Photographers")?.GetChildren()?.Select(x => x.Value)?.ToArray();
            this.Photographers = photographerStrings.Select(s => ulong.Parse(s)).ToArray();
            this._client = client;
        }

        public bool IsIndexer(ulong userId) => this.Indexers.Contains(userId);
        public bool IsPhotographer(ulong userId) => this.Photographers.Contains(userId);

        public Broadcast Broadcast()
        {
            var broadcast = new Broadcast(Guid.NewGuid().ToString(), this._client);
            this._broadcasts[broadcast.Id] = broadcast;
            return broadcast;
        }

        public async Task<bool> HandleComponentInteractionAsync(SocketMessageComponent context)
        {
            foreach (var broadcast in _broadcasts)
            {
                var handled = await broadcast.Value.HandleComponentInteraction(context);
                if (handled) return true;
            }
            return false;
        }

    }

}

