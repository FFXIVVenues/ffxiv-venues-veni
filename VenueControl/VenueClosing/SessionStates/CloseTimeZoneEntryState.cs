using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueClosing.SessionStates;

internal class CloseTimeZoneEntryState : ISessionState
{
       public Task EnterState(VeniInteractionContext interactionContext)
       {
           var component = new ComponentBuilder();
           var timezoneOptions = TimeZones.SupportedTimeZones.Select(dc => new SelectMenuOptionBuilder(dc.TimeZoneLabel, dc.TimeZoneKey)).ToList();
           var selectMenu = new SelectMenuBuilder();
           selectMenu.WithOptions(timezoneOptions);
           selectMenu.WithCustomId(interactionContext.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
               
           return interactionContext.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} { VenueControlStrings.AskForClosingTimeZone}",
               component.WithSelectMenu(selectMenu).WithBackButton(interactionContext).Build());
       }
   
       private Task Handle(ComponentVeniInteractionContext c)
       {
           var selectedTimezone = c.Interaction.Data.Values.Single();
           c.Session.SetItem(SessionKeys.TIMEZONE_ID, selectedTimezone);
           return c.MoveSessionToStateAsync<CloseDayEntryState>();
       }
}

