using System.Linq;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Intents;
using FFXIVVenues.Veni.Utils;
using NChronicle.Core.Model;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Authorisation;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Logging;
using FFXIVVenues.Veni.People;

namespace FFXIVVenues.Veni.Commands;

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
                .AddOption(new SlashCommandOptionBuilder()
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .WithName("start")
                    .WithDescription("Start following Veni's logs in this channel.")
                    .AddOption(verbosityLevel))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .WithName("stop")
                    .WithDescription("Stop following Veni's logs in this channel."))
                .Build();
        }

    }

    internal class CommandHandler : ICommandHandler
    {
        private readonly IAuthorizer _authorizer;
        private readonly IDiscordChronicleLibrary _chronicle;

        public CommandHandler(IAuthorizer authorizer, IDiscordChronicleLibrary chronicle)
        {
            this._authorizer = authorizer;
            this._chronicle = chronicle;
        }
            
        public async Task HandleAsync(SlashCommandVeniInteractionContext slashCommand)
        {
            await slashCommand.Interaction.DeferAsync();
            if (!this._authorizer.Authorize(slashCommand.Interaction.User.Id, Permission.Inspect).Authorized)
            {
                await slashCommand.Interaction.FollowupAsync("Sorry, I only let Engineers do that with me.");
                return;
            }

            switch (slashCommand.Interaction.Data.Options.First().Name)
            {
                case "start":
                    await HandleStartAsync(slashCommand);
                    return;
                case "stop":
                    await HandleStopAsync(slashCommand);
                    return;
            }
        }
            
        private Task HandleStartAsync(SlashCommandVeniInteractionContext slashCommand)
        {
            var verbosity = slashCommand.GetInt(OPTION_VERBOSITY);
            if (this._chronicle.IsSubscribed(slashCommand.Interaction.Channel))
                this._chronicle.Unsubscribe(slashCommand.Interaction.Channel);
            this._chronicle.Subscribe(slashCommand.Interaction.Channel, (ChronicleLevel) (verbosity ?? 3));
            return slashCommand.Interaction.FollowupAsync("Oki, I've **started inspection**. 👀");
        }

        private Task HandleStopAsync(SlashCommandVeniInteractionContext slashCommand)
        {
            this._chronicle.Unsubscribe(slashCommand.Interaction.Channel);
            return slashCommand.Interaction.FollowupAsync("Oki, I've **stopped inspection**. I hope everything looks good!");
        }

    }

}