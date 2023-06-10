namespace FFXIVVenues.Veni.VenueAuditing;

public enum VenueAuditStatus
{
    Pending,
    AwaitingResponse,
    RespondedEdit,
    RespondedConfirmed,
    RespondedDelete,
    RespondedClose,
    EditedLater,
    DeletedLater,
    ClosedLater,
    Skipped,
    Failed
}

public static class VenueAuditStatusExtensions
{
    public static bool IsResponded(this VenueAuditStatus status) =>
        status is VenueAuditStatus.RespondedClose 
               or VenueAuditStatus.RespondedConfirmed 
               or VenueAuditStatus.RespondedDelete 
               or VenueAuditStatus.RespondedEdit
               or VenueAuditStatus.EditedLater
               or VenueAuditStatus.DeletedLater
               or VenueAuditStatus.ClosedLater;
    
    public static bool IsRespondedByLateAction(this VenueAuditStatus status) =>
        status is VenueAuditStatus.EditedLater
               or VenueAuditStatus.DeletedLater
               or VenueAuditStatus.ClosedLater;
}