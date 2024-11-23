using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.ScheduleEntry;

class TimeZoneEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{

    private static string[] _messages = new[]
    {
        "What **time zone** would you like to give opening times in?",
        "What **time zone** would the venues opening times be in?"
    };

    public Task EnterState(VeniInteractionContext interactionContext)
    {
        var component = new ComponentBuilder();
        var timezoneOptions = TimeZones.SupportedTimeZones.Select(dc => new SelectMenuOptionBuilder(dc.TimeZoneLabel, dc.TimeZoneKey)).ToList();
        var selectMenu = new SelectMenuBuilder();
        selectMenu.WithOptions(timezoneOptions);
        selectMenu.WithCustomId(interactionContext.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
        return interactionContext.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} { _messages.PickRandom()}",
            component.WithSelectMenu(selectMenu).WithBackButton(interactionContext).Build());
    }

    private Task Handle(ComponentVeniInteractionContext c)
    {
        var selectedTimezone = c.Interaction.Data.Values.Single();
        c.Session.SetItem(SessionKeys.TIMEZONE_ID, selectedTimezone);
        return c.MoveSessionToStateAsync<DaysEntrySessionState, VenueAuthoringContext>(authoringContext);
    }
}