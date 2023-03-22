using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Auditing;
using FFXIVVenues.Veni.Configuration;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Session;
using FFXIVVenues.Veni.People;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.SessionStates
{
    class SelectVenueToShowSessionState : ISessionState
    {

        private readonly IVenueRenderer _venueRenderer;
        private readonly IApiService _apiService;
        private readonly IStaffService _staffService;
        private IEnumerable<Venue> _managersVenues;

        public SelectVenueToShowSessionState(IVenueRenderer venueRenderer, IApiService apiService, IStaffService staffService)
        {
            this._venueRenderer = venueRenderer;
            this._apiService = apiService;
            this._staffService = staffService;
        }

        public Task Enter(VeniInteractionContext c)
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
            return c.Interaction.RespondAsync(MessageRepository.ShowVenueResponses.PickRandom(), componentBuilder.Build());
        }

        public async Task Handle(MessageComponentVeniInteractionContext context)
        {
            var selectedVenueId = context.Interaction.Data.Values.Single();
            var asker = context.Interaction.User.Id;
            var venue = _managersVenues.FirstOrDefault(v => v.Id == selectedVenueId);

            await context.Session.ClearState(context);
            
            await context.Interaction.FollowupAsync(embed: this._venueRenderer.RenderEmbed(venue).Build(),
                components: this._venueRenderer.RenderActionComponents(context, venue, asker).Build());
        }

        
    }
}
