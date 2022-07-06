using Discord;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class TypeEntryState : IState
    {

        private static List<(string Label, string Value)> _availableTypes = new()
        {
            ("Nightclub", "Nightclub"),
            ("Den", "Den"),
            ("Cafe", "Cafe"),
            ("Tavern", "Tavern"),
            ("Inn", "Inn"),
            ("Lounge", "Lounge"),
            ("Bath house", "Bath house"),
            ("Library", "Library"),
            ("Casino", "Casino"),
            ("Maid cafe / Host club", "Maid cafe")
        };

        private List<string> _tagHandlers = new();

        public Task Init(MessageContext c)
        {
            var component = this.BuildTagsComponent(c);
            return c.RespondAsync(MessageRepository.AskForType.PickRandom(), component.Build());
        }

        private ComponentBuilder BuildTagsComponent(MessageContext c)
        {
            var component = new ComponentBuilder();
            foreach (var tag in _availableTypes)
                this.AddTagButton(component, c, tag.Label, tag.Value);
            return component;
        }

        private void AddTagButton(ComponentBuilder component, MessageContext c, string tagLabel, string tagValue)
        {
            var handler = c.Conversation.RegisterComponentHandler(async cm =>
            {
                var venue = cm.Conversation.GetItem<Venue>("venue");
                venue.Tags = new() { tagValue };

                await cm.MessageComponent.ModifyOriginalResponseAsync(props =>
                    props.Components = new ComponentBuilder().Build());

                foreach (var handlerKey in this._tagHandlers)
                    cm.Conversation.UnregisterComponentHandler(handlerKey);

                await cm.Conversation.ShiftState<TagsEntryState>(cm);
            }, ComponentPersistence.ClearRow);
            this._tagHandlers.Add(handler);
            component.WithButton(tagLabel, handler, ButtonStyle.Secondary);
        }

    }
}
