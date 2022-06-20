using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class HouseOrApartmentEntryState : IState
    {
        public Task Enter(MessageContext c) =>
            c.SendMessageAsync(MessageRepository.AskForHouseOrApartmentMessage.PickRandom());

        public Task Handle(MessageContext c)
        {
            if (c.Message.Content.StripMentions().AnyWordMatchesAnyPhrase("house", "mansion", "estate", "cottage", "large", "medium", "small"))
                c.Conversation.SetItem("isHouse", true);
            else if (c.Message.Content.StripMentions().AnyWordMatchesAnyPhrase("apartment", "hotel", "flat"))
                c.Conversation.SetItem("isHouse", false);
            else
                return c.SendMessageAsync(MessageRepository.DontUnderstandResponses.PickRandom());

            return c.Conversation.ShiftState<WorldEntryState>(c);
        }
    }
}
