using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.SessionStates
{
    class LocationTypeEntrySessionState : ISessionState
    {
        public Task Enter(VeniInteractionContext c)
        {
            return c.Interaction.RespondAsync(MessageRepository.AskForHouseOrApartmentMessage.PickRandom(), new ComponentBuilder()
                .WithBackButton(c)
                .WithButton("A house", c.Session.RegisterComponentHandler(cm =>
                {
                    cm.Session.SetItem("locationType", "house");
                    return cm.Session.MoveStateAsync<DataCenterEntrySessionState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("A room in a house", c.Session.RegisterComponentHandler(cm =>
                {
                    cm.Session.SetItem("locationType", "room");
                    return cm.Session.MoveStateAsync<DataCenterEntrySessionState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("An apartment", c.Session.RegisterComponentHandler(cm =>
                {
                    cm.Session.SetItem("locationType", "apartment");
                    return cm.Session.MoveStateAsync<DataCenterEntrySessionState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("Other", c.Session.RegisterComponentHandler(cm =>
                {
                    cm.Session.SetItem("locationType", "other");
                    return cm.Session.MoveStateAsync<OtherLocationEntrySessionState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }
    }
}
