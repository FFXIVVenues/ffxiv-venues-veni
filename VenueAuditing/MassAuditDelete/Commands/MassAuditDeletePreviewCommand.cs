using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.VenueAuditing.MassAudit;
using FFXIVVenues.Veni.VenueRendering;

namespace FFXIVVenues.Veni.VenueAuditing.MassAuditDelete.Commands;

[DiscordCommand("massaudit delete preview", "See what venues would be deleted.")] 
public class MassAuditDeletePreviewCommand(IAuthorizer authorizer, IMassAuditService massAuditService, UiConfiguration uiConfig) : ICommandHandler
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

        var venueGroups = venues.GroupBy(v => v.Location.DataCenter);
        var builder = new StringBuilder();
        foreach (var group in venueGroups)
        {
            builder.Append("**").Append(group.Key?.ToUpper() ?? "CUSTOM").Append("** (").Append(group.Count()).AppendLine(")").AppendLine();
            foreach (var venue in group)
                builder.Append(venue.Name).Append(" [ ↗](").Append(uiConfig.BaseUrl).Append("/#").Append(venue.Id)
                    .AppendLine(")");
            builder.AppendLine();
        }
        builder.Append("**Total**: ").AppendLine(venues.Count.ToString());
        var embedBuilder = new EmbedBuilder()
            .WithTitle("Venues to be deleted for this Mass Audit")
            .WithDescription(builder.ToString());
            
        await context.Interaction.FollowupAsync("Okay, here it is! 🥰", embed: embedBuilder.Build());
    }
}