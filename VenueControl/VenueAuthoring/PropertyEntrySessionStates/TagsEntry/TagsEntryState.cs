using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.TagsEntry
{
    class TagsEntrySessionState : ISessionState
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
            ("Stylists", "Stylists"),
            ("Performances", "Performances"),
            ("VIP available", "VIP"),
            ("Triple triad", "Triple triad"),
            ("LGBTQIA+ focused", "LGBTQIA+"),
            ("RP Heavily Encouraged", "RP Heavy"),
            ("Strictly in-character RP only", "IC RP Only")
        };

        public Task Enter(VeniInteractionContext c)
        {
            this._venue = c.Session.GetVenue();

            var component = this.BuildTagsComponent(c);
            return c.Interaction.RespondAsync(MessageRepository.AskForTags.PickRandom(), component.Build());
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

        private Task OnComplete(MessageComponentVeniInteractionContext c)
        {
            var venue = c.Session.GetVenue();
            venue.Tags = venue.Tags ?? new();
            venue.Tags.RemoveAll(existingTag => _availableTags.Any(availableTag => existingTag == availableTag.Value));
            venue.Tags.AddRange(c.Interaction.Data.Values);

            if (c.Session.GetItem<bool>("modifying"))
                return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);

            return c.Session.MoveStateAsync<WebsiteEntrySessionState>(c);
        }

    }
}
