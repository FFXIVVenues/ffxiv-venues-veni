using System.Collections.Generic;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Infrastructure.Tasks.Notice;

public class NoticeMemento : Memento
{
    public required string MassAuditId { get; init; }
    public required string Message { get; init; }
    public required Dictionary<string, Venue> NoticeTargets { get; init; }
    public Dictionary<string, NoticeStatus> NoticeProgress { get; init; }
}

public enum NoticeStatus
{  
    Complete = 0,
    Failed = 1
}
