using FFXIVVenues.Veni.Utils.TypeConditioning;

namespace FFXIVVenues.Veni.Context.InteractionWrappers
{
    public interface IInteractionDataWrapper
    {
        string Name { get; }

        string GetArgument(string name);

        ResolutionCondition<T> If<T>();
    }

}
