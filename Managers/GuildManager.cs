using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.Persistance.Abstraction;
using FFXIVVenues.Veni.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Managers
{
    internal class GuildManager : IGuildManager
    {
        private readonly DiscordSocketClient _client;
        private readonly IRepository _repository;
        private readonly IApiService _apiService;
        private IReadOnlyCollection<IGuild> _guildsCache;

        public GuildManager(DiscordSocketClient client, IRepository repository, IApiService apiService)
        {
            this._client = client;
            this._repository = repository;
            this._apiService = apiService;
        }

        public async Task<bool> AssignRolesInAllGuildsAsync(Venue venue)
        {
            var guilds = await GetVenisGuildsAsync();
            var guildIds = guilds.Select(guild => guild.Id.ToString());
            var guildSettings = await this._repository.GetWhere<GuildSettings>(c => guildIds.Contains(c.id));
            var rolesAdded = false;
            foreach (var guildSetting in guildSettings)
            {
                if (!guildSetting.DataCenterRoleMap.TryGetValue(venue.Location.DataCenter, out var roleId))
                    continue;

                var guild = guilds.FirstOrDefault(g => g.Id == guildSetting.GuildId);
                var role = guild.GetRole(roleId);

                foreach (var managerId in venue.Managers)
                {
                    var manager = await guild.GetUserAsync(ulong.Parse(managerId));
                    if (manager == null) continue;
                    if (!manager.RoleIds.Contains(roleId))
                    {
                        await manager.AddRoleAsync(roleId);
                        rolesAdded = true;
                    }
                }
            }
            return rolesAdded;
        }

        public async Task<bool> AssignRolesToGuildUser(IGuildUser user)
        {
            var guildSettings = await this._repository.GetByIdAsync<GuildSettings>(user.GuildId.ToString());
            if (guildSettings == null) return false;
            if (!guildSettings.DataCenterRoleMap.Any())
                return false;

            var venues = await this._apiService.GetAllVenuesAsync(user.Id);
            var rolesToAdd = new HashSet<ulong>();
            foreach (var venue in venues)
                if (guildSettings.DataCenterRoleMap.TryGetValue(venue.Location.DataCenter, out var roleId))
                    if (!user.RoleIds.Contains(roleId))
                        rolesToAdd.Add(roleId);

            if (rolesToAdd.Any())
            {
                foreach (var role in rolesToAdd)
                    _ = user.AddRoleAsync(role);
                return true;
            }
            return false;
        }

        public async Task<bool> WelcomeGuildUser(IGuildUser user)
        {
            var guildSettings = await this._repository.GetByIdAsync<GuildSettings>(user.GuildId.ToString());
            if (guildSettings == null) return false;

            if (!guildSettings.WelcomeJoiners)
                return false;

            var channel = await user.Guild.GetSystemChannelAsync();
            await channel.SendMessageAsync(MessageRepository.WelcomeMessages.PickRandom().Replace("{mention}", user.Mention));
            return true;
        }

        private async Task<IReadOnlyCollection<IGuild>> GetVenisGuildsAsync() =>
            this._guildsCache != null ? this._guildsCache : this._guildsCache = this._client.Guilds;

    }
}
