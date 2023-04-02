namespace FFXIVVenues.Veni.VenueAuditing;

public enum VenueAuditStatus
{
    Pending,
    AwaitingResponse,
    RespondedEdit,
    RespondedConfirmed,
    RespondedDelete,
    RespondedClose,
    Skipped
}