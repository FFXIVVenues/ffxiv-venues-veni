using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Infrastructure.Context.Session
{
    public interface ISessionState
    {
        Task Enter(VeniInteractionContext c);
    }
}