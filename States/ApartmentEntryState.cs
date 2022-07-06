using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class ApartmentEntryState : IState
    {
        public Task Init(MessageContext c)
        {
            c.Conversation.RegisterMessageHandler(this.OnMessageReceived);
            return c.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForApartmentMessage.PickRandom()}");
        }

        public Task OnMessageReceived(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            if (!ushort.TryParse(c.Message.Content.StripMentions(), out var apartment) || apartment < 1)
                return c.RespondAsync("Sorry, I didn't understand that, please enter your apartment number.");

            venue.Location.Apartment = apartment;

            if (c.Conversation.GetItem<bool>("modifying"))
                return c.Conversation.ShiftState<ConfirmVenueState>(c);

            return c.Conversation.ShiftState<SfwEntryState>(c);
        }

    }
}
