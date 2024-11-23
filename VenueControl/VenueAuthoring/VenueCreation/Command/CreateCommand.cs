using System.Threading.Tasks;
using Discord;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.VenueControl.VenueAuthoring.VenueCreation.Command
{
    public static class CreateCommand
    {

        public const string COMMAND_NAME = "create";

        internal class Factory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand()
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Create a new venue! 🥰")
                    .Build();
            }

        }

        internal class Handler(IIntentHandlerProvider intentProvider) : ICommandHandler
        {
            public Task HandleAsync(SlashCommandVeniInteractionContext slashCommand) =>
                intentProvider.HandleIntent(IntentNames.Operation.Create, slashCommand);

        }

    }
}
