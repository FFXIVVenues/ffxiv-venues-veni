using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class WardEntryState : IState
    {
        public Task Init(MessageContext c)
        {
            c.Conversation.RegisterMessageHandler(this.OnMessageReceived);
            return c.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForWardMessage.PickRandom()}");
        }

        public Task OnMessageReceived(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            var match = new Regex("\\b\\d+\\b").Match(c.Message.Content.StripMentions());

            if (!match.Success || !ushort.TryParse(match.Value, out var ward) || ward < 1 || ward > 24)
            {
                return c.RespondAsync("Sorry, I didn't understand that, please enter a number between 1 and 24.");
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
