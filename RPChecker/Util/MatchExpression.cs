using System;
using System.Collections.Generic;
using System.Linq;

namespace RPChecker.Util
{
    public static class MatchExpression
    {
        public static TResult Match<T, TResult>(this IList<T> target, Func<T, IList<T>, TResult> func)
        {
            return func(target.First(), target.Skip(1).TakeWhile(x => true).ToList());
        }

        public static Tuple<object, bool> Match<T1, T2>(this object target, Action<T1, T2> action)
        {
            return new Tuple<object, bool>(target, false).Match(action);
        }

        public static Tuple<object, bool> Match<T1, T2>(this Tuple<object, bool> target, Action<T1, T2> action)
        {
            //just simplely support two-tuple

            if (target.Item2)
            {
                return target;
            }

            var isMatchCompleted = false;

            var props = target.Item1.GetType().GetProperties();
            var values = new object[2];

            for (var i = 0; i < 2; i++)
            {
                values[i] = props[i].GetValue(target.Item1);
            }

            if (values[0] is T1 && values[1] is T2)
            {
                isMatchCompleted = true;
                action((T1)values[0], (T2)values[1]);
            }

            return new Tuple<object, bool>(target.Item1, isMatchCompleted);
        }

        public static Tuple<object, bool> Match<T1>(this Tuple<object, bool> target, Action<T1> action)
        {
            if (target.Item2)
            {
                return target;
            }

            var isMatchCompleted = false;
            if (target.Item1 is T1)
            {
                isMatchCompleted = true;
                action((T1)target.Item1);
            }

            return new Tuple<object, bool>(target.Item1, isMatchCompleted);
        }

        public static Tuple<object, bool> Match<T>(this object target, Action<T> action)
        {
            return new Tuple<object, bool>(target, false).Match(action);
        }
    }
}
