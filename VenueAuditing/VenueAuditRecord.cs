using System;
using System.Collections.Generic;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils.Broadcasting;

namespace FFXIVVenues.Veni.VenueAuditing;

public class VenueAuditRecord : IEntity
{
    public string id { get; init; }
    public string VenueId { get; init; }
    public string RoundId { get; init; }
    public VenueAuditStatus Status { get; set; }
    public DateTime SentTime { get; set; } = DateTime.UtcNow;
    public ulong RequestedIn { get; set; }
    public ulong RequestedBy { get; set; }
    public ulong CompletedBy { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<AuditMessage> Messages { get; set; }
    public List<VenueAuditLog> Logs = new();

    public void Log(string message) =>
        this.Logs.Add(new VenueAuditLog(DateTime.UtcNow, message));

    public VenueAuditRecord() =>
        this.id = this.VenueId + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
    
}

public record AuditMessage(ulong UserId,  ulong ChannelId, ulong MessageId, MessageStatus Status, string Log);