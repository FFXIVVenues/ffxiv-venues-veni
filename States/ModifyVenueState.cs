using Discord;
using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class ModifyVenueState : IState
    {

        public Task Init(MessageContext c)
        {
            c.Conversation.SetItem("modifying", true);

            return c.RespondAsync($"What would you like to change",
                component: new ComponentBuilder()
                    .WithButton("Name", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<NameEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Description", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<DescriptionEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Location", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<HouseOrApartmentEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Schedule", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<HaveScheduleEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("N/SFW status", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<SfwEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Tags", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<TypeEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Website", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<WebsiteEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Discord", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<DiscordEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Banner photo", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<BannerInputState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .Build());
        }

    }
}
