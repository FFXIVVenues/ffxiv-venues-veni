using Discord;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Managers;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class ModifyVenueState : IState
    {

        private readonly IStaffManager _staffService;

        public ModifyVenueState(IStaffManager staffService)
        {
            this._staffService = staffService;
        }

        public Task Enter(InteractionContext c)
        {
            c.Session.SetItem("modifying", true);

            var component = new ComponentBuilder()
                    .WithButton("Name", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<NameEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Description", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<DescriptionEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Location", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<LocationTypeEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Schedule", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<HaveScheduleEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("N/SFW status", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<SfwEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Tags", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<CategoryEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Website", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<WebsiteEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Discord", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<DiscordEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Banner photo", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<BannerEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary);

            if (this._staffService.IsEditor(c.Interaction.User.Id))
                component.WithButton("Managers", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<ManagerEntryState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary);
            else
                component.WithButton("Managers", c.Session.RegisterComponentHandler(cm => {
                    cm.Interaction.FollowupAsync("Sowwy. You'll need to speak my owners at FFXIV Venues to change managers on your venue. 🥲");
                    return Enter(c);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary);

            if (c.Interaction.IsDM)
                return c.Interaction.RespondAsync(MessageRepository.EditVenueMessage.PickRandom(), component: component.Build());
            else
                return c.Interaction.RespondAsync(MessageRepository.EditVenueMessage.PickRandom(),
                                                  embed: new EmbedBuilder
                                                  {
                                                      Color = Color.Red,
                                                      Description = MessageRepository.MentionOrReplyToMeMessage.PickRandom()
                                                  }.Build(),
                                                  component: component.Build());
        }

    }
}
