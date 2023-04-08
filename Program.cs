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
using FFXIVVenues.Veni.Commands;
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
using FFXIVVenues.Veni.Services.Luis;
using FFXIVVenues.Veni.VenueAuditing;
using FFXIVVenues.Veni.VenueAuditing.ComponentHandlers;
using FFXIVVenues.Veni.VenueControl;
using FFXIVVenues.Veni.VenueControl.ComponentHandlers;
using NChronicle.Core.Model;
using NChronicle.Console.Extensions;
using NChronicle.Core.Interfaces;
using FFXIVVenues.Veni.AI;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Authorisation.Configuration;

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
apiHttpClient.Timeout = TimeSpan.FromSeconds(3);

IRepository repository = null;
if (persistenceConfig.Provider == PersistanceProvider.LiteDb)
    repository = new LiteDbRepository(persistenceConfig.ConnectionString);
else if (persistenceConfig.Provider == PersistanceProvider.Cosmos)
    repository = new CosmosDbRepository(persistenceConfig.ConnectionString);
else
    repository = new InMemoryRepository();

var discordChronicleLibrary = new DiscordChronicleLibrary();
NChronicle.Core.NChronicle.Configure(c => {
    c.WithConsoleLibrary().Configure(c => 
        c.ListeningToAllLevels()
    );
    c.WithLibrary(discordChronicleLibrary);
});
var chronicle = new Chronicle();

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
commandBroker.Add<Close.CommandFactory, Close.CommandHandler>(Close.COMMAND_NAME);
commandBroker.Add<Create.CommandFactory, Create.CommandHandler>(Create.COMMAND_NAME);
commandBroker.Add<Delete.CommandFactory, Delete.CommandHandler>(Delete.COMMAND_NAME);
commandBroker.Add<Edit.CommandFactory, Edit.CommandHandler>(Edit.COMMAND_NAME);
commandBroker.Add<Escalate.CommandFactory, Escalate.CommandHandler>(Escalate.COMMAND_NAME);
commandBroker.Add<Find.CommandFactory, Find.CommandHandler>(Find.COMMAND_NAME);
commandBroker.Add<Help.CommandFactory, Help.CommandHandler>(Help.COMMAND_NAME);
commandBroker.Add<Open.CommandFactory, Open.CommandHandler>(Open.COMMAND_NAME);
commandBroker.Add<ShowOpen.CommandFactory, ShowOpen.CommandHandler>(ShowOpen.COMMAND_NAME);
commandBroker.Add<ShowFor.CommandFactory, ShowFor.CommandHandler>(ShowFor.COMMAND_NAME);
commandBroker.Add<ShowMine.CommandFactory, ShowMine.CommandHandler>(ShowMine.COMMAND_NAME);
commandBroker.Add<Inspect.CommandFactory, Inspect.CommandHandler>(Inspect.COMMAND_NAME);
commandBroker.Add<SetRoleMap.CommandFactory, SetRoleMap.CommandHandler>(SetRoleMap.COMMAND_NAME);
commandBroker.Add<SetWelcomeJoiners.CommandFactory, SetWelcomeJoiners.CommandHandler>(SetWelcomeJoiners.COMMAND_NAME);
commandBroker.Add<SetFormatNames.CommandFactory, SetFormatNames.CommandHandler>(SetFormatNames.COMMAND_NAME);
commandBroker.Add<ShowCount.CommandFactory, ShowCount.CommandHandler>(ShowCount.COMMAND_NAME);
commandBroker.Add<Graph.CommandFactory, Graph.CommandHandler>(Graph.COMMAND_NAME);
commandBroker.Add<GetUnapproved.CommandFactory, GetUnapproved.CommandHandler>(GetUnapproved.COMMAND_NAME);

var componentBroker = serviceProvider.GetService<IComponentBroker>();
componentBroker.Add<ConfirmCorrectHandler>(ConfirmCorrectHandler.Key);
componentBroker.Add<EditVenueHandler>(EditVenueHandler.Key);
componentBroker.Add<TemporarilyClosedHandler>(TemporarilyClosedHandler.Key);
componentBroker.Add<PermanentlyClosedHandler>(PermanentlyClosedHandler.Key);

componentBroker.Add<AuditHandler>(AuditHandler.Key);
componentBroker.Add<GetAuditsHandler>(GetAuditsHandler.Key);
componentBroker.Add<GetAuditHandler>(GetAuditHandler.Key);
componentBroker.Add<CloseHandler>(CloseHandler.Key);
componentBroker.Add<DeleteHandler>(DeleteHandler.Key);
componentBroker.Add<DismissHandler>(DismissHandler.Key);
componentBroker.Add<EditHandler>(EditHandler.Key);
componentBroker.Add<EditPhotoHandler>(EditPhotoHandler.Key);
componentBroker.Add<EditManagersHandler>(EditManagersHandler.Key);
componentBroker.Add<OpenHandler>(OpenHandler.Key);

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
