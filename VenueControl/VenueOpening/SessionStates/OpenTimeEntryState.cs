using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;
using SixLabors.ImageSharp.Formats.Tiff;

namespace FFXIVVenues.Veni.VenueControl.VenueOpening.SessionStates;

internal class OpenTimeEntryState : ISessionState
{
    public Task EnterState(VeniInteractionContext interactionContext)
    {
        var component = this.BuildOpenComponent(interactionContext).WithBackButton(interactionContext);
        return interactionContext.Interaction.RespondAsync(VenueControlStrings.AskForTimeOfOpening, component.Build()); //change text later
    }

    private ComponentBuilder BuildOpenComponent(VeniInteractionContext c)
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
        c.Session.SetItem(SessionKeys.OPENING_HOUR, hourSelection);
        return c.MoveSessionToStateAsync<OpenHowLongWhenEntryState>();
    }
}
