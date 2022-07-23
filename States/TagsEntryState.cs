using Discord;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
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
            ("VIP available", "VIP"),
            ("Triple triad", "Triple triad"),
            ("RP Heavily Encouraged", "RP Heavy"),
            ("Strictly in-character RP only", "IC RP Only")
        };

        public Task Init(MessageContext c)
        {
            this._venue = c.Conversation.GetItem<Venue>("venue");

            var component = this.BuildTagsComponent(c);
            return c.RespondAsync(MessageRepository.AskForTags.PickRandom(), component.Build());
        }

        private ComponentBuilder BuildTagsComponent(MessageContext c)
        {
            var selectComponent = new SelectMenuBuilder()
                .WithCustomId(c.Conversation.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
                .WithMaxValues(_availableTags.Count);
            foreach (var (label, value) in _availableTags)
                selectComponent.AddOption(label, value, isDefault: this._venue.Tags.Contains(value));

            return new ComponentBuilder().WithSelectMenu(selectComponent);
        }

        private Task OnComplete(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            venue.Tags = venue.Tags ?? new();
            venue.Tags.RemoveAll(existingTag => _availableTags.Any(availableTag => existingTag == availableTag.Value));
            venue.Tags.AddRange(c.MessageComponent.Data.Values);

            if (c.Conversation.GetItem<bool>("modifying"))
                return c.Conversation.ShiftState<ConfirmVenueState>(c);

            return c.Conversation.ShiftState<WebsiteEntryState>(c);
        }

    }
}
