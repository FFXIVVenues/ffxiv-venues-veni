using System.Threading.Tasks;
using FFXIVVenues.Veni.VenueAuditing.MassAudit.Models;
using FFXIVVenues.Veni.VenueAuditing.MassAuditNotice;

namespace FFXIVVenues.Veni.VenueAuditing.MassAudit;

public interface IMassAuditService
{
    Task<ResumeResult> ResumeAsync(bool activeOnly = true);
    Task<StartResult> StartAsync(ulong requestedIn, ulong requestedBy);
    Task<PauseResult> PauseAsync();
    Task<CancelResult> CancelAsync();
    Task<MassAuditStatusSummary> GetSummaryAsync();
    Task<MassAuditStatusReport> GetReportAsync();
    Task<CloseResult> CloseMassAudit();
    Task<NoticeResult> StartNoticeAsync(ulong requestedIn, ulong requestedBy, string message);
    Task<PauseResult> PauseNoticeAsync();
    Task<ResumeResult> ResumeNoticeAsync();
    Task<CancelResult> CancelNoticeAsync();
    Task<MassNoticeSummary> GetNoticeSummaryAsync();
    Task<MassNoticeTask> GetNoticeTaskAsync();
}