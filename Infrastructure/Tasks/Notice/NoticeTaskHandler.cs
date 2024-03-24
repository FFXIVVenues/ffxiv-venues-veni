using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.Veni.VenueAuditing.MassAudit;
using Serilog;

namespace FFXIVVenues.Veni.Infrastructure.Tasks.Notice;

public class NoticeTask(
    TaskContext<NoticeMemento> context,
    IApiService apiService,
    IRepository repository,
    IDiscordClient discordClient) : ITask<NoticeMemento>
{
    
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if (context.Memento.TaskState == TaskState.Inactive)
        {
            
            
            context.Memento.TaskState = TaskState.Active;
            await context.CommitMementoAsync();
        }

        var massAudit = await repository.GetByIdAsync<MassAuditRecord>(context.Memento.MassAuditId);
        var notice = context.Memento.Message;
        
        massAudit.Log($"Starting broadcast of notice \"{notice}\"");
        Log.Debug("Mass audit: starting broadcast of notice");
        
        var allVenues = await apiService.GetAllVenuesAsync();
        var auditRecords = await repository.GetWhereAsync<VenueAuditRecord>(r => r.MassAuditId == massAudit.id && r.Status == VenueAuditStatus.AwaitingResponse);
        foreach (var auditRecord in auditRecords)
        {
            var venue = allVenues.FirstOrDefault(v => v.Id == auditRecord.VenueId);
            if (venue is null)
                continue;
            
            massAudit.Log($"Sending notice to {auditRecord.VenueId}");
            Log.Debug("Mass audit: sending notice to {Venue}", venue.Name);

            auditRecord.Log($"Sending notice to {venue.Managers.Count} managers.");
            
            var broadcastReceipt = await new Broadcast(IdHelper.GenerateId(8), client)
                .WithEmbed(new EmbedBuilder().WithDescription(notice))
                .SendToAsync(venue.Managers.Select(ulong.Parse).ToArray());
            
            foreach (var message in broadcastReceipt.BroadcastMessages)
                auditRecord.Log($"Notice to {message.UserId}: {message.Log}");
            auditRecord.Log($"Sent notice to {broadcastReceipt.BroadcastMessages.Count(r => r.Status is MessageStatus.Sent)} of {venue.Managers.Count} managers.");
            
            await repository.UpsertAsync(auditRecord);
        }

        Log.Debug("Mass audit: notice broadcast");

        await repository.UpsertAsync(massAudit);
        
        
    }
}