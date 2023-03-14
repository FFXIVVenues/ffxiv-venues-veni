using Discord;
using FFXIVVenues.Veni.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Configuration;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Session;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.People;
using FFXIVVenues.Veni.SessionStates;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class ShowOpen : IntentHandler
    {

        private readonly IApiService _apiService;
        private readonly IStaffManager _staffService;
        private readonly string _uiUrl;
        private readonly string _apiUrl;
        private IEnumerable<Venue> _venues;

        public ShowOpen(IApiService apiService,
                        UiConfiguration uiConfig,
                        ApiConfiguration apiConfig, 
                        IStaffManager staffService)
        {
            this._apiService = apiService;
            this._staffService = staffService;
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

            var isOwnerOrEditor = venue.Managers.Contains(asker.ToString()) || this._staffService.IsEditor(asker);

            if (isOwnerOrEditor)
                return c.Interaction.FollowupAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build(),
                    components: new ComponentBuilder()
                        .WithButton("Open", c.Session.RegisterComponentHandler(async cm =>
                        {
                            cm.Session.SetItem("venue", venue);
                            await cm.Session.MoveStateAsync<OpenEntrySessionState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Primary)
                        .WithButton("Close", c.Session.RegisterComponentHandler(async cm =>
                        {
                            cm.Session.SetItem("venue", venue);
                            await cm.Session.MoveStateAsync<CloseEntrySessionState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                        .WithButton("Edit", c.Session.RegisterComponentHandler(cm =>
                        {
                            c.Session.SetItem("venue", venue);
                            return cm.Session.MoveStateAsync<ModifyVenueSessionState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                        .WithButton("Delete", c.Session.RegisterComponentHandler(cm =>
                        {
                            c.Session.SetItem("venue", venue);
                            return cm.Session.MoveStateAsync<DeleteVenueSessionState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                        .WithButton("Do nothing", c.Session.RegisterComponentHandler(cm => Task.CompletedTask,
                            ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                        .Build());
            else if (this._staffService.IsPhotographer(asker))
                return c.Interaction.FollowupAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build(),
                    components: new ComponentBuilder()
                        .WithButton("Edit Banner Photo", c.Session.RegisterComponentHandler(cm =>
                        {
                            c.Session.SetItem("venue", venue);
                            return cm.Session.MoveStateAsync<BannerEntrySessionState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                        .Build());
            else
                return c.Interaction.FollowupAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build());
        }
    }
}
