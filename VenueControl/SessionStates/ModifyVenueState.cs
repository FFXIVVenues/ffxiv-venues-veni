using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.People;
using FFXIVVenues.Veni.SessionStates;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueControl.SessionStates
{
    class ModifyVenueSessionState : ISessionState
    {

        private readonly IStaffService _staffService;

        public ModifyVenueSessionState(IStaffService staffService)
        {
            this._staffService = staffService;
        }

        public Task Enter(VeniInteractionContext c)
        {
            c.Session.SetItem("modifying", true);

            var component = new ComponentBuilder()
                    .WithButton("Name", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<NameEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Description", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<DescriptionEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Location", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<LocationTypeEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Schedule", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<HaveScheduleEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("N/SFW status", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<SfwEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Tags", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<CategoryEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Website", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<WebsiteEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Discord", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<DiscordEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Banner photo", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<BannerEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary);

            if (this._staffService.IsEditor(c.Interaction.User.Id))
                component.WithButton("Managers", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<ManagerEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary);
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
