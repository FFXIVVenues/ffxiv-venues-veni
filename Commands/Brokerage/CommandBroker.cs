using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Commands.Brokerage
{

    internal class CommandBroker : ICommandBroker
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly TypeMap<ICommandFactory> _factories;
        private readonly TypeMap<ICommandHandler> _handlers;

        public CommandBroker(IServiceProvider serviceProvider)
        {
            this._discordClient = serviceProvider.GetService<DiscordSocketClient>();
            this._factories = new(serviceProvider);
            this._handlers = new(serviceProvider);
        }

        public void Add<Factory, Handler>(string key)
            where Factory : ICommandFactory
            where Handler : ICommandHandler
        {
            this._factories.Add<Factory>(key);
            this._handlers.Add<Handler>(key);
        }

        public Task RegisterAllGloballyAsync()
        {
            var commandProperties = new List<SlashCommandProperties>();
            foreach (var factory in this._factories)
            {
                var command = factory.GetSlashCommand();
                Console.WriteLine($"Configure\t\tGlobally registering application command {command.Name}");
                commandProperties.Add(command);
            }
            return this._discordClient.BulkOverwriteGlobalApplicationCommandsAsync(commandProperties.ToArray());
        }

        public Task RegisterAllAsync(SocketGuild guild)
        {
            var commandProperties = new List<SlashCommandProperties>();
            foreach (var factory in this._factories)
            {
                var command = factory.GetSlashCommand(guild);
                Console.WriteLine($"Configure\t\tRegistering application command {command.Name}");
                commandProperties.Add(command);
            }
            return guild.BulkOverwriteApplicationCommandAsync(commandProperties.ToArray());
        }

        public Task HandleAsync(SlashCommandInteractionContext context) =>
            this._handlers.Activate(context.Interaction.CommandName).HandleAsync(context);

    }

}
