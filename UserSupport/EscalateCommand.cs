using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.UserSupport;

public static class EscalateCommand
{

    public const string COMMAND_NAME = "escalate";

    internal class CommandFactory : ICommandFactory
    {

        public SlashCommandProperties GetSlashCommand()
        {
            return new SlashCommandBuilder()
                .WithName(COMMAND_NAME)
                .WithDescription("Message FFXIV Venues staff for help!")
                .Build();
        }

    }

    internal class CommandHandler(IIntentHandlerProvider intentProvider) : ICommandHandler
    {
        public Task HandleAsync(SlashCommandVeniInteractionContext slashCommand) =>
            intentProvider.HandleIteruptIntent(IntentNames.Interupt.Escalate, slashCommand);

    }

}