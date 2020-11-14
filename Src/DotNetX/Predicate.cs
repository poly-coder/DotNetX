using System;
using System.Linq;

namespace DotNetX
{
    public static class Predicate
    {
        public static bool True<T>(this T _) => true;
        public static bool False<T>(this T _) => false;
        public static bool IsNull<T>(this T? value) where T : class
            => value == null;
        public static bool IsNonNull<T>(this T? value) where T : class
            => value != null;


        public static Func<T, bool> And<T>(params Func<T, bool>[] predicates)
        {
            return value => predicates.All(p => p(value));
        }

        public static Func<T, bool> And<T>(Func<T, bool> predicate1, Func<T, bool> predicate2)
        {
            return value => predicate1(value) && predicate2(value);
        }

        public static Func<T, bool> And<T>(Func<T, bool> predicate1, Func<T, bool> predicate2, Func<T, bool> predicate3)
        {
            return value => predicate1(value) && predicate2(value) && predicate3(value);
        }



        public static Func<T, bool> Or<T>(params Func<T, bool>[] predicates)
        {
            return value => predicates.Any(p => p(value));
        }

        public static Func<T, bool> Or<T>(Func<T, bool> predicate1, Func<T, bool> predicate2)
        {
            return value => predicate1(value) || predicate2(value);
        }

        public static Func<T, bool> Or<T>(Func<T, bool> predicate1, Func<T, bool> predicate2, Func<T, bool> predicate3)
        {
            return value => predicate1(value) || predicate2(value) || predicate3(value);
        }
    }
}
