using System;
using System.Collections.Generic;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.VenueAuditing.MassAudit.Models;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit;

[Serializable]
public class MassAuditRecord : IEntity
{

    public string id { get; init; } = Guid.NewGuid().ToString().Replace("-", "").Substring(8, 8);
    public int TotalVenuesToAudit { get; set; }
    public MassAuditStatus Status { get; set; } = MassAuditStatus.Inactive;
    public DateTime? StartedAt { get; set; }
    public DateTime? PausedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ulong RequestedIn { get; set; }
    public ulong RequestedBy { get; set; }

    public List<VenueAuditLog> Logs { get; private set; } = new();

    public void SetStarted()
    {
        if (this.StartedAt == null) 
            this.StartedAt = DateTime.UtcNow;
        this.PausedAt = null;
        this.Status = MassAuditStatus.Active;
    }

    public void SetPaused()
    {
        this.Status = MassAuditStatus.Inactive;
        this.PausedAt = DateTime.UtcNow;
    }

    public void SetCompleted()
    {
        this.CompletedAt = DateTime.UtcNow;
        this.Status = MassAuditStatus.Complete;
    }

    public void SetCancelled()
    {
        this.CompletedAt = DateTime.UtcNow;
        this.Status = MassAuditStatus.Cancelled;
    }
    
    public void Log(string message) =>
        this.Logs.Add(new (DateTime.UtcNow, message));

}