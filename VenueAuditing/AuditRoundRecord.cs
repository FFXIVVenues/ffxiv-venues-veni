using System;
using System.Collections.Generic;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;

namespace FFXIVVenues.Veni.VenueAuditing;

public class AuditRoundRecord : IEntity
{

    public string id { get; } = DateTime.Now.ToString("yyyyMMddHHmm");
    public AuditStatus Status { get; private set; } = AuditStatus.Inactive;
    public DateTime? StartedAt { get; private set; }
    public DateTime? PausedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public ulong RequestedIn { get; set; }
    public ulong RequestedBy { get; set; }
    
    public List<VenueAuditLog> Logs { get; private set; }

    public void SetStarted()
    {
        if (this.StartedAt == null) 
            this.StartedAt = DateTime.UtcNow;
        this.PausedAt = null;
        this.Status = AuditStatus.Active;
    }

    public void SetPaused()
    {
        this.Status = AuditStatus.Inactive;
        this.PausedAt = DateTime.UtcNow;
    }

    public void SetCompleted()
    {
        this.CompletedAt = DateTime.UtcNow;
        this.Status = AuditStatus.Complete;
    }

    public void Log(string message) =>
        this.Logs.Add(new (DateTime.UtcNow, message));

}