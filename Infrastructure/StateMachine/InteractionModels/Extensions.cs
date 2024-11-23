using System;
using System.Linq;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.StateMachine.Models;
using Stateless;

namespace FFXIVVenues.Veni.Infrastructure.StateMachine.InteractionModels;

public static class Extensions
{
    public static InteractionWrap Wrap(this SocketMessage message) => new InteractionWrap(message);
    public static InteractionWrap Wrap(this SocketMessageComponent component) => new InteractionWrap(component);
    public static InteractionWrap Wrap(this SocketSlashCommand command) => new InteractionWrap(command);

    public static StateMachine<VenueAuthoringState, VenueAuthoringTrigger>.StateConfiguration
        OnEntry(this StateMachine<VenueAuthoringState, VenueAuthoringTrigger>.StateConfiguration config, 
            Action<InteractionWrap, StateMachine<VenueAuthoringState, VenueAuthoringTrigger>.Transition> @delegate)
    {
        config.OnEntry(transition =>
        {
            var interaction = transition.Parameters?.FirstOrDefault(p => p is InteractionWrap) as InteractionWrap;
            @delegate(interaction, transition);
        });
        return config;
    }
}
