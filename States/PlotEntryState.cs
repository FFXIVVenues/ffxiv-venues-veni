using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class PlotEntryState : IState
    {
        public Task Enter(MessageContext c) =>
            c.SendMessageAsync(MessageRepository.AskForPlotMessage.PickRandom());

        public Task Handle(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            if (!int.TryParse(c.Message.Content.StripMentions(), out var plot) || plot < 1 || plot > 60)
            {
                return c.SendMessageAsync("Sorry, I didn't understand that, please enter a number between 1 and 60.");
            }

            venue.Location.Plot = plot;
            venue.Location.Subdivision = plot > 30;

            if (c.Conversation.GetItem<bool>("modifying"))
                return c.Conversation.ShiftState<ConfirmVenueState>(c);

            return c.Conversation.ShiftState<SfwEntryState>(c);
        }
    }
}
