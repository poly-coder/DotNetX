using System;

namespace DotNetX
{
    public static class Funcs
    {
        public static T Identity<T>(T value) => value;
        public static void Ignore<T>(T _value) { }
        public static A Tee<A>(this A value, Action<A> action)
        {
            action(value);
            return value;
        }
    }
}
