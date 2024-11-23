using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

class WorldEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{

    public Task EnterState(VeniInteractionContext interactionContext)
    {
        var venue = interactionContext.Session.GetVenue();

        var worlds = FfxivWorlds.GetWorldsFor(venue.Location.DataCenter)
            .Select(w => new SelectMenuOptionBuilder(w, w)).ToList();
        var selectMenu = new SelectMenuBuilder();
        selectMenu.WithOptions(worlds);
        selectMenu.WithCustomId(interactionContext.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
        return interactionContext.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForWorldMessage.PickRandom()}", 
            new ComponentBuilder().WithSelectMenu(selectMenu).WithBackButton(interactionContext).Build());
    }

    public Task Handle(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        var world = c.Interaction.Data.Values.Single();
        venue.Location.World = world;
        return c.MoveSessionToStateAsync<HousingDistrictEntrySessionState, VenueAuthoringContext>(authoringContext);
    }

}