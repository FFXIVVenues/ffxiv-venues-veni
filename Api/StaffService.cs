using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace FFXIVVenues.Veni.Api
{
    public class StaffService : IStaffService
    {

        public ulong[] Engineers { get; private init; }
        public ulong[] Editors { get; private init; }
        public ulong[] Photographers { get; private init; }
        public ulong[] Approvers { get; private init; }

        private readonly DiscordSocketClient _client;
        private readonly ConcurrentDictionary<string, Broadcast> _broadcasts = new();

        public StaffService(DiscordSocketClient client, IConfiguration config)
        {
            var engineersStrings = config.GetSection("Engineers")?.GetChildren()?.Select(x => x.Value)?.ToArray();
            this.Engineers = engineersStrings.Select(s => ulong.Parse(s)).ToArray();
            var editorsStrings = config.GetSection("Editors")?.GetChildren()?.Select(x => x.Value)?.ToArray();
            this.Editors = editorsStrings.Select(s => ulong.Parse(s)).ToArray();
            var approversStrings = config.GetSection("Approvers")?.GetChildren()?.Select(x => x.Value)?.ToArray();
            this.Approvers = approversStrings.Select(s => ulong.Parse(s)).ToArray();
            var photographerStrings = config.GetSection("Photographers")?.GetChildren()?.Select(x => x.Value)?.ToArray();
            this.Photographers = photographerStrings.Select(s => ulong.Parse(s)).ToArray();

            this._client = client;
        }

        public bool IsEngineer(ulong userId) => this.Engineers.Contains(userId);
        public bool IsApprover(ulong userId) => this.Approvers.Contains(userId);
        public bool IsEditor(ulong userId) => this.Editors.Contains(userId);
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

