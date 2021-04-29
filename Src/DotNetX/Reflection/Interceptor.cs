using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DotNetX.Reflection
{

    public class Interceptor<T> : DispatchProxy
        where T : class
    {
        private T target = default!;
        private InterceptorOptions? options;

        internal void Setup(T target, InterceptorOptions options)
        {
            Debug.Assert(this.options == null);

            this.target = target;
            this.options = options;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            Debug.Assert(targetMethod is not null);
            Debug.Assert(options is not null);

            options.TryToIntercept(target, targetMethod, args, out var result);

            return result;
        }
    }

    public static class InterceptorExtensions
    {
        public static T CreateInterceptor<T>(
            this InterceptorOptions options,
            T target)
            where T : class
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var proxy = DispatchProxy.Create<T, Interceptor<T>>() as Interceptor<T>;

            proxy!.Setup(target, options);

            return (proxy as T)!;
        }
    }
}
