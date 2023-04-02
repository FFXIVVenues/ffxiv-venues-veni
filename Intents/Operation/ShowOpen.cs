using Discord;
using FFXIVVenues.Veni.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Configuration;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.People;
using FFXIVVenues.Veni.Services.Api;
using FFXIVVenues.Veni.SessionStates;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class ShowOpen : IntentHandler
    {

        private readonly IApiService _apiService;
        private readonly IVenueRenderer _venueRenderer;
        private IEnumerable<Venue> _venues;

        public ShowOpen(IApiService apiService,
                        IVenueRenderer venueRenderer)
        {
            this._apiService = apiService;
            this._venueRenderer = venueRenderer;
        }

        public override async Task Handle(VeniInteractionContext c)
        {
            var asker = c.Interaction.User.Id;
            this._venues = await this._apiService.GetOpenVenuesAsync();

            if (this._venues == null || !this._venues.Any())
            {
                await c.Interaction.RespondAsync("There are no venues open at the moment. 🤔");
                return;
            }

            var venueModels = this._venues
                .Select(v => {
                    DateTime? activeOpeningStart = null;
                    DateTime? activeOpeningEnd = null;
                    foreach (var opening in v.Openings)
                    {
                        var resolve = opening.Resolve(DateTime.UtcNow);
                        if (resolve.Open)
                        {
                            (_, activeOpeningStart, activeOpeningEnd) = resolve;
                            break;
                        }
                    }
                    var @override = v.OpenOverrides?.FirstOrDefault(o => o.Open && o.IsNow);

                    return new { 
                        Venue = v, 
                        Start = activeOpeningStart != null ? activeOpeningStart.Value : @override.Start,
                        End = activeOpeningEnd != null ? activeOpeningEnd.Value : @override.End
                    };
                })
                .OrderBy(v => v.Start)
                .Take(25);

            var selectMenuKey = c.Session.RegisterComponentHandler(this.HandleVenueSelection, ComponentPersistence.PersistRow);
            var componentBuilder = new ComponentBuilder();
            var selectMenuBuilder = new SelectMenuBuilder() { CustomId = selectMenuKey };
            foreach (var venue in venueModels)
            {
                var selectMenuOption = new SelectMenuOptionBuilder
                {
                    Label = venue.Venue.Name,
                    Description = $"Open for the next {PrettyPrintNet.TimeSpanExtensions.ToPrettyString(venue.End - DateTime.UtcNow)}",
                    Value = venue.Venue.Id
                };
                selectMenuBuilder.AddOption(selectMenuOption);
            }
            componentBuilder.WithSelectMenu(selectMenuBuilder);

            await c.Interaction.RespondAsync(MessageRepository.WhatsOpenMessage.PickRandom(), componentBuilder.Build());
        }

        private Task HandleVenueSelection(MessageComponentVeniInteractionContext context)
        {
            var selectedVenueId = context.Interaction.Data.Values.Single();
            var asker = context.Interaction.User.Id;
            var venue = this._venues.FirstOrDefault(v => v.Id == selectedVenueId);

            return context.Interaction.RespondAsync(embed: this._venueRenderer.RenderEmbed(venue).Build(),
                components: this._venueRenderer.RenderActionComponents(context, venue, asker).Build());
        }
    }
}
