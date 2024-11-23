using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.StateMachine.InteractionModels;
using FFXIVVenues.Veni.Infrastructure.StateMachine.Models;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.VenueModels;
using Stateless;

namespace FFXIVVenues.Veni.Infrastructure.StateMachine;

public partial class VenueAuthoringStateMachine : StateMachine<VenueAuthoringState, VenueAuthoringTrigger>
{

    public VenueAuthoringStateMachine(IDiscordClient client, Venue venue, 
        VenueAuthoringPhase phase = VenueAuthoringPhase.FullCreation, 
        VenueAuthoringState initialState = VenueAuthoringState.TakingName)
        : base(initialState, FiringMode.Queued)
    {
        Venue = venue;
        Phase = phase; 
        _client = client;
        
        Configure();
        this.Activate();
    }
    
    public Venue Venue { get; private set; }
    public VenueAuthoringPhase Phase { get; private set; }
    
    public TriggerWithParameters<InteractionWrap> MoveBack;
    public TriggerWithParameters<InteractionWrap> MoveNext;
    
    public TriggerWithParameters<InteractionWrap> TakeInteractionTrigger { get; private set; }
    public TriggerWithParameters<SocketMessage> TakeMessageTrigger { get; private set; }
    public TriggerWithParameters<SocketMessageComponent> TakeComponentTrigger { get; private set; }
    public TriggerWithParameters<SocketSlashCommand> TakeCommandTrigger { get; private set; }

    private readonly IDiscordClient _client;

    private void Configure()
    {
        MoveNext = this.SetTriggerParameters<InteractionWrap>(VenueAuthoringTrigger.MoveNext);

        TakeMessageTrigger = this.SetTriggerParameters<SocketMessage>(VenueAuthoringTrigger.TakeMessage);
        TakeComponentTrigger = this.SetTriggerParameters<SocketMessageComponent>(VenueAuthoringTrigger.TakeComponent);
        TakeCommandTrigger = this.SetTriggerParameters<SocketSlashCommand>(VenueAuthoringTrigger.TakeSlashCommand);
        
        this.ConfigureNameEntry();
        this.ConfigureNsfwEntry();
    }
}
