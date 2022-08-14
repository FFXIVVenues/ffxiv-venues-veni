using Discord.WebSocket;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class PlotEntryState : IState
    {
        public Task Init(InteractionContext c)
        {
            c.Session.RegisterMessageHandler(this.OnMessageReceived);
            return c.Interaction.RespondAsync(MessageRepository.AskForPlotMessage.PickRandom());
        }

        public Task OnMessageReceived(MessageInteractionContext c)
        {
            var venue = c.Session.GetItem<Venue>("venue");
            var match = new Regex("\\b\\d+\\b").Match(c.Interaction.Content.StripMentions());

            if (!match.Success || !ushort.TryParse(match.Value, out var plot) || plot < 1 || plot > 60)
            {
                return c.Interaction.Channel.SendMessageAsync("Sorry, I didn't understand that, please enter a number between 1 and 60.");
            }

            venue.Location.Room = 0;
            venue.Location.Apartment = 0;
            venue.Location.Plot = plot;
            venue.Location.Subdivision = plot > 30;

            if (c.Session.GetItem<bool>("modifying"))
                return c.Session.ShiftState<ConfirmVenueState>(c);

            return c.Session.ShiftState<SfwEntryState>(c);
        }
    }
}
