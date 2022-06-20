using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class SelectVenueToCloseState : IState
    {

        private static string[] _messages = new[]
        {
            "Which one are we closing?",
            "Sleepy time! Which venue are we closing?",
            "Which one?"
        };

        private static string[] _responses = new[]
        {
            "The doors are closed! I can't wait til next time. ♥️",
            "We're no longer green! We're all closed up.",
            "Okay! I'll lock up. 😉"
        };

        private IEnumerable<Venue> _contactsVenues;
        private IApiService _apiService;

        public SelectVenueToCloseState(IApiService apiService)
        {
            _apiService = apiService;
        }

        public Task Enter(MessageContext c)
        {
            _contactsVenues = c.Conversation.GetItem<IEnumerable<Venue>>("venues");
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(_messages.PickRandom());
            foreach (var venue in _contactsVenues)
            {
                var isLast = _contactsVenues.Last() == venue;
                if (isLast) stringBuilder.Append("or ");
                stringBuilder.Append(venue.Name);
                if (!isLast) stringBuilder.Append(", ");
                else stringBuilder.Append("?");
            }
            return c.SendMessageAsync(stringBuilder.ToString());
        }

        public async Task Handle(MessageContext c)
        {
            var (phrase, score) = c.Message.Content.StripMentions().IsSimilarToAnyPhrase(_contactsVenues.Select(v => v.Name));
            if (score < 0.35)
            {
                await c.SendMessageAsync(MessageRepository.DontUnderstandResponses.PickRandom());
                return;
            }

            var venue = _contactsVenues.FirstOrDefault(v => v.Name == phrase);

            c.Conversation.ClearItem("venues");
            c.Conversation.ClearState();
            await _apiService.CloseVenue(venue.Id);
            await c.SendMessageAsync(_responses.PickRandom());
        }
    }
}
