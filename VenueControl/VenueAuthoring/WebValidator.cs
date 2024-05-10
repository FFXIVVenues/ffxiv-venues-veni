using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring;

public interface ISiteValidator
{
    Task<SiteCheckResult> CheckUrlAsync(string url);
}

public class SiteValidator(HttpClient client) : ISiteValidator
{
    public async Task<SiteCheckResult> CheckUrlAsync(string url)
    {
        var response = await client.GetAsync(url);
        return response.IsSuccessStatusCode ? SiteCheckResult.Valid : SiteCheckResult.Invalid;
    }
}

public enum SiteCheckResult
{
    Valid,
    Invalid
}