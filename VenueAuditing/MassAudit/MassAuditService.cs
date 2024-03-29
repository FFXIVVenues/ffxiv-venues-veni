using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.VenueAuditing.MassAudit.Exporting;
using FFXIVVenues.Veni.VenueAuditing.MassAudit.Models;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring;
using Serilog;


namespace FFXIVVenues.Veni.VenueAuditing.MassAudit;

internal class MassAuditService :  IMassAuditService
{
    
    private readonly IApiService _apiService;
    private readonly IRepository _repository;
    private readonly IVenueAuditService _venueAuditService;
    private readonly IDiscordClient _client;
    private readonly IMassAuditExporter _massAuditExporter;
    private readonly IDiscordValidator _discordValidator;
    private bool _pause = false;
    private bool _cancel = false;
    private Task _activeAuditTask;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly HttpClient _http;

    public MassAuditService(IApiService apiService,
        IRepository repository,
        IVenueAuditService venueAuditService,
        IDiscordClient client,
        IMassAuditExporter massAuditExporter, 
        IDiscordValidator discordValidator)
    {
        this._apiService = apiService;
        this._repository = repository;
        this._venueAuditService = venueAuditService;
        this._client = client;
        this._massAuditExporter = massAuditExporter;
        this._discordValidator = discordValidator;
        this._http = new HttpClient();
    }

    public async Task<ResumeResult> ResumeMassAuditAsync(bool activeOnly)
    {
        Log.Debug("Resume requested for mass audit.");
        if (this._activeAuditTask != null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Mass audit already running.");
            return ResumeResult.AlreadyRunning;
        }

        IQueryable<MassAuditRecord> activeAudits;
        if (activeOnly)
        {
            Log.Debug("Fetching active mass audits.");
            activeAudits = await this._repository.GetWhere<MassAuditRecord>(a => a.Status == MassAuditStatus.Active);
        }
        else
        {
            Log.Debug("Fetching active and inactive mass audits.");
            activeAudits = await this._repository.GetWhere<MassAuditRecord>(a => 
                a.Status == MassAuditStatus.Active || a.Status == MassAuditStatus.Inactive);
        }
        var audit = activeAudits.OrderByDescending(a => a.StartedAt).ToList().SingleOrDefault();

        if (audit == null)
        {
            Log.Debug("No mass audit to resume.");
            return ResumeResult.NothingToResume;
        }

        var status = audit.Status;

        this.StartThread(audit);

        if (status == MassAuditStatus.Active)
        {
            Log.Debug("Active mass audit resumed.");
            return ResumeResult.ResumedActive;
        }
        
        Log.Information("Inactive mass audit {AuditId} resumed.", audit.id);
        return ResumeResult.ResumedInactive;
    }

    public async Task<StartResult> StartMassAuditAsync(ulong requestedIn, ulong requestedBy)
    {
        Log.Debug("Start request for mass audit.");
        if (this._activeAuditTask != null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Active mass audit already exists.");
            return StartResult.ActiveExists;
        }

        Log.Debug("Fetching active and inactive mass audits.");
        var activeAudits = await this._repository.GetWhere<MassAuditRecord>(a => 
            a.Status == MassAuditStatus.Active ||
            a.Status == MassAuditStatus.Inactive);
        var audit = activeAudits.OrderByDescending(a => a.StartedAt).ToList().SingleOrDefault();
        
        if (audit != null)
            if (audit.Status == MassAuditStatus.Active)
            {
                Log.Debug("Active mass audit already exists (but is not running).");
                return StartResult.ActiveExists;
            }
            else
            {
                Log.Debug("Inactive mass audit already exists.");
                return StartResult.InactiveExists;
            }
                
        audit = new MassAuditRecord()
        {
            RequestedIn = requestedIn,
            RequestedBy = requestedBy
        };
        
        this.StartThread(audit);
        
        Log.Information("Mass audit {AuditId} started.", audit.id);
        return StartResult.Started;
    }

