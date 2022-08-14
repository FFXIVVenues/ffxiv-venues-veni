using System;
using System.Collections.Generic;

namespace FFXIVVenues.Veni.Utils.TypeConditioning
{
    public class ResolutionChain<R> where R : class
    {

        private readonly List<IResolvableResolutionCondition<R>> _conditions = new();
        private readonly object _subject;
        private Func<R> _else;
        private bool _elseThrow;

        public R Result
        {
            get
            {
                foreach (var condition in _conditions)
                {
                    var result = condition.GetResult(_subject);
                    if (result != null || result != default)
                        return result;
                }
                if (_else != null)
                    return _else();
                if (_elseThrow)
                    throw new TypeMismatchException();
                return default;
            }
        }


        public ResolutionChain(object subject)
        {
            _subject = subject;
        }

        public IResolutionCondition<T, R> ElseIf<T>()
        {
            var condition = new ResolutionCondition<T, R>(this);
            _conditions.Add(condition);
            return condition;
        }

        public ResolutionChain<R> Else(Func<R> @else)
        {
            _else = @else;
            return this;
        }

        public ResolutionChain<R> ElseThrow()
        {
            this._elseThrow = true;
            return this;
        }

    }

}
