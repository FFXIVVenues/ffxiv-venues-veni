using Discord.WebSocket;
using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Commands.Brokerage
{
    internal interface ICommandBroker
    {
        void Add<Factory, Handler>(string key)
            where Factory : ICommandFactory
            where Handler : ICommandHandler;
        Task HandleAsync(SlashCommandInteractionContext context);
        Task RegisterAllGloballyAsync();
        Task RegisterAllAsync(SocketGuild guild);
    }

}
