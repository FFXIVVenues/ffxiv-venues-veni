using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class SelectVenueToCloseState : IState
    {

        private static string[] _messages = new[]
        {
            "Which one are we closing?",
            "Sleepy time! Which venue are we closing?",
            "Which one?"
        };

        private IEnumerable<Venue> _managersVenues;
        private readonly IApiService _apiService;

        public SelectVenueToCloseState(IApiService apiService)
        {
            this._apiService = apiService;
        }

        public Task Enter(InteractionContext c)
        {
            this._managersVenues = c.Session.GetItem<IEnumerable<Venue>>("venues");

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

        public async Task Handle(MessageComponentInteractionContext c)
        {
            var selectedVenueId = c.Interaction.Data.Values.Single();
            var venue = _managersVenues.FirstOrDefault(v => v.Id == selectedVenueId);

            _ = c.Session.ClearState(c);

            await _apiService.CloseVenueAsync(venue.Id);
            await c.Interaction.FollowupAsync(MessageRepository.VenueClosedMessage.PickRandom());
        }
    }
}
