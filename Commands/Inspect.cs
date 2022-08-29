﻿using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Api;
using FFXIVVenues.Veni.Commands.Brokerage;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Logging;
using FFXIVVenues.Veni.Utils;
using NChronicle.Core.Model;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Commands
{
    public static class Inspect
    {

        public const string COMMAND_NAME = "inspect";
        public const string OPTION_VERBOSITY = "verbosity";

        internal class CommandFactory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null)
            {
                var verbosityLevel = new SlashCommandOptionBuilder()
                   .WithName(OPTION_VERBOSITY)
                   .WithType(ApplicationCommandOptionType.Number)
                   .WithDescription("The maximum verbosity level of logging output.")
                   .AddChoice("Debug", (int)ChronicleLevel.Debug)
                   .AddChoice("Info", (int)ChronicleLevel.Info)
                   .AddChoice("Warning", (int)ChronicleLevel.Warning)
                   .AddChoice("Success", (int)ChronicleLevel.Success)
                   .AddChoice("Critical", (int)ChronicleLevel.Critical);

                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Monitor Veni's vitals.")
                    .AddOption(verbosityLevel)
                    .Build();
            }

        }

        internal class CommandHandler : ICommandHandler
        {
            private readonly IIndexersService indexersService;
            private readonly IDiscordChronicleLibrary chronicleLibrary;

            public CommandHandler(IIndexersService indexersService, IDiscordChronicleLibrary chronicleLibrary)
            {
                this.indexersService = indexersService;
                this.chronicleLibrary = chronicleLibrary;
            }

            public Task HandleAsync(SlashCommandInteractionContext slashCommand)
            {
                if (!this.indexersService.IsIndexer(slashCommand.Interaction.User.Id))
                    return slashCommand.Interaction.RespondAsync("Sorry, I only let indexers do that with me.", ephemeral: true);


                var subscribed = this.chronicleLibrary.IsSubscribed(slashCommand.Interaction.Channel);
                if (subscribed)
                {
                    this.chronicleLibrary.Unsubscribe(slashCommand.Interaction.Channel);
                    return slashCommand.Interaction.RespondAsync("Oki, I've **stopped inspection**. I hope everything looks good!");
                }
                else
                {
                    var verbosity = slashCommand.GetInt(OPTION_VERBOSITY);
                    this.chronicleLibrary.Subscribe(slashCommand.Interaction.Channel, (ChronicleLevel) (verbosity ?? 3));
                    return slashCommand.Interaction.RespondAsync("Oki, I've **started inspection**. 👀");
                }
            }

        }

    }
}