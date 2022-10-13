using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class WorldEntryState : IState
    {

        public Task Enter(InteractionContext c)
        {
            var venue = c.Session.GetItem<Venue>("venue");

            var worlds = FfxivWorlds.GetWorldsFor(venue.Location.DataCenter)
                .Select(w => new SelectMenuOptionBuilder(w, w)).ToList();
            var selectMenu = new SelectMenuBuilder();
            selectMenu.WithOptions(worlds);
            selectMenu.WithCustomId(c.Session.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
            return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForWorldMessage.PickRandom()}", 
                                  new ComponentBuilder().WithSelectMenu(selectMenu).WithBackButton(c).Build());
        }

        public Task Handle(MessageComponentInteractionContext c)
        {
            var venue = c.Session.GetItem<Venue>("venue");
            var world = c.Interaction.Data.Values.Single();
            venue.Location.World = world;
            return c.Session.MoveStateAsync<HousingDistrictEntryState>(c);
        }

    }
}
