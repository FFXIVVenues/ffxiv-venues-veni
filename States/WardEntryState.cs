using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class WardEntryState : IState
    {
        public Task Enter(MessageContext c) =>
            c.SendMessageAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForWardMessage.PickRandom()}");

        public Task Handle(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            if (!int.TryParse(c.Message.Content.StripMentions().Trim(), out var ward) || ward < 1 || ward > 24)
            {
                return c.SendMessageAsync("Sorry, I didn't understand that, please enter a number between 1 and 24.");
            }

            venue.Location.Ward = ward;

            var isHouse = c.Conversation.GetItem<bool>("isHouse");
            if (isHouse)
                return c.Conversation.ShiftState<PlotEntryState>(c);
            else
                return c.Conversation.ShiftState<IsSubdivisionEntryState>(c);
        }
    }
}
