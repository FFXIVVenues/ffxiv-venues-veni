using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Utils;
using NChronicle.Core.Model;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Logging;
using FFXIVVenues.Veni.People;

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
            private readonly IStaffService indexersService;
            private readonly IDiscordChronicleLibrary chronicleLibrary;

            public CommandHandler(IStaffService indexersService, IDiscordChronicleLibrary chronicleLibrary)
            {
                this.indexersService = indexersService;
                this.chronicleLibrary = chronicleLibrary;
            }

            public Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
            {
                if (!this.indexersService.IsEngineer(slashCommand.Interaction.User.Id))
                    return slashCommand.Interaction.Channel.SendMessageAsync("Sorry, I only let Engineers do that with me.");


                var subscribed = this.chronicleLibrary.IsSubscribed(slashCommand.Interaction.Channel);
                if (subscribed)
                {
                    this.chronicleLibrary.Unsubscribe(slashCommand.Interaction.Channel);
                    return slashCommand.Interaction.Channel.SendMessageAsync("Oki, I've **stopped inspection**. I hope everything looks good!");
                }
                else
                {
                    var verbosity = slashCommand.GetInt(OPTION_VERBOSITY);
                    this.chronicleLibrary.Subscribe(slashCommand.Interaction.Channel, (ChronicleLevel) (verbosity ?? 3));
                    return slashCommand.Interaction.Channel.SendMessageAsync("Oki, I've **started inspection**. 👀");
                }
            }

        }

    }
}
