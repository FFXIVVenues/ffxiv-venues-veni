using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class SelectVenueToDeleteState : IState
    {

        private static string[] _messages = new[]
        {
            "Oh noes! 😥\nWhich venue would you like to delete?",
            "Sadge 😥\nWhich one are we deleting?",
            "😥 Which one?"
        };

        private IEnumerable<Venue> _managersVenues;
        private IIndexersService _indexersService;

        public SelectVenueToDeleteState(IIndexersService indexersService)
        {
            this._indexersService = indexersService;
        }

        public Task Init(MessageContext c)
        {
            _managersVenues = c.Conversation.GetItem<IEnumerable<Venue>>("venues");

            var selectMenuKey = c.Conversation.RegisterComponentHandler(this.Handle);
            var componentBuilder = new ComponentBuilder();
            var selectMenuBuilder = new SelectMenuBuilder() { CustomId = selectMenuKey };
            foreach (var venue in _managersVenues.OrderBy(v => v.Name))
            {
                var selectMenuOption = new SelectMenuOptionBuilder
                {
                    Label = venue.Name,
                    Description = venue.Location.ToString(),
                    Value = venue.Id
                };
                selectMenuBuilder.AddOption(selectMenuOption);
            }
            componentBuilder.WithSelectMenu(selectMenuBuilder);
            return c.RespondAsync(_messages.PickRandom(), componentBuilder.Build());

        }

        public Task OnMessageReceived(MessageContext c) => Task.CompletedTask;

        public Task Handle(MessageContext c)
        {
            _ = c.MessageComponent.DeleteOriginalResponseAsync();
            var selectedVenueId = c.MessageComponent.Data.Values.Single();
            var venue = _managersVenues.FirstOrDefault(v => v.Id == selectedVenueId);

            if (!this._indexersService.IsIndexer(c.MessageComponent.User.Id)
                && !venue.Managers.Contains(c.MessageComponent.User.Id.ToString()))
                return c.RespondAsync("Sorry, you're not a manager of this venue!", flags: MessageFlags.Ephemeral);

            c.Conversation.ClearItem("venues");
            c.Conversation.SetItem("venue", venue);

            return c.Conversation.ShiftState<DeleteVenueState>(c);
        }
    }
}
