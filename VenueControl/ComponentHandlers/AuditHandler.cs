using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.People;
using FFXIVVenues.Veni.Services.Api;
using FFXIVVenues.Veni.VenueAuditing;

namespace FFXIVVenues.Veni.VenueControl.ComponentHandlers;

public class AuditHandler : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "CONTROL_AUDIT";

    private readonly IStaffService _staffService;
    private readonly IApiService _apiService;
    private readonly IVenueAuditFactory _auditFactory;

    public AuditHandler(IStaffService staffService, IApiService apiService, IVenueAuditFactory auditFactory)
    {
        this._staffService = staffService;
        this._apiService = apiService;
        this._auditFactory = auditFactory;
    }
    
    public async Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        if (!this._staffService.IsEditor(user))
            return;
        
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());
        
        var venueId = args[0];
        var venue = await this._apiService.GetVenueAsync(venueId);
        
        var audit = this._auditFactory.CreateAuditFor(venue,
            roundId: null,
            context.Interaction.Channel.Id,
            context.Interaction.User.Id);
        
        await audit.AuditAsync(true);
        await context.Interaction.Channel.SendMessageAsync("Okay, I've messaged the manager(s)! 🥰");
    }
    
}