using System.Text;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.VenueAuditing.MassAudit;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditDelete.Commands;

[DiscordCommand("massaudit delete propose", "See what venues would be deleted.")] 
public class MassAuditDeleteProposeCommand(IAuthorizer authorizer, IMassAuditService massAuditService) : ICommandHandler
{
    public async Task HandleAsync(SlashCommandVeniInteractionContext context)
    {
        var authorized = authorizer.Authorize(context.Interaction.User.Id, Permission.ReportMassAudit, null);
        if (!authorized.Authorized)
        {
            await context.Interaction.RespondAsync("Sorry, I can't let you do that. 👀", ephemeral: true);
            return;
        }
            
        await context.Interaction.DeferAsync();
        context.TypingHandle?.Dispose();

        var venues = await massAuditService.ProposeDelete();
        if (venues is [])
        {
            await context.Interaction.FollowupAsync("No active mass audit and/or no venues to delete.");
            return;
        }

        var builder = new StringBuilder();
        foreach (var venue in venues)
            builder.Append(venue.Id).Append(" - **").Append(venue.Name).AppendLine("**");
        var embedBuilder = new EmbedBuilder()
            .WithTitle("Venues to be deleted for this Mass Audit")
            .WithDescription(builder.ToString());
            
        await context.Interaction.FollowupAsync("Okay, here it is! 🥰", embed: embedBuilder.Build());
    }
}