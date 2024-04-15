using System.Linq;
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
            c.Session.ClearItem(SessionKeys.IS_BIWEEKLY_SCHEDULE);
            c.Session.ClearItem(SessionKeys.IS_MONTHLY_SCHEDULE);
            
            var selectMenu = new SelectMenuBuilder();
            selectMenu.AddOption(VenueControlStrings.OptionWeekly, "weekly", VenueControlStrings.DescriptionHasWeeklySchedule);
            selectMenu.AddOption(VenueControlStrings.OptionBiweekly, "biweekly", VenueControlStrings.DescriptionHasBiweeklySchedule);
            selectMenu.AddOption(VenueControlStrings.OptionMonthly, "monthly", VenueControlStrings.DescriptionHasMonthlySchedule);
            selectMenu.AddOption(VenueControlStrings.OptionNoSchedule, "none", VenueControlStrings.DescriptionHasNoSchedule);
            selectMenu.WithCustomId(c.Session.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
            return c.Interaction.RespondAsync(VenueControlStrings.AskIfHasSchedule,
                new ComponentBuilder()
                    .WithSelectMenu(selectMenu)
                    .WithBackButton(c)
                .Build());
        }
        
        private Task Handle(ComponentVeniInteractionContext c) =>
            c.Interaction.Data.Values.Single() switch
            {
                "weekly" => WeeklySchedule(c),
                "biweekly" => BiweeklySchedule(c),
                "monthly" => MonthlySchedule(c),
                _ => NoWeeklySchedule(c)
            };

        private static Task WeeklySchedule(ComponentVeniInteractionContext c) =>
            c.Session.MoveStateAsync<TimeZoneEntrySessionState>(c);
        
        private static async Task BiweeklySchedule(ComponentVeniInteractionContext c)
        {
            c.Session.SetScheduleAsBiweekly();
            await c.Session.MoveStateAsync<TimeZoneEntrySessionState>(c);
        }

        private static async Task MonthlySchedule(ComponentVeniInteractionContext c)
        {
            c.Session.SetScheduleAsMonthly();
            await c.Session.MoveStateAsync<TimeZoneEntrySessionState>(c);
        }

        private static Task NoWeeklySchedule(ComponentVeniInteractionContext c)
        {
            var venue = c.Session.GetVenue();
            venue.Schedule = new();

            return c.Session.InEditing() 
                ? c.Session.MoveStateAsync<ConfirmVenueSessionState>(c) 
                : c.Session.MoveStateAsync<BannerEntrySessionState>(c);
        }
        
    }
}
