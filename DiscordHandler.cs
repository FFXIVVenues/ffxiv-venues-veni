using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Kana.Pipelines;
using FFXIVVenues.Veni.Middleware;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Api;

namespace FFXIVVenues.Veni
{
    internal class DiscordHandler : IDiscordHandler
    {

        private readonly DiscordSocketClient _client;
        private readonly Pipeline<MessageContext> _pipeline;
        private readonly IConversationContextProvider _conversationContextProvider;
        private readonly IIndexersService _indexersService;
        private readonly ILogger _logger;

        public DiscordHandler(DiscordSocketClient client,
                              IServiceProvider serviceProvider,
                              IConversationContextProvider conversationContextProvider,
                              IIndexersService indexersService,
                              ILogger logger)
        {
            this._logger = logger;
            this._client = client;
            this._conversationContextProvider = conversationContextProvider;
            this._indexersService = indexersService;
            this._client.MessageReceived += MessageReceivedAsync;
            this._client.SelectMenuExecuted += ComponentExecutedAsync;
            this._client.ButtonExecuted += ComponentExecutedAsync;

            this._pipeline = new Pipeline<MessageContext>()
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

        private async Task ComponentExecutedAsync(SocketMessageComponent message)
        {
            await message.DeferAsync();
            if (await this._indexersService.HandleComponentInteractionAsync(message))
                return;

            var conversationContext = _conversationContextProvider.GetContext(message.User.Id.ToString());
            var context = new MessageContext(message, _client, conversationContext, _logger);
            await conversationContext.RunComponentHandlerAsync(context);
        }

        private Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
                return Task.CompletedTask;

            var conversationContext = _conversationContextProvider.GetContext(message.Author.Id.ToString());
            var context = new MessageContext(message, _client, conversationContext, _logger);
            return _pipeline.RunAsync(context);
        }
    }
}
