using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using FFXIVVenues.Veni;
using Discord.WebSocket;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Abstractions;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Infrastructure.Intent;
using FFXIVVenues.Veni.Infrastructure.Logging;
using FFXIVVenues.Veni.Infrastructure.Persistence;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.People;
using FFXIVVenues.Veni.Services.Api;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.Veni.VenueAuditing.ComponentHandlers;
using FFXIVVenues.Veni.VenueControl;
using NChronicle.Core.Model;
using NChronicle.Console.Extensions;
using NChronicle.Core.Interfaces;
using FFXIVVenues.Veni.AI;
using FFXIVVenues.Veni.AI.Davinci;
using FFXIVVenues.Veni.AI.Luis;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Authorisation.Configuration;
using FFXIVVenues.Veni.Engineering;
using FFXIVVenues.Veni.GuildEngagment;
using FFXIVVenues.Veni.UserSupport;
using FFXIVVenues.Veni.VenueApproval;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueCreation.Command;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueEditing.Commands;
using FFXIVVenues.Veni.VenueControl.VenueClosing.Commands;
using FFXIVVenues.Veni.VenueControl.VenueDeletion.Commands;
using FFXIVVenues.Veni.VenueControl.VenueOpening.Command;
using FFXIVVenues.Veni.VenueDiscovery.Commands;
using FFXIVVenues.Veni.VenueRendering;

const string DISCORD_BOT_CONFIG_KEY = "DiscordBotToken";

var config = new ConfigurationBuilder()
                 .AddJsonFile("config.json", optional: true)
                 .AddUserSecrets<DiscordHandler>(optional: true)
                 .AddEnvironmentVariables("FFXIV_VENUES_VENI_")
                 .Build();

var luisConfig = new LuisConfiguration();
config.GetSection("Luis").Bind(luisConfig);
var apiConfig = new ApiConfiguration();
config.GetSection("Api").Bind(apiConfig);
var persistenceConfig = new PersistenceConfiguration();
config.GetSection("Persistence").Bind(persistenceConfig);
var uiConfig = new UiConfiguration();
config.GetSection("Ui").Bind(uiConfig);
var notificationConfig = new NotificationsConfiguration();
config.GetSection("Notifications").Bind(notificationConfig);
var authorisationConfig = new AuthorisationConfiguration();
config.GetSection("Authorisation").Bind(authorisationConfig);
var davinciConfig = new DavinciConfiguration();
config.GetSection("Davinci3").Bind(davinciConfig);

var apiHttpClient = new HttpClient { BaseAddress = new Uri(apiConfig.BaseUrl) };
apiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiConfig.AuthorizationKey);
apiHttpClient.Timeout = TimeSpan.FromSeconds(10);

var discordChronicleLibrary = new DiscordChronicleLibrary();
NChronicle.Core.NChronicle.Configure(c => {
    c.WithConsoleLibrary().Configure(c => 
        c.ListeningToAllLevels()
    );
    c.WithLibrary(discordChronicleLibrary);
});
var chronicle = new Chronicle();

IRepository repository = null;
if (persistenceConfig.Provider == PersistanceProvider.LiteDb)
    repository = new LiteDbRepository(persistenceConfig.ConnectionString);
else if (persistenceConfig.Provider == PersistanceProvider.Cosmos)
    repository = new CosmosDbRepository(persistenceConfig.ConnectionString, chronicle);
else
    repository = new InMemoryRepository();

var serviceCollection = new ServiceCollection();

serviceCollection.AddSingleton<IChronicle>(chronicle);
serviceCollection.AddSingleton<IDiscordChronicleLibrary>(discordChronicleLibrary);
serviceCollection.AddSingleton<IConfiguration>(config);
serviceCollection.AddSingleton(luisConfig);
serviceCollection.AddSingleton(authorisationConfig);
serviceCollection.AddSingleton(notificationConfig);
serviceCollection.AddSingleton(davinciConfig);
serviceCollection.AddSingleton(apiConfig);
serviceCollection.AddSingleton(persistenceConfig);
serviceCollection.AddSingleton(uiConfig);
serviceCollection.AddSingleton(apiHttpClient);
serviceCollection.AddSingleton(repository);
serviceCollection.AddSingleton<ICommandBroker, CommandBroker>();
serviceCollection.AddSingleton<IComponentBroker, ComponentBroker>();
serviceCollection.AddSingleton<IApiService, ApiService>();
serviceCollection.AddSingleton<IAuthorizer, Authorizer>();
serviceCollection.AddSingleton<IGuildManager, GuildManager>();
serviceCollection.AddSingleton<IVenueApprovalService, VenueApprovalService>();
serviceCollection.AddSingleton<IAIHandler, AIHandler>();
serviceCollection.AddSingleton<IDavinciService, DavinciService>();
serviceCollection.AddSingleton<IAIContextBuilder, AIContextBuilder>();
serviceCollection.AddSingleton<IIntentHandlerProvider, IntentHandlerProvider>();
serviceCollection.AddSingleton<ISessionProvider, SessionProvider>();
serviceCollection.AddSingleton<IDiscordHandler, DiscordHandler>();
serviceCollection.AddSingleton<ILuisClient, LuisClient>();
serviceCollection.AddSingleton<IVenueAuditFactory, VenueAuditFactory>();
serviceCollection.AddSingleton<IVenueRenderer, VenueRenderer>();
serviceCollection.AddSingleton<IInteractionContextFactory, InteractionContextFactory>();

