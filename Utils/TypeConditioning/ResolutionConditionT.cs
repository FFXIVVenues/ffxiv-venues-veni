using System;

namespace FFXIVVenues.Veni.Utils.TypeConditioning
{
    public class ResolutionCondition<T>
    {
        private readonly object _subject;

        public ResolutionCondition(object subject)
        {
            _subject = subject;
        }

        public ResolutionChain<R> Then<R>(Func<T, R> resolver) where R : class
        {
            var chain = new ResolutionChain<R>(_subject);
            return chain.ElseIf<T>().Then(resolver);
        }

    }

}
