using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Utils.Broadcasting;
using Microsoft.Extensions.Configuration;

namespace FFXIVVenues.Veni.People
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
            Engineers = engineersStrings.Select(s => ulong.Parse(s)).ToArray();
            var editorsStrings = config.GetSection("Editors")?.GetChildren()?.Select(x => x.Value)?.ToArray();
            Editors = editorsStrings.Select(s => ulong.Parse(s)).ToArray();
            var approversStrings = config.GetSection("Approvers")?.GetChildren()?.Select(x => x.Value)?.ToArray();
            Approvers = approversStrings.Select(s => ulong.Parse(s)).ToArray();
            var photographerStrings = config.GetSection("Photographers")?.GetChildren()?.Select(x => x.Value)?.ToArray();
            Photographers = photographerStrings.Select(s => ulong.Parse(s)).ToArray();

            _client = client;
        }

        public bool IsEngineer(ulong userId) => Engineers.Contains(userId);
        public bool IsApprover(ulong userId) => Approvers.Contains(userId);
        public bool IsEditor(ulong userId) => Editors.Contains(userId);
        public bool IsPhotographer(ulong userId) => Photographers.Contains(userId);

        public Broadcast Broadcast()
        {
            var broadcast = new Broadcast(Guid.NewGuid().ToString(), _client);
            _broadcasts[broadcast.Id] = broadcast;
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

