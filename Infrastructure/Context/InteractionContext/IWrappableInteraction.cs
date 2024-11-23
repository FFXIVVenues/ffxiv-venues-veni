using FFXIVVenues.Veni.Infrastructure.Context.InteractionContext;

namespace FFXIVVenues.Veni.Infrastructure.Context
{
    public interface IWrappableInteraction
    {
        VeniInteractionContext ToWrappedInteraction();
    }
}