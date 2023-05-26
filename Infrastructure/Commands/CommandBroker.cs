using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Utils;
using Microsoft.Extensions.DependencyInjection;
using NChronicle.Core.Interfaces;

namespace FFXIVVenues.Veni.Infrastructure.Commands
{

    internal class CommandBroker : ICommandBroker
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly List<SlashCommandProperties> _commands;
        private readonly TypeMap<ICommandHandler> _handlers;
        private readonly ICommandCartographer _cartographer;
        private readonly IServiceProvider _serviceProvider;
        private readonly IChronicle _chronicle;

        public CommandBroker(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
            this._commands = new();
            this._handlers = new(serviceProvider);
            this._cartographer = serviceProvider.GetService<ICommandCartographer>();;
            this._discordClient = serviceProvider.GetService<DiscordSocketClient>();
            this._chronicle = serviceProvider.GetService<IChronicle>();
        }

        public void AddFromAssembly()
        {
            this._chronicle.Debug($"Adding slash command from Assembly.");
            var (commands, handlers) = this._cartographer.Discover();
            foreach (var handler in handlers)
                this._handlers.Add(handler.Key, handler.Value);
            this._commands.AddRange(commands.Select(c => c.Build()));
        }
        
        public void Add<TFactory, THandler>(string key)
            where TFactory : ICommandFactory
            where THandler : ICommandHandler
        {
            this._chronicle.Debug($"Adding slash command {key}");
            var factory = ActivatorUtilities.CreateInstance<TFactory>(_serviceProvider);
            this._commands.Add(factory.GetSlashCommand());
            this._handlers.Add<THandler>(key);
        }

        public Task RegisterAllGloballyAsync() =>
            this._discordClient.BulkOverwriteGlobalApplicationCommandsAsync(this._commands.ToArray());

        public async Task HandleAsync(SlashCommandVeniInteractionContext context)
        {
            var commandPath = this.GetCommandName(context);
            await this._handlers.Activate(commandPath).HandleAsync(context);
            context.TypingHandle?.Dispose();
        }

        private string GetCommandName(SlashCommandVeniInteractionContext context)
        {
            var commandPath = context.Interaction.CommandName;
            var command = this._commands.FirstOrDefault(c => c.Name.Value == commandPath);
            if (command == null) return commandPath;
            for (var i = 0; true; i++)
            {
                var subCommandName = context.Interaction.Data.Options.Skip(i).FirstOrDefault()?.Name;
                if (subCommandName == null) break;
                var subCommand = command.Options.Value?.FirstOrDefault(o => o.Name == subCommandName);
                if (subCommand == null) break;
                if (subCommand.Type == ApplicationCommandOptionType.SubCommandGroup)
                {
                    commandPath += " " + subCommandName;
                    continue;
                }

                if (subCommand.Type == ApplicationCommandOptionType.SubCommand)
                    commandPath += " " + subCommandName;
                break;
            }

            return commandPath;
        }
    }

}
