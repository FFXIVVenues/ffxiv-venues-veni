using Discord;
using Discord.WebSocket;
using FFXIVVenues.Veni.Intents;
using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Commands;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Intent;

namespace FFXIVVenues.Veni.Commands
{
    public static class Escalate
    {

        public const string COMMAND_NAME = "escalate";

        internal class CommandFactory : ICommandFactory
        {

            public SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null)
            {
                return new SlashCommandBuilder()
                    .WithName(COMMAND_NAME)
                    .WithDescription("Message FFXIV Venues staff for help!")
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
                this._intentProvider.HandleIteruptIntent(IntentNames.Interupt.Escalate, slashCommand);

        }

    }
}
