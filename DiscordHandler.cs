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
using NChronicle.Core.Interfaces;
using FFXIVVenues.Veni.States.Abstractions;
using FFXIVVenues.Veni.Managers;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni
{
    internal class DiscordHandler : IDiscordHandler
    {

        private readonly DiscordSocketClient _client;
        private readonly ICommandBroker _commandBroker;
        private readonly Pipeline<MessageInteractionContext> _pipeline;
        private readonly ISessionContextProvider _sessionContextProvider;
        private readonly IStaffService _staffService;
        private readonly IChronicle _chronicle;
        private readonly IGuildManager _guildManager;

        public DiscordHandler(DiscordSocketClient client,
                              ICommandBroker commandBroker,
                              IServiceProvider serviceProvider,
                              ISessionContextProvider sessionContextProvider,
                              IStaffService staffService, 
                              IChronicle chronicle,
                              IGuildManager guildManager)
        {
            this._client = client;
            this._commandBroker = commandBroker;
            this._sessionContextProvider = sessionContextProvider;
            this._staffService = staffService;
            this._chronicle = chronicle;
            this._guildManager = guildManager;
            this._client.Connected += Connected;
            this._client.SlashCommandExecuted += SlashCommandExecutedAsync;
            this._client.MessageReceived += MessageReceivedAsync;
            this._client.SelectMenuExecuted += ComponentExecutedAsync;
            this._client.ButtonExecuted += ComponentExecutedAsync;
            this._client.UserJoined += UserJoinedAsync;
            
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
            var context = new SlashCommandInteractionContext(slashCommand, _client, sessionContext, this._chronicle);

            var stateText = "";
            IState currentState = null;
            context.Session.StateStack?.TryPeek(out currentState);
            if (currentState != null)
                stateText = " [" + currentState.GetType().Name + "]";
            this._chronicle.Info($"**{slashCommand.User.Mention}{stateText}**: [Command: /{slashCommand.CommandName}]");
            
            await this._commandBroker.HandleAsync(context);
        }

        private async Task ComponentExecutedAsync(SocketMessageComponent message)
        {
            await message.DeferAsync();
            if (await this._staffService.HandleComponentInteractionAsync(message))
                return;

            var conversationContext = _sessionContextProvider.GetContext(message.User.Id.ToString());
            var context = new MessageComponentInteractionContext(message, _client, conversationContext, this._chronicle);

            var stateText = "";
            IState currentState = null;
            context.Session.StateStack?.TryPeek(out currentState);
            if (currentState != null)
                stateText = " [" + currentState.GetType().Name + "]";
            this._chronicle.Info($"**{message.User.Mention}{stateText}**: [Component Interaction]");

            await conversationContext.HandleComponentInteraction(context);
        }

        private Task UserJoinedAsync(SocketGuildUser user)
        {
            Task.Delay(30_000).ContinueWith(async _ =>
            {
                var welcomeTask = this._guildManager.WelcomeGuildUserAsync(user);
                var formatTask = this._guildManager.FormatDisplayNameForGuildUserAsync(user);
                var roleTask = Task.CompletedTask;
                if (await this._guildManager.SyncRolesForGuildUserAsync(user))
                    roleTask = user.Guild.SystemChannel.SendMessageAsync(MessageRepository.RolesAssigned.PickRandom().Replace("{mention}", user.Mention));
                await roleTask;
                await formatTask;
                await welcomeTask;
            });
            return Task.CompletedTask;
        }

        private Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
                return Task.CompletedTask;

            var conversationContext = _sessionContextProvider.GetContext(message.Author.Id.ToString());
            var context = new MessageInteractionContext(message, _client, conversationContext, this._chronicle);
            return _pipeline.RunAsync(context);
        }

    }
}
