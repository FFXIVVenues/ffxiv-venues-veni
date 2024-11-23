using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.UserSupport;

public static class HelpCommand
{

    public const string COMMAND_NAME = "help";

    internal class CommandFactory : ICommandFactory
    {

        public SlashCommandProperties GetSlashCommand()
        {
            return new SlashCommandBuilder()
                .WithName(COMMAND_NAME)
                .WithDescription("Shows information on what I can do!")
                .Build();
        }
    }

    internal class CommandHandler(IIntentHandlerProvider intentProvider) : ICommandHandler
    {
        public Task HandleAsync(SlashCommandVeniInteractionContext slashCommand) =>
            intentProvider.HandleIteruptIntent(IntentNames.Interupt.Help, slashCommand);

    }

}