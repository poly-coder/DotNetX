using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetX
{
    public class InverseComparer<T> : IComparer<T>, IComparer
    {
        private readonly IComparer<T> innerComparer;

        public InverseComparer(IComparer<T> innerComparer)
        {
            this.innerComparer = innerComparer ?? throw new ArgumentNullException(nameof(innerComparer));
        }

        public int Compare(T? x, T? y) => innerComparer.Compare(y, x);

        int IComparer.Compare(object? a, object? b) =>
            a is T x && b is T y
                ? Compare(x, y)
                : throw new InvalidOperationException("Cannot use this comparer with given items");
    }
}