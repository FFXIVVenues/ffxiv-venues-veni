using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.Infrastructure.Commands
{
    internal interface ICommandBroker
    {
        void Add<Factory, Handler>(string key)
            where Factory : ICommandFactory
            where Handler : ICommandHandler;
        Task HandleAsync(SlashCommandVeniInteractionContext context);
        Task RegisterAllGloballyAsync();
        Task RegisterAllAsync(SocketGuild guild);
    }

}