    public async Task<PauseResult> PauseAsync()
    {
        Log.Debug("Pause request for mass audit.");
        var pausedViaTask = false;
        if (this._activeAuditTask != null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Mass audit cancellation token triggered.");
            this._pause = true;
            this._cancellationTokenSource.Cancel();
            await this._activeAuditTask;
            pausedViaTask = true;
        }
        
        Log.Debug("Fetching active and inactive mass audits.");
        var audit = (await this._repository.GetWhere<MassAuditRecord>(a => 
                a.Status == MassAuditStatus.Active || a.Status == MassAuditStatus.Inactive))
            .OrderByDescending(a => a.StartedAt).Take(1).ToList().FirstOrDefault();
        
        if (audit == null)
        {
            Log.Debug("No mass audits to pause.");
            return PauseResult.NothingToPause;
        }
        
        if (audit.Status == MassAuditStatus.Inactive)
        {
            if (!pausedViaTask) {
                Log.Debug("Mass audit already paused.");
                return PauseResult.AlreadyPaused;
            }
            Log.Debug("Mass audit paused.");
            return PauseResult.Paused;
        }
        
        audit.SetPaused();
        audit.Log("Mass audit paused.");
        await this._repository.UpsertAsync(audit);
        
        Log.Information("Mass audit {AuditId} paused.", audit.id);
        return PauseResult.Paused;
    }

    public async Task<CancelResult> CancelAsync()
    {
        Log.Debug("Cancel request for mass audit.");
        var cancelledViaTask = false;
        if (this._activeAuditTask != null && !this._activeAuditTask.IsCompleted)
        {
            Log.Debug("Mass audit cancellation token triggered.");
            this._cancel = true;
            this._cancellationTokenSource.Cancel();
            await this._activeAuditTask;
            cancelledViaTask = true;
        }
        
        Log.Debug("Fetching active and inactive mass audits.");
        var activeAudits = (await this._repository.GetWhere<MassAuditRecord>(a => 
                    a.Status == MassAuditStatus.Active || a.Status == MassAuditStatus.Inactive))
            .OrderByDescending(a => a.StartedAt);

        if (!activeAudits.Any())
        {
            if (cancelledViaTask)
            {
                Log.Debug("Mass audit cancelled.");
                return CancelResult.Cancelled;
            }
            Log.Debug("No mass audits to cancel.");
            return CancelResult.NothingToCancel;
        }

        foreach (var audit in activeAudits)
        {
            audit.SetCancelled();
            audit.Log("Mass audit cancelled.");
            await this._repository.UpsertAsync(audit);
            Log.Debug($"Mass audit {audit.id} cancelled.");
        }

        Log.Information("All active/inactive mass audits cancelled.");
        return CancelResult.Cancelled;
    }

    public async Task<MassAuditStatusSummary> GetStatusSummaryAsync()
    {
        Log.Debug("Mass audit summary requested.");
        var activeAuditRounds = await this._repository.GetAll<MassAuditRecord>();
        var auditRound = activeAuditRounds.OrderByDescending(a => a.StartedAt).Take(1).ToList().FirstOrDefault();
        if (auditRound == null)
            return null;

        var audits = await this._repository.GetWhere<VenueAuditRecord>(a => a.MassAuditId == auditRound.id);
        var allVenues = await this._apiService.GetAllVenuesAsync();

        return this._massAuditExporter.GetSummaryForMassAudit(auditRound, allVenues, audits.ToList());
    }

    public async Task<MassAuditStatusReport> GetStatusReportAsync()
    {
        Log.Debug("Mass audit report requested.");
        var activeAuditRounds = await this._repository.GetAll<MassAuditRecord>();
        var auditRound = activeAuditRounds.OrderByDescending(a => a.StartedAt).Take(1).ToList().FirstOrDefault();
        if (auditRound == null)
            return null;

        var audits = await this._repository.GetWhere<VenueAuditRecord>(a => a.MassAuditId == auditRound.id);
        var allVenues = await this._apiService.GetAllVenuesAsync();
        
        return await this._massAuditExporter.GetExportForMassAuditAsync(auditRound, allVenues, audits.ToList());
    }

