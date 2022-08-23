using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.States
{
    class HaveScheduleEntryState : IState
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
                            cm.Session.MoveStateAsync<TimeZoneEntryState>(cm), 
                        ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("No, we don't have a set weekly schedule",
                        c.Session.RegisterComponentHandler(cm =>
                        {
                            var venue = c.Session.GetItem<Venue>("venue");
                            venue.Openings = new();

                            if (cm.Session.GetItem<bool>("modifying"))
                                return cm.Session.MoveStateAsync<ConfirmVenueState>(cm);
                            return cm.Session.MoveStateAsync<BannerEntryState>(cm);
                        }, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }

    }
}
