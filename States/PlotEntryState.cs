using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class PlotEntryState : IState
    {
        public Task Init(MessageContext c)
        {
            c.Conversation.RegisterMessageHandler(this.OnMessageReceived);
            return c.RespondAsync(MessageRepository.AskForPlotMessage.PickRandom());
        }

        public Task OnMessageReceived(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            var match = new Regex("\\b\\d+\\b").Match(c.Message.Content.StripMentions());

            if (!match.Success || !ushort.TryParse(match.Value, out var plot) || plot < 1 || plot > 60)
            {
                return c.RespondAsync("Sorry, I didn't understand that, please enter a number between 1 and 60.");
            }

            venue.Location.Plot = plot;
            venue.Location.Subdivision = plot > 30;

            if (c.Conversation.GetItem<bool>("modifying"))
                return c.Conversation.ShiftState<ConfirmVenueState>(c);

            return c.Conversation.ShiftState<SfwEntryState>(c);
        }
    }
}
