using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueAuditing.ComponentHandlers;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueAuditing;

public class VenueAudit
{
    private const int MIN_DAYS_SINCE_LAST_UPDATE = 52; // 6 weeks
    
    private readonly VenueAuditRecord _record;
    private readonly Venue _venue;
    private readonly IDiscordClient _discordClient;
    private readonly IVenueRenderer _venueRenderer;
    private readonly IRepository _repository;

    public VenueAudit(Venue venue, string roundId, ulong requestedIn, ulong requestedBy, IDiscordClient discordClient,
        IVenueRenderer venueRenderer, IRepository repository) :
        this(venue,
            record: new()
                { VenueId = venue.Id, RoundId = roundId, RequestedIn = requestedIn, RequestedBy = requestedBy }, 
            discordClient, venueRenderer, repository) { }

    public VenueAudit(Venue venue,
        VenueAuditRecord record,
        IDiscordClient discordClient,
        IVenueRenderer venueRenderer,
        IRepository repository)
    {
        this._venue = venue;
        this._record = record;
        this._discordClient = discordClient;
        this._venueRenderer = venueRenderer;
        this._repository = repository;
    }

    public async Task<VenueAuditStatus> AuditAsync(bool doNotSkip = false)
    {
        this._record.Log($"Venue audit requested by {MentionUtils.MentionUser(this._record.RequestedBy)}.");
        if (this._record.RoundId != null)
            this._record.Log($"Venue audit requested as part of audit round {this._record.RoundId}.");
        
        if (!doNotSkip && !this.ShouldBeAudited())
        {
            this._record.Log("Venue audit skipped; it should not be audited.");
            this._record.Status = VenueAuditStatus.Skipped;
            await this._repository.UpsertAsync(this._record);
            return VenueAuditStatus.Skipped;
        }
        
        this._record.Log($"Sending venue audit message to {this._venue.Managers.Count} managers.");

        var broadcast = new Broadcast(Guid.NewGuid().ToString(), this._discordClient)
            .WithMessage(AuditStrings.Prompt)
            .WithEmbed(this._venueRenderer.RenderEmbed(this._venue))
            .WithComponent(ctx => new ComponentBuilder()
                .WithSelectMenu(new SelectMenuBuilder()
                    .WithValueHandlers()
                    .WithPlaceholder("Select response")
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Confirm Correct")
                        .WithEmote(new Emoji("👍"))
                        .WithDescription("Confirm the details on this venue are correct.")
                        .WithStaticHandler(ConfirmCorrectHandler.Key, this._record.id))
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Edit Venue")
                        .WithEmote(new Emoji("✏️"))
                        .WithDescription("Update the details on this venue.")
                        .WithStaticHandler(EditVenueHandler.Key, this._record.id))
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Temporarily Close")
                        .WithEmote(new Emoji("🔒"))
                        .WithDescription("Put this venue on a hiatus for up to 6 months.")
                        .WithStaticHandler(TemporarilyClosedHandler.Key, this._record.id))
                    .AddOption(new SelectMenuOptionBuilder()
                        .WithLabel("Permanently Close / Delete")
                        .WithEmote(new Emoji("❌"))
                        .WithDescription("Delete this venue completely.")
                        .WithStaticHandler(PermanentlyClosedHandler.Key, this._record.id))));
        var broadcastMessages = await broadcast.SendToAsync(this._venue.Managers.Select(ulong.Parse).ToArray());

        this._record.Log($"Sent venue audit message to {broadcastMessages.Count(m => m.Status == MessageStatus.Sent)} of {this._venue.Managers.Count} managers.");
        this._record.Status = VenueAuditStatus.AwaitingResponse;
        this._record.Messages = broadcastMessages.Select(m => 
            new AuditMessage(m.UserId, m.Message?.Channel?.Id ?? 0, m.Message?.Id ?? 0, m.Status, m.Log)).ToList();
        await this._repository.UpsertAsync(this._record);
        return VenueAuditStatus.AwaitingResponse;
    }

    private bool ShouldBeAudited()
    {
        var boundaryDate = DateTime.UtcNow.AddDays(-MIN_DAYS_SINCE_LAST_UPDATE);
        
        var venueLastChangedAt = this._venue.LastModified;
        var venueCreatedAt = this._venue.Added;
        var venueLastAuditedAt = this._venue.LastAudited;

        if (venueCreatedAt > boundaryDate)
        {
            this._record.Log($"Should not be audited; venue created within the last {MIN_DAYS_SINCE_LAST_UPDATE} days.");
            return false;
        }
        
        if (venueLastChangedAt > boundaryDate)
        {
            this._record.Log($"Should not be audited; venue updated within the last {MIN_DAYS_SINCE_LAST_UPDATE} days.");
            return false;
        }
        
        if (venueLastAuditedAt > boundaryDate)
        {
            this._record.Log($"Should not be audited; venue audited within the last {MIN_DAYS_SINCE_LAST_UPDATE} days.");
            return false;
        }

        return true;
    }

}

public record VenueAuditLog(DateTime Date, string Message);