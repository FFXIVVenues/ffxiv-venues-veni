using System.Threading.Tasks;
using Discord.WebSocket;

namespace FFXIVVenues.Veni.Api
{
    public interface IIndexersService
    {

        ulong[] Indexers { get; }


        bool IsIndexer(ulong userId);

        Broadcast Broadcast();

        Task<bool> HandleComponentInteractionAsync(SocketMessageComponent context);

    }
}