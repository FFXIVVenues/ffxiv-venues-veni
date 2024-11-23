using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

class DataCenterEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{

    public Task EnterState(VeniInteractionContext interactionContext)
    {
        var dataCenters = FfxivWorlds.GetDataCentersFor(FfxivWorlds.GetSupportedRegions())
            .Select(dc => new SelectMenuOptionBuilder(dc, dc)).ToList();
        var selectMenu = new SelectMenuBuilder();
        selectMenu.WithOptions(dataCenters);
        selectMenu.WithCustomId(interactionContext.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
        return interactionContext.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForDataCenterMessage.PickRandom()}", 
            new ComponentBuilder().WithSelectMenu(selectMenu).WithBackButton(interactionContext).Build());
    }

    private Task Handle(ComponentVeniInteractionContext c)
    {
        var venue = c.Session.GetVenue();
        venue.Location.Override = null;
        var world = c.Interaction.Data.Values.Single();
        venue.Location.DataCenter = world;
        return c.MoveSessionToStateAsync<WorldEntrySessionState, VenueAuthoringContext>(authoringContext);
    }

}