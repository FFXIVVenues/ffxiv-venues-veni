using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.VenueModels;
using NChronicle.Core.Interfaces;
using ScottPlot.Renderable;

namespace FFXIVVenues.Veni.Auditing;

internal class AuditingService
{

    
    private readonly IApiService _apiService;
    private readonly IRepository _repository;
    private readonly IChronicle _chronicle;
    private readonly IVenueAuditFactory _venueAuditFactory;
    private Task _activeAuditTask;

    public AuditingService(IApiService apiService, IRepository repository, IChronicle chronicle, IVenueAuditFactory venueAuditFactory)
    {
        this._apiService = apiService;
        this._repository = repository;
        this._chronicle = chronicle;
        this._venueAuditFactory = venueAuditFactory;
    }

    public async Task StartAsync()
    {
        if (this._activeAuditTask != null)
            return;

        var activeAudits = await this._repository.GetWhere<AuditRoundState>(a => a.Status == AuditStatus.Active);
        var audit = activeAudits.FirstOrDefault();
        if (audit == null)
        {
            audit = new AuditRoundState();
        }

        this._activeAuditTask = new TaskFactory().StartNew(_ => ExecuteAudit(audit), TaskCreationOptions.LongRunning);
    }

    private async Task ExecuteAudit(AuditRoundState auditRound)
    {
        var allVenues = await this._apiService.GetAllVenuesAsync();
        foreach (var venue in allVenues)
        {
            var existingRecord = auditRound.VenueAudit.FirstOrDefault(r => r.VenueId == venue.Id);
            if (existingRecord != null && existingRecord.Status != VenueAuditStatus.Pending)
                continue;

            var venueRound = this._venueAuditFactory.CreateAuditFor(venue);
            await venueRound.AuditAsync();
        }
    }

}

public enum AuditStatus
{
    Inactive,
    Skipped,
    Active,
    Complete
}