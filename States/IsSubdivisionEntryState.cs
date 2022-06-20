using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class IsSubdivisionEntryState : IState
    {
        public Task Enter(MessageContext c) =>
            c.SendMessageAsync(MessageRepository.AskForSubdivisionMessage.PickRandom());

        public Task Handle(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");

            var message = c.Message.Content.StripMentions().ToLower();
            venue.Location.Subdivision = message.Contains("y") || message.Contains("true");

            return c.Conversation.ShiftState<ApartmentEntryState>(c);
        }
    }
}
