namespace FFXIVVenues.Veni.Infrastructure.StateMachine.Models;

public enum VenueAuthoringState
{
    None,
    TakingName,
    TakingDescription,
    TakingSfwStatus,
    TakingCategory,
    ConfirmingVenue
}