using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Session;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.SessionStates
{
    class SelectVenueToOpenSessionState : ISessionState
    {

        private static string[] _messages = new[]
        {
            "Wooo! Which one are we opening?",
            "Yaay! 🥳 Which venue are we opening?",
            "🎉 Which one?"
        };

        private IEnumerable<Venue> _managersVenues;
        private readonly IApiService _apiService;

        public SelectVenueToOpenSessionState(IApiService apiService)
        {
            this._apiService = apiService;
        }

        public Task Enter(VeniInteractionContext c)
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

        public async Task Handle(MessageComponentVeniInteractionContext c)
        {
            var selectedVenueId = c.Interaction.Data.Values.Single();
            var venue = _managersVenues.FirstOrDefault(v => v.Id == selectedVenueId);

            _ = c.Session.ClearState(c);

            c.Session.SetItem<Venue>("venue", venue);
            await c.Session.MoveStateAsync<OpenEntrySessionState>(c);

        }
    }
}
