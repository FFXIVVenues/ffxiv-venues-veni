using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class LocationTypeEntryState : IState
    {
        public Task Enter(InteractionContext c)
        {
            return c.Interaction.RespondAsync(MessageRepository.AskForHouseOrApartmentMessage.PickRandom(), new ComponentBuilder()
                .WithBackButton(c)
                .WithButton("A house", c.Session.RegisterComponentHandler(cm =>
                {
                    cm.Session.SetItem("locationType", "house");
                    return cm.Session.MoveStateAsync<WorldEntryState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("A room in a house", c.Session.RegisterComponentHandler(cm =>
                {
                    cm.Session.SetItem("locationType", "room");
                    return cm.Session.MoveStateAsync<WorldEntryState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("An apartment", c.Session.RegisterComponentHandler(cm =>
                {
                    cm.Session.SetItem("locationType", "apartment");
                    return cm.Session.MoveStateAsync<WorldEntryState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }
    }
}
