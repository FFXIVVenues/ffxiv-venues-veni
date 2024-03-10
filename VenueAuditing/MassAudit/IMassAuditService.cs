using System.Threading.Tasks;
using FFXIVVenues.Veni.VenueAuditing.MassAudit.Models;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit;

public interface IMassAuditService
{
    Task<ResumeResult> ResumeMassAuditAsync(bool activeOnly = true);
    Task<StartResult> StartMassAuditAsync(ulong requestedIn, ulong requestedBy);
    Task<PauseResult> PauseAsync();
    Task<CancelResult> CancelAsync();
    Task<MassAuditStatusSummary> GetStatusSummaryAsync();
    Task<MassAuditStatusReport> GetStatusReportAsync();
    Task<CloseResult> CloseMassAudit();
    Task<NoticeResult> SendNoticeAsync(string message);
}