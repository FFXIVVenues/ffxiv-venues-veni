using System;

namespace FFXIVVenues.Veni.Utils.TypeConditioning
{
    public interface IResolutionCondition<T, R> where R : class
    {

        public ResolutionChain<R> Then(Func<T, R> resolver);

    }

}
