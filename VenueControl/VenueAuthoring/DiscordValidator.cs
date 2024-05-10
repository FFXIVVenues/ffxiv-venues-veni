using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring;

public interface IDiscordValidator
{
    Task<(DiscordCheckResult Result, RestInviteMetadata Invite)> CheckInviteAsync(string rawDiscordString);
}

public class DiscordValidator(DiscordSocketClient client) : IDiscordValidator
{
    
    static Regex _discordPattern = new Regex(@"(https?:\/\/)?(www\.)?((discord(app)?(\.com|\.io)(\/invite)?)|(discord\.gg))\/([\w-]+)");
    
    public async Task<(DiscordCheckResult, RestInviteMetadata)> CheckInviteAsync(string rawDiscordString)
    {
        
        var match = _discordPattern.Match(rawDiscordString);
        if (!match.Success)
            return (DiscordCheckResult.BadFormat, null);

        var inviteCode = match.Groups[9].ToString();

        var invite = await client.GetInviteAsync(inviteCode, new RequestOptions());
        if (invite is null)
            return (DiscordCheckResult.InvalidInvite, null);

        if (invite.ExpiresAt is not null)
            return (DiscordCheckResult.Expires, invite);
        
        return (DiscordCheckResult.Valid, invite);
    }
}

public enum DiscordCheckResult
{
    Valid,
    BadFormat,
    InvalidInvite,
    Expires
}