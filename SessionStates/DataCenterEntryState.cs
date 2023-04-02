using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.SessionStates
{
    class DataCenterEntrySessionState : ISessionState
    {

        public Task Enter(VeniInteractionContext c)
        {
            var dataCenters = FfxivWorlds.GetDataCentersFor(FfxivWorlds.GetSupportedRegions())
                .Select(dc => new SelectMenuOptionBuilder(dc, dc)).ToList();
            var selectMenu = new SelectMenuBuilder();
            selectMenu.WithOptions(dataCenters);
            selectMenu.WithCustomId(c.Session.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));
            
            return c.Interaction.RespondAsync($"{MessageRepository.ConfirmMessage.PickRandom()} {MessageRepository.AskForDataCenterMessage.PickRandom()}", 
                                  new ComponentBuilder().WithSelectMenu(selectMenu).WithBackButton(c).Build());
        }

        private Task Handle(MessageComponentVeniInteractionContext c)
        {
            var dataCenter = c.Session.GetItem<Venue>("venue");
            var world = c.Interaction.Data.Values.Single();
            dataCenter.Location.DataCenter = world;
            return c.Session.MoveStateAsync<WorldEntrySessionState>(c);
        }

    }
}
