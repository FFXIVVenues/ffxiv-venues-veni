using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.Veni.VenueControl.VenueDeletion.SessionStates;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.VenueControl.VenueClosing.SessionStates
{
    internal class CloseEntrySessionState(
        IApiService apiService,
        IVenueAuditService auditService,
        IAuthorizer authorizer)
        : ISessionState
    {
        private Venue _venue;

        public Task Enter(VeniInteractionContext c)
        {
            this._venue = c.Session.GetVenue();
            var component = this.BuildCloseComponent(c, this._venue.Resolution?.IsNow ?? false);
            return c.Interaction.RespondAsync("Aaw, how long do you want to close for? 🥲", component.Build()); //change text later
        }

        private ComponentBuilder BuildCloseComponent(VeniInteractionContext c, bool includeCloseCurrentOpening)
        {
            var selectComponent = new SelectMenuBuilder()
                .WithCustomId(c.Session.RegisterComponentHandler(OnComplete, ComponentPersistence.ClearRow));

            if (includeCloseCurrentOpening)
                selectComponent.AddOption("Close current opening", "0");
            
            selectComponent.AddOption("The next 18 hours", "18")
                .AddOption("The next 2 days", "48")
                .AddOption("The next 3 days", "72")
                .AddOption("The next 5 days", "120")
                .AddOption("The next 7 days", "168")
                .AddOption("The next 2 weeks", "336")
                .AddOption("The next 3 weeks", "504")
                .AddOption("The next 4 weeks", "672")
                .AddOption("The next 6 weeks", "1008")
                .AddOption("The next 2 months", "1344")
                .AddOption("The next 3 months", "2016")
                .AddOption("Permanently (delete)", "perm");
            return new ComponentBuilder().WithSelectMenu(selectComponent);
        }

        private async Task OnComplete(MessageComponentVeniInteractionContext c)
        {
            var value = c.Interaction.Data.Values.Single();
            if (value == "perm")
            {
                await c.Session.MoveStateAsync<DeleteVenueSessionState>(c);
                return;
            }
            
            var authorize = authorizer.Authorize(c.Interaction.User.Id, Permission.CloseVenue, _venue);
            if (!authorize.Authorized)
            {
                await c.Interaction.Channel.SendMessageAsync(
                    "Sorry, you do not have permission to close this venue. 😢");
                return;
            }
                
            var until = int.Parse(value);
            if (until == 0)
            {
                if (this._venue.Resolution == null)
                    return;
                await apiService.CloseVenueAsync(this._venue.Id, this._venue.Resolution.End);
            }
            else
                await apiService.CloseVenueAsync(this._venue.Id, DateTime.UtcNow.AddHours(until));
            
            await c.Interaction.Channel.SendMessageAsync(MessageRepository.VenueClosedMessage.PickRandom());
            
            var latestAudit = await auditService.GetLatestRecordFor(this._venue);
            if (latestAudit?.Status is VenueAuditStatus.Failed or VenueAuditStatus.Pending or VenueAuditStatus.AwaitingResponse)
                await auditService.UpdateAuditStatus(latestAudit, this._venue, c.Interaction.User.Id, VenueAuditStatus.ClosedLater);
            
            _ = c.Session.ClearState(c);
        }
    }
}
