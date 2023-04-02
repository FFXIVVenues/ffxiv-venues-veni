using Discord.WebSocket;

namespace FFXIVVenues.Veni.Infrastructure.Context;

public interface IInteractionContextFactory
{

    MessageVeniInteractionContext Create(SocketMessage message);
    MessageComponentVeniInteractionContext Create(SocketMessageComponent message);
    SlashCommandVeniInteractionContext Create(SocketSlashCommand message);

}