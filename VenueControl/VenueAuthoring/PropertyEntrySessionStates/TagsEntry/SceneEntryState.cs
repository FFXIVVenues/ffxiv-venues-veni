using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.TagsEntry;

class SceneEntrySessionState : ISessionState
{

    private Venue _venue;
    private static List<(string Label, string Value)> _availableScenes = new()
    {
        ("Nightclub", "Nightclub"),
        ("Den", "Den"),
        ("Cafe", "Cafe"),
        ("Tavern", "Tavern"),
        ("Inn", "Inn"),
        ("Lounge", "Lounge"),
        ("Bath house", "Bath house"),
        ("Restaurant", "Restaurant"),
        ("Fightclub", "Fightclub"),
        ("Casino", "Casino"),
        ("Shop", "Shop"),
        ("Maid cafe / Host club", "Maid cafe"),
        ("Other", "Other")
    };

    public Task Enter(VeniInteractionContext c)
    {
        this._venue = c.Session.GetVenue(); 

        var component = this.BuildTagsComponent(c).WithBackButton(c).WithSkipButton<FeaturesEntrySessionState, FeaturesEntrySessionState>(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskForScenesMessage, component.Build());
    }

    private ComponentBuilder BuildTagsComponent(VeniInteractionContext c)
    {
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
            .WithMaxValues(Math.Max(2, _availableScenes.Count(t => this._venue.Tags?.Contains(t.Value) ?? false)));
        foreach (var (label, value) in _availableScenes)
            selectComponent.AddOption(label, value, isDefault: this._venue.Tags.Contains(value));

        return new ComponentBuilder()
            .WithSelectMenu(selectComponent);
    }

    private Task OnComplete(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Tags ??= new();
        venue.Tags.RemoveAll(existingTag => _availableScenes.Any(availableTag => existingTag == availableTag.Value));
        venue.Tags.AddRange(c.Interaction.Data.Values);

        return c.Session.MoveStateAsync<FeaturesEntrySessionState>(c);
    }

}