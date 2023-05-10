using System;
using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation.Configuration;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Utils.Broadcasting;
using FFXIVVenues.Veni.VenueRendering;
using FFXIVVenues.VenueModels.Observability;

namespace FFXIVVenues.Veni.VenueObservations.CreatedWithoutSplash;

public class CreatedWithoutSplashObserver : IApiObserver
{
    private readonly IApiService _apiService;
    private readonly IRepository _repository;
    private readonly IVenueRenderer _venueRenderer;
    private readonly IDiscordClient _discordClient;
    private readonly NotificationsConfiguration _config;

    public CreatedWithoutSplashObserver(IApiService apiService,
        IRepository repository,
        IVenueRenderer venueRenderer,
        IDiscordClient discordClient,
        NotificationsConfiguration config)
    {
        this._apiService = apiService;
        this._repository = repository;
        this._venueRenderer = venueRenderer;
        this._discordClient = discordClient;
        this._config = config;
    }
    
    public async Task Handle(Observation observation)
    {
        var venue = await this._apiService.GetVenueAsync(observation.SubjectId);
        if (venue.BannerUri != null)
            return;

        var broadcastId = IdHelper.GenerateId(8);
        var broadcast =  new Broadcast(broadcastId, this._discordClient);
        broadcast.WithMessage($"Heyo photographers!\nI have a new venue in need of a splash banner! :heart:");
        broadcast.WithEmbed(this._venueRenderer.RenderEmbed(venue));
        broadcast.WithComponent(bcc =>
        {
            var button = new ButtonBuilder();
            button.WithLabel("I'll take it!");
            button.WithStaticHandler(VolunteerComponentHandler.Key, broadcastId, venue.Id);
            button.WithStyle(ButtonStyle.Success);
            
            var builder = new ComponentBuilder();
            builder.WithButton(button);

            return builder;
        });
        var broadcastReceipt = await broadcast.SendToAsync(this._config.MissingSplash);
        await this._repository.UpsertAsync(broadcastReceipt);
    }
}