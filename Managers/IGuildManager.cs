using Discord;
using FFXIVVenues.Veni.Api.Models;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Managers
{
    internal interface IGuildManager
    {
        Task<bool> AssignRolesInAllGuildsAsync(Venue venue);

        Task<bool> AssignRolesToGuildUser(IGuildUser user);

        Task<bool> WelcomeGuildUser(IGuildUser user);
    }
}