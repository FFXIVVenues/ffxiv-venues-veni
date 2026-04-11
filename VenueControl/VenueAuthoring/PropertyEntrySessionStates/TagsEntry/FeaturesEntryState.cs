using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.TagsEntry;

class FeaturesEntrySessionState : ISessionState
{
    private Venue _venue;

    private static List<(string Label, string Value)> _availableTags = new()
    {
        ("Courtesans", "Courtesans"),
        ("Gambling", "Gambling"),
        ("Artists", "Artists"),
        ("Dancers", "Dancers"),
        ("Bards", "Bards"),
        ("Food", "Food"),
        ("Drink", "Drink"),
        ("Twitch DJ", "Twitch DJ"),
        ("Sync DJ", "Sync DJ"),
        ("Bar", "Bar"),
        ("Tarot", "Tarot"),
        ("Pillow talk", "Pillow"),
        ("Photography", "Photography"),
        ("Open stage", "Open stage"),
        ("Void", "Void"),
        ("Stylists", "Stylists"),
        ("Novel performances", "Performances"),
        ("VIP available", "VIP"),
        ("LGBTQIA+ focused", "LGBTQIA+"),
        ("RP encouraged", "RP Heavy"),
        ("Strictly in-character RP only", "IC RP Only")
    };

    public Task Enter(VeniInteractionContext c)
    {
        this._venue = c.Session.GetVenue();

        var component = this.BuildTagsComponent(c);
        return c.Interaction.RespondAsync(VenueControlStrings.AskForFeaturesMessage, component.Build());
    }

    private ComponentBuilder BuildTagsComponent(VeniInteractionContext c)
    {
        var selectComponent = new SelectMenuBuilder()
            .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
            .WithMaxValues(_availableTags.Count);
        foreach (var (label, value) in _availableTags)
            selectComponent.AddOption(label, value, isDefault: this._venue.Tags.Contains(value));

        return new ComponentBuilder()
            .WithSelectMenu(selectComponent)
            .WithBackButton(c)
            .WithSkipButton<WebsiteEntrySessionState, ConfirmVenueSessionState>(c);
    }

    private Task OnComplete(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Tags ??= new();
        venue.Tags.RemoveAll(existingTag => _availableTags.Any(availableTag => existingTag == availableTag.Value));
        venue.Tags.AddRange(c.Interaction.Data.Values);

        return c.Session.MoveStateAsync<GamesEntrySessionState>(c);
    }

}