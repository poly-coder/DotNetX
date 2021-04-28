using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace DotNetX.Reflection
{
    public record InterceptorOptions(
        string DisplayName,
        ImmutableList<InterceptMethod> Interceptors)
        : InterceptMethod()
    {
        public static readonly InterceptorOptions Default = CreateDefaultOptions();

        private static InterceptorOptions CreateDefaultOptions()
        {
            return new InterceptorOptions(
                nameof(Default),
                ImmutableList<InterceptMethod>.Empty);
        }

        public InterceptorOptions Prepend(InterceptMethod interceptor)
        {
            return this with
            {
                Interceptors = Interceptors.Insert(0, interceptor),
            };
        }

        public InterceptorOptions Add(InterceptMethod interceptor)
        {
            return this with
            {
                Interceptors = Interceptors.Add(interceptor),
            };
        }

        public InterceptorOptions AddRange(IEnumerable<InterceptMethod> interceptors)
        {
            return this with
            {
                Interceptors = Interceptors.AddRange(interceptors),
            };
        }

        public override bool TryToIntercept(object target, MethodInfo targetMethod, object?[]? args, out object? result)
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
