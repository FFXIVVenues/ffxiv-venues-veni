using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Kana.Pipelines;
using Microsoft.Extensions.Configuration;
using FFXIVVenues.Veni.Middleware;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Context;

namespace FFXIVVenues.Veni
{
    internal class DiscordHandler : IDiscordHandler
    {

        private const string DISCORD_BOT_CONFIG_KEY = "DiscordBotToken";

        private readonly DiscordSocketClient _client;
        private readonly Pipeline<MessageContext> _pipeline;
        private readonly IConversationContextProvider _conversationContextProvider;
        private readonly ILogger _logger;

        public DiscordHandler(IConfiguration config, IServiceProvider serviceProvider, IConversationContextProvider conversationContextProvider, ILogger logger)
        {
            _logger = logger;
            _conversationContextProvider = conversationContextProvider;
            var discordKey = config.GetValue<string>(DISCORD_BOT_CONFIG_KEY);
            if (discordKey == null)
                throw new Exception("Discord Bot Token not set!");

            _client = GetClient(discordKey);
            _client.MessageReceived += MessageReceivedAsync;

            _pipeline = new Pipeline<MessageContext>()
                .WithServiceProvider(serviceProvider)
                .Add<ConversationFilterMiddleware>()
                .Add<StartTypingMiddleware>()
                .Add<LogMiddleware>()
                .Add<LuisPredictionMiddleware>()
                .Add<StopCallingMeMommyMiddleware>()
                .Add<InteruptIntentMiddleware>()
                .Add<StateMiddleware>()
                .Add<IntentMiddleware>();
        }

        public Task ListenAsync() =>
            _client.StartAsync();

        private Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
                return Task.CompletedTask;

            var conversationContext = _conversationContextProvider.GetContext(message.Channel.Id.ToString());
            var context = new MessageContext(message, _client, conversationContext, _logger);
            return _pipeline.RunAsync(context);
        }

        private DiscordSocketClient GetClient(string discordBotToken)
        {
            var client = new DiscordSocketClient(new()
            {
                LogLevel = LogSeverity.Verbose
            });
            client.Log += LogAsync;
            client.LoginAsync(TokenType.Bot, discordBotToken);
            return client;
        }

        private Task LogAsync(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

    }
}
