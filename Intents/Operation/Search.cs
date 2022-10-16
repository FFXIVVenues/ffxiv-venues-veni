using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Managers;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.Services;
using FFXIVVenues.Veni.States;
using FFXIVVenues.Veni.Utils;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Search : IntentHandler
    {

        private readonly IApiService _apiService;
        private readonly IStaffManager _staffService;
        private readonly string _uiUrl;
        private readonly string _apiUrl;

        public Search(IApiService apiService,
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
            var query = c.GetArgument("search-query");

            if (string.IsNullOrWhiteSpace(query))
            {
                await c.Interaction.RespondAsync("What am I looking for? 🤔");
                return;
            }

            var venues = await this._apiService.GetAllVenuesAsync(query);

            if (venues == null || !venues.Any())
                await c.Interaction.RespondAsync("Could find any venues with that name. 😔");
            else if (venues.Count() > 1)
            {
                if (venues.Count() > 25)
                    venues = venues.Take(25);
                c.Session.SetItem("venues", venues);
                await c.Session.MoveStateAsync<SelectVenueToShowState>(c);
            }
            else
            {
                var venue = venues.Single();
                var isOwnerOrEditor = venue.Managers.Contains(asker.ToString()) || this._staffService.IsEditor(asker);

                if (isOwnerOrEditor)
                    await c.Interaction.RespondAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build(),
                        component: new ComponentBuilder()
                            .WithButton("Open", c.Session.RegisterComponentHandler(async cm =>
                            {
                                await this._apiService.OpenVenueAsync(venue.Id);
                                await cm.Interaction.RespondAsync(MessageRepository.VenueOpenMessage.PickRandom());
                            }, ComponentPersistence.ClearRow), ButtonStyle.Primary)
                            .WithButton("Close", c.Session.RegisterComponentHandler(async cm =>
                            {
                                //await this._apiService.CloseVenueAsync(venue.Id);
                                //await cm.Interaction.RespondAsync(MessageRepository.VenueClosedMessage.PickRandom());
                            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                            .WithButton("Edit", c.Session.RegisterComponentHandler(cm =>
                            {
                                cm.Session.SetItem("venue", venue);
                                return cm.Session.MoveStateAsync<ModifyVenueState>(cm);
                            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                            .WithButton("Delete", c.Session.RegisterComponentHandler(cm =>
                            {
                                cm.Session.SetItem("venue", venue);
                                return cm.Session.MoveStateAsync<DeleteVenueState>(cm);
                            }, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                            .WithButton("Do nothing", c.Session.RegisterComponentHandler(cm =>
                                Task.CompletedTask,
                            ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                            .Build());
                else if (this._staffService.IsPhotographer(asker))
                    await c.Interaction.RespondAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build(),
                        component: new ComponentBuilder()
                            .WithButton("Edit Banner Photo", c.Session.RegisterComponentHandler(cm =>
                            {
                                cm.Session.SetItem("venue", venue);
                                return cm.Session.MoveStateAsync<BannerEntryState>(cm);
                            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                            .Build());
                else
                    await c.Interaction.RespondAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build());
            }
        }

    }
}
