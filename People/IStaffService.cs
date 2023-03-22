using System.Threading.Tasks;
using Discord.WebSocket;
using FFXIVVenues.Veni.Utils;
using FFXIVVenues.Veni.Utils.Broadcasting;

namespace FFXIVVenues.Veni.People
{
    public interface IStaffService
    {

        ulong[] Engineers { get; }
        ulong[] Editors { get; }
        ulong[] Approvers { get; }
        ulong[] Photographers { get; }

        bool IsEngineer(ulong userId);
        bool IsEditor(ulong userId);
        bool IsApprover(ulong userId);
        bool IsPhotographer(ulong userId);

        Broadcast Broadcast();

        Task<bool> HandleComponentInteractionAsync(SocketMessageComponent context);

    }
}