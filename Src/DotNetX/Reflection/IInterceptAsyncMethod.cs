using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetX.Reflection
{
    public interface IInterceptAsyncMethod : IInterceptMethodBase
    {
        Task Before(object target, MethodInfo targetMethod, object?[]? args);
        Task After(object target, MethodInfo targetMethod, object?[]? args, object? result);
        Task Error(object target, MethodInfo targetMethod, object?[]? args, Exception exception);
    }

    public interface IInterceptAsyncMethod<TState> : IInterceptMethodBase
    {
        Task<TState> Before(object target, MethodInfo targetMethod, object?[]? args);
        Task After(TState state, object target, MethodInfo targetMethod, object?[]? args, object? result);
        Task Error(TState state, object target, MethodInfo targetMethod, object?[]? args, Exception exception);
    }
}
