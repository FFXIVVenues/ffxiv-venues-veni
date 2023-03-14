using System;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Configuration;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Auditing;

public interface IVenueAuditFactory
{
    VenueAudit CreateAuditFor(Venue venue);
}

public class VenueAuditFactory : IVenueAuditFactory
{
    private readonly IDiscordClient _client;
    private readonly UiConfiguration _uiConfig;
    private readonly ApiConfiguration _apiConfig;

    public VenueAuditFactory(IDiscordClient client, UiConfiguration uiConfig, ApiConfiguration apiConfig)
    {
        this._client = client;
        this._uiConfig = uiConfig;
        this._apiConfig = apiConfig;
    }

    public VenueAudit CreateAuditFor(Venue venue) =>
        new (venue, this._client, this._uiConfig, this._apiConfig);
}