﻿using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.GuildEngagement;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueApproval;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.SessionStates;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring
{
    class ConfirmVenueSessionState : ISessionState
    {
        private readonly IVenueRenderer _venueRenderer;
        private readonly IApiService _apiService;
        private readonly IVenueApprovalService _venueApprovalService;
        private readonly IGuildManager _guildManager;
        private readonly IAuthorizer _authorizer;
        private readonly IRepository _repository;
        private readonly IVenueAuditService _auditService;

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
            "Yaaay! I'm working on it! So excited. 🥳",
            "Woo, working on it! This bit is always so exciting. 🎉",
            "Alright, working on it! 😊"
        };

        private static string[] _successfulNewResponse = new[]
        {
            "Wooo! I've sent it. Once it's approved, it'll show on the index!",
            "All done! Once Sumi or Kana approves it, it'll be live! 🥳",
            "Ok! We'll get that approved and get it live soon! 🎉"
        };

        public ConfirmVenueSessionState(IVenueRenderer venueRenderer,
                                IApiService apiService,
                                IVenueApprovalService indexersService,
                                IGuildManager guildManager,
                                IAuthorizer authorizer,
                                IRepository repository,
                                IVenueAuditService auditService)
        {
            this._venueRenderer = venueRenderer;
            this._apiService = apiService;
            this._venueApprovalService = indexersService;
            this._guildManager = guildManager;
            this._authorizer = authorizer;
            this._repository = repository;
            this._auditService = auditService;
        }

        public async Task Enter(VeniInteractionContext c)
        {
            var bannerUrl = c.Session.GetItem<string>("bannerUrl");
            var modifying = c.Session.GetItem<bool>("modifying");
            var isBiweeklySchedule = c.Session.GetItem<bool>("hasBiweeklySchedule");
            var venue = c.Session.GetVenue();
            
            await c.Interaction.RespondAsync(_summaryResponse.PickRandom(),
                embed: this._venueRenderer.RenderEmbed(venue, bannerUrl, isBiweeklySchedule ? VenueRenderFlags.AddBiweeklyScheduleUserMessage : VenueRenderFlags.None).Build(),
                component: new ComponentBuilder()
                    .WithButton("Looks perfect! Save!", c.Session.RegisterComponentHandler(this.LooksPerfect, ComponentPersistence.ClearRow), ButtonStyle.Success)
                    .WithButton(modifying ? "Edit more" : "Edit", c.Session.RegisterComponentHandler(this.Edit, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Cancel", c.Session.RegisterComponentHandler(this.Cancel, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                    .Build());
        }

        private async Task LooksPerfect(MessageComponentVeniInteractionContext c)
        {
            var isNewVenue = c.Session.GetItem<bool>("isNewVenue");
            var modifying = c.Session.GetItem<bool>("modifying");
            var isBiweeklySchedule = c.Session.GetItem<bool>("hasBiweeklySchedule");


            _ = c.Interaction.Channel.SendMessageAsync(_workingOnItResponse.PickRandom());
            _ = c.Interaction.Channel.TriggerTypingAsync();
            
            var venue = c.Session.GetVenue();
            var bannerUrl = c.Session.GetItem<string>("bannerUrl");
            var isApprover = this._authorizer
                .Authorize(c.Interaction.User.Id, Permission.ApproveVenue, venue)
                .Authorized;
            
            var uploadVenueResponse = await this._apiService.PutVenueAsync(venue);
            if (!uploadVenueResponse.IsSuccessStatusCode)
            {
                _ = c.Interaction.Channel.SendMessageAsync("Ooops! Something went wrong. 😢");
                await c.Interaction.RespondAsync(_summaryResponse.PickRandom(),
                    embed: this._venueRenderer.RenderEmbed(venue, bannerUrl, VenueRenderFlags.AddBiweeklyScheduleUserMessage).Build(),
                    components: new ComponentBuilder()
                        .WithButton("Looks perfect! Save!", c.Session.RegisterComponentHandler(this.LooksPerfect, ComponentPersistence.ClearRow), ButtonStyle.Success)
                        .WithButton(modifying ? "Edit more" : "Edit", c.Session.RegisterComponentHandler(this.Edit, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                        .WithButton("Cancel", c.Session.RegisterComponentHandler(this.Cancel, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                        .Build());
                return;
            }
            
            if (bannerUrl != null) // changed
                await this._apiService.PutVenueBannerAsync(venue.Id, bannerUrl);

            if (!isNewVenue)
            {
                _ = this._guildManager.SyncRolesForVenueAsync(venue);
                _ = this._guildManager.FormatDisplayNamesForVenueAsync(venue);
                await c.Interaction.Channel.SendMessageAsync(_preexisingResponse.PickRandom());
                var latestAudit = await this._auditService.GetLatestRecordFor(venue);
                if (latestAudit?.Status is VenueAuditStatus.Failed or VenueAuditStatus.Pending or VenueAuditStatus.AwaitingResponse)
                    await this._auditService.UpdateAuditStatus(latestAudit, venue, c.Interaction.User.Id, VenueAuditStatus.EditedLater);
            }
            else if (isApprover)
            {
                var success = await this._venueApprovalService.ApproveVenueAsync(venue);
                if (success)
                    await c.Interaction.Channel.SendMessageAsync("All done and auto-approved for you. :heart:");
                else
                    await c.Interaction.Channel.SendMessageAsync("Something, went wrong while trying to auto-approve it for you. 😢");
            }
            else
            {
                await c.Interaction.Channel.SendMessageAsync(_successfulNewResponse.PickRandom());
                await SendToApprovers(venue, bannerUrl, isBiweeklySchedule);
            }

            _ = c.Session.ClearState(c);
        }

        private Task Edit(MessageComponentVeniInteractionContext c) =>
            c.Session.MoveStateAsync<EditVenueSessionState>(c);

        private Task Cancel(MessageComponentVeniInteractionContext c)
        {
            _ = c.Session.ClearState(c);
            return c.Interaction.Channel.SendMessageAsync("It's as if it never happened! 😅");
        }

        private Task SendToApprovers(Venue venue, string bannerUrl, bool needsBiweeklyAdding) =>
            this._venueApprovalService.SendForApproval(venue, bannerUrl, needsBiweeklyAdding ? VenueRenderFlags.AddBiweeklyScheduleStaffMessage : VenueRenderFlags.None);

    }
}
