using Discord;
using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class HousingDistrictEntryState : IState
    {
        public Task Init(MessageContext c)
        {
            var districts = new[] { "Mist", "Empyreum", "Goblet", "Lavender Beds", "Shirogane" }
                .Select(zone => new SelectMenuOptionBuilder(zone, zone)).ToList();
            var selectMenu = new SelectMenuBuilder();
            selectMenu.WithOptions(districts);
            selectMenu.WithCustomId(c.Conversation.RegisterComponentHandler(Handle, ComponentPersistence.ClearRow));


            return c.RespondAsync(MessageRepository.AskForHousingDistrictMessage.PickRandom(), 
                                  new ComponentBuilder().WithSelectMenu(selectMenu).Build());
        }

        public Task Handle(MessageContext c)
        {
            var district = c.MessageComponent.Data.Values.Single();
            var venue = c.Conversation.GetItem<Venue>("venue");
            venue.Location.District = district;
            return c.Conversation.ShiftState<WardEntryState>(c);
        }

    }
}
