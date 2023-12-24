using System;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry
{
    class HaveScheduleEntrySessionState : ISessionState
    {

        public Task Enter(VeniInteractionContext c)
        {
            return c.Interaction.RespondAsync(VenueControlStrings.AskIfHasSchedule,
                new ComponentBuilder()
                    .WithBackButton(c)
                    .WithButton(VenueControlStrings.AnswerHasWeeklySchedule,
                        c.Session.RegisterComponentHandler(cm => 
                            cm.Session.MoveStateAsync<TimeZoneEntrySessionState>(cm), 
                        ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton(VenueControlStrings.AnswerHasBiweeklySchedule,
                        c.Session.RegisterComponentHandler(async cm =>
                            {
                                cm.Session.SetItem("hasBiweeklySchedule", true);
                                await cm.Interaction.Channel.SendMessageAsync(VenueControlStrings.RespondContactStaffForBiweekly);
                                await NoWeeklySchedule(c, cm);
                            }, 
                            ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton(VenueControlStrings.AnswerHasNoSchedule,
                        c.Session.RegisterComponentHandler(cm => NoWeeklySchedule(c, cm), 
                            ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .Build());
        }

        private static async Task NoWeeklySchedule(VeniInteractionContext c, MessageComponentVeniInteractionContext cm)
        {
            var venue = c.Session.GetVenue();
            venue.Schedule = new();

            if (cm.Session.GetItem<bool>("modifying"))
                await cm.Session.MoveStateAsync<ConfirmVenueSessionState>(cm);
            else
                await cm.Session.MoveStateAsync<BannerEntrySessionState>(cm);
        }
    }
}
