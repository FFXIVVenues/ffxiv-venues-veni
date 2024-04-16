using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueOpening.SessionStates
{
    internal class OpenEntryState(IApiService apiService, IAuthorizer authorizer) : ISessionState
    {
        private Venue _venue;

        public Task Enter(VeniInteractionContext c)
        {
            this._venue = c.Session.GetVenue();
            var component = this.BuildOpenComponent(c);
            return c.Interaction.RespondAsync("Yay, how long are we opening for? 🥰", component.Build()); //change text later
        }

        private ComponentBuilder BuildOpenComponent(VeniInteractionContext c)
        {
            var selectComponent = new SelectMenuBuilder()
                .WithCustomId(c.Session.RegisterComponentHandler(OnSelect, ComponentPersistence.ClearRow));

            selectComponent.AddOption("The next hour", "1")
                .AddOption("The next 2 hours", "2")
                .AddOption("The next 3 hours", "3")
                .AddOption("The next 4 hours", "4")
                .AddOption("The next 5 hours", "5")
                .AddOption("The next 6 hours", "6");
            return new ComponentBuilder().WithSelectMenu(selectComponent);
        }

        private async Task OnSelect(ComponentVeniInteractionContext c)
        {
            var authorize = authorizer.Authorize(c.Interaction.User.Id, Permission.OpenVenue, _venue);
            if (!authorize.Authorized)
            {
                await c.Interaction.Channel.SendMessageAsync(
                    "Sorry, you do not have permission to open this venue. 😢");
                return;
            }
            
            var until = int.Parse(c.Interaction.Data.Values.Single()); 
            await apiService.OpenVenueAsync(this._venue.Id, DateTime.UtcNow.AddHours(until));
            
            await c.Interaction.Channel.SendMessageAsync(MessageRepository.VenueOpenMessage.PickRandom());
            _ = c.Session.ClearState(c);
        }
    }
}
