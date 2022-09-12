using Discord;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Models;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Managers
{
    internal interface IGuildManager
    {
        Task<bool> AssignRolesForVenueAsync(Venue venue);

        Task<bool> SyncRolesForVenueAsync(Venue venue);

        Task<bool> SyncRolesForGuildUserAsync(IGuildUser user, GuildSettings guildSettings = null);

        Task<bool> FormatDisplayNamesForVenueAsync(Venue venue);

        Task<bool> FormatDisplayNameForGuildUserAsync(IGuildUser user, GuildSettings guildSettings = null);
        
        Task<bool> WelcomeGuildUserAsync(IGuildUser user);
    }
}