using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Utils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


namespace FFXIVVenues.Veni.Infrastructure.Commands
{

    internal class CommandBroker : ICommandBroker
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly List<SlashCommandProperties> _commands;
        private readonly TypeMap<ICommandHandler> _handlers;
        private readonly ICommandCartographer _cartographer;
        private readonly IServiceProvider _serviceProvider;

        public CommandBroker(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            this._commands = new();
            this._handlers = new(serviceProvider);
            this._cartographer = serviceProvider.GetService<ICommandCartographer>();;
            this._discordClient = serviceProvider.GetService<DiscordSocketClient>();
        }

        public void AddFromAssembly()
        {
            Log.Debug($"Adding slash command from Assembly.");
            var (commands, handlers) = this._cartographer.Discover();
            foreach (var handler in handlers)
                this._handlers.Add(handler.Key, handler.Value);
            this._commands.AddRange(commands.Select(c => c.Build()));
        }
        
        public void Add<TFactory, THandler>(string key)
            where TFactory : ICommandFactory
            where THandler : ICommandHandler
        {
            Log.Debug("Adding slash command {Command}", key);
            var factory = ActivatorUtilities.CreateInstance<TFactory>(_serviceProvider);
            this._commands.Add(factory.GetSlashCommand());
            this._handlers.Add<THandler>(key);
        }

        public Task RegisterAllGloballyAsync() =>
            this._discordClient.BulkOverwriteGlobalApplicationCommandsAsync(this._commands.ToArray());

        public async Task HandleAsync(SlashCommandVeniInteractionContext context)
        {
            var handler = this._handlers.Activate(context.Interaction.CommandName);
            if (handler == null)
            {
                var commandPath = this.GetCommandName(context);
                handler = this._handlers.Activate(commandPath);
            }

            if (handler != null)
                await handler.HandleAsync(context);
                
            context.TypingHandle?.Dispose();
        }

        private string GetCommandName(SlashCommandVeniInteractionContext context)
        {
            var commandPath = context.Interaction.CommandName;
            var command = this._commands.FirstOrDefault(c => c.Name.Value == commandPath);
            if (command == null) return commandPath;

            var subCommandInteraction = context.Interaction.Data.Options.FirstOrDefault();
            if (subCommandInteraction is null) return commandPath;
            var subCommand = command.Options.Value?.FirstOrDefault(o => o.Name == subCommandInteraction.Name);
            if (subCommand is null) return commandPath;

            do
            {
                if (subCommand.Type is ApplicationCommandOptionType.SubCommand 
                                    or ApplicationCommandOptionType.SubCommandGroup)
                    commandPath += " " + subCommandInteraction.Name;

                if (subCommand.Type is not ApplicationCommandOptionType.SubCommandGroup)
                    return commandPath;
                
                subCommandInteraction = subCommandInteraction.Options.FirstOrDefault();
                if (subCommandInteraction is null) return commandPath;
                subCommand = command.Options.Value?.FirstOrDefault(o => o.Name == subCommandInteraction.Name);
                if (subCommand is null) return commandPath;
            } while (true);
        }
    }

}
