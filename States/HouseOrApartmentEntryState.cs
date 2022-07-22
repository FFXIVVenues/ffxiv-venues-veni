using Discord;
using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class HouseOrApartmentEntryState : IState
    {
        public Task Init(MessageContext c)
        {
            return c.RespondAsync(MessageRepository.AskForHouseOrApartmentMessage.PickRandom(), new ComponentBuilder()
                .WithButton("A house", c.Conversation.RegisterComponentHandler(cm =>
                {
                    c.Conversation.SetItem("isHouse", true);
                    return c.Conversation.ShiftState<WorldEntryState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("An apartment", c.Conversation.RegisterComponentHandler(cm =>
                {
                    c.Conversation.SetItem("isHouse", false);
                    return c.Conversation.ShiftState<WorldEntryState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }
    }
}
