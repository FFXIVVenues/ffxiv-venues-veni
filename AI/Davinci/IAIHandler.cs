using System.Threading.Tasks;
using FFXIVVenues.Veni.Infrastructure.Context;
using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;

namespace FFXIVVenues.Veni.AI.Davinci;

internal interface IAiHandler
{
    Task<string> ResponseHandler(MessageVeniInteractionContext context);
}