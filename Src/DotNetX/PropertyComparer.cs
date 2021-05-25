using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetX
{
    public class PropertyComparer<T, P> : IComparer<T>, IComparer
        // where P : notnull
    {
        readonly Func<T, P> getProperty;
        readonly IComparer<P> propertyComparer;

        public PropertyComparer(Func<T, P> getProperty, IComparer<P>? propertyComparer = null)
        {
            this.getProperty = getProperty ?? throw new ArgumentNullException(nameof(getProperty));
            this.propertyComparer = propertyComparer.OrDefault();
        }

        public int Compare(T? x, T? y)
        {
            if (x is { } xx)
            {
                if (y is { } yy)
                {
                    var px = getProperty(x);
                    var py = getProperty(y);
                    return propertyComparer.Compare(px, py);
                }

                return 1;
            }

            if (y is { })
            {
                return -1;
            }

            return 0;
        }

        int IComparer.Compare(object? a, object? b) =>
            a is T x && b is T y
                ? Compare(x, y)
                : throw new InvalidOperationException("Cannot use this comparer with given items");
    }

    public static class PropertyComparer
    {
        public static PropertyComparer<T, P> Create<T, P>(
            Func<T, P> getProperty,
            IComparer<P>? propertyComparer = null) =>
            new(getProperty, propertyComparer);
    }
}