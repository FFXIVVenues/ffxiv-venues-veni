using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.VenueModels;

namespace FFXIVVenues.Veni.Auditing.ComponentHandlers;

public class ConfirmCorrectHandler : IComponentHandler
{
    // Change this key and any existing buttons linked to this will die
    public static string Key => "AUDIT_CONFIRM_CORRECT";
    
    private readonly IRepository _repository;
    private readonly IApiService _apiService;
    private readonly IDiscordClient _discordClient;
    private readonly IVenueRenderer _venueRenderer;

    private readonly string[] _responses = new[]
    {
        "🥰 Thanks!",
        "🥰 Thank you!",
        "💕 Thankies!",
        "😘 Thank you so much!"
    };

    public ConfirmCorrectHandler(IRepository repository, IApiService apiService, IDiscordClient discordClient, IVenueRenderer venueRenderer)
    {
        this._repository = repository;
        this._apiService = apiService;
        this._discordClient = discordClient;
        this._venueRenderer = venueRenderer;
    }
    
    public async Task HandleAsync(SocketMessageComponent component, string[] args)
    {
        var auditId = args[0];
        var audit = await this._repository.GetByIdAsync<VenueAuditRecord>(auditId);
        var venue = await this._apiService.GetVenueAsync(audit.VenueId);
        await this.UpdateResponder(venue, component.User, audit.SentMessages);
        await this.UpdateOtherManagers(venue, component.User, audit.SentMessages);
        await component.Message.Channel.SendMessageAsync(_responses.PickRandom());
        audit.Status = VenueAuditStatus.RespondedConfirmed;
        audit.Log($"{component.User.Username}#{component.User.Discriminator} confirmed the venue details.");
        audit.ResolutionTime = DateTime.UtcNow;
        await this._repository.UpsertAsync(audit);
    }

    private async Task UpdateOtherManagers(Venue venue, IUser responder, IEnumerable<AuditMessage> sentMessages)
    {
        foreach (var message in sentMessages)
        {
            if (message.UserId == responder.Id) continue;
            
            var channel = await this._discordClient.GetChannelAsync(message.ChannelId);
            (channel as IDMChannel)?.ModifyMessageAsync(message.MessageId, props =>
            {
                props.Components = new ComponentBuilder().Build();
                props.Embeds = new[]
                {
                    this._venueRenderer.RenderEmbed(venue).Build(),
                    new EmbedBuilder().WithDescription($"{responder.Username} handled this and confirmed the venue's details. 🥳").Build()
                };
            });
        }
    }
    
    private async Task UpdateResponder(Venue venue, IUser responder, IEnumerable<AuditMessage> sentMessages)
    {
        var message = sentMessages.FirstOrDefault(m => m.UserId == responder.Id);
        
        var channel = await this._discordClient.GetChannelAsync(message.ChannelId);
        (channel as IDMChannel)?.ModifyMessageAsync(message.MessageId, props =>
        {
            props.Components = new ComponentBuilder().Build();
            props.Embeds = new[]
            {
                this._venueRenderer.RenderEmbed(venue).Build(),
                new EmbedBuilder().WithDescription($"You handled this and confirmed the venue's details. 🥳").Build()
            };
        });
    }
    
}