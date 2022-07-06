using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class DescriptionEntryState : IState
    {
        public Task Init(MessageContext c) =>
            c.RespondAsync(MessageRepository.AskForDescriptionMessage.PickRandom());

        public Task OnMessageReceived(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            venue.Description = c.Message.Content.StripMentions().AsListOfParagraphs();
            if (c.Conversation.GetItem<bool>("modifying"))
                return c.Conversation.ShiftState<ConfirmVenueState>(c);
            return c.Conversation.ShiftState<HouseOrApartmentEntryState>(c);
        }

    }
}
