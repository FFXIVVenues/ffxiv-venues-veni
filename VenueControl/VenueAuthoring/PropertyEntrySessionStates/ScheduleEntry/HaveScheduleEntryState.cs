using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry;

class HaveScheduleEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{

    public Task EnterState(VeniInteractionContext interactionContext)
    {
        interactionContext.Session.ClearItem(SessionKeys.IS_BIWEEKLY_SCHEDULE);
        interactionContext.Session.ClearItem(SessionKeys.IS_MONTHLY_SCHEDULE);
            
        var selectMenu = new SelectMenuBuilder();
        selectMenu.AddOption(VenueControlStrings.OptionWeekly, "weekly", VenueControlStrings.DescriptionHasWeeklySchedule);
        selectMenu.AddOption(VenueControlStrings.OptionBiweekly, "biweekly", VenueControlStrings.DescriptionHasBiweeklySchedule);
        selectMenu.AddOption(VenueControlStrings.OptionMonthly, "monthly", VenueControlStrings.DescriptionHasMonthlySchedule);
        selectMenu.AddOption(VenueControlStrings.OptionNoSchedule, "none", VenueControlStrings.DescriptionHasNoSchedule);
        selectMenu.WithCustomId(interactionContext.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
        return interactionContext.Interaction.RespondAsync(VenueControlStrings.AskIfHasSchedule,
            new ComponentBuilder()
                .WithSelectMenu(selectMenu)
                .WithBackButton(interactionContext)
                .Build());
    }

    private Task Handle(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Schedule = new();
        return c.Interaction.Data.Values.Single() switch
        {
            "weekly" => c.MoveSessionToStateAsync<TimeZoneEntrySessionState, VenueAuthoringContext>(authoringContext),
            "biweekly" => BiweeklySchedule(c),
            "monthly" => MonthlySchedule(c),
            "none" or _ => c.Session.InEditing() 
                ? c.MoveSessionToStateAsync<ConfirmVenueSessionState, VenueAuthoringContext>(authoringContext) 
                : c.MoveSessionToStateAsync<BannerEntrySessionState, VenueAuthoringContext>(authoringContext)
        };
    }

    private async Task BiweeklySchedule(ComponentVeniInteractionContext c)
    {
        c.Session.SetScheduleAsBiweekly();
        await c.MoveSessionToStateAsync<TimeZoneEntrySessionState, VenueAuthoringContext>(authoringContext);
    }

    private async Task MonthlySchedule(ComponentVeniInteractionContext c)
    {
        c.Session.SetScheduleAsMonthly();
        await c.MoveSessionToStateAsync<TimeZoneEntrySessionState, VenueAuthoringContext>(authoringContext);
    }
        
}