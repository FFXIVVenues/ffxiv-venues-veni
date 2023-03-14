using Discord;
using FFXIVVenues.Veni.Utils;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Configuration;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Session;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.People;
using FFXIVVenues.Veni.SessionStates;

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
                await c.Session.MoveStateAsync<SelectVenueToShowSessionState>(c);
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
                                cm.Session.SetItem("venue", venue);
                                return cm.Session.MoveStateAsync<ModifyVenueSessionState>(cm);
                            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                            .WithButton("Delete", c.Session.RegisterComponentHandler(cm =>
                            {
                                cm.Session.SetItem("venue", venue);
                                return cm.Session.MoveStateAsync<DeleteVenueSessionState>(cm);
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
                                return cm.Session.MoveStateAsync<BannerEntrySessionState>(cm);
                            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                            .Build());
                else
                    await c.Interaction.RespondAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build());
            }
        }

    }
}
