using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetX
{
    public class DelegateEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
    {
        private readonly Func<T, T, bool> equalsFunc;
        private readonly Func<T, int> getHashCodeFunc;

        public DelegateEqualityComparer(
            Func<T, T, bool> equalsFunc,
            Func<T, int> getHashCodeFunc)
        {
            this.equalsFunc = equalsFunc ?? throw new ArgumentNullException(nameof(equalsFunc));
            this.getHashCodeFunc = getHashCodeFunc ?? throw new ArgumentNullException(nameof(getHashCodeFunc));
        }

        public bool Equals(T x, T y) => equalsFunc(x, y);

        bool IEqualityComparer.Equals(object a, object b) =>
            a is T x && b is T y 
                ? Equals(x, y)
                : throw new InvalidOperationException("Cannot use this comparer with given items");

        public int GetHashCode(T obj) => getHashCodeFunc(obj);

        int IEqualityComparer.GetHashCode(object obj) =>
            obj is T x 
                ? GetHashCode(x) 
                : throw new InvalidOperationException("Cannot use this comparer with given item");
    }
}
