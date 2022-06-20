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
    class SelectVenueToOpenState : IState
    {

        private static string[] _messages = new[]
        {
            "Wooo! Which one are we opening?",
            "Yaay! 🥳 Which venue are we opening?",
            "🎉 Which one?"
        };

        private static string[] _responses = new[]
        {
            "Woo! The doors are open. You're green and announcements have been sent! Let's have fun today! ♥️",
            "Yay! It's that time again. 😀 You're all green on the index, and everyone's been notified. ♥️",
            "Let's do it! We... are... live!!! We're green on the index and the pings are flying! So excited. 🙂"
        };


        private IEnumerable<Venue> _contactsVenues;
        private IApiService _apiService;

        public SelectVenueToOpenState(IApiService apiService)
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
            await _apiService.OpenVenue(venue.Id);
            await c.SendMessageAsync(_responses.PickRandom());
        }
    }
}
