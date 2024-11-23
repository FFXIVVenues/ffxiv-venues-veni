﻿using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueClosing.SessionStates;

internal class CloseTimeEntryState : ISessionState
{
    public Task EnterState(VeniInteractionContext interactionContext)
    {
        var component = this.BuildCloseComponent(interactionContext).WithBackButton(interactionContext);
        return interactionContext.Interaction.RespondAsync(VenueControlStrings.AskForTimeOfClosing, component.Build()); //change text later
    }

    private ComponentBuilder BuildCloseComponent(VeniInteractionContext c)
    {
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.RegisterComponentHandler(OnSelect, ComponentPersistence.ClearRow));
        for (var i = 0; i < 24; i++)
            selectComponent.AddOption($"{i % 12}:00{(i > 12 ? "pm" : "am")}", i.ToString());
        return new ComponentBuilder().WithSelectMenu(selectComponent);
    }

    private Task OnSelect(ComponentVeniInteractionContext c)
    {
        var hourSelection = int.Parse(c.Interaction.Data.Values.Single()); 
        c.Session.SetItem(SessionKeys.CLOSING_HOUR, hourSelection);
        return c.MoveSessionToStateAsync<CloseHowLongWhenEntryState>();
    }
}
