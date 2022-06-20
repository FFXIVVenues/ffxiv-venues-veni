using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    interface IState
    {
        Task Enter(MessageContext c);
        Task Handle(MessageContext c);
    }
}