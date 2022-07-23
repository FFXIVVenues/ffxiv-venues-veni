using Discord;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class CategoryEntryState : IState
    {

        private Venue _venue;
        private static List<(string Label, string Value)> _availableCategories = new()
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
            ("Auction house", "Auction house"),
            ("Casino", "Casino"),
            ("Shop", "Shop"),
            ("Maid cafe / Host club", "Maid cafe"),
            ("Other", "other")
        };

        public Task Init(MessageContext c)
        {
            this._venue = c.Conversation.GetItem<Venue>("venue");

            var component = this.BuildTagsComponent(c);
            return c.RespondAsync(MessageRepository.AskForCategories.PickRandom(), component.Build());
        }

        private ComponentBuilder BuildTagsComponent(MessageContext c)
        {
            var selectComponent = new SelectMenuBuilder()
                .WithCustomId(c.Conversation.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
                .WithMaxValues(Math.Max(2, _availableCategories.Count(t => this._venue.Tags?.Contains(t.Value) ?? false)));
            foreach (var (label, value) in _availableCategories)
                selectComponent.AddOption(label, value, isDefault: this._venue.Tags.Contains(value));

            return new ComponentBuilder().WithSelectMenu(selectComponent);
        }

        private Task OnComplete(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            venue.Tags = venue.Tags ?? new();
            venue.Tags.RemoveAll(existingTag => _availableCategories.Any(availableTag => existingTag == availableTag.Value));
            venue.Tags.AddRange(c.MessageComponent.Data.Values);

            return c.Conversation.ShiftState<TagsEntryState>(c);
        }

    }
}
