using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Models;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class DataCenterEntryState : IState
    {

        public Task Enter(InteractionContext c)
        {
            var dataCenters = FfxivWorlds.GetDataCentersFor(FfxivWorlds.GetSupportedRegions())
                .Select(dc => new SelectMenuOptionBuilder(dc, dc)).ToList();
            var selectMenu = new SelectMenuBuilder();
            selectMenu.WithOptions(dataCenters);
            selectMenu.WithCustomId(c.Session.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
            return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForDataCenterMessage.PickRandom()}", 
                                  new ComponentBuilder().WithSelectMenu(selectMenu).WithBackButton(c).Build());
        }

        private Task Handle(MessageComponentInteractionContext c)
        {
            var dataCenter = c.Session.GetItem<Venue>("venue");
            var world = c.Interaction.Data.Values.Single();
            dataCenter.Location.DataCenter = world;
            return c.Session.MoveStateAsync<WorldEntryState>(c);
        }

    }
}
