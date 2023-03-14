namespace FFXIVVenues.Veni.Infrastructure.Context
{
    public interface IWrappableInteraction
    {

        InteractionContext ToWrappedInteraction();

    }
}