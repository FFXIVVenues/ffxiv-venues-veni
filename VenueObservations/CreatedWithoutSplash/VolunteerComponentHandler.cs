using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.Veni.VenueRendering;

namespace FFXIVVenues.Veni.VenueObservations.CreatedWithoutSplash;

public class VolunteerComponentHandler : IComponentHandler
{
    public static string Key => "VENUE-OBS__WITHOUT-SPLASH__CLAIM";
    
    private readonly IRepository _repository;
    private readonly IApiService _apiService;
    private readonly IDiscordClient _client;
    private readonly IVenueRenderer _venueRenderer;

    public VolunteerComponentHandler(IRepository repository,
        IApiService apiService,
        IDiscordClient client,
        IVenueRenderer venueRenderer)
    {
        this._repository = repository;
        this._apiService = apiService;
        this._client = client;
        this._venueRenderer = venueRenderer;
    }
    
    public async Task HandleAsync(MessageComponentVeniInteractionContext context, string[] args)
    {
        var broadcastId = args[0];
        var venueId = args[1];
        var venue = await this._apiService.GetVenueAsync(venueId);
        var broadcast = await this._repository.GetByIdAsync<BroadcastReceipt>(broadcastId);
        var responder = context.Interaction.User;
        
        foreach (var broadcastMessage in broadcast.BroadcastMessages)
        {
            if (broadcastMessage.Status != MessageStatus.Sent) continue;
            var newMessage = "You're handling this.  🥳";
            if (broadcastMessage.UserId != responder.Id)
                newMessage = $"{context.Interaction.User.Username} is handling this. 🥰";

            var channel = await this._client.GetChannelAsync(broadcastMessage.ChannelId) as IDMChannel;
            channel?.ModifyMessageAsync(broadcastMessage.MessageId, props =>
            {
                props.Components = new ComponentBuilder().Build();
                props.Embeds = new[]
                {
                    this._venueRenderer.RenderEmbed(venue).Build(),
                    new EmbedBuilder().WithDescription(newMessage).Build()
                };
            });

        }
        
        await context.Interaction.Channel.SendMessageAsync("Have fun with it! 💕");
    }
}