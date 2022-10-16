using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.Services;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    internal class CloseEntryState : IState
    {
        private IApiService _apiService;

        public CloseEntryState(IApiService _apiService)
        {
            this._apiService = _apiService;
        }
        public Task Enter(InteractionContext c)
        {
            var component = this.BuildCloseComponent(c);
            return c.Interaction.RespondAsync("How long you want to stay closed up?", component.Build()); //change text later
        }

        private ComponentBuilder BuildCloseComponent(InteractionContext c)
        {
            var selectComponent = new SelectMenuBuilder()
                .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow))
                .AddOption("The next 18 hours", "18")
                .AddOption("The next 2 days", "48")
                .AddOption("The next 3 days", "72")
                .AddOption("The next 5 days", "120")
                .AddOption("The next 7 days", "168")
                .AddOption("The next 2 weeks", "336")
                .AddOption("The next 3 weeks", "504")
                .AddOption("The next 4 weeks", "672")
                .AddOption("The next 6 weeks", "1008")
                .AddOption("The next 2 months", "1344")
                .AddOption("The next 3 months", "2016");
            return new ComponentBuilder().WithSelectMenu(selectComponent);
        }

        private async Task OnComplete(MessageComponentInteractionContext c)
        {
            
            var venue = c.Session.GetItem<Venue>("venue");

            var until = int.Parse(c.Interaction.Data.Values.Single());
            await _apiService.CloseVenueAsync(venue.Id, DateTime.UtcNow.AddHours(until));
            await c.Interaction.FollowupAsync(MessageRepository.VenueClosedMessage.PickRandom());
            _ = c.Session.ClearState(c);
        }
    }
}
