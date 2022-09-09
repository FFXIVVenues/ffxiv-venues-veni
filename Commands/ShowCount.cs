using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Commands.Brokerage;
using FFXIVVenues.Veni.Context;
using FFXIVVenues.Veni.Intents;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Commands
{
    internal class ShowCount
    {
        public const string COMMAND_NAME = "showcount";

        internal class CommandFactory : ICommandFactory
        {
            public SlashCommandProperties GetSlashCommand (SocketGuild guildContext = null)
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Show how many venues we have indexed so far")
                    .Build();
            }
        }

        internal class CommandHandler : ICommandHandler
        {
            private readonly IIntentHandlerProvider _intentHandlerProvider;

            public CommandHandler(IIntentHandlerProvider intentHandlerProvider)
            {
                _intentHandlerProvider = intentHandlerProvider;
            }

            public Task HandleAsync(SlashCommandInteractionContext slashCommand) => 
                this._intentHandlerProvider.HandleIntent(IntentNames.Operation.ShowCount, slashCommand);
        }
    }
}
