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
                    .WithButton("Name", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<NameEntryState>(cm)), ButtonStyle.Secondary)
                    .WithButton("Description", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<DescriptionEntryState>(cm)), ButtonStyle.Secondary)
                    .WithButton("Location", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<HouseOrApartmentEntryState>(cm)), ButtonStyle.Secondary)
                    .WithButton("Schedule", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<HaveScheduleEntryState>(cm)), ButtonStyle.Secondary)
                    .WithButton("N/SFW status", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<SfwEntryState>(cm)), ButtonStyle.Secondary)
                    .WithButton("Tags", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<TypeEntryState>(cm)), ButtonStyle.Secondary)
                    .WithButton("Website", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<WebsiteEntryState>(cm)), ButtonStyle.Secondary)
                    .WithButton("Discord", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<DiscordEntryState>(cm)), ButtonStyle.Secondary)
                    .WithButton("Banner photo", c.Conversation.RegisterComponentHandler(cm => cm.Conversation.ShiftState<BannerInputState>(cm)), ButtonStyle.Secondary)
                    .Build());
        }

        public Task OnMessageReceived(MessageContext c) => Task.CompletedTask;

    }
}
