using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring;

interface IDiscordValidator
{
    Task<(DiscordCheckResult, DiscordInvite)> CheckInviteAsync(string rawDiscordString);
}

public class DiscordValidator : IDiscordValidator
{
    
    static HttpClient _discordClient = new HttpClient();
    static Regex _discordPattern = new Regex(@"(https?:\/\/)?(www\.)?((discord(app)?(\.com|\.io)(\/invite)?)|(discord\.gg))\/(\w+)");

    public async Task<(DiscordCheckResult, DiscordInvite)> CheckInviteAsync(string rawDiscordString)
    {
        var match = _discordPattern.Match(rawDiscordString);
        if (!match.Success)
            return (DiscordCheckResult.BadFormat, null);

        var inviteCode = match.Groups[9].ToString();
        var responseMessage = await _discordClient.GetAsync($"https://discordapp.com/api/invite/{inviteCode}");

        if (!responseMessage.IsSuccessStatusCode)
            return (DiscordCheckResult.InvalidInvite, null);

        var response = await responseMessage.Content.ReadAsStreamAsync();
        var invite = await JsonSerializer.DeserializeAsync<DiscordInvite>(response);

        if (invite.expires_at != null)
            return (DiscordCheckResult.IsTemporaryInvite, null);

        return (DiscordCheckResult.Valid, invite);
    }
}


public class DiscordInvite
{
    public DateTime? expires_at { get; set; }

    public Guild guild { get; set; }
    public class Guild
    {
        public string id { get; set; }
    }

}

public enum DiscordCheckResult
{
    Valid,
    BadFormat,
    InvalidInvite,
    IsTemporaryInvite
}