using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;

namespace FFXIVVenues.Veni.AI
{
    internal interface IAIHandler
    {
        Task<string> ResponseHandler(MessageVeniInteractionContext context);
    }
}