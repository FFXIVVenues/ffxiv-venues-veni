using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueDeletion.SessionStates
{
    class DeleteVenueSessionState(IApiService apiService, IVenueAuditService auditService, IAuthorizer authorizer)
        : ISessionState
    {

        private static string[] _messages = new[]
        {
            "Are you super sure you want to **delete {0}**? 😢",
            "Are you really sure you want to **delete {0}**?",
            "R-really? Are you sure you want me to **delete {0}**?"
        };

        private static string[] _deleteMessages = new[]
        {
            "Okay, that's done. 😢",
            "It's gone. 😢"
        };

        private Venue _venue;

        public Task EnterState(VeniInteractionContext interactionContext)
        {
            this._venue = interactionContext.Session.GetVenue();
            return interactionContext.Interaction.RespondAsync(string.Format(_messages.PickRandom(), _venue.Name), new ComponentBuilder()
                .WithButton("Yes, delete it 😢", interactionContext.RegisterComponentHandler(
                    async cm =>
                    {
                        var authorize = authorizer.Authorize(cm.Interaction.User.Id, Permission.DeleteVenue, _venue);
                        if (!authorize.Authorized)
                        {
                            await cm.Interaction.Channel.SendMessageAsync(
                                "Sorry, you do not have permission to delete this venue. 😢");
                            return;
                        }
                        
                        _ = cm.Interaction.Channel.SendMessageAsync(_deleteMessages.PickRandom());
                        await apiService.DeleteVenueAsync(_venue.Id);
                        var latestAudit = await auditService.GetLatestRecordFor(this._venue);
                        if (latestAudit?.Status is VenueAuditStatus.Failed or VenueAuditStatus.Pending or VenueAuditStatus.AwaitingResponse)
                            await auditService.UpdateAuditStatus(latestAudit, this._venue, interactionContext.Interaction.User.Id, VenueAuditStatus.DeletedLater);
                    }, 
                    ComponentPersistence.ClearRow), ButtonStyle.Danger)
                .WithButton("No, don't! I've changed my mind. 🙂", interactionContext.RegisterComponentHandler(cm => cm.Interaction.Channel.SendMessageAsync("Phew 😅"), ComponentPersistence.ClearRow))
                .Build());
        }
    }
}
