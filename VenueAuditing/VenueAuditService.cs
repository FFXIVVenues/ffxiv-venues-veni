using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueAuditing;

public class VenueAuditService : IVenueAuditService
{
    private readonly IDiscordClient _client;
    private readonly IVenueRenderer _venueRenderer;
    private readonly IRepository _repository;
    private readonly IApiService _apiService;

    public VenueAuditService(IDiscordClient client, IVenueRenderer venueRenderer, IRepository repository, IApiService apiService)
    {
        this._client = client;
        this._venueRenderer = venueRenderer;
        this._repository = repository;
        this._apiService = apiService;
    }

    public VenueAudit CreateAuditFor(Venue venue, string roundId, ulong requestedIn, ulong requestedBy) =>
        new (venue, roundId, requestedIn, requestedBy, this._client, this._venueRenderer, this._repository);
    
    public VenueAudit CreateAuditFor(Venue venue, VenueAuditRecord record) =>
        new (venue, record, this._client, this._venueRenderer, this._repository);

    public Task<VenueAuditRecord> GetAudit(string auditId) =>
        this._repository.GetByIdAsync<VenueAuditRecord>(auditId);
    
    public Task<VenueAuditRecord> GetLatestRecordFor(Venue venue) =>
        this.GetLatestRecordFor(venue.Id);

    public async Task<VenueAuditRecord> GetLatestRecordFor(string venueId)
    {
        var query = await this._repository.GetWhere<VenueAuditRecord>(a => a.VenueId == venueId);
        return query.OrderByDescending(a => a.SentTime).Take(1).ToList().FirstOrDefault();
    }

    public async Task UpdateAuditStatus(Venue venue, ulong actingUserId, VenueAuditStatus status) =>
        await UpdateAuditStatus(await GetLatestRecordFor(venue), venue, actingUserId, status);
    
    public async Task UpdateAuditStatus(VenueAuditRecord audit, ulong actingUserId, VenueAuditStatus status) =>
        await UpdateAuditStatus(audit, await this._apiService.GetVenueAsync(audit.VenueId), actingUserId, status);
    
    public async Task UpdateAuditStatus(VenueAuditRecord audit, Venue venue, ulong actingUserId, VenueAuditStatus status)
    {
        var actionLanguage = status switch
        {
            VenueAuditStatus.RespondedConfirmed => "confirmed the venue's details",
            VenueAuditStatus.RespondedEdit => "edited the venue details",
            VenueAuditStatus.EditedLater => "edited the venue details",
            VenueAuditStatus.RespondedClose => "temporarily closed the venue",
            VenueAuditStatus.ClosedLater => "temporarily closed the venue",
            VenueAuditStatus.RespondedDelete => "deleted the venue",
            VenueAuditStatus.DeletedLater => "deleted the venue",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, @"Only responsive statuses can be set")
        };

        await this.UpdateMessagesSentToManagers(audit, venue, actingUserId, $"You handled this and {actionLanguage}.",
            $"{MentionUtils.MentionUser(actingUserId)} handled this and {actionLanguage}.");
        
        if (audit.MassAuditId == null && audit.RequestedBy != actingUserId 
                                      && audit.Status is VenueAuditStatus.AwaitingResponse or VenueAuditStatus.Pending)
            await NotifyRequsterOfCompletion(audit, venue, $"{MentionUtils.MentionUser(actingUserId)} {actionLanguage}.");

        audit.Log($"{MentionUtils.MentionUser(actingUserId)} {actionLanguage}.");
        audit.Status = status;
        audit.CompletedBy = actingUserId;
        audit.CompletedAt = DateTime.UtcNow;
        
        await this._repository.UpsertAsync(audit);
    }

    private async Task UpdateMessagesSentToManagers(VenueAuditRecord audit, Venue venue, ulong actingUserId, 
        string responderMessage, string othersMessage)
    {
        foreach (var message in audit.Messages)
        {
            if (message.Status != MessageStatus.Sent) continue;
            var newMessage = responderMessage;
            if (message.UserId != actingUserId)
                newMessage = othersMessage;

            var channel = await this._client.GetChannelAsync(message.ChannelId);
            (channel as IDMChannel)?.ModifyMessageAsync(message.MessageId, props =>
            {
                props.Components = new ComponentBuilder().Build();
                props.Embeds = new[]
                {
                    this._venueRenderer.RenderEmbed(venue).Build(),
                    new EmbedBuilder().WithDescription(newMessage).Build()
                };
            });
        }
    }
    
    private async Task NotifyRequsterOfCompletion(VenueAuditRecord audit, Venue venue, string message)
    {
        var channel = await this._client.GetChannelAsync(audit.RequestedIn);
        if (channel == null)
            channel = await this._client.GetDMChannelAsync(audit.RequestedIn);

        if (channel is SocketTextChannel sTextChannel)
            await sTextChannel.SendMessageAsync(
                $"Hey {MentionUtils.MentionUser(audit.RequestedBy)}! The audit of **{venue.Name}** you requested has been completed. \n{message}");
        if (channel is RestTextChannel rTextChannel)
            await rTextChannel.SendMessageAsync(
                $"Hey {MentionUtils.MentionUser(audit.RequestedBy)}! The audit of **{venue.Name}** you requested has been completed. \n{message}");
        if (channel is RestDMChannel rdmChannel)
            await rdmChannel.SendMessageAsync(
                $"Heyo! The audit of **{venue.Name}** you requested has been completed. \n{message}");
        if (channel is SocketDMChannel sdmChannel)
            await sdmChannel.SendMessageAsync(
                $"Heyo! The audit of **{venue.Name}** you requested has been completed. \n{message}");
    }
}