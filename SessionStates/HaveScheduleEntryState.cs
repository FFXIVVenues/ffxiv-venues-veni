using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Session;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.SessionStates
{
    class HaveScheduleEntrySessionState : ISessionState
    {

        private static string[] _messages = new[]
        {
            "Oki! Do you have a weekly opening schedule?",
            "Great! Does your venue have a weekly schedule for opening?",
            "Is your venue generally open at the same days and times every week?",
        };

        public Task Enter(InteractionContext c)
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
                            var venue = c.Session.GetItem<Venue>("venue");
                            venue.Openings = new();

                            if (cm.Session.GetItem<bool>("modifying"))
                                return cm.Session.MoveStateAsync<ConfirmVenueSessionState>(cm);
                            return cm.Session.MoveStateAsync<BannerEntrySessionState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }

    }
}
