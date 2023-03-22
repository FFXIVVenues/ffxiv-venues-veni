using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Session;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.SessionStates
{
    class WardEntrySessionState : ISessionState
    {
        public Task Enter(VeniInteractionContext c)
        {
            c.Session.RegisterMessageHandler(this.OnMessageReceived);
            return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForWardMessage.PickRandom()}",
                                                new ComponentBuilder()
                                                .WithBackButton(c)
                                                .Build());
        }

        public Task OnMessageReceived(MessageVeniInteractionContext c)
        {
            var venue = c.Session.GetItem<Venue>("venue");
            var match = new Regex("\\b\\d+\\b").Match(c.Interaction.Content.StripMentions());

            if (!match.Success || !ushort.TryParse(match.Value, out var ward) || ward < 1 || ward > 30)
                return c.Interaction.Channel.SendMessageAsync("Sorry, I didn't understand that, please enter a number between 1 and 30.");

            venue.Location.Ward = ward;

            var locationType = c.Session.GetItem<string>("locationType");
            if (locationType == "house" || locationType == "room")
                return c.Session.MoveStateAsync<PlotEntrySessionState>(c);
            else
                return c.Session.MoveStateAsync<IsSubdivisionEntrySessionState>(c);
        }
    }
}
