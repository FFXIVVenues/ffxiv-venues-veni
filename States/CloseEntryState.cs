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
        private Venue _venue;

        public CloseEntryState(IApiService _apiService)
        {
            this._apiService = _apiService;
        }
        
        public Task Enter(InteractionContext c)
        {
            this._venue = c.Session.GetItem<Venue>("venue");
            var component = this.BuildCloseComponent(c, this._venue.Open);
            return c.Interaction.RespondAsync("Aaw, how long do you want to close for? 🥲", component.Build()); //change text later
        }

        private ComponentBuilder BuildCloseComponent(InteractionContext c, bool includeCloseCurrentOpening)
        {
            var selectComponent = new SelectMenuBuilder()
                .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow));

            if (includeCloseCurrentOpening)
                selectComponent.AddOption("Close current opening", "0");
            
            selectComponent.AddOption("The next 18 hours", "18")
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
            var until = int.Parse(c.Interaction.Data.Values.Single());

            if (until == 0)
            {
                var end = this._venue.OpenOverrides.FirstOrDefault(o => o.IsNow)?.End ??
                          this._venue.GetActiveOpening()?.Resolve(DateTime.UtcNow).End;
                if (end == null)
                    return;
                await _apiService.CloseVenueAsync(this._venue.Id, end.Value);
            }
            else
                await _apiService.CloseVenueAsync(this._venue.Id, DateTime.UtcNow.AddHours(until));
            
            await c.Interaction.FollowupAsync(MessageRepository.VenueClosedMessage.PickRandom());
            _ = c.Session.ClearState(c);
        }
    }
}
