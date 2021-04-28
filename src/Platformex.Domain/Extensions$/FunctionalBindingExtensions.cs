using System;

namespace Platformex.Domain
{
    public static class FunctionalBindingExtensions
    {
        public static Action<T2> Bind<T1, T2>(this Action<T1, T2> action, T1 value)
        {
            return openArg => action(value, openArg);
        }

        public static Func<T2, T3> Bind<T1, T2, T3>(this Func<T1, T2, T3> action, T1 value)
        {
            return openArg => action(value, openArg);
        }
    }
}