using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States;
using FFXIVVenues.Veni.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents.Operation
{
    internal class Show : IntentHandler
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

        public override async Task Handle(InteractionContext c)
        {
            var asker = c.Interaction.User.Id;
            var venues = await this._apiService.GetAllVenuesAsync(asker);

            if (venues == null || !venues.Any())
                await c.Interaction.RespondAsync("You don't seem to be an assigned manager for any venues. 🤔");
            else if (venues.Count() > 1)
            {
                if (venues.Count() > 25)
                    venues = venues.Take(25);
                c.Session.SetItem("venues", venues);
                await c.Session.SetStateAsync<SelectVenueToShowState>(c);
            }
            else
            {
                var venue = venues.Single();
                var isOwnerOrIndexer = venue.Managers.Contains(asker.ToString()) || this._indexersService.IsIndexer(asker);

                if (isOwnerOrIndexer)
                    await c.Interaction.RespondAsync(MessageRepository.ShowVenueResponses.PickRandom(), 
                                         embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build(),
                                         component: new ComponentBuilder()
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
                                                cm.Session.SetItem("venue", venue);
                                                return cm.Session.ShiftState<ModifyVenueState>(cm);
                                            }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                                            .WithButton("Delete", c.Session.RegisterComponentHandler(cm =>
                                            {
                                                cm.Session.SetItem("venue", venue);
                                                return cm.Session.ShiftState<DeleteVenueState>(cm);
                                            }, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                                            .WithButton("Do nothing", c.Session.RegisterComponentHandler(cm =>
                                                Task.CompletedTask,
                                            ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                                            .Build());
                else
                    await c.Interaction.RespondAsync(MessageRepository.ShowVenueResponses.PickRandom(), 
                            embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media")
                            .Build());
            }
        }

    }
}
