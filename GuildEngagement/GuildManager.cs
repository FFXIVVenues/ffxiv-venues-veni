﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.GuildEngagement
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

        /// <summary>
        /// For all guilds for which Veni is a member check for
        /// roles mapped to the given venue's Data Center and assign
        /// them to the venue's managers if they are a member of that
        /// guild. 
        /// </summary>
        /// <param name="venue">
        /// The venue with managers to which assign roles in all guilds.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> when a role has been 
        /// assigned to any manager in any guild.
        /// </returns>
        public async Task<bool> AssignRolesForVenueAsync(Venue venue)
        {
            var guilds = this.GetVenisGuilds();
            var guildIds = guilds.Select(guild => guild.Id.ToString());
            var guildSettings = await this._repository.GetWhereAsync<GuildSettings>(c => guildIds.Contains(c.id));
            var rolesAdded = false;
            foreach (var guildSetting in guildSettings)
            {
                if (!guildSetting.DataCenterRoleMap.TryGetValue(venue.Location.DataCenter, out var roleId))
                    continue;

                var guild = guilds.FirstOrDefault(g => g.Id == guildSetting.GuildId);
                if (guild == null) continue;

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

        /// <summary>
        /// For all of the venues managed by the given managers of the 
        /// give venue assign the relevant Venue Manager role in each
        /// guild veni is a member, and remove other Venue Manager roles. 
        /// </summary>
        /// <param name="venue">
        /// The venue with managers to which synchronise roles in all guilds.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> when a role has been 
        /// assigned to or removed from the guild user.
        /// </returns>
        public async Task<bool> SyncRolesForVenueAsync(Venue venue)
        {
            var guilds = this.GetVenisGuilds();
            var guildIds = guilds.Select(guild => guild.Id.ToString());
            var guildSettings = await this._repository.GetWhereAsync<GuildSettings>(c => guildIds.Contains(c.id));
            var namesFormatted = false;
            foreach (var guildSetting in guildSettings)
            {
                var guild = guilds.FirstOrDefault(g => g.Id == guildSetting.GuildId);
                if (guild == null) continue;

                foreach (var managerId in venue.Managers)
                {
                    var manager = await guild.GetUserAsync(ulong.Parse(managerId));
                    if (manager == null) continue;
                    await this.SyncRolesForGuildUserAsync(manager, guildSetting);
                }
            }
            return namesFormatted;
        }

        /// <summary>
        /// For all of the venues managed by the given guild user
        /// assign the relevant Venue Manager role for that guild, 
        /// and remove other Venue Manager roles. 
        /// </summary>
        /// <param name="user">
        /// The guild user to synchronise Venue Manager roles for.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> when a role has been 
        /// assigned to or removed from the guild user.
        /// </returns>
        public async Task<bool> SyncRolesForGuildUserAsync(IGuildUser user, GuildSettings guildSettings = null)
        {
            if (guildSettings == null)
                guildSettings = await this._repository.GetByIdAsync<GuildSettings>(user.GuildId.ToString());
            if (guildSettings == null) return false;

            if (!guildSettings.DataCenterRoleMap.Any())
                return false;

            var venues = await this._apiService.GetAllVenuesAsync(user.Id);
            var rolesToKeep = new HashSet<ulong>();
            var rolesToAdd = new HashSet<ulong>();
            foreach (var venue in venues)
                if (guildSettings.DataCenterRoleMap.TryGetValue(venue.Location.DataCenter, out var roleId))
                {
                    rolesToKeep.Add(roleId);
                    if (!user.RoleIds.Contains(roleId))
                        rolesToAdd.Add(roleId);
                }

            var rolesToRemove = guildSettings.DataCenterRoleMap.Values.Where(r => !rolesToKeep.Contains(r) && user.RoleIds.Contains(r)).ToHashSet();

            var changesMade = false;
            if (rolesToAdd.Any())
            {
                foreach (var role in rolesToAdd)
                    _ = user.AddRoleAsync(role);
                changesMade = true;
            }

            if (rolesToRemove.Any())
            {
                foreach (var role in rolesToRemove)
                    _ = user.RemoveRoleAsync(role);
                changesMade = true;
            }

            return changesMade;
        }

        /// <summary>
        /// For all guilds veni is a member, changes the Display Names
        /// of the managers of the given venue to format of 
        /// Name | Venue Name. Where the combination is too long, 
        /// the venue name is stripped of conjunctions, then 
        /// the display name is trim to one word, and then the
        /// venue name is trimmed until it will fit.
        /// </summary>
        /// <param name="user">
        /// The guild user to rename.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> when a Welcome has been sent. 
        /// </returns>
        public async Task<bool> FormatDisplayNamesForVenueAsync(Venue venue)
        {
            var guilds = this.GetVenisGuilds();
            var guildIds = guilds.Select(guild => guild.Id.ToString());
            var guildSettings = await this._repository.GetWhereAsync<GuildSettings>(c => guildIds.Contains(c.id));
            var namesFormatted = false;
            foreach (var guildSetting in guildSettings)
            {
                var guild = guilds.FirstOrDefault(g => g.Id == guildSetting.GuildId);
                if (guild == null) continue;

                foreach (var managerId in venue.Managers)
                {
                    var manager = await guild.GetUserAsync(ulong.Parse(managerId));
                    if (manager == null) continue;
                    await this.FormatDisplayNameForGuildUserAsync(manager);
                }
            }
            return namesFormatted;
        }

        /// <summary>
        /// Changes the Display Name of the given guild user
        /// to format of Name | Venue Name. Where the combination
        /// is too long, the venue name is stripped of conjunctions,
        /// then the display name is trim to one word, and then the
        /// venue name is trimmed until it will fit.
        /// 
        /// This will not reformat a Display name if it appears 
        /// that it already follows the format.
        /// </summary>
        /// <param name="user">
        /// The guild user to rename.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> when a Welcome has been sent. 
        /// </returns>
        public async Task<bool> FormatDisplayNameForGuildUserAsync(IGuildUser user, GuildSettings guildSettings = null)
        {
            if (user.DisplayName.Contains(" | "))
                return false;

            if (guildSettings == null) 
                guildSettings = await this._repository.GetByIdAsync<GuildSettings>(user.GuildId.ToString());
            if (guildSettings == null) return false;

            if (!guildSettings.FormatNames)
                return false;

            var venues = await this._apiService.GetAllVenuesAsync(user.Id);
            if (!venues.Any())
                return false;

            var venue = venues.OrderBy(v => v.Added).First();

            var displayName = user.DisplayName;
            var venueName = venue.Name;
            var result = user.DisplayName + " | " + venue.Name;

            var nameNext = true;
            var maxTrimmed = false;
            while (result.Length > 32)
            {
                var success = false;
                if (nameNext)
                    success = TrimOnce(displayName, out displayName);
                else
                    success = TrimOnce(venueName, out venueName);

                if (!success && maxTrimmed)
                {
                    break;
                }
                else if (!success)
                {
                    maxTrimmed = true;
                }

                nameNext = !nameNext;
                result = displayName + " | " + venueName;
            }

            await user.ModifyAsync(up => up.Nickname = result);
            
            return true;
        }

        /// <summary>
        /// Has Veni send a welcoming message at the given user
        /// via the guild's system channel. If not system channel
        /// exists for the guild, no Welcome message is sent.
        /// </summary>
        /// <param name="user">
        /// The guild user to send a Welcome message at.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> when a Welcome has been sent. 
        /// </returns>
        public async Task<bool> WelcomeGuildUserAsync(IGuildUser user)
        {
            var guildSettings = await this._repository.GetByIdAsync<GuildSettings>(user.GuildId.ToString());
            if (guildSettings == null) return false;

            if (!guildSettings.WelcomeJoiners)
                return false;

            var channel = await user.Guild.GetSystemChannelAsync();
            await channel.SendMessageAsync(MessageRepository.WelcomeMessages.PickRandom().Replace("{mention}", user.Mention));
            return true;
        }

        private IReadOnlyCollection<IGuild> GetVenisGuilds() =>
            this._guildsCache != null ? this._guildsCache : this._guildsCache = this._client.Guilds;

        private bool TrimOnce(string input, out string output)
        {
            var success = true;

            if (input.ToLower().StartsWith("the "))
            {
                output = input.Substring(4);
                return success;
            }

            if (input.ToLower().StartsWith("a "))
            {
                output = input.Substring(2);
                return success;
            }

            var nameParts = input.Split(' ');
            if (nameParts.Length > 1)
            {
                var deductParts = 1;
                while (true)
                {
                    input = string.Join(" ", nameParts[0..(nameParts.Length - deductParts)]);
                    if (input.EndsWith("and") || input.EndsWith("of") || input.EndsWith("&"))
                    {
                        deductParts++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                success = false;
            }
            output = input;
            return success;
        }

    }
}
