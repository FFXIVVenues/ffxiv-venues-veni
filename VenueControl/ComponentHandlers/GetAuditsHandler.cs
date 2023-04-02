using System.Linq;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.People;
using FFXIVVenues.Veni.VenueAuditing;

namespace FFXIVVenues.Veni.VenueControl.ComponentHandlers;

public class GetAuditsHandler : IComponentHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "CONTROL_GET_AUDITS";

    private readonly IStaffService _staffService;
    private readonly IRepository _repository;

    public GetAuditsHandler(IStaffService staffService, IRepository repository)
    {
        this._staffService = staffService;
        this._repository = repository;
    }
    
    public async Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args)
    {
        var user = context.Interaction.User.Id;
        if (!this._staffService.IsEditor(user))
            return;
        
        _ = context.Interaction.ModifyOriginalResponseAsync(props =>
            props.Components = new ComponentBuilder().Build());
        
        var venueId = args[0];
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

        foreach (var audit in audits)
            dropDown.AddOption(audit.SentTime.ToString("G"), audit.id);
            // this._repository.DeleteAsync<VenueAuditRecord>(audit.id);

        builder.WithSelectMenu(dropDown);
        await context.Interaction.Channel.SendMessageAsync("Okay, here they are! 🥰", components: builder.Build());
    }
    
}