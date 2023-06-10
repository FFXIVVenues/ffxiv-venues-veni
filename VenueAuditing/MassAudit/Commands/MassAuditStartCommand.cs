﻿using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit.Commands
{
    [DiscordCommand("massaudit start", "Start a new audit of all venues.")] 
    public class MassAuditStartCommand : ICommandHandler
    {
        private readonly IAuthorizer _authorizer;
        private readonly IMassAuditService _massAuditService;

        public MassAuditStartCommand(IAuthorizer authorizer, IMassAuditService massAuditService)
        {
            _authorizer = authorizer;
            _massAuditService = massAuditService;
        }

        public async Task HandleAsync(SlashCommandVeniInteractionContext context)
        {
            var authorized = this._authorizer.Authorize(context.Interaction.User.Id, Permission.ControlMassAudit, null);
            if (!authorized.Authorized)
            {
                await context.Interaction.RespondAsync("Sorry, I can't let you do that. 👀", ephemeral: true);
                return;
            }

            await context.Interaction.DeferAsync();
            var result = await this._massAuditService.StartMassAuditAsync(context.Interaction.Channel.Id, context.Interaction.User.Id);
            switch (result)
            {
                case StartResult.AlreadyRunning:
                    await context.Interaction.FollowupAsync("The mass audit is already running. 😊");
                    break;
                case StartResult.ActiveExists:
                    await context.Interaction.FollowupAsync("An active mass audit already exists but it has faulted. 🤔");
                    break;
                case StartResult.InactiveExists:
                    await context.Interaction.FollowupAsync("An mass audit already exists but isn't running. 🤔");
                    break;
                case StartResult.Started:
                    await context.Interaction.FollowupAsync("The mass audit has started! 🥳");
                    break;
            }

        }
    }
}
