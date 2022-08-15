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
var uiConfig = new UiConfiguration();
config.GetSection("Ui").Bind(uiConfig);

var apiHttpClient = new HttpClient { BaseAddress = new Uri(apiConfig.BaseUrl) };
apiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiConfig.AuthorizationKey);

var serviceCollection = new ServiceCollection();

serviceCollection.AddSingleton<IConfiguration>(config);
serviceCollection.AddSingleton<LuisConfiguration>(luisConfig);
serviceCollection.AddSingleton<ApiConfiguration>(apiConfig);
serviceCollection.AddSingleton<UiConfiguration>(uiConfig);

serviceCollection.AddSingleton<HttpClient>(apiHttpClient);

serviceCollection.AddSingleton<ICommandBroker, CommandBroker>();
serviceCollection.AddSingleton<IApiService, ApiService>();
serviceCollection.AddSingleton<IIndexersService, IndexersService>();
serviceCollection.AddSingleton<IIntentHandlerProvider, IntentHandlerProvider>();
serviceCollection.AddSingleton<ISessionContextProvider, SessionContextProvider>();
serviceCollection.AddSingleton<IDiscordHandler, DiscordHandler>();
serviceCollection.AddSingleton<ILuisClient, LuisClient>();

serviceCollection.AddSingleton<DiscordSocketClient>(_ =>
{
    var discordKey = config.GetValue<string>(DISCORD_BOT_CONFIG_KEY);
    if (discordKey == null)
        throw new Exception("Discord Bot Token not set!");

    var client = new DiscordSocketClient(new()
    {
        LogLevel = LogSeverity.Verbose
    });
    client.Log += (LogMessage msg) =>
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    };
    client.LoginAsync(TokenType.Bot, discordKey);

    return client;
});

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

await serviceProvider.GetService<IDiscordHandler>().ListenAsync();

await Task.Delay(Timeout.Infinite);