    private void StartThread(MassAuditRecord massAudit)
    {
        Log.Debug("Start mass audit thread.");
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
            
            Log.Debug($"Mass audit: started executing.");
            massAudit.SetStarted();
            massAudit.Log("Mass audit started/resumed.");
            await this._repository.UpsertAsync(massAudit);
            
            Log.Debug($"Mass audit: fetching all venues.");
            var allVenues = await this._apiService.GetAllVenuesAsync();
            massAudit.TotalVenuesToAudit = allVenues.Count();
            Log.Debug($"Mass audit: fetching all existing audits for mass audit.");
            var auditRecordsQuery =
                await this._repository.GetWhere<VenueAuditRecord>(r => r.MassAuditId == massAudit.id);
            var auditRecords = auditRecordsQuery.ToList();
            foreach (var venue in allVenues)
            {
                if (cancellationToken.IsCancellationRequested && this._pause)
                {
                    massAudit.SetPaused();
                    massAudit.Log("Mass audit paused.");
                    await this._repository.UpsertAsync(massAudit);
                    return;
                }

                if (cancellationToken.IsCancellationRequested && this._cancel)
                {
                    massAudit.SetCancelled();
                    massAudit.Log("Mass audit cancelled.");
                    await this._repository.UpsertAsync(massAudit);
                    return;
                }
                
                massAudit.Log($"Sending audit for {venue.Name}.");
                var existingRecord = auditRecords.FirstOrDefault(r => r.VenueId == venue.Id);

                var auditStatus = VenueAuditStatus.Pending;
                try
                {
                    if (existingRecord == null)
                    {
                        Log.Debug("Mass audit: starting audit for {Venue}.", venue);
                        auditStatus = await this._venueAuditService
                            .CreateAuditFor(venue, massAudit.id, massAudit.RequestedIn, massAudit.RequestedBy)
                            .AuditAsync();
                    }
                    else if (existingRecord.Status == VenueAuditStatus.Pending)
                    {
                        Log.Debug("Mass audit: starting audit for {Venue} (picking up pending audit).", venue);
                        auditStatus = await this._venueAuditService.CreateAuditFor(venue, existingRecord).AuditAsync();
                    }
                    else
                    {
                        Log.Debug("Mass audit: already sent audit for {Venue}.", venue);
                        massAudit.Log($"Already sent audit for {venue.Name}");
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Log.Warning("Mass audit: exception occured while auditing {Venue}.", venue, e);
                    massAudit.Log($"Exception while sending audit for {venue.Name}. {e.Message}");
                    await this._repository.UpsertAsync(massAudit);
                    await Task.Delay(3000, cancellationToken);
                    continue;
                }

                if (auditStatus == VenueAuditStatus.AwaitingResponse)
                {
                    Log.Debug("Mass audit: sent audit for {Venue}.", venue);
                    massAudit.Log($"Sent audit for {venue.Name}.");
                }
                else if (auditStatus == VenueAuditStatus.Skipped)
                {
                    Log.Debug("Mass audit: skipped audit for {Venue}.", venue);
                    massAudit.Log($"Skipped audit for {venue.Name}.");
                } 
                else if (auditStatus == VenueAuditStatus.Failed)
                {
                    Log.Debug("Mass audit: failed to send audit for {Venue}.", venue);
                    massAudit.Log($"Failed to send audit for {venue.Name}.");
                }

                await this._repository.UpsertAsync(massAudit);

                if (auditStatus != VenueAuditStatus.Skipped)
                    await Task.Delay(3000, cancellationToken);
            }
            
            Log.Debug("Mass audit: audits sent to all venues.");
            massAudit.SetCompleted();
            massAudit.Log("Mass audit complete.");
            await this._repository.UpsertAsync(massAudit);

            Log.Debug("Mass audit: sending completion notification.");
            if (await this._client.GetChannelAsync(massAudit.RequestedIn) is not IMessageChannel channel)
                channel = await this._client.GetDMChannelAsync(massAudit.RequestedIn);
            await channel.SendMessageAsync($"Hey {MentionUtils.MentionUser(massAudit.RequestedBy)}, I've completed sending all audits!");
            
            Log.Debug($"Mass audit: complete.");
        } 
        catch (Exception e)
        {
            Log.Error("Exception occured in mass audit execution.", e);
        } 
    }

}

public enum PauseResult
{
    NothingToPause,
    AlreadyPaused,
    Paused
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