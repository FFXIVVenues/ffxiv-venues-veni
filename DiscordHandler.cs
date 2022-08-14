using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Kana.Pipelines;
using FFXIVVenues.Veni.Middleware;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Commands.Brokerage;
using FFXIVVenues.Veni.Context.Abstractions;
using Discord;

namespace FFXIVVenues.Veni
{
    internal class DiscordHandler : IDiscordHandler
    {

        private readonly DiscordSocketClient _client;
        private readonly ICommandBroker _commandBroker;
        private readonly Pipeline<MessageInteractionContext> _pipeline;
        private readonly ISessionContextProvider _sessionContextProvider;
        private readonly IIndexersService _indexersService;

        public DiscordHandler(DiscordSocketClient client,
                              ICommandBroker commandBroker,
                              IServiceProvider serviceProvider,
                              ISessionContextProvider sessionContextProvider,
                              IIndexersService indexersService)
        {
            this._client = client;
            this._commandBroker = commandBroker;
            this._sessionContextProvider = sessionContextProvider;
            this._indexersService = indexersService;

            this._client.Connected += Connected;
            //this._client.GuildAvailable += GuildAvailableAsync;
            this._client.SlashCommandExecuted += SlashCommandExecutedAsync;
            this._client.MessageReceived += MessageReceivedAsync;
            this._client.SelectMenuExecuted += ComponentExecutedAsync;
            this._client.ButtonExecuted += ComponentExecutedAsync;

            this._pipeline = new Pipeline<MessageInteractionContext>()
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

        private Task Connected() =>
            this._commandBroker.RegisterAllGloballyAsync();

        private async Task SlashCommandExecutedAsync(SocketSlashCommand slashCommand)
        {
            var sessionContext = _sessionContextProvider.GetContext(slashCommand.User.Id.ToString());
            var context = new SlashCommandInteractionContext(slashCommand, _client, sessionContext);

            await this._commandBroker.HandleAsync(context);
        }

        private async Task ComponentExecutedAsync(SocketMessageComponent message)
        {
            await message.DeferAsync();
            if (await this._indexersService.HandleComponentInteractionAsync(message))
                return;

            var conversationContext = _sessionContextProvider.GetContext(message.User.Id.ToString());
            var context = new MessageComponentInteractionContext(message, _client, conversationContext);
            await conversationContext.HandleComponentInteraction(context);
        }

        private Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
                return Task.CompletedTask;

            var conversationContext = _sessionContextProvider.GetContext(message.Author.Id.ToString());
            var context = new MessageInteractionContext(message, _client, conversationContext);
            return _pipeline.RunAsync(context);
        }
    }
}
