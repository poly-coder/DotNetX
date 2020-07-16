using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetX
{
    public class PropertyEqualityComparer<T, P> : IEqualityComparer<T>, IEqualityComparer
    {
        readonly Func<T, P> getProperty;
        readonly IEqualityComparer<P> propertyComparer;

        public PropertyEqualityComparer(Func<T, P> getProperty, IEqualityComparer<P>? propertyComparer = null)
        {
            this.getProperty = getProperty ?? throw new ArgumentNullException(nameof(getProperty));
            this.propertyComparer = propertyComparer ?? EqualityComparer<P>.Default;
        }

        public bool Equals(T x, T y) =>
            propertyComparer.Equals(getProperty(x), getProperty(y));

        public int GetHashCode(T obj) =>
            propertyComparer.GetHashCode(getProperty(obj));

        bool IEqualityComparer.Equals(object x, object y) =>
            x is T a && y is T b ? Equals(a, b) : Object.Equals(x, y);

        int IEqualityComparer.GetHashCode(object obj) =>
            obj is T item ? GetHashCode(item) : (obj == null ? 0 : obj.GetHashCode());
    }
}
