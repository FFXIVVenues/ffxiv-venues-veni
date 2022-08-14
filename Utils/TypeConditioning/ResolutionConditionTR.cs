using System;

namespace FFXIVVenues.Veni.Utils.TypeConditioning
{
    public class ResolutionCondition<T, R> : IResolutionCondition<T, R>, IResolvableResolutionCondition<R> where R : class
    {

        private ResolutionChain<R> _resolutionChain;
        private Func<T, R> _resolver;

        public ResolutionCondition(ResolutionChain<R> chain)
        {
            _resolutionChain = chain;
        }

        public ResolutionChain<R> Then(Func<T, R> resolver)
        {
            _resolver = resolver;
            return _resolutionChain;
        }

        public R GetResult(object obj)
        {
            if (obj is T input)
                return _resolver(input);

            return default;
        }

    }

}
