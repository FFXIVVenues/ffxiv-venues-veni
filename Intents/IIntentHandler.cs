using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.Intents
{
    internal interface IIntentHandler
    {
        Task Handle(MessageContext context);
    }
}