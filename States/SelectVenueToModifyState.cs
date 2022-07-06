using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class SelectVenueToModifyState : IState
    {

        private static string[] _messages = new[]
        {
            "Which venue would you like to edit?",
            "Which one would you like to change?",
            "Oki, which one? 🙂"
        };

        private IEnumerable<Venue> _managersVenues;

        public Task Init(MessageContext c)
        {
            _managersVenues = c.Conversation.GetItem<IEnumerable<Venue>>("venues");

            var selectMenuKey = c.Conversation.RegisterComponentHandler(this.Handle, ComponentPersistence.DeleteMessage);
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

        public Task Handle(MessageContext c)
        {
            var selectedVenueId = c.MessageComponent.Data.Values.Single();
            var venue = _managersVenues.FirstOrDefault(v => v.Id == selectedVenueId);

            c.Conversation.ClearItem("venues");
            c.Conversation.SetItem("venue", venue);

            return c.Conversation.ShiftState<ModifyVenueState>(c);
        }
    }
}
