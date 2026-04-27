using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.TagsEntry;

class GamesEntrySessionState : ISessionState
{
    private Venue _venue;

    private static List<(string Label, string Description, string Value)> _availableGames = new()
    {
        ("Triple triad", "The venue offers individual or competitive games of Triple Triad via the game interface itself.", "Triple triad"),
        ("Truth or dare", "The venue offers a game of Truth or Dare for whomever feels like joining. Sometimes in private alliance, sometimes venue wide.", "Truth or dare"),
        ("Blackjack", "The venue holds a table for players who wish to gamble gil in a game of 21.", "Blackjack"),
        ("Deathroll", "The venue offers individual or competitive game of Deathroll usually with prizes involved.", "Deathroll"),
        ("Texas holdem", "The venue holds a table for players who wish to gamble gil in a game of Texas holdem.", "Texas holdem"),
        ("Bingo", "The venue holds a venue wide bingo game, usually with prizes involved.", "Bingo"),
        ("Roulette", "The venue holds a table for players who wish to gamble gil in a game of Roulette.", "Roulette")
    };

    public Task Enter(VeniInteractionContext c)
    {
        this._venue = c.Session.GetVenue();

        var component = this.BuildTagsComponent(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskForGamesMessage, component.Build());
    }

    private ComponentBuilder BuildTagsComponent(VeniInteractionContext c)
    {
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
            .WithMaxValues(_availableGames.Count);
        foreach (var (label, description, value) in _availableGames)
            selectComponent.AddOption(label, value, isDefault: this._venue.Tags.Contains(value), description: description);

        return new ComponentBuilder()
            .WithSelectMenu(selectComponent)
            .WithBackButton(c)
            .WithSkipButton<WebsiteEntrySessionState, ConfirmVenueSessionState>(c);
    }

    private Task OnComplete(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Tags ??= new();
        venue.Tags.RemoveAll(existingTag => _availableGames.Any(availableTag => existingTag == availableTag.Value));
        venue.Tags.AddRange(c.Interaction.Data.Values);

        if (c.Session.InEditing())
            return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);

        return c.Session.MoveStateAsync<WebsiteEntrySessionState>(c);
    }

}