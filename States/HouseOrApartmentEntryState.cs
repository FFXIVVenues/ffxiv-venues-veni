using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class HouseOrApartmentEntryState : IState
    {
        public Task Init(InteractionContext c)
        {
            return c.Interaction.RespondAsync(MessageRepository.AskForHouseOrApartmentMessage.PickRandom(), new ComponentBuilder()
                .WithButton("A house", c.Session.RegisterComponentHandler(cm =>
                {
                    cm.Session.SetItem("isHouse", true);
                    return cm.Session.ShiftState<WorldEntryState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("An apartment", c.Session.RegisterComponentHandler(cm =>
                {
                    cm.Session.SetItem("isHouse", false);
                    return cm.Session.ShiftState<WorldEntryState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }
    }
}
