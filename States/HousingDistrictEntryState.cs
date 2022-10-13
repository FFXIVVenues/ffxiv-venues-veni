using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class HousingDistrictEntryState : IState
    {
        public Task Enter(InteractionContext c)
        {
            var districts = new[] { "Mist", "Empyreum", "Goblet", "Lavender Beds", "Shirogane" }
                .Select(zone => new SelectMenuOptionBuilder(zone, zone)).ToList();
            var selectMenu = new SelectMenuBuilder();
            selectMenu.WithOptions(districts);
            selectMenu.WithCustomId(c.Session.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));


            return c.Interaction.RespondAsync(MessageRepository.AskForHousingDistrictMessage.PickRandom(), 
                                  new ComponentBuilder().WithSelectMenu(selectMenu).WithBackButton(c).Build());
        }

        public Task Handle(MessageComponentInteractionContext c)
        {
            var district = c.Interaction.Data.Values.Single();
            var venue = c.Session.GetItem<Venue>("venue");
            venue.Location.District = district;
            return c.Session.MoveStateAsync<WardEntryState>(c);
        }

    }
}
