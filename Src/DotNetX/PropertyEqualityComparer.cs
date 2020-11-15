using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetX
{
    public class PropertyEqualityComparer<T, P> : IEqualityComparer<T>, IEqualityComparer
        where P : notnull
    {
        readonly Func<T, P> getProperty;
        readonly IEqualityComparer<P> propertyComparer;

        public PropertyEqualityComparer(Func<T, P> getProperty, IEqualityComparer<P>? propertyComparer = null)
        {
            this.getProperty = getProperty ?? throw new ArgumentNullException(nameof(getProperty));
            this.propertyComparer = propertyComparer.OrDefault();
        }

        public bool Equals(T? x, T? y) =>
            x is not null && 
            y is not null && 
            propertyComparer.Equals(getProperty(x), getProperty(y));

        bool IEqualityComparer.Equals(object? x, object? y) =>
            x is T a && y is T b ? Equals(a, b) : false;

        public int GetHashCode(T obj) =>
            propertyComparer.GetHashCode(getProperty(obj));

        int IEqualityComparer.GetHashCode(object obj) =>
            obj is T item ? GetHashCode(item) : (obj == null ? 0 : obj.GetHashCode());
    }

    public static class PropertyEqualityComparer
    {
        public static PropertyEqualityComparer<T, P> Create<T, P>(
            Func<T, P> getProperty,
            IEqualityComparer<P>? propertyComparer = null)
            where P : notnull =>
            new PropertyEqualityComparer<T, P>(getProperty, propertyComparer);
    }
}
