using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.States
{
    public interface IState
    {
        Task Init(MessageContext c);
    }
}