using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.PropertyEntrySessionStates.LocationEntry;

class HousingDistrictEntrySessionState(VenueAuthoringContext authoringContext) : ISessionState<VenueAuthoringContext>
{
    public Task EnterState(VeniInteractionContext interactionContext)
    {
        var districts = new[] { "Mist", "Empyreum", "Goblet", "Lavender Beds", "Shirogane" }
            .Select(zone => new SelectMenuOptionBuilder(zone, zone)).ToList();
        var selectMenu = new SelectMenuBuilder();
        selectMenu.WithOptions(districts);
        selectMenu.WithCustomId(interactionContext.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));


        return interactionContext.Interaction.RespondAsync(MessageRepository.AskForHousingDistrictMessage.PickRandom(), 
            new ComponentBuilder().WithSelectMenu(selectMenu).WithBackButton(interactionContext).Build());
    }

    public Task Handle(ComponentVeniInteractionContext c)
    {
        var district = c.Interaction.Data.Values.Single();
        var venue = c.Session.GetVenue();
        venue.Location.District = district;
        return c.MoveSessionToStateAsync<WardEntrySessionState, VenueAuthoringContext>(authoringContext);
    }

}