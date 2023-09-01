using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.MareEntry;

class HasMareEntrySessionState : ISessionState
{

    public Task Enter(VeniInteractionContext c)
    {
        return c.Interaction.RespondAsync(VenueControlStrings.AskIfHasMareMessage,
            new ComponentBuilder()
                .WithBackButton(c)
                .WithButton("Yes", c.Session.RegisterComponentHandler(cm =>
                    cm.Session.MoveStateAsync<MareIdEntryState>(cm),
                    ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("No",
                     c.Session.RegisterComponentHandler(cm =>
                        {
                            var venue = c.Session.GetVenue();
                            venue.MareCode = venue.MarePassword = null;

                            if (cm.Session.GetItem<bool>("modifying"))
                                return cm.Session.MoveStateAsync<ConfirmVenueSessionState>(cm);
                            return cm.Session.MoveStateAsync<SfwEntrySessionState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
    }


}