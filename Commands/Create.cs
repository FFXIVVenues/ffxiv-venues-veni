using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Intents;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Commands
{
    public static class Create
    {

        public const string COMMAND_NAME = "create";

        internal class CommandFactory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null)
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Create a new venue! 🥰")
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

            public Task HandleAsync(SlashCommandInteractionContext slashCommand) =>
                this._intentProvider.HandleIntent(IntentNames.Operation.Create, slashCommand);

        }

    }
}
