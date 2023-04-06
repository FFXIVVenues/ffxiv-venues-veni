using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.SessionStates;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.SessionStates
{
    class ModifyVenueSessionState : ISessionState
    {
        private readonly IAuthorizer _authorizer;

        public ModifyVenueSessionState(IAuthorizer authorizer)
        {
            this._authorizer = authorizer;
        }

        public Task Enter(VeniInteractionContext c)
        {
            c.Session.SetItem("modifying", true);
            var venue = c.Session.GetItem<Venue>("venue");

            var component = new ComponentBuilder()
                    .WithButton("Name", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<NameEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Description", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<DescriptionEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Location", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<LocationTypeEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Schedule", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<HaveScheduleEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("N/SFW status", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<SfwEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Tags", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<CategoryEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Website", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<WebsiteEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Discord", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<DiscordEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary);
            
            if (this._authorizer.Authorize(c.Interaction.User.Id, Permission.EditPhotography, venue).Authorized)
                component.WithButton("Banner photo", c.Session.RegisterComponentHandler(cm => cm.Session.MoveStateAsync<BannerEntrySessionState>(cm), ComponentPersistence.ClearRow), ButtonStyle.Secondary);
            else
                component.WithButton("Banner photo", c.Session.RegisterComponentHandler(cm => {
                    cm.Interaction.FollowupAsync("Aaaah. You'll need to speak my owners at FFXIV Venues to change the banner photo for your venue. 🥲");
                    return Enter(c);
                }, ComponentPersistence.ClearRow), ButtonStyle.Secondary);

            if (this._authorizer.Authorize(c.Interaction.User.Id, Permission.EditManagers, venue).Authorized)
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
