using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.TagsEntry;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates
{
    class SfwEntrySessionState : ISessionState
    {
        public Task Enter(VeniInteractionContext c)
        {
            return c.Interaction.RespondAsync(MessageRepository.AskForSfwMessage.PickRandom(), new ComponentBuilder()
                .WithBackButton(c)
                .WithButton("Yes, it's safe on entry", c.Session.RegisterComponentHandler(cm =>
                {
                    var venue = cm.Session.GetVenue();
                    venue.Sfw = true;
                    if (cm.Session.GetItem<bool>("modifying"))
                        return cm.Session.MoveStateAsync<ConfirmVenueSessionState>(cm);
                    return cm.Session.MoveStateAsync<CategoryEntrySessionState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("No, we're openly NSFW", c.Session.RegisterComponentHandler(cm =>
                {
                    var venue = cm.Session.GetVenue();
                    venue.Sfw = false;
                    if (cm.Session.GetItem<bool>("modifying"))
                        return cm.Session.MoveStateAsync<ConfirmVenueSessionState>(cm);
                    return cm.Session.MoveStateAsync<CategoryEntrySessionState>(cm);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }

    }

}
