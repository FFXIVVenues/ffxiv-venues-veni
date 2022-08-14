using Discord;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.Commands.Brokerage
{
    internal interface ICommandFactory
    {
        SlashCommandProperties GetSlashCommand(SocketGuild guildContext = null);
    }

}
