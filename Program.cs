using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using FFXIVVenues.Veni;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Luis;
using FFXIVVenues.Veni.Intents;

var config = new ConfigurationBuilder()
                    .AddJsonFile("config.json", optional: true)
                    .AddUserSecrets<DiscordHandler>(optional: true)
                    .Build();
var luisConfig = new LuisConfiguration();
config.GetSection("Luis").Bind(luisConfig);
var apiConfig = new ApiConfiguration();
config.GetSection("Api").Bind(apiConfig);
var apiHttpClient = new HttpClient { BaseAddress = new Uri(apiConfig.BaseUrl) };
apiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiConfig.AuthorizationKey);

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<IConfiguration>(config);
serviceCollection.AddSingleton<LuisConfiguration>(luisConfig);
serviceCollection.AddSingleton<ApiConfiguration>(apiConfig);
serviceCollection.AddSingleton<HttpClient>(apiHttpClient);
serviceCollection.AddSingleton<ILogger, Logger>();
serviceCollection.AddSingleton<IApiService, ApiService>();
serviceCollection.AddSingleton<IIntentHandlerProvider, IntentHandlerProvider>();
serviceCollection.AddSingleton<IConversationContextProvider, ConversationContextProvider>();
serviceCollection.AddSingleton<IDiscordHandler, DiscordHandler>();
serviceCollection.AddSingleton<ILuisClient, LuisClient>();
var serviceProvider = serviceCollection.BuildServiceProvider();

await serviceProvider.GetService<IDiscordHandler>().ListenAsync();

await Task.Delay(Timeout.Infinite);
 