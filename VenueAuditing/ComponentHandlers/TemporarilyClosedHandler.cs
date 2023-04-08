using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Services.Api;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.VenueControl.SessionStates;

namespace FFXIVVenues.Veni.VenueAuditing.ComponentHandlers;

public class TemporarilyClosedHandler : BaseAuditHandler
{

    // Change this key and any existing buttons linked to this will die
    public static string Key => "AUDIT_TEMP_CLOSED";
    
    private readonly IRepository _repository;
    private readonly IApiService _apiService;
    private readonly DiscordSocketClient _discordClient;
    private readonly IVenueRenderer _venueRenderer;

    public TemporarilyClosedHandler(IRepository repository,
        IApiService apiService,
        DiscordSocketClient discordClient,
        IVenueRenderer venueRenderer)
    {
        this._repository = repository;
        this._apiService = apiService;
        this._discordClient = discordClient;
        this._venueRenderer = venueRenderer;
    }
    
    public override async Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args)
    {
        var auditId = args[0];
        var audit = await this._repository.GetByIdAsync<VenueAuditRecord>(auditId);
        var venue = await this._apiService.GetVenueAsync(audit.VenueId);
        await this.UpdateSentMessages(this._discordClient, this._venueRenderer, 
            venue, context.Interaction.User, audit.Messages, 
            $"You handled this and temporarily closed the venue. 😢", 
            $"{context.Interaction.User.Username} handled this and temporarily closed the venue. 😢");
        
        context.Session.SetItem("venue", venue);
        await context.Session.MoveStateAsync<CloseEntrySessionState>(context);
        
        UpdateAudit(context, audit, VenueAuditStatus.RespondedClose,
            $"{MentionUtils.MentionUser(context.Interaction.User.Id)} temporarily closed the venue.");
        await this._repository.UpsertAsync(audit);
        
        if (audit.RoundId == null) 
            await NotifyRequesterAsync(context, audit, venue, 
                $"{MentionUtils.MentionUser(context.Interaction.User.Id)} temporarily closed the venue. 😢");
    }
    
}