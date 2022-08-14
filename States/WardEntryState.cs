using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class WardEntryState : IState
    {
        public Task Init(InteractionContext c)
        {
            c.Session.RegisterMessageHandler(this.OnMessageReceived);
            return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForWardMessage.PickRandom()}");
        }

        public Task OnMessageReceived(MessageInteractionContext c)
        {
            var venue = c.Session.GetItem<Venue>("venue");
            var match = new Regex("\\b\\d+\\b").Match(c.Interaction.Content.StripMentions());

            if (!match.Success || !ushort.TryParse(match.Value, out var ward) || ward < 1 || ward > 24)
                return c.Interaction.Channel.SendMessageAsync("Sorry, I didn't understand that, please enter a number between 1 and 24.");

            venue.Location.Ward = ward;

            var isHouse = c.Session.GetItem<bool>("isHouse");
            if (isHouse)
                return c.Session.ShiftState<PlotEntryState>(c);
            else
                return c.Session.ShiftState<IsSubdivisionEntryState>(c);
        }
    }
}
