using System.Reflection;

namespace DotNetX.Reflection
{
    public interface IInterceptMethod
    {
        bool TryToIntercept(object target, MethodInfo targetMethod, object?[]? args, out object? result);

        public bool ShouldIntercept(object target, MethodInfo targetMethod, object?[]? args) => true;
    }
}
