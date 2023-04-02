using Discord;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueAuditing;

public interface IVenueAuditFactory
{
    VenueAudit CreateAuditFor(Venue venue, string roundId, ulong requestedIn, ulong requestedBy);
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

    public VenueAudit CreateAuditFor(Venue venue, string roundId, ulong requestedIn, ulong requestedBy) =>
        new (venue, roundId, requestedIn, requestedBy, this._client, this._venueRenderer, this._repository);
    
    public VenueAudit CreateAuditFor(Venue venue, VenueAuditRecord record) =>
        new (venue, record, this._client, this._venueRenderer, this._repository);
}