using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;

namespace FFXIVVenues.Veni.Infrastructure.Context;

public interface IInteractionContextFactory
{

    MessageVeniInteractionContext Create(SocketMessage message);
    ComponentVeniInteractionContext Create(SocketMessageComponent message);
    SlashCommandVeniInteractionContext Create(SocketSlashCommand message);

}