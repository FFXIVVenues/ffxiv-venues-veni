﻿using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Kana.Pipelines;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Components;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;
using FFXIVVenues.Veni.Infrastructure.Middleware;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Infrastructure.Persistence.Abstraction;
using FFXIVVenues.Veni.Authorisation.Blacklist;
using FFXIVVenues.Veni.GuildEngagement;
using FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueApproval;
using Serilog;

namespace FFXIVVenues.Veni
{
    internal class DiscordHandler : IDiscordHandler
    {

        private readonly DiscordSocketClient _client;
        private readonly ICommandBroker _commandBroker;
        private readonly IInteractionContextFactory _contextFactory;
        private readonly IComponentBroker _componentBroker;
        private readonly Pipeline<MessageVeniInteractionContext> _messagePipeline;
        private readonly IVenueApprovalService _venueApprovalService;
        private readonly IGuildManager _guildManager;
        private readonly IRepository _db;

        public DiscordHandler(DiscordSocketClient client,
                              ICommandBroker commandBroker,
                              IInteractionContextFactory contextFactory,
                              IComponentBroker componentBroker,
                              IServiceProvider serviceProvider,
                              IVenueApprovalService venueApprovalService, 
                              IGuildManager guildManager,
                              IRepository db)
        {
            this._client = client;
            this._commandBroker = commandBroker;
            this._contextFactory = contextFactory;
            this._componentBroker = componentBroker;
            this._venueApprovalService = venueApprovalService;
            this._guildManager = guildManager;
            this._client.Connected += Connected;
            this._client.SlashCommandExecuted += SlashCommandExecutedAsync;
            this._client.MessageReceived += MessageReceivedAsync;
            this._client.SelectMenuExecuted += ComponentExecutedAsync;
            this._client.ButtonExecuted += ComponentExecutedAsync;
            this._client.UserJoined += UserJoinedAsync;
            this._client.GuildAvailable += GuildAvailableAsync;
            this._db = db;

            this._messagePipeline = new Pipeline<MessageVeniInteractionContext>()
                .WithServiceProvider(serviceProvider)
                .Add<LogOutboundMiddleware>()
                .Add<FilterSelfMiddleware>()
                .Add<ConversationFilterMiddleware>()
                .Add<LogInboundMiddleware>()
                .Add<BlacklistMiddleware>()
                .Add<StartTypingMiddleware>()
                .Add<CluPredictionMiddleware>()
                .Add<StopCallingMeMommyMiddleware>()
                .Add<InteruptIntentMiddleware>()
                .Add<StateMiddleware>()
                .Add<IntentMiddleware>()
                .Add<ChatMiddleware>()
                .Add<DontUnderstandMiddleware>();
        }

        private async Task GuildAvailableAsync(SocketGuild guild)
        {
            Log.Information("Connected to guild {GuildId} {GuildName}.", guild.Id, guild.Name);
            if (!await _db.ExistsAsync<BlacklistEntry>(guild.Id.ToString()))
                return;
            
            var ownerId = guild.OwnerId;
            await guild.LeaveAsync();
            
            var owner = await _client.GetUserAsync(ownerId);
            if (owner == null)
                return;
            
            var dm = await owner.CreateDMChannelAsync();
            await dm.SendMessageAsync(
                $"Sorry, I left **{guild.Name}**. My family said I'm not allowed to go there. 😢" +
                $" If you think this was a mistake please let my family know.");
        }

        public Task ListenAsync() =>
            _client.StartAsync();

        private Task Connected() =>
            this._commandBroker.RegisterAllGloballyAsync();

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            var context = this._contextFactory.Create(message);
            try
            {
                await _messagePipeline.RunAsync(context);
            }
            catch (Exception e)
            {
                Log.Error(e, "An unhandled exception was thrown in handling a received message");
            }
            finally
            {
                context.TypingHandle?.Dispose();
            }
        }

        private async Task SlashCommandExecutedAsync(SocketSlashCommand message)
        {
            try
            {
                if (await _db.ExistsAsync<BlacklistEntry>(message.User.Id.ToString()))
                {
                    await message.RespondAsync($"Sorry, my family said I'm not allowed to speak to you. 😢" +
                                               $" If you think this was a mistake please let my family know.",
                        ephemeral: true);
                    return;
                }

                var context = this._contextFactory.Create(message);
                LogSlashCommandExecuted(message, context);
                await this._commandBroker.HandleAsync(context);
            }
            catch (Exception e)
            {
                Log.Error(e, "An unhandled exception was thrown in handling a /{Command} command", message.CommandName);
            }
        }

        private async Task ComponentExecutedAsync(SocketMessageComponent message)
        {
            await message.DeferAsync();
            var typingHandle = message.Channel.EnterTypingState();

            try
            {
                if (await _db.ExistsAsync<BlacklistEntry>(message.User.Id.ToString()))
                {
                    await message.FollowupAsync($"Sorry, my family said I'm not allowed to speak to you. 😢" +
                                                $" If you think this was a mistake please let my family know.",
                        ephemeral: true);
                    typingHandle?.Dispose();
                    return;
                }

                if (await this._venueApprovalService.HandleComponentInteractionAsync(message))
                {
                    typingHandle?.Dispose();
                    return;
                }

                var context = this._contextFactory.Create(message);
                context.TypingHandle = typingHandle;
                LogComponentExecuted(message, context);
                await context.Session.HandleComponentInteraction(context);
                await this._componentBroker.HandleAsync(context);
            }
            catch (Exception e)
            {
                Log.Error(e, "An unhadnled exception was thrown in handling a execution of component {ComponentId}", message.Data.CustomId);
            }
            finally
            {
                typingHandle?.Dispose();
            }
            
        }

        private void LogSlashCommandExecuted(SocketSlashCommand slashCommand, SlashCommandVeniInteractionContext context)
        {
            ISessionState currentSessionState = null;
            context.Session.StateStack?.TryPeek(out currentSessionState);
            if (currentSessionState is not null) Log.Information("[{State}] {Username} used command /{Command}", currentSessionState.GetType().Name, slashCommand.User.Username, slashCommand.CommandName);
            else Log.Information("{Username} used command /{Command}", slashCommand.User.Username, slashCommand.CommandName);
        }

        private void LogComponentExecuted(SocketMessageComponent message, ComponentVeniInteractionContext context)
        {
            ISessionState currentSessionState = null;
            context.Session.StateStack?.TryPeek(out currentSessionState);
            if (currentSessionState is not null) Log.Information("[{State}] {Username} interacted with a component.", currentSessionState.GetType().Name, message.User.Username);
            else Log.Information("{Username} interacted with a component", message.User.Username);
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

    }
}
