namespace FFXIVVenues.Veni.Utils.TypeConditioning
{
    public interface IResolvableResolutionCondition<R>
    {

        R GetResult(object obj);

    }

}
