﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueOpening.SessionStates
{
    internal class OpenEntrySessionState : ISessionState
    {
        private IApiService _apiService;
        private Venue _venue;

        public OpenEntrySessionState(IApiService _apiService)
        {
            this._apiService = _apiService;
        }
        
        public Task Enter(VeniInteractionContext c)
        {
            this._venue = c.Session.GetVenue();
            var component = this.BuildOpenComponent(c);
            return c.Interaction.RespondAsync("Yay, how long are we opening for? 🥰", component.Build()); //change text later
        }

        private ComponentBuilder BuildOpenComponent(VeniInteractionContext c)
        {
            var selectComponent = new SelectMenuBuilder()
                .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow));

            selectComponent.AddOption("The next hour", "1")
                .AddOption("The next 2 hours", "2")
                .AddOption("The next 3 hours", "3")
                .AddOption("The next 4 hours", "4")
                .AddOption("The next 5 hours", "5")
                .AddOption("The next 6 hours", "6");
            return new ComponentBuilder().WithSelectMenu(selectComponent);
        }

        private async Task OnComplete(MessageComponentVeniInteractionContext c)
        {
            var until = int.Parse(c.Interaction.Data.Values.Single()); 
            await _apiService.OpenVenueAsync(this._venue.Id, DateTime.UtcNow.AddHours(until));
            
            await c.Interaction.Channel.SendMessageAsync(MessageRepository.VenueOpenMessage.PickRandom());
            _ = c.Session.ClearState(c);
        }
    }
}