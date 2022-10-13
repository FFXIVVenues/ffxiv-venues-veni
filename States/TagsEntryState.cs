using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class TagsEntryState : IState
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
            ("Bar", "Bar"),
            ("Tarot", "Tarot"),
            ("Pillow talk", "Pillow"),
            ("Photography", "Photography"),
            ("Open stage", "Open stage"),
            ("Void", "Void"),
            ("VIP available", "VIP"),
            ("Triple triad", "Triple triad"),
            ("RP Heavily Encouraged", "RP Heavy"),
            ("Strictly in-character RP only", "IC RP Only")
        };

        public Task Enter(InteractionContext c)
        {
            this._venue = c.Session.GetItem<Venue>("venue");

            var component = this.BuildTagsComponent(c);
            return c.Interaction.RespondAsync(MessageRepository.AskForTags.PickRandom(), component.Build());
        }

        private ComponentBuilder BuildTagsComponent(InteractionContext c)
        {
            var selectComponent = new SelectMenuBuilder()
                .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
                .WithMaxValues(_availableTags.Count);
            foreach (var (label, value) in _availableTags)
                selectComponent.AddOption(label, value, isDefault: this._venue.Tags.Contains(value));

            return new ComponentBuilder()
                .WithSelectMenu(selectComponent)
                .WithBackButton(c)
                .WithSkipButton<WebsiteEntryState, ConfirmVenueState>(c);
        }

        private Task OnComplete(MessageComponentInteractionContext c)
        {
            var venue = c.Session.GetItem<Venue>("venue");
            venue.Tags = venue.Tags ?? new();
            venue.Tags.RemoveAll(existingTag => _availableTags.Any(availableTag => existingTag == availableTag.Value));
            venue.Tags.AddRange(c.Interaction.Data.Values);

            if (c.Session.GetItem<bool>("modifying"))
                return c.Session.MoveStateAsync<ConfirmVenueState>(c);

            return c.Session.MoveStateAsync<WebsiteEntryState>(c);
        }

    }
}
