using System;
using System.Threading;

namespace DotNetX
{
    public static class LazyFn
    {
        public static Lazy<T> Create<T>(Func<T> valueFactory) =>
            new Lazy<T>(valueFactory);

        public static Lazy<T> Create<T>(Func<T> valueFactory, bool isThreadSafe) =>
            new Lazy<T>(valueFactory, isThreadSafe);

        public static Lazy<T> Create<T>(Func<T> valueFactory, LazyThreadSafetyMode mode) =>
            new Lazy<T>(valueFactory, mode);
    }
}
