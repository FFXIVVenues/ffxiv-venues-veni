namespace FFXIVVenues.Veni.Auditing;

public enum VenueAuditStatus
{
    Pending,
    AwaitingResponse,
    RespondedEdit,
    RespondedConfirmed,
    RespondedDelete,
    Skipped
}