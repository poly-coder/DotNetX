using System;
using System.Collections;
using System.Collections.Generic;

namespace DotNetX
{
    public class DelegateComparer<T> : IComparer<T>, IComparer
    {
        private readonly Func<T?, T?, int> compareFunc;

        public DelegateComparer(Func<T?, T?, int> compareFunc)
        {
            this.compareFunc = compareFunc ?? throw new ArgumentNullException(nameof(compareFunc));
        }

        public int Compare(T? x, T? y) => compareFunc(x, y);

        int IComparer.Compare(object? a, object? b) =>
            a is T x && b is T y
                ? Compare(x, y)
                : throw new InvalidOperationException("Cannot use this comparer with given items");
    }
}
