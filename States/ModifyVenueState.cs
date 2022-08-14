using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.States.Abstractions;
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

        public Task Init(InteractionContext c)
        {
            c.Session.SetItem("modifying", true);

            var component = new ComponentBuilder()
                    .WithButton("Name", c.Session.RegisterComponentHandler(cm => cm.Session.ShiftState<NameEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Description", c.Session.RegisterComponentHandler(cm => cm.Session.ShiftState<DescriptionEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Location", c.Session.RegisterComponentHandler(cm => cm.Session.ShiftState<HouseOrApartmentEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Schedule", c.Session.RegisterComponentHandler(cm => cm.Session.ShiftState<HaveScheduleEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("N/SFW status", c.Session.RegisterComponentHandler(cm => cm.Session.ShiftState<SfwEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Tags", c.Session.RegisterComponentHandler(cm => cm.Session.ShiftState<CategoryEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Website", c.Session.RegisterComponentHandler(cm => cm.Session.ShiftState<WebsiteEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Discord", c.Session.RegisterComponentHandler(cm => cm.Session.ShiftState<DiscordEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Banner photo", c.Session.RegisterComponentHandler(cm => cm.Session.ShiftState<BannerInputState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary);

            if (this._indexersService.IsIndexer(c.Interaction.User.Id))
                component.WithButton("Managers", c.Session.RegisterComponentHandler(cm => cm.Session.ShiftState<ManagerEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary);

            return c.Interaction.RespondAsync($"What would you like to change? 🥰",
                component: component.Build());
        }

    }
}
