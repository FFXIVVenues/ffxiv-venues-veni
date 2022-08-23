using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class NameEntryState : IState
    {
        public Task Enter(InteractionContext c)
        {
            c.Session.RegisterMessageHandler(this.OnMessageReceived);
            return c.Interaction.RespondAsync(MessageRepository.AskForNameMessage.PickRandom());
        }

        public Task OnMessageReceived(MessageInteractionContext c)
        {
            var venue = c.Session.GetItem<Venue>("venue");
            venue.Name = c.Interaction.Content.StripMentions();
            if (c.Session.GetItem<bool>("modifying"))
                return c.Session.MoveStateAsync<ConfirmVenueState>(c);
            return c.Session.MoveStateAsync<DescriptionEntryState>(c);
        }
    }
}
