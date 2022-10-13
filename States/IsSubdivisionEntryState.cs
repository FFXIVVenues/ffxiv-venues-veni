using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class IsSubdivisionEntryState : IState
    {
        public Task Enter(InteractionContext c)
        {
            return c.Interaction.RespondAsync(MessageRepository.AskForSubdivisionMessage.PickRandom(), new ComponentBuilder()
                .WithBackButton(c)
                .WithButton("Yes, it's subdivision", c.Session.RegisterComponentHandler(cm =>
                {
                    var venue = cm.Session.GetItem<Venue>("venue");
                    venue.Location.Subdivision = true;
                    return cm.Session.MoveStateAsync<ApartmentEntryState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("No, the first division", c.Session.RegisterComponentHandler(cm =>
                {
                    var venue = cm.Session.GetItem<Venue>("venue");
                    venue.Location.Subdivision = false;
                    return cm.Session.MoveStateAsync<ApartmentEntryState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }
    }
}
