using Discord;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
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

        public Task Init(InteractionContext c)
        {
            this._venue = c.Session.GetItem<Venue>("venue");

            var component = this.BuildTagsComponent(c);
            return c.Interaction.RespondAsync(MessageRepository.AskForCategories.PickRandom(), component.Build());
        }

        private ComponentBuilder BuildTagsComponent(InteractionContext c)
        {
            var selectComponent = new SelectMenuBuilder()
                .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
                .WithMaxValues(Math.Max(2, _availableCategories.Count(t => this._venue.Tags?.Contains(t.Value) ?? false)));
            foreach (var (label, value) in _availableCategories)
                selectComponent.AddOption(label, value, isDefault: this._venue.Tags.Contains(value));

            return new ComponentBuilder()
                .WithSelectMenu(selectComponent)
                .WithBackButton(c)
                .WithSkipButton<TagsEntryState, TagsEntryState>(c);
        }

        private Task OnComplete(MessageComponentInteractionContext c)
        {
            var venue = c.Session.GetItem<Venue>("venue");
            venue.Tags = venue.Tags ?? new();
            venue.Tags.RemoveAll(existingTag => _availableCategories.Any(availableTag => existingTag == availableTag.Value));
            venue.Tags.AddRange(c.Interaction.Data.Values);

            return c.Session.ShiftState<TagsEntryState>(c);
        }

    }
}
