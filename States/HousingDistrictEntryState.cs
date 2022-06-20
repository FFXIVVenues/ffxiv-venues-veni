using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class HousingDistrictEntryState : IState
    {
        public Task Enter(MessageContext c) =>
            c.SendMessageAsync(MessageRepository.AskForHousingDistrictMessage.PickRandom());

        public Task Handle(MessageContext c)
        {
            var venue = c.Conversation.GetItem<Venue>("venue");
            var result = c.Message.Content.StripMentions().IsSimilarToAnyPhrase("Mist", "Empyreum", "Goblet", "Lavender Beds", "Shirogane");
            if (result.Score < 0.5)
                return c.SendMessageAsync(MessageRepository.DontUnderstandResponses.PickRandom());

            venue.Location.District = result.Phrase;
            return c.Conversation.ShiftState<WardEntryState>(c);
        }

    }
}
