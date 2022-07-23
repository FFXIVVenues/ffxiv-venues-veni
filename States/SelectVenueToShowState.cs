using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class SelectVenueToShowState : IState
    {

        private readonly string _apiUrl;
        private readonly string _uiUrl;
        private readonly IApiService _apiService;
        private readonly IIndexersService _indexersService;
        private IEnumerable<Venue> _managersVenues;

        public SelectVenueToShowState(UiConfiguration uiConfig, ApiConfiguration apiConfig, IApiService apiService, IIndexersService indexersService)
        {
            this._uiUrl = uiConfig.BaseUrl;
            this._apiUrl = apiConfig.BaseUrl;
            this._apiService = apiService;
            this._indexersService = indexersService;
        }

        public Task Init(MessageContext c)
        {
            _managersVenues = c.Conversation.GetItem<IEnumerable<Venue>>("venues");

            var selectMenuKey = c.Conversation.RegisterComponentHandler(this.Handle, ComponentPersistence.DeleteMessage);
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
            return c.RespondAsync(MessageRepository.ShowVenueResponses.PickRandom(), componentBuilder.Build());
        }

        public Task Handle(MessageContext c)
        {
            var selectedVenueId = c.MessageComponent.Data.Values.Single();
            var asker = c.MessageComponent.User.Id;
            var venue = _managersVenues.FirstOrDefault(v => v.Id == selectedVenueId);

            c.Conversation.ClearState();

            var isOwnerOrIndexer = venue.Managers.Contains(asker.ToString()) || this._indexersService.IsIndexer(asker);

            if (isOwnerOrIndexer)
                return c.RespondAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build(),
                    component: new ComponentBuilder()
                        .WithButton("Open", c.Conversation.RegisterComponentHandler(async cm =>
                        {
                            await this._apiService.OpenVenueAsync(venue.Id);
                            await cm.RespondAsync(MessageRepository.VenueOpenMessage.PickRandom());
                        }, ComponentPersistence.ClearRow), ButtonStyle.Primary)
                        .WithButton("Close", c.Conversation.RegisterComponentHandler(async cm =>
                        {
                            await this._apiService.CloseVenueAsync(venue.Id);
                            await cm.RespondAsync(MessageRepository.VenueClosedMessage.PickRandom());
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                        .WithButton("Edit", c.Conversation.RegisterComponentHandler(cm =>
                        {
                            c.Conversation.SetItem("venue", venue);
                            return cm.Conversation.ShiftState<ModifyVenueState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                        .WithButton("Delete", c.Conversation.RegisterComponentHandler(cm =>
                        {
                            c.Conversation.SetItem("venue", venue);
                            return cm.Conversation.ShiftState<DeleteVenueState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                        .WithButton("Do nothing", c.Conversation.RegisterComponentHandler(cm => Task.CompletedTask,
                            ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                        .Build());
            else if (this._indexersService.IsPhotographer(asker))
                return c.RespondAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build(),
                    component: new ComponentBuilder()
                        .WithButton("Edit Banner Photo", c.Conversation.RegisterComponentHandler(cm =>
                        {
                            c.Conversation.SetItem("venue", venue);
                            return cm.Conversation.ShiftState<BannerInputState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                        .Build());
            else
                return c.RespondAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build());
        }
    }
}
