using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Api.Models;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Managers;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Utils;
using System.Linq;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    class ConfirmVenueState : IState
    {
        private readonly IApiService _apiService;
        private readonly IStaffService _staffService;
        private readonly IGuildManager _guildManager;
        private readonly string _uiUrl;
        private readonly string _apiUrl;

        private static string[] _preexisingResponse = new[]
        {
            "Wooo! All updated!",
            "All done for you! 🥳",
            "Ok, that's updated for you! 😊"
        };

        private static string[] _summaryResponse = new[]
        {
            "Here's a preview of your venue!",
            "Okay! 🙌 Here's what your venue will look like!",
            "Nice! So, how does it look? 😊"
        };

        private static string[] _workingOnItResponse = new[]
        {
            "Yaaay! I'm working on it! So exicted. 🥳",
            "Woo, working on it! This bit is always so exciting. 🎉",
            "Alright, working on it! 😊"
        };

        private static string[] _successfulNewResponse = new[]
        {
            "Wooo! I've sent it. Once it's approved, it'll show on the index!",
            "All done! Once Kana or Sumi approves it, it'll be live! 🥳",
            "Ok! We'll get that approved and get it live soon! 🎉"
        };

        public ConfirmVenueState(IApiService apiService,
                                 UiConfiguration uiConfig,
                                 ApiConfiguration apiConfig,
                                 IStaffService indexersService,
                                 IGuildManager guildManager)
        {
            this._apiService = apiService;
            this._staffService = indexersService;
            this._guildManager = guildManager;
            this._uiUrl = uiConfig.BaseUrl;
            this._apiUrl = apiConfig.BaseUrl;
        }

        public async Task Enter(InteractionContext c)
        {
            var bannerUrl = c.Session.GetItem<string>("bannerUrl");
            var modifying = c.Session.GetItem<bool>("modifying");
            var venue = c.Session.GetItem<Venue>("venue");

            await c.Interaction.RespondAsync(_summaryResponse.PickRandom(),
                                    embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", bannerUrl ?? $"{this._apiUrl}/venue/{venue.Id}/media").Build(),
                                    component: new ComponentBuilder()
                                        .WithButton("Looks perfect!", c.Session.RegisterComponentHandler(this.LooksPerfect, ComponentPersistence.ClearRow), ButtonStyle.Success)
                                        .WithButton(modifying ? "Edit more" : "Edit", c.Session.RegisterComponentHandler(this.Edit, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                                        .WithButton("Cancel", c.Session.RegisterComponentHandler(this.Cancel, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                                        .Build());
        }

        private async Task LooksPerfect(MessageComponentInteractionContext c)
        {
            var isNewVenue = c.Session.GetItem<bool>("isNewVenue");
            _ = c.Interaction.Channel.SendMessageAsync(_workingOnItResponse.PickRandom());
            _ = c.Interaction.Channel.TriggerTypingAsync();

            var venue = c.Session.GetItem<Venue>("venue");
            var uploadVenueResponse = await this._apiService.PutVenueAsync(venue);
            if (!uploadVenueResponse.IsSuccessStatusCode)
            {
                _ = c.Interaction.Channel.SendMessageAsync("Ooops! Something went wrong. 😢");
                return;
            }
            var bannerUrl = c.Session.GetItem<string>("bannerUrl");
            if (bannerUrl != null) // changed
                await this._apiService.PutVenueBannerAsync(venue.Id, bannerUrl);

            var isEditorOrIndexer = this._staffService.IsEditor(c.Interaction.User.Id) 
                                    || this._staffService.IsApprover(c.Interaction.User.Id);
            if (isEditorOrIndexer)
            {
                var approvalResponse = await this._apiService.ApproveAsync(venue.Id);
                if (!approvalResponse.IsSuccessStatusCode)
                {
                    await c.Interaction.Channel.SendMessageAsync("Something, went wrong while trying to auto-approve it for you. 😢");
                    _ = c.Session.ClearState(c);
                    return;
                }
            }

            if (!isNewVenue)
            {
                _ = this._guildManager.SyncRolesForVenueAsync(venue);
                _ = this._guildManager.FormatDisplayNamesForVenueAsync(venue);
                await c.Interaction.Channel.SendMessageAsync(_preexisingResponse.PickRandom());
            }
            else if (isEditorOrIndexer)
            {
                await c.Interaction.Channel.SendMessageAsync("All done and auto-approved for you. :heart:");
                await this.ApproveVenueAsync(venue, c.Client);
            }
            else
            {
                await c.Interaction.Channel.SendMessageAsync(_successfulNewResponse.PickRandom());
                await SendToApprovers(venue, bannerUrl);
            }

            _ = c.Session.ClearState(c);
        }

        private Task Edit(MessageComponentInteractionContext c) =>
            c.Session.MoveStateAsync<ModifyVenueState>(c);

        private Task Cancel(MessageComponentInteractionContext c)
        {
            _ = c.Interaction.RespondAsync("It's as if it never happened! 😅");
            _ = c.Session.ClearState(c);
            return Task.CompletedTask;
        }

        private Task SendToApprovers(Venue venue, string bannerUrl) =>
            this._staffService
                .Broadcast()
                .WithMessage($"Heyo indexers!\nVenue '**{venue.Name}**' ({venue.Id}) needs approving! :heart:")
                .WithEmbed(venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", bannerUrl))
                .WithComponent(bcc =>
                {
                    ComponentBuilder approveRejectComponent = null;
                    var approveHandler = bcc.RegisterComponentHandler(c => this.ApproveVenueHandler(venue, c));
                    var rejectHandler = bcc.RegisterComponentHandler(c => this.DeleteVenueHandler(approveRejectComponent, venue, bcc, c));
                    return approveRejectComponent = new ComponentBuilder()
                        .WithButton("Approve", approveHandler, ButtonStyle.Success)
                        .WithButton("Reject", rejectHandler, ButtonStyle.Secondary);
                })
                .SendToAsync(this._staffService.Approvers);

        private async Task ApproveVenueHandler(Venue venue, Broadcast.BroadcastInteractionContext approveBic)
        {
            if (!this._staffService.IsApprover(approveBic.Component.User.Id))
            {
                await approveBic.Component.RespondAsync("Sorry, only indexers can do this! :sad:");
                return;
            }

            // It may have been edited by indexers, so get the latest.
            venue = await this._apiService.GetVenueAsync(venue.Id);
            await this._apiService.ApproveAsync(venue.Id);

            _ = approveBic.ModifyForOtherUsers((props, original) =>
            {
                props.Components = new ComponentBuilder().Build();
                props.Embeds = original.Embeds.Select(e => (Embed)e)
                    .Concat(new[] { new EmbedBuilder().WithDescription($"{approveBic.CurrentUser.Username} handled this and approved the venue. 🥳").Build() })
                    .ToArray();
            });
            await approveBic.ModifyForCurrentUser((props, original) =>
            {
                props.Components = new ComponentBuilder().Build();
                props.Embeds = original.Embeds.Select(e => (Embed)e)
                    .Concat(new[] { new EmbedBuilder().WithDescription($"You handled this and approved the venue. 🥳").Build() })
                    .ToArray();
            });
            await ApproveVenueAsync(venue, approveBic.Broadcast.Client);
        }

        private async Task ApproveVenueAsync(Venue venue, DiscordSocketClient client)
        {
            _ = this._guildManager.AssignRolesForVenueAsync(venue);
            _ = this._guildManager.FormatDisplayNamesForVenueAsync(venue);

            foreach (var managerId in venue.Managers)
            {
                var manager = await client.GetUserAsync(ulong.Parse(managerId));
                var dmChannel = await manager.CreateDMChannelAsync();
                _ = dmChannel.SendMessageAsync($"Hey hey! :heart:\n**{venue.Name}** has been **approved** and it's live!\n{this._uiUrl}/#{venue.Id}\n" +
                                               $"I've assigned you your Venue Manager discord role too.\n" +
                                               $"Let me know if you'd like anything edited or anything you'd like help with. 🥳",
                                               embed: venue.ToEmbed($"{this._uiUrl}/#{venue.Id}", $"{this._apiUrl}/venue/{venue.Id}/media").Build());
            }
        }

        private async Task DeleteVenueHandler(ComponentBuilder approveRejectComponent, 
                                              Venue venue, 
                                              Broadcast.ComponentContext bcc, 
                                              Broadcast.BroadcastInteractionContext rejectBic)
        {
            if (!this._staffService.IsApprover(rejectBic.Component.User.Id))
            {
                await rejectBic.Component.RespondAsync("Sorry, only indexers can do this! :sad:");
                return;
            }

            var confirmDeleteHandler = bcc.RegisterComponentHandler(async confirmDeleteBic =>
            {
                await this._apiService.DeleteVenueAsync(venue.Id);

                _ = rejectBic.ModifyForOtherUsers((props, original) =>
                {
                    props.Components = new ComponentBuilder().Build();
                    props.Embeds = original.Embeds.Select(e => (Embed)e)
                        .Concat(new[] { new EmbedBuilder().WithDescription($"{rejectBic.CurrentUser.Username} handled this and rejected the venue.").Build() })
                        .ToArray();
                });

                await rejectBic.ModifyForCurrentUser((props, original) =>
                {
                    props.Components = new ComponentBuilder().Build();
                    props.Embeds = original.Embeds.Select(e => e as Embed)
                        .Concat(new[] { new EmbedBuilder().WithDescription($"You handled this and rejected the venue.").Build() })
                        .ToArray();
                });
            });

            var cancelHandler = bcc.RegisterComponentHandler(cancelBic =>
                cancelBic.ModifyForCurrentUser((props, original) =>
                    props.Components = approveRejectComponent.Build()));

            await rejectBic.ModifyForCurrentUser((props, original) =>
                props.Components = new ComponentBuilder()
                    .WithButton("Yes, delete venue", confirmDeleteHandler, ButtonStyle.Danger)
                    .WithButton("No, cancel", cancelHandler, ButtonStyle.Secondary)
                    .Build());
        }

    }
}
