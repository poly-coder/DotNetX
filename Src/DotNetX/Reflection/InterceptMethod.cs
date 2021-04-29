using System.Reflection;

namespace DotNetX.Reflection
{
    public abstract record InterceptMethod()
    {
        public abstract bool TryToIntercept(object target, MethodInfo targetMethod, object?[]? args, out object? result);
    }
}
