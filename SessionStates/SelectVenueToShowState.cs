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

        private readonly string _apiUrl;
        private readonly string _uiUrl;
        private readonly IApiService _apiService;
        private readonly IStaffManager _staffService;
        private readonly IVenueAuditFactory _venueAuditFactory;
        private IEnumerable<Venue> _managersVenues;

        public SelectVenueToShowSessionState(UiConfiguration uiConfig, ApiConfiguration apiConfig, IApiService apiService, IStaffManager staffService, IVenueAuditFactory venueAuditFactory)
        {
            this._uiUrl = uiConfig.BaseUrl;
            this._apiUrl = apiConfig.BaseUrl;
            this._apiService = apiService;
            this._staffService = staffService;
            this._venueAuditFactory = venueAuditFactory;
        }

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
            return c.Interaction.RespondAsync(MessageRepository.ShowVenueResponses.PickRandom(), componentBuilder.Build());
        }

        public async Task Handle(MessageComponentInteractionContext c)
        {
            var selectedVenueId = c.Interaction.Data.Values.Single();
            var asker = c.Interaction.User.Id;
            var venue = _managersVenues.FirstOrDefault(v => v.Id == selectedVenueId);

            await c.Session.ClearState(c);
            var isOwnerOrIndexer = venue.Managers.Contains(asker.ToString()) || this._staffService.IsEditor(asker);

            if (isOwnerOrIndexer)
                await c.Interaction.FollowupAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build(),
                    components: new ComponentBuilder()
                        .WithButton(new ButtonBuilder().WithLabel("[ AUDIT DEV ]")
                            .WithSessionHandler(c.Session, ctx =>
                            {
                                var audit = this._venueAuditFactory.CreateAuditFor(venue);
                                return audit.AuditAsync();
                            }, ComponentPersistence.ClearRow)
                            .WithStyle(ButtonStyle.Secondary))
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
                await c.Interaction.FollowupAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build(),
                    components: new ComponentBuilder()
                        .WithButton("Edit Banner Photo", c.Session.RegisterComponentHandler(cm =>
                        {
                            c.Session.SetItem("venue", venue);
                            return cm.Session.MoveStateAsync<BannerEntrySessionState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                        .Build());
            else
                await c.Interaction.FollowupAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build());
        }
    }
}
