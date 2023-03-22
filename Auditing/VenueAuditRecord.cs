using System;
using System.Collections.Generic;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils.Broadcasting;

namespace FFXIVVenues.Veni.Auditing;

public class VenueAuditRecord : IEntity
{
    public string id => VenueId + DateTime.Now.ToString("yyyyMMddHHmm");
    public string VenueId { get; init; }
    public string RoundId { get; init; }
    public VenueAuditStatus Status { get; set; }
    public DateTime SentTime { get; set; } = DateTime.UtcNow;
    public DateTime ResolutionTime { get; set; }
    public List<AuditMessage> SentMessages { get; set; }
    public List<VenueAuditLog> Logs = new();

    public void Log(string message) =>
        this.Logs.Add(new VenueAuditLog(DateTime.UtcNow, message));
}

public record AuditMessage(ulong UserId,  ulong ChannelId, ulong MessageId, MessageStatus Status, string Log);