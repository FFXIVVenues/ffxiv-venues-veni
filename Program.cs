﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Luis;
using FFXIVVenues.Veni.Intents;
using Discord.WebSocket;
using Discord;
using FFXIVVenues.Veni.Commands.Brokerage;
using FFXIVVenues.Veni.Commands;
using FFXIVVenues.Veni.Context.Abstractions;
using NChronicle.Core.Model;
using NChronicle.Console.Extensions;
using NChronicle.Core.Interfaces;
using FFXIVVenues.Veni.Logging;
using FFXIVVenues.Veni.Persistance;
using FFXIVVenues.Veni.Persistance.Abstraction;
using FFXIVVenues.Veni.Managers;

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

var apiHttpClient = new HttpClient { BaseAddress = new Uri(apiConfig.BaseUrl) };
apiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiConfig.AuthorizationKey);

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
serviceCollection.AddSingleton<LuisConfiguration>(luisConfig);
serviceCollection.AddSingleton<ApiConfiguration>(apiConfig);
serviceCollection.AddSingleton<PersistenceConfiguration>(persistenceConfig);
serviceCollection.AddSingleton<UiConfiguration>(uiConfig);
serviceCollection.AddSingleton<HttpClient>(apiHttpClient);
serviceCollection.AddSingleton<IRepository>(repository);
serviceCollection.AddSingleton<ICommandBroker, CommandBroker>();
serviceCollection.AddSingleton<IApiService, ApiService>();
serviceCollection.AddSingleton<IGuildManager, GuildManager>();
serviceCollection.AddSingleton<IStaffService, StaffService>();
serviceCollection.AddSingleton<IIntentHandlerProvider, IntentHandlerProvider>();
serviceCollection.AddSingleton<ISessionContextProvider, SessionContextProvider>();
serviceCollection.AddSingleton<IDiscordHandler, DiscordHandler>();
serviceCollection.AddSingleton<ILuisClient, LuisClient>();
serviceCollection.AddSingleton(GetDiscordSocketClient(config, chronicle));

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
