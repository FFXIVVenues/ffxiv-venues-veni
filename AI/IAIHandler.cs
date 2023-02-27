using FFXIVVenues.Veni.Context;
using System.Threading.Tasks;

namespace FFXIVVenues.Veni.AI
{
    internal interface IAIHandler
    {
        Task<string> ResponseHandler(MessageInteractionContext context);
    }
}