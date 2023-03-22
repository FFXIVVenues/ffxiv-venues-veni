using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Kana.Pipelines;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.Abstractions;
using FFXIVVenues.Veni.Infrastructure.Context.Session;
using FFXIVVenues.Veni.Infrastructure.Middleware;
using FFXIVVenues.Veni.People;
using NChronicle.Core.Interfaces;
using FFXIVVenues.Veni.Utils;

namespace FFXIVVenues.Veni
{
    internal class DiscordHandler : IDiscordHandler
    {

        private readonly DiscordSocketClient _client;
        private readonly ICommandBroker _commandBroker;
        private readonly IComponentBroker _componentBroker;
        private readonly Pipeline<MessageVeniInteractionContext> _pipeline;
        private readonly ISessionContextProvider _sessionContextProvider;
        private readonly IStaffService _staffService;
        private readonly IChronicle _chronicle;
        private readonly IGuildManager _guildManager;

        public DiscordHandler(DiscordSocketClient client,
                              ICommandBroker commandBroker,
                              IComponentBroker componentBroker,
                              IServiceProvider serviceProvider,
                              ISessionContextProvider sessionContextProvider,
                              IStaffService staffService, 
                              IChronicle chronicle,
                              IGuildManager guildManager)
        {
            this._client = client;
            this._commandBroker = commandBroker;
            this._componentBroker = componentBroker;
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
            
            this._pipeline = new Pipeline<MessageVeniInteractionContext>()
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
            var context = new SlashCommandVeniInteractionContext(slashCommand, _client, sessionContext, this._chronicle);

            LogSlashCommandExecuted(slashCommand, context);

            await this._commandBroker.HandleAsync(context);
        }

        private async Task ComponentExecutedAsync(SocketMessageComponent message)
        {
            await message.DeferAsync();
            if (await this._staffService.HandleComponentInteractionAsync(message))
                return;

            var conversationContext = _sessionContextProvider.GetContext(message.User.Id.ToString());
            var context = new MessageComponentVeniInteractionContext(message, _client, conversationContext, this._chronicle);
            
            LogComponentExecuted(message, context);

            await conversationContext.HandleComponentInteraction(context);
            
            await this._componentBroker.HandleAsync(message);
        }

        private void LogSlashCommandExecuted(SocketSlashCommand slashCommand, SlashCommandVeniInteractionContext context)
        {
            var stateText = "";
            ISessionState currentSessionState = null;
            context.Session.StateStack?.TryPeek(out currentSessionState);
            if (currentSessionState != null)
                stateText = " [" + currentSessionState.GetType().Name + "]";
            this._chronicle.Info($"**{slashCommand.User.Mention}{stateText}**: [Command: /{slashCommand.CommandName}]");
        }

        private void LogComponentExecuted(SocketMessageComponent message, MessageComponentVeniInteractionContext context)
        {
            var stateText = "";
            ISessionState currentSessionState = null;
            context.Session.StateStack?.TryPeek(out currentSessionState);
            if (currentSessionState != null)
                stateText = " [" + currentSessionState.GetType().Name + "]";
            this._chronicle.Info($"**{message.User.Mention}{stateText}**: [Component Interaction]");
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
            var context = new MessageVeniInteractionContext(message, _client, conversationContext, this._chronicle);
            return _pipeline.RunAsync(context);
        }

    }
}
