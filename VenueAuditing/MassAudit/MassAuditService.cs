using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueAuditing.MassAudit.Exporting;
using FFXIVVenues.Veni.VenueAuditing.MassAudit.Models;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring;
using Serilog;


namespace FFXIVVenues.Veni.VenueAuditing.MassAudit;

internal class MassAuditService(
    IApiService apiService,
    IRepository repository,
    IVenueAuditService venueAuditService,
    IDiscordClient client,
    IMassAuditExporter massAuditExporter,
    IDiscordValidator discordValidator)
    : IMassAuditService
{
    private bool _pause = false;
    private bool _cancel = false;
    private Task _activeAuditTask;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly HttpClient _http = new();

    public async Task<ResumeResult> ResumeMassAuditAsync(bool activeOnly)
    {
        Log.Debug("Resume requested for mass audit");
        if (this._activeAuditTask is not null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Mass audit already running");
            return ResumeResult.AlreadyRunning;
        }

        IQueryable<MassAuditRecord> activeAudits;
        if (activeOnly)
        {
            Log.Debug("Fetching active mass audits");
            activeAudits = await repository.GetWhereAsync<MassAuditRecord>(a => a.Status == MassAuditStatus.Active);
        }
        else
        {
            Log.Debug("Fetching active and inactive mass audits");
            activeAudits = await repository.GetWhereAsync<MassAuditRecord>(a => 
                a.Status == MassAuditStatus.Active || a.Status == MassAuditStatus.Inactive);
        }
        var audit = activeAudits.OrderByDescending(a => a.StartedAt).ToList().SingleOrDefault();

        if (audit == null)
        {
            Log.Debug("No mass audit to resume");
            return ResumeResult.NothingToResume;
        }

        var status = audit.Status;

        this.StartThread(audit);

        if (status == MassAuditStatus.Active)
        {
            Log.Debug("Active mass audit resumed");
            return ResumeResult.ResumedActive;
        }
        
        Log.Information("Inactive mass audit {AuditId} resumed", audit.id);
        return ResumeResult.ResumedInactive;
    }

    public async Task<StartResult> StartMassAuditAsync(ulong requestedIn, ulong requestedBy)
    {
        Log.Debug("Start request for mass audit");
        if (this._activeAuditTask is not null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Active mass audit already exists");
            return StartResult.ActiveExists;
        }

        Log.Debug("Fetching active and inactive mass audits");
        var activeAudits = await repository.GetWhereAsync<MassAuditRecord>(a => 
            a.Status == MassAuditStatus.Active ||
            a.Status == MassAuditStatus.Inactive);
        var audit = activeAudits.OrderByDescending(a => a.StartedAt).ToList().SingleOrDefault();
        
        if (audit is not null)
            if (audit.Status == MassAuditStatus.Active)
            {
                Log.Debug("Active mass audit already exists (but is not running)");
                return StartResult.ActiveExists;
            }
            else
            {
                Log.Debug("Inactive mass audit already exists");
                return StartResult.InactiveExists;
            }
                
        audit = new MassAuditRecord()
        {
            RequestedIn = requestedIn,
            RequestedBy = requestedBy
        };
        
        this.StartThread(audit);
        
        Log.Information("Mass audit {AuditId} started", audit.id);
        return StartResult.Started;
    }

    public async Task<PauseResult> PauseAsync()
    {
        Log.Debug("Pause request for mass audit");
        var pausedViaTask = false;
        if (this._activeAuditTask is not null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Mass audit cancellation token triggered");
            this._pause = true;
            this._cancellationTokenSource.Cancel();
            await this._activeAuditTask;
            pausedViaTask = true;
        }
        
        Log.Debug("Fetching active and inactive mass audits");
        var audit = (await repository.GetWhereAsync<MassAuditRecord>(a => 
                a.Status == MassAuditStatus.Active || a.Status == MassAuditStatus.Inactive))
            .OrderByDescending(a => a.StartedAt).Take(1).ToList().FirstOrDefault();
        
        if (audit == null)
        {
            Log.Debug("No mass audits to pause");
            return PauseResult.NothingToPause;
        }
        
        if (audit.Status == MassAuditStatus.Closed)
        {
            Log.Debug("Mass audit is closed");
            return PauseResult.Closed;
        }
        
        if (audit.Status == MassAuditStatus.Inactive)
        {
            if (!pausedViaTask) {
                Log.Debug("Mass audit already paused");
                return PauseResult.AlreadyPaused;
            }
            Log.Debug("Mass audit paused");
            return PauseResult.Paused;
        }
        
        audit.SetPaused();
        audit.Log("Mass audit paused");
        await repository.UpsertAsync(audit);
        
        Log.Information("Mass audit {AuditId} paused", audit.id);
        return PauseResult.Paused;
    }

    public async Task<CancelResult> CancelAsync()
    {
        Log.Debug("Cancel request for mass audit");
        var cancelledViaTask = false;
        if (this._activeAuditTask is not null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Mass audit cancellation token triggered");
            this._cancel = true;
            this._cancellationTokenSource.Cancel();
            await this._activeAuditTask;
            cancelledViaTask = true;
        }
        
        Log.Debug("Fetching active and inactive mass audits");
        var activeAudits = (await repository.GetWhereAsync<MassAuditRecord>(a => 
                    a.Status == MassAuditStatus.Active || a.Status == MassAuditStatus.Inactive))
            .OrderByDescending(a => a.StartedAt);

        if (!activeAudits.Any())
        {
            if (cancelledViaTask)
            {
                Log.Debug("Mass audit cancelled");
                return CancelResult.Cancelled;
            }
            Log.Debug("No mass audits to cancel");
            return CancelResult.NothingToCancel;
        }

        foreach (var audit in activeAudits)
        {
            audit.SetCancelled();
            audit.Log("Mass audit cancelled");
            await repository.UpsertAsync(audit);
            Log.Information("Mass audit {AuditId} cancelled", audit.id);
        }

        Log.Information("All active/inactive mass audits cancelled");
        return CancelResult.Cancelled;
    }

    public async Task<MassAuditStatusSummary> GetStatusSummaryAsync()
    {
        Log.Debug("Mass audit summary requested");
        var activeAuditRounds = await repository.GetAllAsync<MassAuditRecord>();
        var auditRound = activeAuditRounds.OrderByDescending(a => a.StartedAt).Take(1).ToList().FirstOrDefault();
        if (auditRound == null)
            return null;

        var audits = await repository.GetWhereAsync<VenueAuditRecord>(a => a.MassAuditId == auditRound.id);
        var allVenues = await apiService.GetAllVenuesAsync();

        return massAuditExporter.GetSummaryForMassAudit(auditRound, allVenues, audits.ToList());
    }

    public async Task<MassAuditStatusReport> GetStatusReportAsync()
    {
        Log.Debug("Mass audit report requested");
        var activeAuditRounds = await repository.GetAllAsync<MassAuditRecord>();
        var auditRound = activeAuditRounds.OrderByDescending(a => a.StartedAt).Take(1).ToList().FirstOrDefault();
        if (auditRound == null)
            return null;

        var audits = await repository.GetWhereAsync<VenueAuditRecord>(a => a.MassAuditId == auditRound.id);
        var allVenues = await apiService.GetAllVenuesAsync();
        
        return await massAuditExporter.GetExportForMassAuditAsync(auditRound, allVenues, audits.ToList());
    }
    
    public async Task<NoticeResult> SendNoticeAsync(string notice)
    {
        Log.Debug("Notice requested for mass audit");
        if (this._activeAuditTask is not null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Mass audit is active");
            return NoticeResult.MassAuditRunning;
        }
        
        var activeAuditRounds = await repository.GetAllAsync<MassAuditRecord>();
        var massAudit = activeAuditRounds.OrderByDescending(a => a.StartedAt).Take(1).ToList().FirstOrDefault();
        if (massAudit == null)
            return NoticeResult.NoMassAudits;

        if (massAudit.Status is MassAuditStatus.Closed)
            return NoticeResult.MassAuditClosed;

        if (massAudit.Status is not MassAuditStatus.Complete)
            return NoticeResult.MassAuditNotComplete;
        
        

        return NoticeResult.Sent;
    }

    public async Task<CloseResult> CloseMassAudit()
    {
        Log.Debug("Close request for mass audit");
        if (this._activeAuditTask is not null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Mass audit is active");
            return CloseResult.StillRunning;
        }
        
        Log.Debug("Fetching most recent mass audits");
        var audit = (await repository.GetAllAsync<MassAuditRecord>())
            .OrderByDescending(a => a.StartedAt).Take(1).ToList().FirstOrDefault();

        if (audit.Status == MassAuditStatus.Active)
        {
            Log.Debug("Mass audit is active");
            return CloseResult.StillRunning;
        }

        if (audit.Status == MassAuditStatus.Closed)
        {
            Log.Debug("Mass audit is already closed");
            return CloseResult.AlreadyClosed;
        }

        audit.SetClosed();
        audit.Log("Mass audit closed");
        await repository.UpsertAsync(audit);
        Log.Debug("Mass audit {AuditId} closed", audit.id);
        
        return CloseResult.Closed;
    }

    private void StartThread(MassAuditRecord massAudit)
    {
        Log.Debug("Start mass audit thread");
        this._cancellationTokenSource?.Dispose();
        this._cancellationTokenSource = new CancellationTokenSource();
        this._activeAuditTask?.Dispose();
        this._activeAuditTask = this.ExecuteMassAudit(massAudit, this._cancellationTokenSource.Token);
    }
    
    private async Task ExecuteMassAudit(MassAuditRecord massAudit, CancellationToken cancellationToken)
    {
        try {
            this._pause = false;
            this._cancel = false;
            
            Log.Debug($"Mass audit: started executing");
            massAudit.SetStarted();
            massAudit.Log("Mass audit started/resumed");
            await repository.UpsertAsync(massAudit);
            
            Log.Debug($"Mass audit: fetching all venues");
            var allVenues = await apiService.GetAllVenuesAsync();
            massAudit.TotalVenuesToAudit = allVenues.Count();
            Log.Debug($"Mass audit: fetching all existing audits for mass audit");
            var auditRecordsQuery =
                await repository.GetWhereAsync<VenueAuditRecord>(r => r.MassAuditId == massAudit.id);
            var auditRecords = auditRecordsQuery.ToList();
            foreach (var venue in allVenues)
            {
                if (cancellationToken.IsCancellationRequested && this._pause)
                {
                    massAudit.SetPaused();
                    massAudit.Log("Mass audit paused");
                    await repository.UpsertAsync(massAudit);
                    return;
                }

                if (cancellationToken.IsCancellationRequested && this._cancel)
                {
                    massAudit.SetCancelled();
                    massAudit.Log("Mass audit cancelled");
                    await repository.UpsertAsync(massAudit);
                    return;
                }
                
                massAudit.Log($"Sending audit for {venue.Name}");
                var existingRecord = auditRecords.FirstOrDefault(r => r.VenueId == venue.Id);

                var auditStatus = VenueAuditStatus.Pending;
                try
                {
                    if (existingRecord == null)
                    {
                        Log.Debug("Mass audit: starting audit for {Venue}", venue);
                        auditStatus = await venueAuditService
                            .CreateAuditFor(venue, massAudit.id, massAudit.RequestedIn, massAudit.RequestedBy)
                            .AuditAsync();
                    }
                    else if (existingRecord.Status == VenueAuditStatus.Pending)
                    {
                        Log.Debug("Mass audit: starting audit for {Venue} (picking up pending audit)", venue);
                        auditStatus = await venueAuditService.CreateAuditFor(venue, existingRecord).AuditAsync();
                    }
                    else
                    {
                        Log.Debug("Mass audit: already sent audit for {Venue}", venue);
                        massAudit.Log($"Already sent audit for {venue.Name}");
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Log.Warning("Mass audit: exception occured while auditing {Venue}", venue, e);
                    massAudit.Log($"Exception while sending audit for {venue.Name}. {e.Message}");
                    await repository.UpsertAsync(massAudit);
                    await Task.Delay(3000, cancellationToken);
                    continue;
                }

                if (auditStatus == VenueAuditStatus.AwaitingResponse)
                {
                    Log.Debug("Mass audit: sent audit for {Venue}", venue);
                    massAudit.Log($"Sent audit for {venue.Name}");
                }
                else if (auditStatus == VenueAuditStatus.Skipped)
                {
                    Log.Debug("Mass audit: skipped audit for {Venue}", venue);
                    massAudit.Log($"Skipped audit for {venue.Name}");
                } 
                else if (auditStatus == VenueAuditStatus.Failed)
                {
                    Log.Debug("Mass audit: failed to send audit for {Venue}", venue);
                    massAudit.Log($"Failed to send audit for {venue.Name}");
                }

                await repository.UpsertAsync(massAudit);

                if (auditStatus != VenueAuditStatus.Skipped)
                    await Task.Delay(3000, cancellationToken);
            }
            
            Log.Debug("Mass audit: audits sent to all venues");
            massAudit.SetCompleted();
            massAudit.Log("Mass audit complete");
            await repository.UpsertAsync(massAudit);

            Log.Debug("Mass audit: sending completion notification");
            if (await client.GetChannelAsync(massAudit.RequestedIn) is not IMessageChannel channel)
                channel = await client.GetDMChannelAsync(massAudit.RequestedIn);
            await channel.SendMessageAsync($"Hey {MentionUtils.MentionUser(massAudit.RequestedBy)}, I've completed sending all audits!");
            
            Log.Debug($"Mass audit: complete");
        } 
        catch (Exception e)
        {
            Log.Error(e, "Exception occured in mass audit execution");
        }
    }

}

public enum NoticeResult
{
    NoMassAudits,
    MassAuditRunning,
    MassAuditClosed,
    MassAuditNotComplete,
    Sent
}

public enum PauseResult
{
    NothingToPause,
    AlreadyPaused,
    Closed,
    Paused,
}

public enum CancelResult
{
    NothingToCancel,
    Cancelled
}

public enum StartResult
{
    AlreadyRunning,
    ActiveExists,
    InactiveExists,
    Started
}

public enum ResumeResult
{
    AlreadyRunning,
    NothingToResume,
    ResumedActive,
    ResumedInactive
}

public enum CloseResult
{
    AlreadyClosed,
    StillRunning,
    Closed
}