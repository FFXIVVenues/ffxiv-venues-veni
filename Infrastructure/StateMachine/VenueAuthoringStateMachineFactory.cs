using Discord;
using FFXIVVenues.Veni.Infrastructure.StateMachine.Models;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Infrastructure.StateMachine;

public class VenueAuthoringStateMachineFactory(IDiscordClient client)
{
    public VenueAuthoringStateMachine Create(Venue venue, VenueAuthoringPhase phase = VenueAuthoringPhase.FullCreation, VenueAuthoringState initialState = VenueAuthoringState.TakingName)
    {
        return new VenueAuthoringStateMachine(client, venue, phase, initialState);
    }
}