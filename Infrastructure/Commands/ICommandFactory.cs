using Discord;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.Infrastructure.Commands
{
    internal interface ICommandFactory
    {
        SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null);
    }

}
