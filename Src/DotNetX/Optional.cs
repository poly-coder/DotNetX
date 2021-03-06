﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetX
{
#pragma warning disable CA1716
    public struct Optional<T> : IEquatable<Optional<T>>
#pragma warning restore CA1716
    {
        internal static Optional<T> None => default;
        internal static Optional<T> Some(T value) => new Optional<T>(value);

        internal readonly bool isSome;
        internal readonly T value;

        public Optional(T value)
        {
            this.value = value;
            isSome = true;
        }
        
        public override string ToString()
        {
            if (isSome)
            {
                return $"Some({value})";
            }
            else
            {
                return "None";
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is Optional<T> optional && Equals(optional);
        }

        public bool Equals(Optional<T> other)
        {
            return isSome == other.isSome &&
                   EqualityComparer<T>.Default.Equals(value, other.value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(isSome, value);
        }

        public static bool operator ==(Optional<T> left, Optional<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Optional<T> left, Optional<T> right)
        {
            return !(left == right);
        }

        public bool IsSome => isSome;
        
        public bool IsNone => !isSome;

        public TResult MatchWith<TResult>(Func<T, TResult> onSome, Func<TResult> onNone)
        {
            if (onSome is null)
            {
                throw new ArgumentNullException(nameof(onSome));
            }

            if (onNone is null)
            {
                throw new ArgumentNullException(nameof(onNone));
            }

            if (isSome)
            {
                return onSome(value);
            }
            else
            {
                return onNone();
            }
        }

        public void MatchWith(Action<T> onSome, Action onNone)
        {
            if (onSome is null)
            {
                throw new ArgumentNullException(nameof(onSome));
            }

            if (onNone is null)
            {
                throw new ArgumentNullException(nameof(onNone));
            }

            if (isSome)
            {
                onSome(value);
            }
            else
            {
                onNone();
            }
        }

        public bool TryGetValue(out T value)
        {
            if (isSome)
            {
                value = this.value;
                return true;
            }
            else
            {
#pragma warning disable CS8601 // Possible null reference assignment.
                value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
                return false;
            }
        }

        public T GetValue()
        {
            if (TryGetValue(out var value))
            {
                return value;
            }
            throw new InvalidOperationException();
        }

        public T GetValue(Func<T> defaultValueFn)
        {
            return MatchWith(v => v, defaultValueFn);
        }

        public T GetValue(T defaultValue)
        {
            return GetValue(() => defaultValue);
        }

    }

#pragma warning disable CA1716
    public static class Optional
#pragma warning restore CA1716
    {
        public static Optional<T> None<T>() => Optional<T>.None;

        public static Optional<T> Some<T>(T value) => Optional<T>.Some(value);

        public static Optional<B> Bind<A, B>(this Optional<A> optional, Func<A, Optional<B>> f)
        {
            return optional.MatchWith(f, None<B>);
        }

        public static Task<Optional<B>> BindAsync<A, B>(this Optional<A> optional, Func<A, Task<Optional<B>>> f)
        {
            return optional.MatchWith(f, () => Task.FromResult(Optional<B>.None));
        }

        public static Optional<B> Map<A, B>(this Optional<A> optional, Func<A, B> f)
        {
            return optional.Bind(a => Some(f(a)));
        }

        public static Task<Optional<B>> MapAsync<A, B>(this Optional<A> optional, Func<A, Task<B>> f)
        {
            return optional.BindAsync(async a => Some(await f(a)));
        }

        public static IEnumerable<T> AsEnumerable<T>(this Optional<T> optional)
        {
            return optional.MatchWith(
                v => v.Singleton(),
                () => Enumerable.Empty<T>());
        }

        public static Optional<T> FirstSome<T>(this IEnumerable<Optional<T>> optionals)
        {
            foreach (var item in optionals)
            {
                if (item.isSome)
                {
                    return item;
                }
            }

            return None<T>();
        }
    }
}