var discordClient = GetDiscordSocketClient(config, chronicle);
serviceCollection.AddSingleton<IDiscordClient>(discordClient);
serviceCollection.AddSingleton(discordClient);

var serviceProvider = serviceCollection.BuildServiceProvider();

var commandBroker = serviceProvider.GetService<ICommandBroker>();
commandBroker.AddVenueControlCommands();
commandBroker.Add<EscalateCommand.CommandFactory, EscalateCommand.CommandHandler>(EscalateCommand.COMMAND_NAME);
commandBroker.Add<FindCommand.CommandFactory, FindCommand.CommandHandler>(FindCommand.COMMAND_NAME);
commandBroker.Add<HelpCommand.CommandFactory, HelpCommand.CommandHandler>(HelpCommand.COMMAND_NAME);
commandBroker.Add<ShowOpen.CommandFactory, ShowOpen.CommandHandler>(ShowOpen.COMMAND_NAME);
commandBroker.Add<ShowFor.CommandFactory, ShowFor.CommandHandler>(ShowFor.COMMAND_NAME);
commandBroker.Add<ShowMine.CommandFactory, ShowMine.CommandHandler>(ShowMine.COMMAND_NAME);
commandBroker.Add<InspectCommand.Factory, InspectCommand.Handler>(InspectCommand.COMMAND_NAME);
commandBroker.Add<OfflineJsonCommand.CommandFactory, OfflineJsonCommand.CommandHandler>(OfflineJsonCommand.COMMAND_NAME);
commandBroker.Add<SetRoleMap.CommandFactory, SetRoleMap.CommandHandler>(SetRoleMap.COMMAND_NAME);
commandBroker.Add<SetWelcomeJoinersCommand.CommandFactory, SetWelcomeJoinersCommand.CommandHandler>(SetWelcomeJoinersCommand.COMMAND_NAME);
commandBroker.Add<SetFormatNamesCommand.CommandFactory, SetFormatNamesCommand.CommandHandler>(SetFormatNamesCommand.COMMAND_NAME);
commandBroker.Add<ShowCount.CommandFactory, ShowCount.CommandHandler>(ShowCount.COMMAND_NAME);
commandBroker.Add<GraphCommand.CommandFactory, GraphCommand.CommandHandler>(GraphCommand.COMMAND_NAME);
commandBroker.Add<GetUnapprovedCommand.CommandFactory, GetUnapprovedCommand.CommandHandler>(GetUnapprovedCommand.COMMAND_NAME);
commandBroker.Add<BlacklistCommand.CommandFactory, BlacklistCommand.CommandHandler>(BlacklistCommand.COMMAND_NAME);

serviceProvider.GetService<IComponentBroker>()
    .AddVenueAuditingHandlers()
    .AddVenueControlHandlers()
    .AddVenueRenderingHandlers();

await serviceProvider.GetService<IDiscordHandler>().ListenAsync();

await Task.Delay(Timeout.Infinite);

// Factory
static DiscordSocketClient GetDiscordSocketClient(IConfigurationRoot config, IChronicle chronicle)
{
    var discordKey = config.GetValue<string>(DISCORD_BOT_CONFIG_KEY);
    if (discordKey == null)
        throw new Exception("Discord Bot Token not set!");

    var client = new DiscordSocketClient(new() {
        LogLevel = LogSeverity.Verbose,
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
    });
    client.Log += (LogMessage msg) =>
    {
        if (msg.Severity == LogSeverity.Critical)
            chronicle.Critical(msg.Message, msg.Exception);
        if (msg.Severity == LogSeverity.Error || msg.Severity == LogSeverity.Warning)
            chronicle.Warning(msg.Message, msg.Exception);
        if (msg.Severity == LogSeverity.Info)
            chronicle.Info(msg.Message);
        if (msg.Severity == LogSeverity.Debug || msg.Severity == LogSeverity.Verbose)
            chronicle.Debug(msg.Message);
        return Task.CompletedTask;
    };
    client.LoginAsync(TokenType.Bot, discordKey);
    return client;
}
