using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.Infrastructure.Commands
{
    public interface ICommandBroker
    {
        void Add<TFactory, THandler>(string key)
            where TFactory : ICommandFactory
            where THandler : ICommandHandler;
        Task HandleAsync(SlashCommandVeniInteractionContext context);
        Task RegisterAllGloballyAsync();
        Task RegisterAllAsync(SocketGuild guild);
    }

}
