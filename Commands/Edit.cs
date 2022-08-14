using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Commands.Brokerage;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Commands
{
    public static class Edit
    {

        public const string COMMAND_NAME = "edit";

        internal class CommandFactory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null)
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Edit your venue(s)!")
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
                this._intentProvider.HandleIntent(IntentNames.Operation.Edit, slashCommand);

        }

    }
}
