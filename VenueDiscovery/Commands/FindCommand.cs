using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.VenueDiscovery.Commands
{
    public static class FindCommand
    {

        public const string COMMAND_NAME = "find";

        internal class CommandFactory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null)
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Find a venue by it's name!")
                    .AddOption("search-query", ApplicationCommandOptionType.String, "Part or all of the name of the venues you want to find.")
                    .Build();
            }

        }

        internal class CommandHandler : ICommandHandler
        {
            private readonly IIntentHandlerProvider _intentProvider;

            public CommandHandler(IIntentHandlerProvider intentProvider)
            {
                this._intentProvider = intentProvider;
            }

            public Task HandleAsync(SlashCommandVeniInteractionContext slashCommand) =>
                this._intentProvider.HandleIntent(IntentNames.Operation.Search, slashCommand);

        }

    }
}
