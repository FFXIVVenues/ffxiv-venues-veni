using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.VenueControl.VenueClosing.SessionStates;

namespace FFXIVVenues.Veni.VenueAuditing.ComponentHandlers.AuditResponse;

public class TemporarilyClosedHandler : BaseAuditHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "AUDIT_TEMP_CLOSED";
    
    private readonly IRepository _repository;
    private readonly IApiService _apiService;
    private readonly IAuthorizer _authorizer;
    private readonly IVenueAuditService _auditService;

    public TemporarilyClosedHandler(IRepository repository,
        IApiService apiService,
        IAuthorizer authorizer,
        IVenueAuditService auditService)
    {
        this._repository = repository;
        this._apiService = apiService;
        this._authorizer = authorizer;
        this._auditService = auditService;
    }
    
    public override async Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args)
    {
        var auditId = args[0];
        var audit = await this._repository.GetByIdAsync<VenueAuditRecord>(auditId);
        var venue = await this._apiService.GetVenueAsync(audit.VenueId);
        
        if (!this._authorizer.Authorize(context.Interaction.User.Id, Permission.EditVenue, venue).Authorized)
        {
            await context.Interaction.Message.Channel.SendMessageAsync("Sorry, I can't let you do that. 🥲");
            return;
        }
        
        context.Session.SetVenue(venue);
        await context.Session.MoveStateAsync<CloseEntrySessionState>(context);
        
        await this._auditService.UpdateAuditStatus(audit, venue, context.Interaction.User.Id,
            VenueAuditStatus.RespondedClose);
        
        if (audit.Messages.All(m => m.MessageId != context.Interaction.Message.Id))
            await context.Interaction.ModifyOriginalResponseAsync(m => m.Components = new ComponentBuilder().Build());
    }
    
}