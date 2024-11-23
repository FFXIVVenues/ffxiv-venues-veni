using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.GuildEngagement;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueApproval;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.SessionStates;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring;

class ConfirmVenueSessionState(
    VenueAuthoringContext venueAuthoringContext,
    IVenueRenderer venueRenderer,
    IApiService apiService,
    IVenueApprovalService indexersService,
    IGuildManager guildManager,
    IAuthorizer authorizer,
    IVenueAuditService auditService)
    : ISessionState<VenueAuthoringContext>
{

    public async Task EnterState(VeniInteractionContext interactionContext)
    {
        var bannerUrl = interactionContext.Session.GetItem<string>(SessionKeys.BANNER_URL);
        var modifying = interactionContext.Session.InEditing();
        var venue = interactionContext.Session.GetVenue();
            
        await interactionContext.Interaction.RespondAsync(VenueControlStrings.VenuePreview,
            embed: (await venueRenderer.ValidateAndRenderAsync(venue, bannerUrl)).Build(),
            component: new ComponentBuilder()
                .WithButton("Looks perfect! Save!", interactionContext.RegisterComponentHandler(this.LooksPerfect, ComponentPersistence.ClearRow), ButtonStyle.Success)
                .WithButton(modifying ? "Edit more" : "Edit", interactionContext.RegisterComponentHandler(this.Edit, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                .WithButton("Cancel", interactionContext.RegisterComponentHandler(this.Cancel, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                .Build());
    }

    private async Task LooksPerfect(ComponentVeniInteractionContext c)
    {
        var isNewVenue = c.Session.GetItem<bool>(SessionKeys.IS_NEW_VENUE);
        var modifying = c.Session.InEditing();

        _ = c.Interaction.Channel.SendMessageAsync(VenueControlStrings.SavingVenue);
        _ = c.Interaction.Channel.TriggerTypingAsync();
            
        var venue = c.Session.GetVenue();
        var bannerUrl = c.Session.GetItem<string>(SessionKeys.BANNER_URL);
        var isApprover = authorizer
            .Authorize(c.Interaction.User.Id, Permission.ApproveVenue, venue)
            .Authorized;
            
        var uploadVenueResponse = await apiService.PutVenueAsync(venue);
        if (!uploadVenueResponse.IsSuccessStatusCode)
        {
            _ = c.Interaction.Channel.SendMessageAsync("Ooops! Something went wrong. 😢");
            await c.Interaction.RespondAsync(VenueControlStrings.VenuePreview,
                embed: (await venueRenderer.ValidateAndRenderAsync(venue, bannerUrl)).Build(),
                components: new ComponentBuilder()
                    .WithButton("Looks perfect! Save!", c.RegisterComponentHandler(this.LooksPerfect, ComponentPersistence.ClearRow), ButtonStyle.Success)
                    .WithButton(modifying ? "Edit more" : "Edit", c.RegisterComponentHandler(this.Edit, ComponentPersistence.ClearRow), ButtonStyle.Secondary)
                    .WithButton("Cancel", c.RegisterComponentHandler(this.Cancel, ComponentPersistence.ClearRow), ButtonStyle.Danger)
                    .Build());
            return;
        }
            
        if (bannerUrl != null) // changed
            await apiService.PutVenueBannerAsync(venue.Id, bannerUrl);

        if (!isNewVenue)
        {
            _ = guildManager.SyncRolesForVenueAsync(venue);
            _ = guildManager.FormatDisplayNamesForVenueAsync(venue);
            await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueUpdatedConfirmation);
            var latestAudit = await auditService.GetLatestRecordFor(venue);
            if (latestAudit?.Status is VenueAuditStatus.Failed or VenueAuditStatus.Pending or VenueAuditStatus.AwaitingResponse)
                await auditService.UpdateAuditStatus(latestAudit, venue, c.Interaction.User.Id, VenueAuditStatus.EditedLater);
        }
        else if (isApprover)
        {
            var success = await indexersService.ApproveVenueAsync(venue);
            if (success)
                await c.Interaction.Channel.SendMessageAsync("All done and auto-approved for you. :heart:");
            else
                await c.Interaction.Channel.SendMessageAsync("Something, went wrong while trying to auto-approve it for you. 😢");
        }
        else
        {
            await c.Interaction.Channel.SendMessageAsync(VenueControlStrings.VenueCreatedPendingApproval);
            await SendToApprovers(venue, bannerUrl);
        }

        _ = c.ClearSessionAsync();
    }

    private Task Edit(ComponentVeniInteractionContext c) =>
        c.MoveSessionToStateAsync<EditVenueSessionState>();

    private Task Cancel(ComponentVeniInteractionContext c)
    {
        _ = c.ClearSessionAsync();
        return c.Interaction.Channel.SendMessageAsync("It's as if it never happened! 😅");
    }

    private Task SendToApprovers(Venue venue, string bannerUrl) =>
        indexersService.SendForApproval(venue, bannerUrl);

}

enum VenueAuthoringType
{
    Edit, 
    Create
}

record VenueAuthoringContext(Venue Venue, VenueAuthoringType ControlType);