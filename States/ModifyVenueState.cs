using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class ModifyVenueState : IState
    {

        private readonly IIndexersService _indexersService;

        public ModifyVenueState(IIndexersService indexersService)
        {
            this._indexersService = indexersService;
        }

        public Task Init(MessageContext c)
        {
            c.Conversation.SetItem("modifying", true);

            var component = new ComponentBuilder()
                    .WithButton("Name", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<NameEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Description", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<DescriptionEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Location", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<HouseOrApartmentEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Schedule", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<HaveScheduleEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("N/SFW status", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<SfwEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Tags", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<CategoryEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Website", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<WebsiteEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Discord", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<DiscordEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Banner photo", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<BannerInputState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary);

            if (this._indexersService.IsIndexer(c.User.Id))
                component.WithButton("Managers", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<ManagerEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary);

            return c.RespondAsync($"What would you like to change? 🥰",
                component: component.Build());
        }

    }
}
