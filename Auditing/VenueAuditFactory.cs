using System;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Configuration;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Auditing;

public interface IVenueAuditFactory
{
    VenueAudit CreateAuditFor(Venue venue, string roundId);
    VenueAudit CreateAuditFor(Venue venue, VenueAuditRecord record);

}

public class VenueAuditFactory : IVenueAuditFactory
{
    private readonly IDiscordClient _client;
    private readonly IVenueRenderer _venueRenderer;
    private readonly IRepository _repository;

    public VenueAuditFactory(IDiscordClient client, IVenueRenderer venueRenderer, IRepository repository)
    {
        this._client = client;
        this._venueRenderer = venueRenderer;
        this._repository = repository;
    }

    public VenueAudit CreateAuditFor(Venue venue, string roundId) =>
        new (venue, roundId, this._client, this._venueRenderer, this._repository);
    
    public VenueAudit CreateAuditFor(Venue venue, VenueAuditRecord record) =>
        new (venue, record, this._client, this._venueRenderer, this._repository);
}