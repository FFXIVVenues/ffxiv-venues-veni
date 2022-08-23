using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States.Abstractions
{
    public interface IState
    {
        Task Enter(InteractionContext c);
    }
}