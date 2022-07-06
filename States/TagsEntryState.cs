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

        private static List<(string Label, string Value)> _availableTags = new()
        {
            ("Courtesans", "Courtesans"),
            ("Gambling", "Gambling"),
            ("Artists", "Artists"),
            ("Dancers", "Dancers"),
            ("Bards", "Bards"),
            ("Twitch DJ", "Twitch DJ"),
            ("Tarot", "Tarot"),
            ("Pillow talk", "Pillow"),
            ("VIP", "VIP"),
            ("Triple triad", "Triple triad"),
            ("IC RP only", "IC RP Only")
        };

        private Dictionary<string, string> _tags = new();
        private Dictionary<string, string> _tagHandlers = new();

        public Task Init(MessageContext c)
        {
            var component = this.BuildTagsComponent(c);
            return c.RespondAsync(MessageRepository.AskForTags.PickRandom(), component.Build());
        }

        private ComponentBuilder BuildTagsComponent(MessageContext c)
        {
            var component = new ComponentBuilder();
            foreach (var tag in _availableTags)
                this.AddTagButton(component, c, tag.Label, tag.Value);
            component.WithButton("Complete",
                c.Conversation.RegisterComponentHandler(this.OnComplete), ButtonStyle.Success);
            return component;
        }

        private void AddTagButton(ComponentBuilder component, MessageContext c, string tagLabel, string tagValue = null) =>
            component.WithButton(tagLabel,
                this._tagHandlers.ContainsKey(tagValue)
                    ? this._tagHandlers[tagValue]
                    : this._tagHandlers[tagValue] = c.Conversation.RegisterComponentHandler(async cm =>
                    {
                        if (this._tags.ContainsKey(tagValue))
                            this._tags.Remove(tagValue);
                        else
                            this._tags[tagValue] = tagLabel;
                        //await cm.MessageComponent.DeferAsync();
                        await cm.MessageComponent.ModifyOriginalResponseAsync(props =>
                        {
                            var rebuilder = this.BuildTagsComponent(c);
                            props.Components = rebuilder.Build();
                        });
                    }), this._tags.ContainsKey(tagValue) ? ButtonStyle.Primary : ButtonStyle.Secondary);

        private Task OnComplete(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            venue.Tags.AddRange(this._tags.Keys);

            _ = c.MessageComponent.ModifyOriginalResponseAsync(props =>
            {
                props.Components = null;
            });

            if (c.Conversation.GetItem<bool>("modifying"))
                return c.Conversation.ShiftState<ConfirmVenueState>(c);

            return c.Conversation.ShiftState<WebsiteEntryState>(c);
        }

        public Task OnMessageReceived(MessageContext c)
        {
            return Task.CompletedTask;
        }

    }
}
