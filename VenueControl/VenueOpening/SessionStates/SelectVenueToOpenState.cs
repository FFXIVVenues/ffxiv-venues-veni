﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueOpening.SessionStates;

class SelectVenueToOpenSessionState : ISessionState
{

    private static string[] _messages = new[]
    {
        "Wooo! Which one are we opening?",
        "Yaay! 🥳 Which venue are we opening?",
        "🎉 Which one?"
    };

    private IEnumerable<Venue> _managersVenues;

    public Task EnterState(VeniInteractionContext interactionContext)
    {
        this._managersVenues = interactionContext.Session.GetItem<IEnumerable<Venue>>(SessionKeys.VENUES);

        var selectMenuKey = interactionContext.RegisterComponentHandler(this.Handle, ComponentPersistence.ClearRow);
        var componentBuilder = new ComponentBuilder();
        var selectMenuBuilder = new SelectMenuBuilder() { CustomId = selectMenuKey };
        foreach (var venue in _managersVenues.OrderBy(v => v.Name))
        {
            var selectMenuOption = new SelectMenuOptionBuilder
            {
                Label = venue.Name,
                Description = venue.Location.ToString(),
                Value = venue.Id
            };
            selectMenuBuilder.AddOption(selectMenuOption);
        }
        componentBuilder.WithSelectMenu(selectMenuBuilder);
        return interactionContext.Interaction.RespondAsync(_messages.PickRandom(), componentBuilder.Build());
    }

    public async Task Handle(ComponentVeniInteractionContext c)
    {
        var selectedVenueId = c.Interaction.Data.Values.Single();
        var venue = _managersVenues.FirstOrDefault(v => v.Id == selectedVenueId);

        _ = c.ClearSessionAsync();

        c.Session.SetVenue(venue);
        await c.MoveSessionToStateAsync<OpenEntryState>();

    }
}