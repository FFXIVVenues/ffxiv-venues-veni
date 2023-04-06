using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Services.Api;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.VenueModels;
using Microsoft.Azure.Cosmos;

namespace FFXIVVenues.Veni.VenueAuditing.ComponentHandlers;

public class ConfirmCorrectHandler : BaseAuditHandler
{
    
    // Change this key and any existing buttons linked to this will die
    public static string Key => "AUDIT_CONFIRM_CORRECT";
    
    private readonly IRepository _repository;
    private readonly IApiService _apiService;
    private readonly DiscordSocketClient _discordClient;
    private readonly IVenueRenderer _venueRenderer;

    private readonly string[] _responses = new[]
    {
        "Thanks! 🥰",
        "Thank you! 🥰",
        "Thankies! 💕",
        "Thank you so much! 💕"
    };

    public ConfirmCorrectHandler(IRepository repository,
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
            $"You handled this and confirmed the venue's details. 🥳", 
            $"{context.Interaction.User.Username} handled this and confirmed the venue's details. 🥳");
        
        await context.Interaction.Message.Channel.SendMessageAsync(_responses.PickRandom());
        
        UpdateAudit(context, audit, VenueAuditStatus.RespondedConfirmed,
            $"{MentionUtils.MentionUser(context.Interaction.User.Id)} confirmed the venues details.");
        await this._repository.UpsertAsync(audit);
        
        if (audit.RoundId == null) 
            await NotifyRequesterAsync(context, audit, venue, 
                $"{MentionUtils.MentionUser(context.Interaction.User.Id)} confirmed the venues details. 😘");
    }
    
}