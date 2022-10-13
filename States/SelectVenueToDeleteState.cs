using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
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

        public Task Enter(InteractionContext c)
        {
            _managersVenues = c.Session.GetItem<IEnumerable<Venue>>("venues");

            var selectMenuKey = c.Session.RegisterComponentHandler(this.Handle, ComponentPersistence.DeleteMessage);
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
            return c.Interaction.RespondAsync(_messages.PickRandom(), componentBuilder.Build());
        }

        public Task Handle(MessageComponentInteractionContext c)
        {
            var selectedVenueId = c.Interaction.Data.Values.Single();
            var venue = _managersVenues.FirstOrDefault(v => v.Id == selectedVenueId);

            c.Session.ClearItem("venues");
            c.Session.SetItem("venue", venue);

            return c.Session.MoveStateAsync<DeleteVenueState>(c);
        }
    }
}
