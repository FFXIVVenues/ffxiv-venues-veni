using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using Newtonsoft.Json.Linq;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates
{
    class ManagerEntrySessionState : ISessionState
    {
        private readonly IAuthorizer _authorizer;

        public ManagerEntrySessionState(IAuthorizer authorizer)
        {
            this._authorizer = authorizer;
        }

        public Task Enter(VeniInteractionContext c)
        {
            var venue = c.Session.GetVenue();

            if (!this._authorizer.Authorize(c.Interaction.User.Id, Permission.EditManagers, venue).Authorized)
                return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);

            c.Session.RegisterMessageHandler(this.OnMessageReceived);
            return c.Interaction.RespondAsync("Who is/are the manager(s)? :heart:",
                                               new ComponentBuilder()
                                               .WithBackButton(c)
                                               .WithSkipButton<ConfirmVenueSessionState, ConfirmVenueSessionState>(c)
                                               .Build());
        }

        public Task OnMessageReceived(MessageVeniInteractionContext c)
        {
            var venue = c.Session.GetVenue();

            JArray discordIds = null;
            if (c.Prediction.Entities.ContainsKey("discord-id"))
                discordIds = c.Prediction.Entities["discord-id"] as JArray;

            if (discordIds == null || discordIds.Count() == 0)
                return c.Interaction.Channel.SendMessageAsync(MessageRepository.DontUnderstandResponses.PickRandom());

            venue.Managers = discordIds.Select(id => id.Value<string>()).ToList();
            return c.Session.MoveStateAsync<ConfirmVenueSessionState>(c);
        }

    }
}