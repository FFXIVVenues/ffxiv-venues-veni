using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit.Commands
{
    [DiscordCommand("massaudit resume", "Resume a currently paused audit round.")] 
    public class MassAuditResumeCommand : ICommandHandler
    {
        private readonly IAuthorizer _authorizer;
        private readonly IMassAuditService _massAuditService;

        public MassAuditResumeCommand(IAuthorizer authorizer, IMassAuditService massAuditService)
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

            var result = await this._massAuditService.ResumeMassAuditAsync();
            switch (result)
            {
                case ResumeResult.AlreadyRunning:
                    await context.Interaction.RespondAsync("The mass audit is already running. 😊");
                    break;
                case ResumeResult.NothingToResume:
                    await context.Interaction.RespondAsync("There's no current mass audit to resume. 🤔");
                    break;
                case ResumeResult.ResumedActive:
                    await context.Interaction.RespondAsync("A mass audit that did not gracefully stop has been resumed. 🤔");
                    break;
                case ResumeResult.ResumedInactive:
                    await context.Interaction.RespondAsync("The mass audit has been resumed. 🥳");
                    break;
            }
            
        }
    }
}
