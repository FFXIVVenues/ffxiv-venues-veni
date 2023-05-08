using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry
{
    class HaveScheduleEntrySessionState : ISessionState
    {

        private static string[] _messages = new[]
        {
            "Oki! Do you have a weekly opening schedule?",
            "Great! Does your venue have a weekly schedule for opening?",
            "Is your venue generally open at the same days and times every week?",
        };

        public Task Enter(VeniInteractionContext c)
        {
            return c.Interaction.RespondAsync(_messages.PickRandom(),
                new ComponentBuilder()
                    .WithBackButton(c)
                    .WithButton("Yes, we have a set weekly schedule",
                        c.Session.RegisterComponentHandler(cm => 
                            cm.Session.MoveStateAsync<TimeZoneEntrySessionState>(cm), 
                        ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("No, we don't have a set weekly schedule",
                        c.Session.RegisterComponentHandler(cm =>
                        {
                            var venue = c.Session.GetVenue();
                            venue.Openings = new();

                            if (cm.Session.GetItem<bool>("modifying"))
                                return cm.Session.MoveStateAsync<ConfirmVenueSessionState>(cm);
                            return cm.Session.MoveStateAsync<BannerEntrySessionState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }

    }
}
