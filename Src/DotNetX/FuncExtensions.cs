using System;
using System.Threading.Tasks;

namespace DotNetX
{

    public static class FuncExtensions
    {
        public static Func<T> FirstValid<T>(Func<T, bool> isValid, params Func<T>[] choices)
        {
            return () =>
            {
                foreach (var func in choices)
                {
                    var value = func();
                    if (isValid(value)) return value;
                }
                throw new InvalidOperationException($"Could not find a valid choice");
            };
        }

        public static Func<T> FirstNonNull<T>(params Func<T>[] choices) where T : class =>
            FirstValid(Predicate.IsNonNull, choices);

        public static Func<A, T> FirstValid<A, T>(Func<T, bool> isValid, params Func<A, T>[] choices)
        {
            return (a) =>
            {
                foreach (var func in choices)
                {
                    var value = func(a);
                    if (isValid(value)) return value;
                }
                throw new InvalidOperationException($"Could not find a valid choice");
            };
        }

        public static Func<A, T> FirstNonNull<A, T>(params Func<A, T>[] choices) where T : class =>
            FirstValid(Predicate.IsNonNull, choices);

        public static Func<A, B, T> FirstValid<A, B, T>(Func<T, bool> isValid, params Func<A, B, T>[] choices)
        {
            return (a, b) =>
            {
                foreach (var func in choices)
                {
                    var value = func(a, b);
                    if (isValid(value)) return value;
                }
                throw new InvalidOperationException($"Could not find a valid choice");
            };
        }

        public static Func<A, B, T> FirstNonNull<A, B, T>(params Func<A, B, T>[] choices) where T : class =>
            FirstValid(Predicate.IsNonNull, choices);

        public static Func<B, T> MapFrom<A, B, T>(this Func<A, T> source, Func<B, A> mapper)
        {
            return b => source(mapper(b));
        }

        public static Func<ValueTask<T>> AsValueTask<T>(this Func<T> func) => ()
            => new ValueTask<T>(func());

        public static Func<A, ValueTask<T>> AsValueTaskResult<A, T>(this Func<A, T> func) =>
            (A a) => new ValueTask<T>(func(a));

        public static Func<A, Task<T>> AsTaskResult<A, T>(this Func<A, T> func) =>
            (A a) => Task.FromResult(func(a));
    }
}
