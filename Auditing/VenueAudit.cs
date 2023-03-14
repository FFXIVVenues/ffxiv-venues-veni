using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Auditing.ComponentHandlers;
using FFXIVVenues.Veni.Configuration;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Auditing;

public record VenueAudit
{
    private const int MIN_DAYS_SINCE_LAST_UPDATE = 52; // 6 weeks

    public string VenueId => this._venue.Id;
    public VenueAuditStatus Status { get; private set;  }

    private readonly List<VenueAuditLog> _log = new();
    private readonly Venue _venue;
    private readonly IDiscordClient _discordClient;
    private readonly string _uiUrl;
    private readonly string _apiUrl;

    public VenueAudit(Venue venue, IDiscordClient discordClient, UiConfiguration uiConfig, ApiConfiguration apiConfig)
    {
        this._venue = venue;
        this._discordClient = discordClient;
        this._uiUrl = uiConfig.BaseUrl;
        this._apiUrl = apiConfig.BaseUrl;
    }

    public async Task<AuditStatus> AuditAsync(bool doNotSkip = false)
    {
        if (!doNotSkip && !this.ShouldBeAudited())
        {
            this.Log("Venue audit skipped; it should not be audited.");
            return AuditStatus.Skipped;
        }

#pragma warning disable CS4014
        await new Broadcast(Guid.NewGuid().ToString(), this._discordClient)
            .WithMessage(AuditStrings.Prompt)
            .WithEmbed(this._venue.ToEmbed($"{this._uiUrl}/#{_venue.Id}", $"{this._apiUrl}/venue/{_venue.Id}/media"))
            .WithComponent(ctx => new ComponentBuilder()
                .WithButton(new ButtonBuilder()
                    .WithLabel("Confirm Correct")
                    .WithStyle(ButtonStyle.Success)
                    .WithStaticHandler(ConfirmCorrectHandler.Key))
                .WithButton(new ButtonBuilder()
                    .WithLabel("Edit Venue")
                    .WithStyle(ButtonStyle.Secondary)
                    .WithStaticHandler(EditVenueHandler.Key))
                .WithButton(new ButtonBuilder()
                    .WithLabel("Temporarily Close")
                    .WithStyle(ButtonStyle.Secondary)
                    .WithStaticHandler(TemporarilyClosedHandler.Key))
                .WithButton(new ButtonBuilder()
                    .WithLabel("Permanently Close (Delete)")
                    .WithStyle(ButtonStyle.Danger)
                    .WithStaticHandler(PermanentlyClosedHandler.Key)))
            .SendToAsync(this._venue.Managers.Select(ulong.Parse).ToArray());
#pragma warning restore CS4014

        return AuditStatus.Active;
    }

    private bool ShouldBeAudited()
    {
        var boundaryDate = DateTime.UtcNow.AddDays(-MIN_DAYS_SINCE_LAST_UPDATE);
        
        var venueLastChangedAt = this._venue.LastModified;
        var venueCreatedAt = this._venue.Added;
        var venueLastAuditedAt = this._venue.LastAudited;

        if (venueCreatedAt > boundaryDate)
        {
            this.Log($"Should not be audited; venue created within the last {MIN_DAYS_SINCE_LAST_UPDATE} days.");
            return false;
        }
        
        if (venueLastChangedAt > boundaryDate)
        {
            this.Log($"Should not be audited; venue updated within the last {MIN_DAYS_SINCE_LAST_UPDATE} days.");
            return false;
        }
        
        if (venueLastAuditedAt > boundaryDate)
        {
            this.Log($"Should not be audited; venue audited within the last {MIN_DAYS_SINCE_LAST_UPDATE} days.");
            return false;
        }

        return true;
    }

    private void Log(string message) =>
        this._log.Add(new VenueAuditLog(DateTime.UtcNow, message));
    
}

public record VenueAuditLog(DateTime date, string message);
