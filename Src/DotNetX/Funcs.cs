using System;

namespace DotNetX
{
    public static class Funcs
    {
        public static T Identity<T>(T value) => value;

#pragma warning disable IDE0060 // Remove unused parameter
        public static void Ignore<T>(T _) { }
#pragma warning restore IDE0060 // Remove unused parameter

        public static T Tee<T>(this T value, Action<T> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            action(value);
            return value;
        }
    }
}
