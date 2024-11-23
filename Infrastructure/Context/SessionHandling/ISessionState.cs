using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;

namespace FFXIVVenues.Veni.Infrastructure.Context.SessionHandling;

public interface ISessionStateBase
{
    Task EnterState(VeniInteractionContext interactionContext);
}

public interface ISessionState : ISessionStateBase;
public interface ISessionState<in T> : ISessionStateBase;
