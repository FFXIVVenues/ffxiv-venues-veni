using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueClosing.SessionStates;

namespace FFXIVVenues.Veni.VenueControl.VenueOpening.SessionStates;

internal class OpenEntryState : ISessionState
{
    public Task EnterState(VeniInteractionContext interactionContext)
    {
        var component = this.BuildOpenComponent(interactionContext);
        return interactionContext.Interaction.RespondAsync(VenueControlStrings.AskIfOpeningIsNowOrLater, component.Build());
    }

    private ComponentBuilder BuildOpenComponent(VeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var isOpen = venue.Resolution?.IsNow ?? false;
        var isClosed = venue.ScheduleOverrides.Any(s => s.IsNow && s.Open is false);
        var hasFutureClosure = venue.ScheduleOverrides.Any(s => !s.Open && s.Start > DateTime.UtcNow);
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.RegisterComponentHandler(OnSelect, ComponentPersistence.ClearRow));

        if (isOpen)
            selectComponent
                .AddOption("End current opening", "EndOpening")
                .AddOption("Extend current opening", "Extend");
        else if (isClosed)
            selectComponent
                .AddOption("End current closure", "EndClosure")
                .AddOption("Open now", "Now");
        else
            selectComponent
                .AddOption("Open now", "Now");
                
        selectComponent
            .AddOption("Open later", "Later")
            .AddOption("Cancel future opening", "CancelOpening");
            
        if (hasFutureClosure)
            selectComponent.AddOption("Cancel later closure", "CancelClosure");
        
        return new ComponentBuilder().WithSelectMenu(selectComponent).WithBackButton(c);
    }

    private Task OnSelect(ComponentVeniInteractionContext c)
    {
        c.Session.ClearItem(SessionKeys.OPENING_DATE);
        c.Session.ClearItem(SessionKeys.OPENING_HOUR);
        var selection = c.Interaction.Data.Values.Single();
        return selection switch
        {
            "EndOpening" => c.MoveSessionToStateAsync<EndCurrentOpeningState>(),
            "EndClosure" => c.MoveSessionToStateAsync<EndCurrentClosureState>(),
            "CancelOpening" => c.MoveSessionToStateAsync<CancelOpeningState>(),
            "CancelClosure" => c.MoveSessionToStateAsync<CancelClosureState>(),
            "Extend" => c.MoveSessionToStateAsync<OpenHowLongWhenEntryState>(),
            "Now" => c.MoveSessionToStateAsync<OpenHowLongWhenEntryState>(),
            "Later" => c.MoveSessionToStateAsync<OpenTimeZoneEntryState>(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
}

