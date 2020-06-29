using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DotNetX
{
    public static class PredicateExtensions
    {
        public static Func<T, bool> IsReferenceEqualsPredicate<T>(this T value)
        {
            return v => ReferenceEquals(v, value);
        }

        public static Func<T, bool> IsSamePredicate<T>(this T value, IEqualityComparer<T>? comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            return v => comparer.Equals(v, value);
        }

        public static Func<string, bool> IsSameOrdinalPredicate(this string value)
        {
            return value.IsSamePredicate(StringComparer.Ordinal);
        }

        public static Func<string, bool> IsSameOrdinalIgnoreCasePredicate(this string value)
        {
            return value.IsSamePredicate(StringComparer.OrdinalIgnoreCase);
        }

        public static Func<string, bool> IsSameInvariantCulturePredicate(this string value)
        {
            return value.IsSamePredicate(StringComparer.InvariantCulture);
        }

        public static Func<string, bool> IsSameInvariantCultureIgnoreCasePredicate(this string value)
        {
            return value.IsSamePredicate(StringComparer.InvariantCultureIgnoreCase);
        }

        public static Func<string, bool> IsSameCurrentCultureIgnoreCasePredicate(this string value)
        {
            return value.IsSamePredicate(StringComparer.CurrentCultureIgnoreCase);
        }

        public static Func<string, bool> IsSameCurrentCulturePredicate(this string value)
        {
            return value.IsSamePredicate(StringComparer.CurrentCulture);
        }

        public static Func<string, bool> IsMatchPredicate(this Regex regex, int startAt = 0)
        {
            if (regex is null)
            {
                throw new ArgumentNullException(nameof(regex));
            }

            return value =>
            {
                if (value == null) return false;
                return regex.IsMatch(value, startAt);
            };
        }

        public static Func<string, bool> IsMatchPredicate(this string regex, RegexOptions options = RegexOptions.None, int startAt = 0)
        {
            if (regex is null)
            {
                throw new ArgumentNullException(nameof(regex));
            }

            return new Regex(regex, options).IsMatchPredicate(startAt);
        }


        public static Func<T, bool> ContainsPredicate<T>(this HashSet<T> values)
        {
            return value => values.Contains(value);
        }

        public static Func<T, bool> InvertedPredicate<T>(this Func<T, bool> predicate)
        {
            return value => !predicate(value);
        }
    }
}
