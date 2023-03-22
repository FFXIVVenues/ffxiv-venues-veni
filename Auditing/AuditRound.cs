using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using NChronicle.Core.Interfaces;

namespace FFXIVVenues.Veni.Auditing;

internal class AuditRound
{
    
    private readonly IApiService _apiService;
    private readonly IRepository _repository;
    private readonly IChronicle _chronicle;
    private readonly IVenueAuditFactory _venueAuditFactory;
    private bool _pause = false;
    private Task _activeAuditTask;

    public AuditRound(IApiService apiService, IRepository repository, IChronicle chronicle, IVenueAuditFactory venueAuditFactory)
    {
        this._apiService = apiService;
        this._repository = repository;
        this._chronicle = chronicle;
        this._venueAuditFactory = venueAuditFactory;
    }

    public async Task StartRoundAsync()
    {
        if (this._activeAuditTask != null)
            return;

        var activeAudits = await this._repository.GetWhere<AuditRoundRecord>(a => a.Status == AuditStatus.Active);
        var audit = activeAudits.FirstOrDefault();
        if (audit == null)
            audit = new AuditRoundRecord();

        this._pause = false;
        this._activeAuditTask = new TaskFactory().StartNew(_ => ExecuteAudit(audit), null, TaskCreationOptions.LongRunning);
    }

    public void Pause()
    {
        this._pause = true;
    }

    private async Task ExecuteAudit(AuditRoundRecord auditRound)
    {
        auditRound.SetStarted();
        auditRound.Log("Audit round started/resumed.");
        await this._repository.UpsertAsync(auditRound);
        
        var allVenues = await this._apiService.GetAllVenuesAsync();
        var auditRecords = await this._repository.GetWhere<VenueAuditRecord>(r => r.RoundId == auditRound.id);
        foreach (var venue in allVenues)
        {
            if (this._pause)
            {
                auditRound.SetPaused();
                auditRound.Log("Audit round paused.");
                await this._repository.UpsertAsync(auditRound);
                return;
            }
            
            auditRound.Log($"Sending audit for {venue.Name}.");
            var existingRecord = auditRecords.FirstOrDefault(r => r.VenueId == venue.Id);
            if (existingRecord == null)
                await this._venueAuditFactory.CreateAuditFor(venue, auditRound.id).AuditAsync();
            else if (existingRecord.Status == VenueAuditStatus.Pending)
                await this._venueAuditFactory.CreateAuditFor(venue, existingRecord).AuditAsync();
            auditRound.Log($"Sent audit for {venue.Name}.");
        }
        
        auditRound.SetCompleted();
        auditRound.Log("Audit round complete.");
        await this._repository.UpsertAsync(auditRound);
    }

}

public enum AuditStatus
{
    Inactive,
    Active,
    Complete
}