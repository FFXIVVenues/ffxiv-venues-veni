using FFXIVVenues.Veni.AI.Davinci;
using FFXIVVenues.Veni.AI.Luis;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Authorisation.Configuration;
using FFXIVVenues.Veni.Infrastructure.Persistence;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring;
using FFXIVVenues.Veni.VenueRendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FFXIVVenues.Veni;

internal static partial class Bootstrap
{
    internal static Configurations LoadConfiguration(ServiceCollection serviceCollection)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("config.json", optional: true)
            .AddUserSecrets<DiscordHandler>(optional: true)
            .AddEnvironmentVariables("FFXIV_VENUES_VENI_")
            .Build();
        
        var allConfig = new Configurations
        {
            DiscordToken = config.GetValue<string>("DiscordBotToken"),
            LoggingConfig = config.GetSection("Logging").Get<LoggingConfiguration>() ?? new(),
            LuisConfig = config.GetSection("Clu").Get<CluConfiguration>() ?? new(),
            ApiConfig = config.GetSection("Api").Get<ApiConfiguration>() ?? new(),
            PersistenceConfig = config.GetSection("Persistence").Get<PersistenceConfiguration>() ?? new(),
            UiConfig = config.GetSection("Ui").Get<UiConfiguration>() ?? new(),
            NotificationConfig = config.GetSection("Notifications").Get<NotificationsConfiguration>() ?? new(),
            AuthorisationConfig = config.GetSection("Authorisation").Get<AuthorisationConfiguration>() ?? new(),
            DavinciConfig = config.GetSection("Davinci3").Get<DavinciConfiguration>() ?? new()
        };

        serviceCollection.AddSingleton<IConfiguration>(config);
        serviceCollection.AddSingleton(allConfig.LuisConfig);
        serviceCollection.AddSingleton(allConfig.AuthorisationConfig);
        serviceCollection.AddSingleton(allConfig.NotificationConfig);
        serviceCollection.AddSingleton(allConfig.DavinciConfig);
        serviceCollection.AddSingleton(allConfig.ApiConfig);
        serviceCollection.AddSingleton(allConfig.PersistenceConfig);
        serviceCollection.AddSingleton(allConfig.UiConfig);

        return allConfig;
    }
}

internal class Configurations
{
    public string DiscordToken { get; set; }
    public LoggingConfiguration LoggingConfig { get; set; }
    public CluConfiguration LuisConfig { get; set; }
    public ApiConfiguration ApiConfig { get; set; }
    public PersistenceConfiguration PersistenceConfig { get; set; }
    public UiConfiguration UiConfig { get; set; }
    public NotificationsConfiguration NotificationConfig { get; set; }
    public AuthorisationConfiguration AuthorisationConfig { get; set; }
    public DavinciConfiguration DavinciConfig { get; set; }
}