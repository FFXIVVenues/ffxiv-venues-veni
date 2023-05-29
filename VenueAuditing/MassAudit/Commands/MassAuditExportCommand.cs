﻿using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit.Commands
{
    [DiscordCommand("massaudit export", "Export a detailed spreadsheet of an audit round.")] 
    public class MassAuditExportCommand : ICommandHandler
    {
        private readonly IAuthorizer _authorizer;
        private readonly IMassAuditService _massAuditService;

        public MassAuditExportCommand(IAuthorizer authorizer, IMassAuditService massAuditService)
        {
            _authorizer = authorizer;
            _massAuditService = massAuditService;
        }

        public async Task HandleAsync(SlashCommandVeniInteractionContext context)
        {
            var authorized = this._authorizer.Authorize(context.Interaction.User.Id, Permission.ReportMassAudit, null);
            if (!authorized.Authorized)
            {
                await context.Interaction.RespondAsync("Sorry, I can't let you do that. 👀", ephemeral: true);
                return;
            }
            
            var report = await this._massAuditService.GetStatusReportAsync();
            if (report == null)
            {
                await context.Interaction.RespondAsync("There has never been a mass audit to report to.");
                return;
            }

            await context.Interaction.DeferAsync();
            await context.Interaction.FollowupWithFileAsync(report.ContentStream, report.FileName, "Okay, here it is! 👀");
        }
    }
}