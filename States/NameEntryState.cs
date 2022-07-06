using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class NameEntryState : IState
    {
        public Task Init(MessageContext c) =>
            c.RespondAsync(MessageRepository.AskForNameMessage.PickRandom());

        public Task OnMessageReceived(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            venue.Name = c.Message.Content.StripMentions();
            if (c.Conversation.GetItem<bool>("modifying"))
                return c.Conversation.ShiftState<ConfirmVenueState>(c);
            return c.Conversation.ShiftState<DescriptionEntryState>(c);
        }
    }
}
