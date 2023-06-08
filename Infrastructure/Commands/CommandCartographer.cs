using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Commands.Attributes;

namespace FFXIVVenues.Veni.Infrastructure.Commands;

public class CommandCartographer : ICommandCartographer
{
    public (SlashCommandBuilder[], Dictionary<string, Type>) Discover()
    {
        var commands = new List<SlashCommandBuilder>();
        var handlers = new Dictionary<string, Type>();
        
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
            return default;
        
        var types = 
            from type in entryAssembly.GetTypes()
            where Attribute.IsDefined(type, typeof(DiscordCommandAttribute))
            select type;

        foreach (var @type in types)
        {
            var commandAttributes = type.GetCustomAttributes<DiscordCommandAttribute>()!;
            var argAttributes = @type.GetCustomAttributes<DiscordCommandArgumentAttribute>().ToArray();
            
            foreach (var commandAttribute in commandAttributes)
            {
                
                var commandPath = commandAttribute.Command.Split(' ');
                handlers.Add(string.Join(' ', commandPath), @type);
                var command = GetOrCreateCommand(commands, commandPath[0]);
                if (commandPath.Length == 1)
                {
                    command.WithDescription(commandAttribute.Description);
                    foreach (var arg in argAttributes)
                        command.AddOption(arg.Name, arg.Type, arg.Description, arg.Required);
                }
                else
                {
                    var subCommand = GetOrCreateSubCommand(command, commandPath[1..]);
                    subCommand.WithDescription(commandAttribute.Description);
                    foreach (var arg in argAttributes)
                        subCommand.AddOption(arg.Name, arg.Type, arg.Description, arg.Required);
                }
            }
        }

        return (commands.ToArray(), handlers);
    }

    private static SlashCommandBuilder GetOrCreateCommand(in List<SlashCommandBuilder> commands, string commandName)
    {
        var command = commands.FirstOrDefault(c =>
            c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));
        if (command == null)
        {
            command = new SlashCommandBuilder().WithName(commandName).WithDescription("Meow");
            commands.Add(command);
        }

        return command;
    }
    
    private static SlashCommandOptionBuilder GetOrCreateSubCommand(in SlashCommandBuilder command, string[] subCommandPath)
    {
        SlashCommandOptionBuilder lastSubCommand = null;
        var options = command.Options ??= new();
        
        foreach (var subCommandKey in subCommandPath)
        {
            var subCommand = options.FirstOrDefault(c =>
                c.Name.Equals(subCommandKey, StringComparison.OrdinalIgnoreCase));
            if (subCommand == null)
            {
                lastSubCommand?.WithType(ApplicationCommandOptionType.SubCommandGroup);
                subCommand = new SlashCommandOptionBuilder().WithName(subCommandKey)
                    .WithDescription("Meow").WithType(ApplicationCommandOptionType.SubCommand);
                options.Add(subCommand);
            }

            options = subCommand.Options ??= new();
            lastSubCommand = subCommand;
        }

        return lastSubCommand;
    }
    
}