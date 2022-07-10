using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Show : IIntentHandler
    {

        private readonly IApiService _apiService;
        private readonly IIndexersService _indexersService;
        private readonly string _uiUrl;
        private readonly string _apiUrl;

        public Show(IApiService apiService,
                    UiConfiguration uiConfig,
                    ApiConfiguration apiConfig, 
                    IIndexersService indexersService)
        {
            this._apiService = apiService;
            this._indexersService = indexersService;
            this._uiUrl = uiConfig.BaseUrl;
            this._apiUrl = apiConfig.BaseUrl;
        }

        public async Task Handle(MessageContext c)
        {
            var asker = c.Message.Author.Id;
            var venues = await this._apiService.GetAllVenuesAsync(asker);

            if (venues == null || !venues.Any())
                await c.RespondAsync("You don't seem to be an assigned manager for any venues. 🤔");
            else if (venues.Count() > 1)
            {
                if (venues.Count() > 25)
                    venues = venues.Take(25);
                c.Conversation.SetItem("venues", venues);
                await c.Conversation.ShiftState<SelectVenueToShowState>(c);
            }
            else
            {
                var venue = venues.Single();
                var isOwnerOrIndexer = venue.Managers.Contains(asker.ToString()) || this._indexersService.IsIndexer(asker);

                if (isOwnerOrIndexer)
                    await c.RespondAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build(),
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
                            .WithButton("Dismiss", c.Conversation.RegisterComponentHandler(cm =>
                                cm.MessageComponent.DeleteOriginalResponseAsync(),
                            ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                            .Build());
                else
                    await c.RespondAsync(embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build());
            }
        }

    }
}
