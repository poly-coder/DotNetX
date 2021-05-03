using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace DotNetX.Reflection
{
    public record InterceptorOptions(
        string DisplayName,
        ImmutableList<IInterceptMethod> Interceptors)
        : IInterceptMethod
    {
        public static readonly InterceptorOptions Default = CreateDefaultOptions();

        private static InterceptorOptions CreateDefaultOptions()
        {
            return new InterceptorOptions(
                nameof(Default),
                ImmutableList<IInterceptMethod>.Empty);
        }

        public InterceptorOptions Prepend(IInterceptMethod interceptor)
        {
            return this with
            {
                Interceptors = Interceptors.Insert(0, interceptor),
            };
        }

        public InterceptorOptions Add(IInterceptMethod interceptor)
        {
            return this with
            {
                Interceptors = Interceptors.Add(interceptor),
            };
        }

        public InterceptorOptions AddRange(IEnumerable<IInterceptMethod> interceptors)
        {
            return this with
            {
                Interceptors = Interceptors.AddRange(interceptors),
            };
        }

        public bool TryToIntercept(object target, MethodInfo targetMethod, object?[]? args, out object? result)
        {
            foreach (var interceptor in Interceptors)
            {
                if (interceptor.TryToIntercept(target, targetMethod, args, out result))
                {
                    return true;
                }
            }

            result = targetMethod.Invoke(target, args);
            return true;
        }
    }
}
