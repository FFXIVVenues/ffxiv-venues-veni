using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Session;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.SessionStates
{
    class HousingDistrictEntrySessionState : ISessionState
    {
        public Task Enter(VeniInteractionContext c)
        {
            var districts = new[] { "Mist", "Empyreum", "Goblet", "Lavender Beds", "Shirogane" }
                .Select(zone => new SelectMenuOptionBuilder(zone, zone)).ToList();
            var selectMenu = new SelectMenuBuilder();
            selectMenu.WithOptions(districts);
            selectMenu.WithCustomId(c.Session.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));


            return c.Interaction.RespondAsync(MessageRepository.AskForHousingDistrictMessage.PickRandom(), 
                                  new ComponentBuilder().WithSelectMenu(selectMenu).WithBackButton(c).Build());
        }

        public Task Handle(MessageComponentVeniInteractionContext c)
        {
            var district = c.Interaction.Data.Values.Single();
            var venue = c.Session.GetItem<Venue>("venue");
            venue.Location.District = district;
            return c.Session.MoveStateAsync<WardEntrySessionState>(c);
        }

    }
}
