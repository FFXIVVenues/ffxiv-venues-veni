using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class ApartmentEntryState : IState
    {
        public Task Enter(InteractionContext c)
        {
            c.Session.RegisterMessageHandler(this.OnMessageReceived);
            return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForApartmentMessage.PickRandom()}",
                                                new ComponentBuilder().WithBackButton(c).Build());
        }

        public Task OnMessageReceived(MessageInteractionContext c)
        {
            var venue = c.Session.GetItem<Venue>("venue");
            var match = new Regex("\\b\\d+\\b").Match(c.Interaction.Content.StripMentions());
            if (!match.Success || !ushort.TryParse(match.Value, out var apartment) || apartment < 1)
                return c.Interaction.Channel.SendMessageAsync("Sorry, I didn't understand that, please enter your apartment number.");

            venue.Location.Plot = 0;
            venue.Location.Room = 0;
            venue.Location.Apartment = apartment;

            if (c.Session.GetItem<bool>("modifying"))
                return c.Session.MoveStateAsync<ConfirmVenueState>(c);

            return c.Session.MoveStateAsync<SfwEntryState>(c);
        }

    }
}
