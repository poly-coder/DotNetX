using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetX
{
    public static class ComparingExtensions
    {
        public static IComparer<T> Inverse<T>(this IComparer<T> comparer) =>
            new InverseComparer<T>(comparer);

        #region [ IComparable<T> | Is(Between|AtLeast|AtMost)[Exclusive], Clamp, AtLeast, AtMost ]

        public static bool IsAtLeast<T>(this T value, T min)
            where T : IComparable<T> =>
            value.CompareTo(min) >= 0;

        public static bool IsAtLeastExclusive<T>(this T value, T min)
            where T : IComparable<T> =>
            value.CompareTo(min) > 0;

        public static bool IsAtMost<T>(this T value, T max)
            where T : IComparable<T> =>
            value.CompareTo(max) <= 0;

        public static bool IsAtMostExclusive<T>(this T value, T max)
            where T : IComparable<T> =>
            value.CompareTo(max) < 0;

        public static bool IsBetween<T>(this T value, T min, T max)
            where T : IComparable<T> => 
            value.CompareTo(min) >= 0 && value.CompareTo(max) <= 0;

        public static bool IsBetweenExclusive<T>(this T value, T min, T max)
            where T : IComparable<T> => 
            value.CompareTo(min) > 0 && value.CompareTo(max) < 0;

        public static T AtLeast<T>(this T value, T min) 
            where T : IComparable<T> =>
            !value.IsAtLeastExclusive(min) ? min
            : value;

        public static T AtMost<T>(this T value, T max) 
            where T : IComparable<T> =>
            !value.IsAtMostExclusive(max) ? max
            : value;

        public static T Clamp<T>(this T value, T min, T max) 
            where T : IComparable<T> =>
            !value.IsAtLeastExclusive(min) ? min
            : !value.IsAtMostExclusive(max) ? max
            : value;
        
        #endregion [ IComparable<T> ]

        #region [ IComparer<T> | Is(Between|AtLeast|AtMost)[Exclusive], Clamp, AtLeast, AtMost ]

        public static bool IsAtLeast<T>(this T value, T min, IComparer<T>? comparer) =>
            comparer.OrDefault().Compare(value, min) >= 0;

        public static bool IsAtLeastExclusive<T>(this T value, T min, IComparer<T>? comparer) =>
            comparer.OrDefault().Compare(value, min) > 0;

        public static bool IsAtMost<T>(this T value, T max, IComparer<T>? comparer) =>
            comparer.OrDefault().Compare(value, max) <= 0;

        public static bool IsAtMostExclusive<T>(this T value, T max, IComparer<T>? comparer) =>
            comparer.OrDefault().Compare(value, max) < 0;

        public static bool IsBetween<T>(this T value, T min, T max, IComparer<T>? comparer)
        {
            comparer ??= Comparer<T>.Default;
            return comparer.Compare(value, min) >= 0 && comparer.Compare(value, max) <= 0;
        }

        public static bool IsBetweenExclusive<T>(this T value, T min, T max, IComparer<T>? comparer)
        {
            comparer ??= Comparer<T>.Default;
            return comparer.Compare(value, min) > 0 && comparer.Compare(value, max) < 0;
        }

        public static T AtLeast<T>(this T value, T min, IComparer<T>? comparer) =>
            !value.IsAtLeastExclusive(min, comparer) ? min
            : value;

        public static T AtMost<T>(this T value, T max, IComparer<T>? comparer) =>
            !value.IsAtMostExclusive(max, comparer) ? max
            : value;

        public static T Clamp<T>(this T value, T min, T max, IComparer<T>? comparer)
        {
            comparer ??= Comparer<T>.Default;
            return 
                !value.IsAtLeastExclusive(min, comparer) ? min
                : !value.IsAtMostExclusive(max, comparer) ? max
                : value;
        }

        #endregion [ IComparable<T> ]
    }
}
