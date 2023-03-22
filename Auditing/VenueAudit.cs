using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Auditing.ComponentHandlers;
using FFXIVVenues.Veni.Configuration;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Auditing;

public class VenueAudit
{
    private const int MIN_DAYS_SINCE_LAST_UPDATE = 52; // 6 weeks
    
    private readonly VenueAuditRecord _record;
    private readonly Venue _venue;
    private readonly IDiscordClient _discordClient;
    private readonly IVenueRenderer _venueRenderer;
    private readonly IRepository _repository;

    public VenueAudit(Venue venue, string roundId, IDiscordClient discordClient,
        IVenueRenderer venueRenderer, IRepository repository) :
        this(venue, record: new() { VenueId = venue.Id, RoundId = roundId }, 
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
        if (!doNotSkip && !this.ShouldBeAudited())
        {
            this._record.Log("Venue audit skipped; it should not be audited.");
            this._record.Status = VenueAuditStatus.Skipped;
            await this._repository.UpsertAsync(this._record);
            return VenueAuditStatus.Skipped;
        }

        var broadcast = new Broadcast(Guid.NewGuid().ToString(), this._discordClient)
            .WithMessage(AuditStrings.Prompt)
            .WithEmbed(this._venueRenderer.RenderEmbed(this._venue))
            .WithComponent(ctx => new ComponentBuilder()
                .WithButton(new ButtonBuilder()
                    .WithLabel("Confirm Correct")
                    .WithStyle(ButtonStyle.Success)
                    .WithStaticHandler(ConfirmCorrectHandler.Key, this._record.id))
                .WithButton(new ButtonBuilder()
                    .WithLabel("Edit Venue")
                    .WithStyle(ButtonStyle.Secondary)
                    .WithStaticHandler(EditVenueHandler.Key, this._record.id))
                .WithButton(new ButtonBuilder()
                    .WithLabel("Temporarily Close")
                    .WithStyle(ButtonStyle.Secondary)
                    .WithStaticHandler(TemporarilyClosedHandler.Key, this._record.id))
                .WithButton(new ButtonBuilder()
                    .WithLabel("Permanently Close (Delete)")
                    .WithStyle(ButtonStyle.Danger)
                    .WithStaticHandler(PermanentlyClosedHandler.Key, this._record.id)));
        var broadcastedMessages = await broadcast.SendToAsync(this._venue.Managers.Select(ulong.Parse).ToArray());

        this._record.Status = VenueAuditStatus.AwaitingResponse;
        this._record.SentMessages = broadcastedMessages.Select(m => 
            new AuditMessage(m.UserId, m.Message.Channel.Id, m.Message.Id, m.Status, m.Log)).ToList();
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

public record VenueAuditLog(DateTime date, string message);
