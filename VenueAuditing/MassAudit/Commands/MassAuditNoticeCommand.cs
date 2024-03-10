using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit.Commands
{
    [DiscordCommand("massaudit notice", "Send a message to all venues from which we're still awaiting response in the current mass audit.")]
    [DiscordCommandOption("message", "Custom message to send to all venues.", ApplicationCommandOptionType.String, Required = true)]
    public class MassAuditNoticeCommand(IAuthorizer authorizer, IMassAuditService massAuditService) : ICommandHandler
    {
        public async Task HandleAsync(SlashCommandVeniInteractionContext context)
        {
            var authorized = authorizer.Authorize(context.Interaction.User.Id, Permission.ControlMassAudit, null);
            if (!authorized.Authorized)
            {
                await context.Interaction.RespondAsync("Sorry, I can't let you do that. 👀", ephemeral: true);
                return;
            }

            var message = context.GetStringArg("message");
            if (string.IsNullOrWhiteSpace(message))
                return;

            await context.Interaction.DeferAsync();
            var result = await massAuditService.SendNoticeAsync(message);
            switch (result)
            {
                case NoticeResult.Sent:
                    await context.Interaction.FollowupAsync("The mass audit is already running. 😊");
                    break;
                case NoticeResult.MassAuditRunning:
                    await context.Interaction.FollowupAsync("An active mass audit already exists but it has faulted. 🤔");
                    break;
                case NoticeResult.MassAuditClosed:
                    await context.Interaction.FollowupAsync("An mass audit already exists but isn't running. 🤔");
                    break;
                case NoticeResult.MassAuditNotComplete:
                    await context.Interaction.FollowupAsync("The mass audit has started! 🥳");
                    break;
                case NoticeResult.NoMassAudits:
                    await context.Interaction.FollowupAsync("The mass audit has started! 🥳");
                    break;
            }

        }
    }
}
