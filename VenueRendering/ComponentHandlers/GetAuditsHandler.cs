using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.VenueAuditing;

namespace FFXIVVenues.Veni.VenueRendering.ComponentHandlers;

public class GetAuditsHandler : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "CONTROL_GET_AUDITS";

    private readonly IAuthorizer _authorizer;
    private readonly IRepository _repository;
    private readonly IApiService _apiService;

    public GetAuditsHandler(IAuthorizer authorizer, IRepository repository, IApiService apiService)
    {
        this._authorizer = authorizer;
        this._repository = repository;
        this._apiService = apiService;
    }
    
    public async Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        var venueId = args[0];
        var venue = await this._apiService.GetVenueAsync(venueId);
        if (!this._authorizer.Authorize(user, Permission.ViewAuditHistory, venue).Authorized)
            return;
        
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());
        
        var auditsQuery = await this._repository.GetWhere<VenueAuditRecord>(r => r.VenueId == venueId);
        var audits = auditsQuery.ToList();
        
        if (!audits.Any())
        {
            await context.Interaction.Channel.SendMessageAsync("No audits on record for this venue yet! 🥰");
            return;
        }
        
        var builder = new ComponentBuilder();
        var dropDown = new SelectMenuBuilder()
            .WithStaticHandler(GetAuditHandler.Key)
            .WithPlaceholder("What would you like to do?");

        foreach (var audit in audits.OrderByDescending(a => a.SentTime))
            dropDown.AddOption($"Audit sent at {audit.SentTime.ToString("G")}", 
                audit.id, $"Status: {audit.Status}");
        
        builder.WithSelectMenu(dropDown);
        await context.Interaction.Channel.SendMessageAsync("Okay, here they are! 🥰", components: builder.Build());
    }
    
}