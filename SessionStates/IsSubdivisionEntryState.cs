using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Session;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.SessionStates
{
    class IsSubdivisionEntrySessionState : ISessionState
    {
        public Task Enter(VeniInteractionContext c)
        {
            return c.Interaction.RespondAsync(MessageRepository.AskForSubdivisionMessage.PickRandom(), new ComponentBuilder()
                .WithBackButton(c)
                .WithButton("Yes, it's subdivision", c.Session.RegisterComponentHandler(cm =>
                {
                    var venue = cm.Session.GetItem<Venue>("venue");
                    venue.Location.Subdivision = true;
                    return cm.Session.MoveStateAsync<ApartmentEntrySessionState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("No, the first division", c.Session.RegisterComponentHandler(cm =>
                {
                    var venue = cm.Session.GetItem<Venue>("venue");
                    venue.Location.Subdivision = false;
                    return cm.Session.MoveStateAsync<ApartmentEntrySessionState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }
    }
}
