using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using FFXIVVenues.Veni.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class ShowOpen : IntentHandler
    {

        private readonly IApiService _apiService;
        private readonly IIndexersService _indexersService;
        private readonly string _uiUrl;
        private readonly string _apiUrl;
        private IEnumerable<Venue> _venues;

        public ShowOpen(IApiService apiService,
                    UiConfiguration uiConfig,
                    ApiConfiguration apiConfig, 
                    IIndexersService indexersService)
        {
            this._apiService = apiService;
            this._indexersService = indexersService;
            this._uiUrl = uiConfig.BaseUrl;
            this._apiUrl = apiConfig.BaseUrl;
        }

        public override async Task Handle(InteractionContext c)
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

        private Task HandleVenueSelection(MessageComponentInteractionContext c)
        {
            var selectedVenueId = c.Interaction.Data.Values.Single();
            var asker = c.Interaction.User.Id;
            var venue = this._venues.FirstOrDefault(v => v.Id == selectedVenueId);

            var isOwnerOrIndexer = venue.Managers.Contains(asker.ToString()) || this._indexersService.IsIndexer(asker);

            if (isOwnerOrIndexer)
                return c.Interaction.FollowupAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build(),
                    components: new ComponentBuilder()
                        .WithButton("Open", c.Session.RegisterComponentHandler(async cm =>
                        {
                            await this._apiService.OpenVenueAsync(venue.Id);
                            await cm.Interaction.RespondAsync(MessageRepository.VenueOpenMessage.PickRandom());
                        }, ComponentPersistence.ClearRow), ButtonStyle.Primary)
                        .WithButton("Close", c.Session.RegisterComponentHandler(async cm =>
                        {
                            await this._apiService.CloseVenueAsync(venue.Id);
                            await cm.Interaction.RespondAsync(MessageRepository.VenueClosedMessage.PickRandom());
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                        .WithButton("Edit", c.Session.RegisterComponentHandler(cm =>
                        {
                            c.Session.SetItem("venue", venue);
                            return cm.Session.ShiftState<ModifyVenueState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                        .WithButton("Delete", c.Session.RegisterComponentHandler(cm =>
                        {
                            c.Session.SetItem("venue", venue);
                            return cm.Session.ShiftState<DeleteVenueState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                        .WithButton("Do nothing", c.Session.RegisterComponentHandler(cm => Task.CompletedTask,
                            ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                        .Build());
            else if (this._indexersService.IsPhotographer(asker))
                return c.Interaction.FollowupAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build(),
                    components: new ComponentBuilder()
                        .WithButton("Edit Banner Photo", c.Session.RegisterComponentHandler(cm =>
                        {
                            c.Session.SetItem("venue", venue);
                            return cm.Session.ShiftState<BannerInputState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                        .Build());
            else
                return c.Interaction.FollowupAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build());
        }
    }
}
